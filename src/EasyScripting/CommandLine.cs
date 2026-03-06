// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using Spectre.Console;

namespace EasyScripting;

/// <summary>
/// Provides the <see cref="Shell"/> entry point for building CLI commands.
/// Import with <c>using static EasyScripting.CommandLine;</c> for top-level access.
/// </summary>
public static class CommandLine
{
    /// <summary>
    /// Creates a new <see cref="CommandBuilder"/> for the given command line,
    /// with default handlers that log stdout and stderr to the console.
    /// </summary>
    /// <param name="commandLine">
    /// The full command line to execute. The first whitespace-delimited token is the
    /// executable name; the remainder is passed as arguments.
    /// </param>
    public static CommandBuilder Shell(string commandLine) =>
        new CommandBuilder { CommandLine = commandLine }
            .OnStandardOutput(line => AnsiConsole.MarkupLineInterpolated($"[dim]| {line}[/]"))
            .OnStandardError(line => AnsiConsole.MarkupLineInterpolated($"[yellow]| {line}[/]"));
}
