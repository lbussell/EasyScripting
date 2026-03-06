// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using System.Text;
using CliWrap;
using CliWrap.Buffered;
using Spectre.Console;

namespace EasyScripting;

/// <summary>
/// A convenience wrapper around a CLI command that pipes output to an <see cref="IAnsiConsole"/>
/// and supports confirmation prompts before execution.
/// </summary>
public class CliWrapper
{
    private readonly string _commandName;
    private readonly Command _command;
    private readonly IAnsiConsole _ansiConsole;

    /// <summary>
    /// Creates a new <see cref="CliWrapper"/> for the specified CLI command using
    /// the default <see cref="AnsiConsole.Console"/>.
    /// </summary>
    /// <param name="command">The command name or path (e.g. "git", "gh").</param>
    public static CliWrapper Create(string command) => new(command, AnsiConsole.Console);

    /// <summary>
    /// Creates a new <see cref="CliWrapper"/> for the specified CLI command.
    /// </summary>
    /// <param name="command">The command name or path (e.g. "git", "gh").</param>
    /// <param name="ansiConsole">The <see cref="IAnsiConsole"/> instance to use for output.</param>
    public CliWrapper(string command, IAnsiConsole ansiConsole)
    {
        _commandName = command;
        _ansiConsole = ansiConsole;

        PipeTarget stdOutPipe = PipeTarget.ToDelegate(HandleStandardOutput);
        PipeTarget stdErrPipe = PipeTarget.ToDelegate(HandleStandardError);

        _command = Cli.Wrap(command)
            .WithStandardOutputPipe(stdOutPipe)
            .WithStandardErrorPipe(stdErrPipe)
            .WithValidation(CommandResultValidation.ZeroExitCode);
    }

    /// <summary>
    /// Prompts the user for confirmation, then executes the command with the given arguments.
    /// </summary>
    /// <param name="arguments">The command-line arguments.</param>
    /// <param name="standardInput">Optional text to pipe to standard input.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The buffered command result.</returns>
    /// <exception cref="OperationCanceledException">Thrown when the user declines the confirmation prompt.</exception>
    public Task<BufferedCommandResult> RunWithConfirmationAsync(
        string arguments,
        string? standardInput = null,
        CancellationToken cancellationToken = default
    )
    {
        string commandString = Markup.Escape($"{_commandName} {arguments}");
        return !_ansiConsole.Confirm($"Run [blue]{commandString}[/]?")
            ? throw new OperationCanceledException("User aborted the operation.")
            : RunAsync(arguments, standardInput, cancellationToken);
    }

    /// <summary>
    /// Executes the command with the given arguments.
    /// </summary>
    /// <param name="arguments">The command-line arguments.</param>
    /// <param name="standardInput">Optional text to pipe to standard input.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The buffered command result.</returns>
    public Task<BufferedCommandResult> RunAsync(
        string arguments,
        string? standardInput = null,
        CancellationToken cancellationToken = default
    )
    {
        string commandString = Markup.Escape($"{_commandName} {arguments}");
        _ansiConsole.MarkupLineInterpolated($"[blue][[exec]] {commandString}[/]");
        Command cmd = _command.WithArguments(arguments);

        if (standardInput is not null)
            cmd = cmd.WithStandardInputPipe(PipeSource.FromString(standardInput));

        return cmd.ExecuteBufferedAsync(Encoding.UTF8, Encoding.UTF8, cancellationToken);
    }

    /// <summary>
    /// Displays a single line emitted to standard output.
    /// </summary>
    /// <param name="line">The line of output text.</param>
    private void HandleStandardOutput(string line) =>
        _ansiConsole.MarkupLineInterpolated($"[dim]| {line}[/]");

    /// <summary>
    /// Displays a single line emitted to standard error.
    /// </summary>
    /// <param name="line">The line of error text.</param>
    private void HandleStandardError(string line) =>
        _ansiConsole.MarkupLineInterpolated($"[yellow]| {line}[/]");
}
