using System;
using System.Collections.Generic;
using System.IO;

namespace GuidedCoding.Release.Tests;

// A throwaway git repository with the release-relevant files and a local bare repository as origin.
internal sealed class ReleaseRepository : IDisposable
{
    public const string InitialChangelog = "# Changelog\n\n## [Unreleased]\n\n- Add a skill.\n";

    public static readonly IReadOnlyList<string> ManifestPaths =
    [
        "plugin.json",
        "claude-plugin/.claude-plugin/plugin.json",
        ".claude-plugin/marketplace.json"
    ];

    private readonly string _root;

    static ReleaseRepository()
    {
        // Keep the developer's git configuration (signing, hooks, identity) out of the tests.
        var configPath = Path.Combine(Path.GetTempPath(), "guided-coding-release-tests.gitconfig");
        File.WriteAllText(configPath, "[user]\n\tname = Release Tests\n\temail = release-tests@example.com\n");
        Environment.SetEnvironmentVariable("GIT_CONFIG_GLOBAL", configPath);
        Environment.SetEnvironmentVariable("GIT_CONFIG_NOSYSTEM", "1");
    }

    private ReleaseRepository(string root)
    {
        _root = root;
        WorkingDirectory = Path.Combine(root, "work");
        OriginDirectory = Path.Combine(root, "origin.git");
    }

    public string WorkingDirectory { get; }

    public string Head => Git("rev-parse", "HEAD").Trim();

    public bool IsClean => Git("status", "--porcelain").Length == 0;

    private string OriginDirectory { get; }

    // Creates a repository at version 1.0.0 whose initial commit is tagged v1.0.0 unless released is false.
    public static ReleaseRepository Create(bool released = true)
    {
        var repository = new ReleaseRepository(
            Path.Combine(Path.GetTempPath(), "guided-coding-release-tests", Guid.NewGuid().ToString("N"))
        );
        Directory.CreateDirectory(repository.WorkingDirectory);
        RunGit(repository._root, "init", "--bare", "--initial-branch=main", repository.OriginDirectory);
        repository.Git("init", "--initial-branch=main");

        foreach (var path in ManifestPaths)
        {
            repository.WriteFile(path, Manifest(path, "1.0.0"));
        }

        repository.WriteFile("CHANGELOG.md", InitialChangelog);
        repository.Commit("chore: initial commit");
        if (released)
        {
            repository.Tag("v1.0.0");
        }

        repository.Git("remote", "add", "origin", repository.OriginDirectory);
        repository.Git("push", "--set-upstream", "origin", "main", "--tags");
        return repository;
    }

    public static string Manifest(string path, string version) =>
        path switch
        {
            "plugin.json" =>
                $$"""
                {
                  "name": "guided-coding",
                  "version": "{{version}}",
                  "description": "Skills for Guided Coding."
                }

                """,
            "claude-plugin/.claude-plugin/plugin.json" =>
                $$"""
                {
                  "name": "guided-coding",
                  "version": "{{version}}",
                  "skills": "./claude-skills"
                }

                """,
            ".claude-plugin/marketplace.json" =>
                $$"""
                {
                  "name": "guided-coding",
                  "plugins": [
                    {
                      "name": "guided-coding",
                      "source": "./claude-plugin",
                      "version": "{{version}}"
                    }
                  ]
                }

                """,
            _ => throw new ArgumentOutOfRangeException(nameof(path), path, "Unknown manifest.")
        };

    public string Git(params string[] arguments) => RunGit(WorkingDirectory, arguments);

    public string Origin(params string[] arguments) => RunGit(OriginDirectory, arguments);

    public void Commit(string message)
    {
        Git("add", "--all");
        Git("commit", "--allow-empty", "-m", message);
    }

    public void Tag(string tag) => Git("tag", "-a", tag, "-m", tag);

    public void PushCommitFromAnotherClone(string message)
    {
        var clone = Path.Combine(_root, $"clone-{Guid.NewGuid():N}");
        RunGit(_root, "clone", OriginDirectory, clone);
        RunGit(clone, "commit", "--allow-empty", "-m", message);
        RunGit(clone, "push", "origin", "main");
    }

    public void RejectPushes() => InstallFailingHook(OriginDirectory, "pre-receive", "Pushes are rejected.");

    public void RejectCommits() =>
        InstallFailingHook(Path.Combine(WorkingDirectory, ".git"), "pre-commit", "Commits are rejected.");

    public string ReadFile(string relativePath) => File.ReadAllText(Path.Combine(WorkingDirectory, relativePath));

    public void WriteFile(string relativePath, string content)
    {
        var path = Path.Combine(WorkingDirectory, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content);
    }

    public void Dispose()
    {
        if (!Directory.Exists(_root))
        {
            return;
        }

        // Git marks its object files as read-only, which prevents deleting them on Windows.
        foreach (var file in Directory.EnumerateFiles(_root, "*", SearchOption.AllDirectories))
        {
            File.SetAttributes(file, FileAttributes.Normal);
        }

        Directory.Delete(_root, recursive: true);
    }

    private static void InstallFailingHook(string gitDirectory, string name, string message)
    {
        var hook = Path.Combine(gitDirectory, "hooks", name);
        Directory.CreateDirectory(Path.GetDirectoryName(hook)!);
        File.WriteAllText(hook, $"#!/bin/sh\necho '{message}' >&2\nexit 1\n");
        if (!OperatingSystem.IsWindows())
        {
            File.SetUnixFileMode(hook, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute);
        }
    }

    private static string RunGit(string workingDirectory, params string[] arguments) =>
        Processes.Capture(workingDirectory, "git", arguments);
}
