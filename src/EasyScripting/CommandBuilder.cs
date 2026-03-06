// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace EasyScripting;

/// <summary>
/// An immutable description of a CLI command to execute, with optional behavioral flags.
/// Configure via extension methods in <see cref="CommandBuilderExtensions"/> and execute
/// with <see cref="CommandBuilderExtensions.RunAsync"/>.
/// </summary>
public readonly record struct CommandBuilder
{
    /// <summary>The full command line to execute (e.g. "git remote get-url origin").</summary>
    public required string CommandLine { get; init; }

    /// <summary>Optional text to pipe to standard input.</summary>
    public string? StandardInput { get; init; }

    /// <summary>When <see langword="true"/>, trims leading/trailing whitespace from the captured standard output.</summary>
    public bool TrimOutput { get; init; }

    /// <summary>When <see langword="true"/>, suppresses the <c>[exec]</c> log line and streamed output.</summary>
    public bool SuppressOutput { get; init; }

    /// <summary>When <see langword="true"/>, prompts the user for confirmation before executing.</summary>
    public bool RequireConfirmation { get; init; }
}
