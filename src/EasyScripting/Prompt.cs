// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

using Spectre.Console;

namespace EasyScripting;

/// <summary>
/// Console prompt helpers using <see cref="AnsiConsole"/> for styled output and user input.
/// </summary>
public static class Prompt
{
    /// <summary>
    /// Displays an informational message.
    /// </summary>
    /// <param name="message">The message (may contain Spectre.Console markup).</param>
    public static void Info(string message) =>
        AnsiConsole.MarkupLine($"[green][[info]][/] {message}");

    /// <summary>
    /// Displays a success message.
    /// </summary>
    /// <param name="message">The message (may contain Spectre.Console markup).</param>
    public static void Success(string message) =>
        AnsiConsole.MarkupLine($"[green][[success]][/] {message}");

    /// <summary>
    /// Displays an error message.
    /// </summary>
    /// <param name="message">The message (may contain Spectre.Console markup).</param>
    public static void Error(string message) =>
        AnsiConsole.MarkupLine($"[red][[error]][/] {message}");

    /// <summary>
    /// Displays a warning message.
    /// </summary>
    /// <param name="message">The message (may contain Spectre.Console markup).</param>
    public static void Warning(string message) =>
        AnsiConsole.MarkupLine($"[yellow][[warning]][/] {message}");

    /// <summary>
    /// Displays a "skipped" indicator.
    /// </summary>
    public static void Skip() => AnsiConsole.MarkupLine("[yellow]Skipped.[/]");

    /// <summary>
    /// Prompts the user for a yes/no confirmation.
    /// </summary>
    /// <param name="message">The confirmation message (may contain Spectre.Console markup).</param>
    /// <returns><see langword="true"/> if the user confirmed; otherwise <see langword="false"/>.</returns>
    public static bool Confirm(string message) =>
        AnsiConsole.Confirm($"[purple][[confirm]][/] {message}");

    /// <summary>
    /// Prompts the user for text input.
    /// </summary>
    /// <param name="message">The prompt message (may contain Spectre.Console markup).</param>
    /// <returns>The text entered by the user.</returns>
    public static string Ask(string message) =>
        AnsiConsole.Prompt(new TextPrompt<string>(message).PromptStyle("blue"));
}
