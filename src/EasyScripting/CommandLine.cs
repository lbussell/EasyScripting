// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace EasyScripting;

/// <summary>
/// Provides the <see cref="Shell"/> entry point for building CLI commands.
/// Import with <c>using static EasyScripting.CommandLine;</c> for top-level access.
/// </summary>
public static class CommandLine
{
    /// <summary>
    /// Creates a new <see cref="CommandBuilder"/> for the given command line.
    /// </summary>
    /// <param name="commandLine">
    /// The full command line to execute. The first whitespace-delimited token is the
    /// executable name; the remainder is passed as arguments.
    /// </param>
    public static CommandBuilder Shell(string commandLine) => new() { CommandLine = commandLine };
}
