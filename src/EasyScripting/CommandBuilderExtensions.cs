// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using Spectre.Console;

namespace EasyScripting;

/// <summary>
/// Extension methods that add fluent convenience configuration to <see cref="CommandBuilder"/>.
/// </summary>
public static class CommandBuilderExtensions
{
    /// <summary>Trims leading and trailing whitespace from the captured standard output.</summary>
    public static CommandBuilder Trim(this CommandBuilder builder) =>
        builder.AfterExecute(result =>
            result with
            {
                StandardOutput = result.StandardOutput.Trim(),
            }
        );

    /// <summary>Prompts the user for confirmation before executing the command.</summary>
    public static CommandBuilder Confirm(this CommandBuilder builder) =>
        builder.BeforeExecute(ConfirmBeforeRun);

    private static Task ConfirmBeforeRun(
        CommandBuilder builder,
        CancellationToken cancellationToken
    )
    {
        string display = Markup.Escape(builder.CommandLine);
        return !Prompt.Confirm($"Run [blue]{display}[/]?")
            ? throw new OperationCanceledException("User aborted the operation.")
            : Task.CompletedTask;
    }
}
