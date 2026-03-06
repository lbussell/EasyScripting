#!/usr/bin/env dotnet
// SPDX-FileCopyrightText: Copyright (c) 2026 Logan Bussell
// SPDX-License-Identifier: MIT

// Automates the GitHub setup steps from the README:
// 1. Creates the 'production' GitHub environment
// 2. Sets the NUGET_USER environment secret
//
// Usage: dotnet run scripts/SetupPublishing.cs

#:project ../src/EasyScripting/EasyScripting.csproj

using System.Text.RegularExpressions;
using EasyScripting;
using Spectre.Console;

var git = CliWrapper.Create("git");
var gh = CliWrapper.Create("gh");

AnsiConsole.WriteLine();
(var owner, var repo) = await DetectGitHubRepoAsync();
await EnsureGhAuthenticatedAsync();
await CreateEnvironmentAsync(gh, owner, repo);
await SetNugetUserSecretAsync(gh, owner, repo);
await SetupTrustedPublishingAsync(owner, repo);

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
    var repo = ParseGitHubRepo(url);

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

static async Task CreateEnvironmentAsync(CliWrapper gh, string owner, string repo)
{
    AnsiConsole.WriteLine();
    AnsiConsole.MarkupLine("[bold]Creating [green]production[/] GitHub environment[/]");
    await gh.RunWithConfirmationAsync($"api --method PUT repos/{owner}/{repo}/environments/production");
    Prompt.Success("Environment [green]production[/] created.");
}

static async Task SetNugetUserSecretAsync(CliWrapper gh, string owner, string repo)
{
    AnsiConsole.WriteLine();
    AnsiConsole.MarkupLine("[bold]Set the [green]NUGET_USER[/] environment secret[/]");

    var nugetUser = Prompt.Ask("Enter your [green]NuGet.org username[/]:");

    await gh.RunWithConfirmationAsync(
        $"secret set NUGET_USER --env production --repo {owner}/{repo}",
        standardInput: nugetUser
    );

    Prompt.Success("Secret [green]NUGET_USER[/] set.");
}

static async Task SetupTrustedPublishingAsync(string owner, string repo)
{
    AnsiConsole.WriteLine();
    AnsiConsole.MarkupLine("[bold green]Next step:[/] set up Trusted Publishing on NuGet.org.");
    AnsiConsole.MarkupLine("Go to [link]https://www.nuget.org/account/trustedpublishing[/] and add a new Trusted Publisher with:");
    AnsiConsole.WriteLine();
    AnsiConsole.MarkupLine($"[bold]Policy Name:[/]      {Markup.Escape(repo)}");
    AnsiConsole.MarkupLine($"[bold]Repository Owner:[/] {Markup.Escape(owner)}");
    AnsiConsole.MarkupLine($"[bold]Repository:[/]       {Markup.Escape(repo)}");
    AnsiConsole.MarkupLine($"[bold]Workflow File:[/]     publish-nuget.yml");
    AnsiConsole.MarkupLine($"[bold]Environment:[/]       production");
}

partial class Program
{
    // Match HTTPS: https://github.com/{owner}/{repo}.git
    // Match SSH:   git@github.com:{owner}/{repo}.git
    [GeneratedRegex(@"github\.com[:/](?<owner>[^/]+)/(?<repo>[^/.]+)")]
    private static partial Regex GitHubUrlRegex { get; }

    private static (string Owner, string Repo)? ParseGitHubRepo(string url)
    {
        var match = GitHubUrlRegex.Match(url);
        return !match.Success ? null : (match.Groups["owner"].Value, match.Groups["repo"].Value);
    }
}
