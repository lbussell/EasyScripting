// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

namespace EasyScripting;

/// <summary>
/// Captures the result of a completed command, including standard output,
/// standard error, and exit code.
/// </summary>
/// <param name="StandardOutput">The captured standard output text.</param>
/// <param name="StandardError">The captured standard error text.</param>
/// <param name="ExitCode">The process exit code.</param>
public readonly record struct CommandResult(
    string StandardOutput,
    string StandardError,
    int ExitCode
);
