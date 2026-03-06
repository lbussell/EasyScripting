#!/usr/bin/env dotnet
// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

// Configures GitHub repository settings:
// 1. Enables release immutability
// 2. Disables wikis
// 3. Disables discussions
// 4. Disables merge commit for pull requests
//
// Usage: dotnet run scripts/SetupRepository.cs

#:project ../src/EasyScripting/EasyScripting.csproj

using System.Text.RegularExpressions;
using EasyScripting;
using Spectre.Console;

var git = CliWrapper.Create("git");
var gh = CliWrapper.Create("gh");

AnsiConsole.WriteLine();
(string? owner, string? repo) = await DetectGitHubRepoAsync();
await EnsureGhAuthenticatedAsync();
await RunEditRepoCommandAsync(gh, owner, repo);
await EnableReleaseImmutabilityAsync(gh, owner, repo);
Prompt.Success("Repository settings configured.");

async Task<(string Owner, string Repo)> DetectGitHubRepoAsync()
{
    string url;
    try
    {
        var result = await git.RunAsync("remote get-url origin");
        url = result.StandardOutput.Trim();
    }
    catch (CliWrap.Exceptions.CommandExecutionException)
    {
        Prompt.Error("Could not detect git remote. Are you in a git repository?");
        Environment.Exit(1);
        return default;
    }
    (string Owner, string Repo)? repo = ParseGitHubRepo(url);

    if (repo is null)
    {
        Prompt.Error($"Origin URL is not a GitHub repository: [dim]{url}[/]");
        Environment.Exit(1);
    }

    AnsiConsole.MarkupLine($"[bold]Detected repository:[/] [link]https://github.com/{repo.Value.Owner}/{repo.Value.Repo}[/]");

    if (!Prompt.Confirm("Is this correct?"))
    {
        AnsiConsole.MarkupLine("[yellow]Aborted.[/]");
        Environment.Exit(1);
    }

    return (repo.Value.Owner, repo.Value.Repo);
}

async Task EnsureGhAuthenticatedAsync()
{
    AnsiConsole.MarkupLine("Checking GitHub CLI authentication...");
    try
    {
        await gh.RunAsync("auth status");
    }
    catch (CliWrap.Exceptions.CommandExecutionException)
    {
        Prompt.Error("The GitHub CLI is not authenticated. Run [blue]gh auth login[/] first.");
        Environment.Exit(1);
    }
}

static async Task EnableReleaseImmutabilityAsync(CliWrapper gh, string owner, string repo)
{
    AnsiConsole.WriteLine();
    AnsiConsole.MarkupLine("[bold]Enabling [green]release immutability[/][/]");
    await gh.RunWithConfirmationAsync(
        $"api --method PATCH repos/{owner}/{repo} -f security_and_analysis[release_immutability][status]=enabled"
    );
    Prompt.Success("Release immutability enabled.");
}

static async Task RunEditRepoCommandAsync(CliWrapper gh, string owner, string repo)
{
    AnsiConsole.WriteLine();
    AnsiConsole.MarkupLine("[bold]Disabling [green]wikis[/], [green]discussions[/], and [green]merge commit[/][/]");
    await gh.RunWithConfirmationAsync(
        $"repo edit {owner}/{repo} --enable-wiki=false --enable-discussions=false --enable-merge-commit=false"
    );
    Prompt.Success("Wikis, discussions, and merge commit disabled.");
}

partial class Program
{
    // Match HTTPS: https://github.com/{owner}/{repo}.git
    // Match SSH:   git@github.com:{owner}/{repo}.git
    [GeneratedRegex(@"github\.com[:/](?<owner>[^/]+)/(?<repo>[^/.]+)")]
    private static partial Regex GitHubUrlRegex { get; }

    private static (string Owner, string Repo)? ParseGitHubRepo(string url)
    {
        Match match = GitHubUrlRegex.Match(url);
        return !match.Success ? null : (match.Groups["owner"].Value, match.Groups["repo"].Value);
    }
}
