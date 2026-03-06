#!/usr/bin/env dotnet
// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

// Creates a release by tagging the current commit and pushing the tag.
// The publish workflow triggers automatically when a version tag is pushed.
//
// Usage: dotnet run scripts/Release.cs

#:project ../src/EasyScripting/EasyScripting.csproj

using System.Text.RegularExpressions;
using EasyScripting;
using Spectre.Console;

var git = CliWrapper.Create("git");
var gh = CliWrapper.Create("gh");

var statusResult = await git.RunAsync("status --porcelain");
if (!string.IsNullOrWhiteSpace(statusResult.StandardOutput))
{
    Prompt.Error("Working tree is not clean. Commit or stash your changes first.");
    return 1;
}

Prompt.Info("Checking GitHub CLI authentication...");
await gh.RunAsync("auth status");

// List existing tags for context
var existingTags = await git.RunAsync("tag --list --sort=-v:refname");
if (!string.IsNullOrWhiteSpace(existingTags.StandardOutput))
{
    AnsiConsole.WriteLine();
    AnsiConsole.MarkupLine("[bold]Existing tags:[/]");
    foreach (var t in existingTags.StandardOutput.Trim().Split('\n'))
        AnsiConsole.MarkupLine($"  [dim]{Markup.Escape(t)}[/]");
    AnsiConsole.WriteLine();
}

var version = Prompt.Ask("Enter the version to release (e.g. [green]0.6.0[/]):");
if (!IsValidSemVer(version))
{
    Prompt.Error($"'{Markup.Escape(version)}' is not a valid SemVer version.");
    return 1;
}

var tag = $"v{version}";

Prompt.Info($"Preparing release [green]{Markup.Escape(tag)}[/]");

var tagResult = await git.RunAsync($"tag --list {tag}");
if (!string.IsNullOrWhiteSpace(tagResult.StandardOutput))
{
    Prompt.Error($"Tag [green]{Markup.Escape(tag)}[/] already exists.");
    return 1;
}

await git.RunWithConfirmationAsync($"tag {tag}");
Prompt.Success($"Created tag [green]{Markup.Escape(tag)}[/]");

await git.RunWithConfirmationAsync($"push origin {tag}");
Prompt.Success($"Pushed tag [green]{Markup.Escape(tag)}[/] — publish workflow will trigger automatically.");

var isPreRelease = version.Contains('-');
var prereleaseFlag = isPreRelease ? " --prerelease" : "";
await gh.RunWithConfirmationAsync($"release create {tag} --generate-notes{prereleaseFlag}");
Prompt.Success($"Created GitHub Release [green]{Markup.Escape(tag)}[/]");

return 0;

static bool IsValidSemVer(string version) =>
    Regex.IsMatch(version, @"^\d+\.\d+\.\d+(-[0-9A-Za-z\-]+(\.[0-9A-Za-z\-]+)*)?(\+[0-9A-Za-z\-]+(\.[0-9A-Za-z\-]+)*)?$");
