// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Text;
using CliWrap;
using CliWrap.Buffered;
using Spectre.Console;

namespace EasyScripting;

/// <summary>
/// A fluent builder for a CLI command, configured via hook methods.
/// Use <see cref="BeforeExecute"/>, <see cref="AfterExecute"/>, <see cref="OnStandardOutput"/>,
/// and <see cref="OnStandardError"/> to add hooks, then call <see cref="RunAsync"/> to execute.
/// </summary>
public class CommandBuilder
{
    /// <summary>The full command line to execute (e.g. "git remote get-url origin").</summary>
    public required string CommandLine { get; init; }

    private string? StandardInput { get; set; }
    private List<Func<CommandBuilder, CancellationToken, Task>> BeforeHooks { get; } = [];
    private List<Func<string, string>> AfterHooks { get; } = [];
    private List<Action<string>> StandardOutputHooks { get; } = [];
    private List<Action<string>> StandardErrorHooks { get; } = [];

    /// <summary>Adds a hook that runs before command execution. Can throw to abort.</summary>
    public CommandBuilder BeforeExecute(Func<CommandBuilder, CancellationToken, Task> hook)
    {
        BeforeHooks.Add(hook);
        return this;
    }

    /// <summary>Adds a hook that transforms the captured standard output after execution.</summary>
    public CommandBuilder AfterExecute(Func<string, string> hook)
    {
        AfterHooks.Add(hook);
        return this;
    }

    /// <summary>Adds a hook called per line of standard output.</summary>
    public CommandBuilder OnStandardOutput(Action<string> hook)
    {
        StandardOutputHooks.Add(hook);
        return this;
    }

    /// <summary>Adds a hook called per line of standard error.</summary>
    public CommandBuilder OnStandardError(Action<string> hook)
    {
        StandardErrorHooks.Add(hook);
        return this;
    }

    /// <summary>Pipes the specified text to the command's standard input.</summary>
    public CommandBuilder WithStandardInput(string text)
    {
        StandardInput = text;
        return this;
    }

    /// <summary>Suppresses the <c>[exec]</c> log line and streamed console output.</summary>
    public CommandBuilder Quiet()
    {
        StandardOutputHooks.Clear();
        StandardErrorHooks.Clear();
        return this;
    }

    /// <summary>
    /// Executes the command and returns the captured standard output.
    /// </summary>
    /// <returns>The standard output, after any after-hooks have been applied.</returns>
    public async Task<string> RunAsync(CancellationToken cancellationToken = default)
    {
        foreach (Func<CommandBuilder, CancellationToken, Task> hook in BeforeHooks)
            await hook(this, cancellationToken);

        string[] parts = CommandLine.Split(' ', count: 2);
        string executable = parts[0];
        string arguments = parts.Length > 1 ? parts[1] : string.Empty;

        Command command = Cli.Wrap(executable)
            .WithArguments(arguments)
            .WithValidation(CommandResultValidation.ZeroExitCode);

        if (StandardOutputHooks.Count > 0 || StandardErrorHooks.Count > 0)
            AnsiConsole.MarkupLine($"[blue][[exec]] {Markup.Escape(CommandLine)}[/]");

        if (StandardOutputHooks.Count > 0)
        {
            List<Action<string>> stdoutHooks = StandardOutputHooks;
            command = command.WithStandardOutputPipe(
                PipeTarget.ToDelegate(line =>
                {
                    foreach (Action<string> hook in stdoutHooks)
                        hook(line);
                })
            );
        }

        if (StandardErrorHooks.Count > 0)
        {
            List<Action<string>> stderrHooks = StandardErrorHooks;
            command = command.WithStandardErrorPipe(
                PipeTarget.ToDelegate(line =>
                {
                    foreach (Action<string> hook in stderrHooks)
                        hook(line);
                })
            );
        }

        if (StandardInput is not null)
            command = command.WithStandardInputPipe(PipeSource.FromString(StandardInput));

        BufferedCommandResult result = await command.ExecuteBufferedAsync(
            Encoding.UTF8,
            Encoding.UTF8,
            cancellationToken
        );

        string output = result.StandardOutput;

        foreach (Func<string, string> hook in AfterHooks)
            output = hook(output);

        return output;
    }
}
