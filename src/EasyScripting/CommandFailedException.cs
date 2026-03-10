// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace EasyScripting;

/// <summary>
/// Thrown when a command exits with a non-zero exit code and throw-on-failure is enabled.
/// </summary>
public sealed class CommandFailedException : Exception
{
    /// <summary>The command line that was executed.</summary>
    public string CommandLine { get; }

    /// <summary>The full result of the failed command.</summary>
    public CommandResult Result { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="CommandFailedException"/>.
    /// </summary>
    /// <param name="commandLine">The command line that was executed.</param>
    /// <param name="result">The result containing stdout, stderr, and exit code.</param>
    public CommandFailedException(string commandLine, CommandResult result)
        : base($"Command '{commandLine}' failed with exit code {result.ExitCode}.")
    {
        CommandLine = commandLine;
        Result = result;
    }
}
