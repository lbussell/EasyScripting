// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Text;
using CliWrap;
using CliWrap.Buffered;
using Spectre.Console;

namespace EasyScripting;

/// <summary>
/// Extension methods that add fluent configuration and execution to <see cref="CommandBuilder"/>.
/// </summary>
public static class CommandBuilderExtensions
{
    /// <summary>Trims leading and trailing whitespace from the captured standard output.</summary>
    public static CommandBuilder Trim(this CommandBuilder builder) =>
        builder with
        {
            TrimOutput = true,
        };

    /// <summary>Suppresses the <c>[exec]</c> log line and streamed console output.</summary>
    public static CommandBuilder Quiet(this CommandBuilder builder) =>
        builder with
        {
            SuppressOutput = true,
        };

    /// <summary>Prompts the user for confirmation before executing the command.</summary>
    public static CommandBuilder Confirm(this CommandBuilder builder) =>
        builder with
        {
            RequireConfirmation = true,
        };

    /// <summary>Pipes the specified text to the command's standard input.</summary>
    public static CommandBuilder Input(this CommandBuilder builder, string text) =>
        builder with
        {
            StandardInput = text,
        };

    /// <summary>
    /// Executes the command and returns the captured standard output.
    /// </summary>
    /// <returns>The standard output of the command, optionally trimmed.</returns>
    /// <exception cref="OperationCanceledException">
    /// Thrown when <see cref="CommandBuilder.RequireConfirmation"/> is set and the user declines.
    /// </exception>
    public static async Task<string> RunAsync(
        this CommandBuilder builder,
        CancellationToken cancellationToken = default
    )
    {
        (string executable, string arguments) = SplitCommandLine(builder.CommandLine);

        Command command = Cli.Wrap(executable)
            .WithArguments(arguments)
            .WithValidation(CommandResultValidation.ZeroExitCode);

        if (!builder.SuppressOutput)
        {
            command = command
                .WithStandardOutputPipe(
                    PipeTarget.ToDelegate(line =>
                        AnsiConsole.MarkupLineInterpolated($"[dim]| {line}[/]")
                    )
                )
                .WithStandardErrorPipe(
                    PipeTarget.ToDelegate(line =>
                        AnsiConsole.MarkupLineInterpolated($"[yellow]| {line}[/]")
                    )
                );
        }

        if (builder.RequireConfirmation)
        {
            string display = Markup.Escape(builder.CommandLine);
            if (!Prompt.Confirm($"Run [blue]{display}[/]?"))
                throw new OperationCanceledException("User aborted the operation.");
        }

        if (!builder.SuppressOutput)
        {
            string display = Markup.Escape(builder.CommandLine);
            AnsiConsole.MarkupLine($"[blue][[exec]] {display}[/]");
        }

        if (builder.StandardInput is not null)
            command = command.WithStandardInputPipe(PipeSource.FromString(builder.StandardInput));

        BufferedCommandResult result = await command.ExecuteBufferedAsync(
            Encoding.UTF8,
            Encoding.UTF8,
            cancellationToken
        );

        return builder.TrimOutput ? result.StandardOutput.Trim() : result.StandardOutput;
    }

    private static (string Executable, string Arguments) SplitCommandLine(string commandLine)
    {
        string[] parts = commandLine.Split(' ', count: 2);
        return (parts[0], parts.Length > 1 ? parts[1] : string.Empty);
    }
}
