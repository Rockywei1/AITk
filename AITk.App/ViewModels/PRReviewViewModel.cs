using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;
using AITk.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;

namespace AITk.App.ViewModels;

/// <summary>
/// PR Review Toolkit - Comprehensive Code Review Tool.
/// Uses multiple specialized AI Agents to review code from different perspectives.
/// </summary>
public partial class PRReviewViewModel : ObservableObject
{
      private readonly GitContextService _git = new();
      private readonly SkillLoader _skillLoader = new();

      // ═══════════════════════════════════════════════════════════
      // Observable Properties
      // ═══════════════════════════════════════════════════════════

      [ObservableProperty] private string _workingDirectory = "";
      [ObservableProperty] private string _branch = "";
      [ObservableProperty] private string _promptPreview = "";
      [ObservableProperty] private string _status = "Ready";
      [ObservableProperty] private Brush _statusColor = Brushes.Gray;

      // Review Aspect Toggles
      [ObservableProperty] private bool _reviewCode = true;
      [ObservableProperty] private bool _reviewTests = true;
      [ObservableProperty] private bool _reviewErrors = true;
      [ObservableProperty] private bool _reviewComments = false;
      [ObservableProperty] private bool _reviewTypes = false;
      [ObservableProperty] private bool _reviewSimplify = false;

      // Scan Mode: true = Git changes only, false = All files
      [ObservableProperty] private bool _gitModeEnabled = true;

      // Changed Files Info
      [ObservableProperty] private string _changedFilesInfo = "";
      [ObservableProperty] private int _changedFilesCount = 0;

      // Scanned file paths for Generate
      private List<string> _scannedFiles = new();

      // Busy flag to prevent race conditions
      [ObservableProperty]
      [NotifyCanExecuteChangedFor(nameof(ScanCommand))]
      [NotifyCanExecuteChangedFor(nameof(GenerateCommand))]
      private bool _isBusy = false;

      // ═══════════════════════════════════════════════════════════
      // Commands
      // ═══════════════════════════════════════════════════════════

      [RelayCommand]
      private void Browse()
      {
            var dialog = new OpenFolderDialog
            {
                  Title = "Select Project Directory"
            };

            if (dialog.ShowDialog() == true)
            {
                  WorkingDirectory = dialog.FolderName;
                  Status = "📁 Directory selected. Choose scan mode and click Scan.";
                  StatusColor = Brushes.LimeGreen;
            }
      }

      private bool CanExecuteCommand() => !IsBusy;

      [RelayCommand(CanExecute = nameof(CanExecuteCommand))]
      private async Task Scan()
      {
            if (string.IsNullOrWhiteSpace(WorkingDirectory))
            {
                  Status = "Please select a project directory first";
                  StatusColor = Brushes.Yellow;
                  return;
            }

            if (!Directory.Exists(WorkingDirectory))
            {
                  Status = "Directory not found";
                  StatusColor = Brushes.Red;
                  return;
            }

            IsBusy = true;
            try
            {
                  if (GitModeEnabled)
                  {
                        await ScanGitChanges();
                  }
                  else
                  {
                        await ScanAllFiles();
                  }
            }
            catch (Exception ex)
            {
                  Status = $"Error: {ex.Message}";
                  StatusColor = Brushes.Red;
            }
            finally
            {
                  IsBusy = false;
            }
      }

      private async Task ScanGitChanges()
      {
            Status = "Scanning git changes...";
            StatusColor = Brushes.Orange;

            // Get branch name
            Branch = await _git.GetCurrentBranchAsync(WorkingDirectory);

            // Get changed files
            var changedFiles = await _git.GetChangedFilesAsync(WorkingDirectory);
            ChangedFilesCount = changedFiles.Count;
            _scannedFiles = changedFiles.Select(f => f.Path).ToList();

            if (changedFiles.Count == 0)
            {
                  Status = "No uncommitted changes found. Try 'Scan All Files' mode for full project review.";
                  StatusColor = Brushes.Yellow;
                  ChangedFilesInfo = "No changes detected";
                  return;
            }

            // Build file info
            var sb = new StringBuilder();
            foreach (var file in changedFiles.Take(20))
            {
                  sb.AppendLine($"  - {file.Path}");
            }
            if (changedFiles.Count > 20)
            {
                  sb.AppendLine($"  ... and {changedFiles.Count - 20} more files");
            }
            ChangedFilesInfo = sb.ToString();

            Status = $"✅ Found {changedFiles.Count} changed files on branch '{Branch}'";
            StatusColor = Brushes.LimeGreen;
      }

      private async Task ScanAllFiles()
      {
            Status = "Scanning all code files...";
            StatusColor = Brushes.Orange;

            Branch = "N/A (Full Scan)";

            // Scan for common code file extensions
            var extensions = new[] { ".cs", ".py", ".js", ".ts", ".java", ".cpp", ".c", ".h", ".go", ".rs", ".rb", ".php", ".swift", ".kt" };
            var excludeDirs = new[] { "bin", "obj", "node_modules", ".git", "packages", "dist", "build", "__pycache__", ".vs", ".idea" };

            var allFiles = await Task.Run(() =>
            {
                  var files = new List<string>();
                  ScanDirectory(WorkingDirectory, extensions, excludeDirs, files, maxFiles: 100);
                  return files;
            });

            ChangedFilesCount = allFiles.Count;
            _scannedFiles = allFiles;

            if (allFiles.Count == 0)
            {
                  Status = "No code files found in directory";
                  StatusColor = Brushes.Yellow;
                  ChangedFilesInfo = "No code files detected";
                  return;
            }

            // Build file info
            var sb = new StringBuilder();
            foreach (var file in allFiles.Take(20))
            {
                  var relativePath = Path.GetRelativePath(WorkingDirectory, file);
                  sb.AppendLine($"  - {relativePath}");
            }
            if (allFiles.Count > 20)
            {
                  sb.AppendLine($"  ... and {allFiles.Count - 20} more files");
            }
            ChangedFilesInfo = sb.ToString();

            Status = $"✅ Found {allFiles.Count} code files (Full Scan Mode)";
            StatusColor = Brushes.LimeGreen;
      }

      private void ScanDirectory(string dir, string[] extensions, string[] excludeDirs, List<string> files, int maxFiles)
      {
            if (files.Count >= maxFiles) return;

            try
            {
                  var dirName = Path.GetFileName(dir);
                  if (excludeDirs.Contains(dirName, StringComparer.OrdinalIgnoreCase)) return;

                  foreach (var file in Directory.GetFiles(dir))
                  {
                        if (files.Count >= maxFiles) return;
                        var ext = Path.GetExtension(file).ToLowerInvariant();
                        if (extensions.Contains(ext))
                        {
                              files.Add(file);
                        }
                  }

                  foreach (var subDir in Directory.GetDirectories(dir))
                  {
                        if (files.Count >= maxFiles) return;
                        ScanDirectory(subDir, extensions, excludeDirs, files, maxFiles);
                  }
            }
            catch { /* Ignore access denied errors */ }
      }

      [RelayCommand(CanExecute = nameof(CanExecuteCommand))]
      private async Task Generate()
      {
            if (string.IsNullOrWhiteSpace(WorkingDirectory))
            {
                  Status = "Please select a project directory first";
                  StatusColor = Brushes.Yellow;
                  return;
            }

            if (ChangedFilesCount == 0)
            {
                  Status = "Please scan for changes first";
                  StatusColor = Brushes.Yellow;
                  return;
            }

            // Check if at least one review aspect is selected
            if (!ReviewCode && !ReviewTests && !ReviewErrors && !ReviewComments && !ReviewTypes && !ReviewSimplify)
            {
                  Status = "⚠️ Please select at least one review aspect";
                  StatusColor = Brushes.Yellow;
                  return;
            }

            IsBusy = true;
            Status = "Generating comprehensive review prompt...";
            StatusColor = Brushes.Orange;

            try
            {
                  var sb = new StringBuilder();

                  // Header
                  sb.AppendLine("# Comprehensive PR Review Request");
                  sb.AppendLine();
                  sb.AppendLine($"**Branch**: {Branch}");
                  sb.AppendLine($"**Changed Files**: {ChangedFilesCount}");
                  sb.AppendLine();

                  // Project guidelines
                  var repoRoot = await _git.GetRepositoryRootAsync(WorkingDirectory);
                  var guidelines = await _git.FindProjectGuidelinesAsync(repoRoot);
                  if (!string.IsNullOrEmpty(guidelines))
                  {
                        sb.AppendLine("## Project Guidelines");
                        sb.AppendLine("```");
                        sb.AppendLine(guidelines.Length > 2000 ? guidelines.Substring(0, 2000) + "..." : guidelines);
                        sb.AppendLine("```");
                        sb.AppendLine();
                  }

                  // Selected Review Aspects
                  sb.AppendLine("## Review Aspects");
                  sb.AppendLine("Please analyze the following aspects:");
                  sb.AppendLine();

                  // Load and append Skills for each selected aspect
                  if (ReviewCode)
                  {
                        sb.AppendLine("### 📖 Code Review");
                        var skill = await _skillLoader.LoadSkillAsync("code-reviewer");
                        if (!string.IsNullOrEmpty(skill))
                        {
                              sb.AppendLine(TruncateSkill(skill, 1500));
                        }
                        else
                        {
                              sb.AppendLine("- Check for bugs, logic errors, and guideline compliance");
                              sb.AppendLine("- Identify potential issues and improvements");
                        }
                        sb.AppendLine();
                  }

                  if (ReviewTests)
                  {
                        sb.AppendLine("### 🧪 Test Coverage Analysis");
                        var skill = await _skillLoader.LoadSkillAsync("pr-test-analyzer");
                        if (!string.IsNullOrEmpty(skill))
                        {
                              sb.AppendLine(TruncateSkill(skill, 1500));
                        }
                        else
                        {
                              sb.AppendLine("- Review test coverage quality and completeness");
                              sb.AppendLine("- Identify critical gaps and missing edge cases");
                        }
                        sb.AppendLine();
                  }

                  if (ReviewErrors)
                  {
                        sb.AppendLine("### ⚠️ Error Handling Review");
                        var skill = await _skillLoader.LoadSkillAsync("silent-failure-hunter");
                        if (!string.IsNullOrEmpty(skill))
                        {
                              sb.AppendLine(TruncateSkill(skill, 1500));
                        }
                        else
                        {
                              sb.AppendLine("- Find silent failures and inadequate error handling");
                              sb.AppendLine("- Check catch blocks and error logging");
                        }
                        sb.AppendLine();
                  }

                  if (ReviewComments)
                  {
                        sb.AppendLine("### 💬 Comment Analysis");
                        var skill = await _skillLoader.LoadSkillAsync("comment-analyzer");
                        if (!string.IsNullOrEmpty(skill))
                        {
                              sb.AppendLine(TruncateSkill(skill, 1500));
                        }
                        else
                        {
                              sb.AppendLine("- Verify comment accuracy vs code");
                              sb.AppendLine("- Identify comment rot and outdated documentation");
                        }
                        sb.AppendLine();
                  }

                  if (ReviewTypes)
                  {
                        sb.AppendLine("### 📐 Type Design Analysis");
                        var skill = await _skillLoader.LoadSkillAsync("type-design-analyzer");
                        if (!string.IsNullOrEmpty(skill))
                        {
                              sb.AppendLine(TruncateSkill(skill, 1500));
                        }
                        else
                        {
                              sb.AppendLine("- Analyze type encapsulation and invariants");
                              sb.AppendLine("- Rate type design quality");
                        }
                        sb.AppendLine();
                  }

                  if (ReviewSimplify)
                  {
                        sb.AppendLine("### ✨ Code Simplification");
                        var skill = await _skillLoader.LoadSkillAsync("code-simplifier");
                        if (!string.IsNullOrEmpty(skill))
                        {
                              sb.AppendLine(TruncateSkill(skill, 1500));
                        }
                        else
                        {
                              sb.AppendLine("- Simplify complex code for clarity");
                              sb.AppendLine("- Apply project standards while preserving functionality");
                        }
                        sb.AppendLine();
                  }

                  // Changed files list
                  sb.AppendLine("## Files to Review");
                  sb.AppendLine("```");
                  sb.AppendLine(ChangedFilesInfo);
                  sb.AppendLine("```");
                  sb.AppendLine();

                  // Include file content based on scan mode
                  if (GitModeEnabled)
                  {
                        // Git mode: show diffs
                        var changedFiles = await _git.GetChangedFilesAsync(WorkingDirectory);
                        var filesToDiff = changedFiles.Take(5).ToList();

                        if (filesToDiff.Count > 0)
                        {
                              sb.AppendLine("## Code Changes (Git Diff)");
                              foreach (var file in filesToDiff)
                              {
                                    var diff = await _git.GetDiffPreviewAsync(WorkingDirectory, file.Path, 100);
                                    if (!string.IsNullOrEmpty(diff))
                                    {
                                          sb.AppendLine($"### {file.Path}");
                                          sb.AppendLine("```diff");
                                          sb.AppendLine(diff);
                                          sb.AppendLine("```");
                                          sb.AppendLine();
                                    }
                              }

                              if (changedFiles.Count > 5)
                              {
                                    sb.AppendLine($"*(Showing diffs for first 5 files. {changedFiles.Count - 5} more files changed)*");
                                    sb.AppendLine();
                              }
                        }
                  }
                  else
                  {
                        // Full Scan mode: read file content
                        var filesToInclude = _scannedFiles.Take(5).ToList();

                        if (filesToInclude.Count > 0)
                        {
                              sb.AppendLine("## Code Files (Content Preview)");
                              foreach (var filePath in filesToInclude)
                              {
                                    try
                                    {
                                          var content = await File.ReadAllTextAsync(filePath);
                                          var preview = content.Length > 1500
                                                ? content.Substring(0, 1500) + "\n\n... (truncated)"
                                                : content;

                                          var relativePath = Path.GetRelativePath(WorkingDirectory, filePath);
                                          var ext = Path.GetExtension(filePath).TrimStart('.');

                                          sb.AppendLine($"### {relativePath}");
                                          sb.AppendLine($"```{ext}");
                                          sb.AppendLine(preview);
                                          sb.AppendLine("```");
                                          sb.AppendLine();
                                    }
                                    catch { /* Ignore file read errors */ }
                              }

                              if (_scannedFiles.Count > 5)
                              {
                                    sb.AppendLine($"*(Showing first 5 files. {_scannedFiles.Count - 5} more files scanned)*");
                                    sb.AppendLine();
                              }
                        }
                  }

                  // Output format instructions
                  sb.AppendLine("## Expected Output Format");
                  sb.AppendLine(@"
Please provide your analysis in this structure:

### Critical Issues (must fix)
- [Issue]: Description [file:line]

### Important Issues (should fix)
- [Issue]: Description [file:line]

### Suggestions (nice to have)
- [Suggestion]: Description [file:line]

### Positive Observations
- What's well-done in this code
");

                  PromptPreview = sb.ToString();

                  // Copy to clipboard
                  Clipboard.SetText(PromptPreview);

                  Status = $"✅ Review prompt generated and copied! ({PromptPreview.Length:N0} chars)";
                  StatusColor = Brushes.LimeGreen;
            }
            catch (Exception ex)
            {
                  Status = $"Error: {ex.Message}";
                  StatusColor = Brushes.Red;
            }
            finally
            {
                  IsBusy = false;
            }
      }

      // ═══════════════════════════════════════════════════════════
      // Helpers
      // ═══════════════════════════════════════════════════════════

      /// <summary>
      /// Truncates Skill content, keeping the first N characters.
      /// </summary>
      private static string TruncateSkill(string skill, int maxLength)
      {
            if (skill.Length <= maxLength)
                  return skill;

            // Find a good break point (end of line)
            var truncated = skill.Substring(0, maxLength);
            var lastNewline = truncated.LastIndexOf('\n');
            if (lastNewline > maxLength / 2)
            {
                  truncated = truncated.Substring(0, lastNewline);
            }

            return truncated + "\n\n*(Instructions truncated for brevity)*";
      }
}
