using System.Diagnostics;
using AITk.Core.Models;

namespace AITk.Core.Services;

/// <summary>
/// Git Context Service - Retrieves repository information for Code Review.
/// </summary>
public class GitContextService
{
      private readonly CommandRunner _runner = new();

      /// <summary>
      /// Gets the list of changed files in the current repository.
      /// </summary>
      public async Task<List<ChangedFile>> GetChangedFilesAsync(string workingDir)
      {
            var result = await RunGitAsync("status --porcelain", workingDir);
            if (!result.Success) return new List<ChangedFile>();

            var files = new List<ChangedFile>();
            foreach (var line in result.StdOut.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                  if (line.Length < 3) continue;
                  var status = line.Substring(0, 2).Trim();
                  var path = line.Substring(3).Trim();

                  // Handle quotes added by Git for filenames with spaces
                  if (path.StartsWith('"') && path.EndsWith('"'))
                  {
                        path = path[1..^1];
                  }

                  files.Add(new ChangedFile
                  {
                        Path = path,
                        Status = ParseStatus(status),
                        IsSelected = true
                  });
            }
            return files;
      }

      /// <summary>
      /// Gets the diff for a specific file.
      /// </summary>
      public async Task<string> GetFileDiffAsync(string filePath, string workingDir)
      {
            var result = await RunGitAsync($"diff HEAD -- \"{filePath}\"", workingDir);
            if (string.IsNullOrEmpty(result.StdOut))
            {
                  // Might be a new file, try --cached
                  result = await RunGitAsync($"diff --cached -- \"{filePath}\"", workingDir);
            }
            return result.StdOut;
      }

      /// <summary>
      /// Gets the full repository diff.
      /// </summary>
      public async Task<string> GetFullDiffAsync(string workingDir)
      {
            var result = await RunGitAsync("diff HEAD", workingDir);
            return result.StdOut;
      }

      /// <summary>
      /// Gets a preview of the file diff (limited lines).
      /// </summary>
      public async Task<string> GetDiffPreviewAsync(string workingDir, string filePath, int maxLines = 50)
      {
            var diff = await GetFileDiffAsync(filePath, workingDir);
            if (string.IsNullOrEmpty(diff)) return "";

            var lines = diff.Split('\n');
            if (lines.Length <= maxLines)
                  return diff;

            return string.Join('\n', lines.Take(maxLines)) + $"\n\n... ({lines.Length - maxLines} more lines)";
      }

      /// <summary>
      /// Gets the current branch name.
      /// </summary>
      public async Task<string> GetCurrentBranchAsync(string workingDir)
      {
            var result = await RunGitAsync("branch --show-current", workingDir);
            return result.StdOut.Trim();
      }

      /// <summary>
      /// Gets the Git repository root directory.
      /// </summary>
      public async Task<string> GetRepositoryRootAsync(string path)
      {
            var dir = File.Exists(path) ? Path.GetDirectoryName(path) : path;
            var result = await RunGitAsync("rev-parse --show-toplevel", dir ?? "");
            return result.Success ? result.StdOut.Trim() : dir ?? "";
      }

      /// <summary>
      /// Finds CLAUDE.md or README.md as project guidelines.
      /// </summary>
      public async Task<string?> FindProjectGuidelinesAsync(string workingDir)
      {
            var candidates = new[] { "CLAUDE.md", "claude.md", "SKILL.md", "skill.md", "README.md", "readme.md" };
            foreach (var file in candidates)
            {
                  var fullPath = Path.Combine(workingDir, file);
                  if (File.Exists(fullPath))
                  {
                        return await File.ReadAllTextAsync(fullPath);
                  }
            }
            return null;
      }

      private async Task<ExecutionResult> RunGitAsync(string args, string workingDir)
      {
            return await _runner.ExecuteAsync(new ExecutionConfig
            {
                  Command = $"git {args}",
                  WorkingDirectory = workingDir,
                  TimeoutMs = 10000
            });
      }

      private static string ParseStatus(string status) => status switch
      {
            "M" or " M" => "Modified",
            "A" or " A" => "Added",
            "D" or " D" => "Deleted",
            "R" => "Renamed",
            "??" => "Untracked",
            _ => status
      };
}

/// <summary>
/// Changed file model.
/// </summary>
public class ChangedFile
{
      public string Path { get; set; } = "";
      public string Status { get; set; } = "";
      public bool IsSelected { get; set; }
}
