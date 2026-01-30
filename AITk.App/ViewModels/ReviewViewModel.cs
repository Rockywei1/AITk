using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using AITk.Core.Services;

namespace AITk.App.ViewModels;

/// <summary>
/// Smart Review ViewModel - Generates AI-readable Review Packets (<300 lines).
/// Includes CancellationToken support to handle race conditions.
/// </summary>
public partial class ReviewViewModel : ObservableObject
{
      private readonly GitContextService _git = new();
      private readonly SkillLoader _skillLoader = new();
      private CancellationTokenSource? _diffLoadCts;  // Fix race condition

      // ═══════════════════════════════════════════════════════════
      // Observable Properties
      // ═══════════════════════════════════════════════════════════
      [ObservableProperty] private string _workingDirectory = Environment.CurrentDirectory;
      [ObservableProperty] private string _status = "Ready to scan";
      [ObservableProperty] private string _branch = "";
      [ObservableProperty] private string _diffPreview = "Select a file to preview diff...";
      [ObservableProperty] private Brush _statusColor = Brushes.Gray;
      [ObservableProperty] private ChangedFile? _selectedFile;

      // Busy flag to prevent race conditions
      [ObservableProperty]
      [NotifyCanExecuteChangedFor(nameof(ScanCommand))]
      [NotifyCanExecuteChangedFor(nameof(GenerateCommand))]
      private bool _isBusy = false;

      private bool CanExecuteCommand() => !IsBusy;

      public ObservableCollection<ChangedFile> ChangedFiles { get; } = new();

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
                  Status = "📁 Directory selected. Click Scan to find changes.";
                  StatusColor = Brushes.LimeGreen;
            }
      }

      [RelayCommand(CanExecute = nameof(CanExecuteCommand))]
      private async Task ScanAsync()
      {
            IsBusy = true;
            Status = "Scanning...";
            StatusColor = Brushes.Orange;
            ChangedFiles.Clear();
            DiffPreview = "";

            try
            {
                  Branch = await _git.GetCurrentBranchAsync(WorkingDirectory);
                  var files = await _git.GetChangedFilesAsync(WorkingDirectory);

                  foreach (var file in files)
                  {
                        ChangedFiles.Add(file);
                  }

                  if (ChangedFiles.Count == 0)
                  {
                        Status = "No changes found";
                        StatusColor = Brushes.Yellow;
                  }
                  else
                  {
                        Status = $"Found {ChangedFiles.Count} changed files";
                        StatusColor = Brushes.LimeGreen;
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

      [RelayCommand(CanExecute = nameof(CanExecuteCommand))]
      private async Task GenerateAsync()
      {
            var selectedFiles = ChangedFiles.Where(f => f.IsSelected).ToList();
            if (selectedFiles.Count == 0)
            {
                  Status = "Please select at least one file";
                  StatusColor = Brushes.Yellow;
                  return;
            }

            IsBusy = true;
            Status = "Generating review packet...";
            StatusColor = Brushes.Orange;

            try
            {
                  var sb = new StringBuilder();

                  // Load code-reviewer Skill
                  var skill = await _skillLoader.LoadSkillForModuleAsync("Review");
                  if (!string.IsNullOrEmpty(skill))
                  {
                        sb.AppendLine("## Expert Reviewer Instructions");
                        sb.AppendLine(skill.Trim());
                        sb.AppendLine();
                  }

                  sb.AppendLine("# Code Review Packet");
                  sb.AppendLine($"Branch: {Branch}");
                  sb.AppendLine($"Files: {selectedFiles.Count}");
                  sb.AppendLine();

                  var guidelines = await _git.FindProjectGuidelinesAsync(WorkingDirectory);
                  if (!string.IsNullOrEmpty(guidelines))
                  {
                        sb.AppendLine("## Project Guidelines (CLAUDE.md)");
                        sb.AppendLine("```");
                        sb.AppendLine(guidelines.Length > 2000 ? guidelines.Substring(0, 2000) + "..." : guidelines);
                        sb.AppendLine("```");
                        sb.AppendLine();
                  }

                  sb.AppendLine("## Changes");
                  foreach (var file in selectedFiles)
                  {
                        var diff = await _git.GetFileDiffAsync(file.Path, WorkingDirectory);
                        sb.AppendLine($"### {file.Path} ({file.Status})");
                        sb.AppendLine("```diff");
                        sb.AppendLine(diff.Length > 3000 ? diff.Substring(0, 3000) + "..." : diff);
                        sb.AppendLine("```");
                        sb.AppendLine();
                  }

                  var packet = new
                  {
                        Action = "CodeReview",
                        Branch,
                        Files = selectedFiles.Select(f => f.Path).ToList(),
                        Guidelines = guidelines?.Substring(0, Math.Min(guidelines.Length, 1000)),
                        Diff = sb.ToString()
                  };

                  var json = JsonSerializer.Serialize(packet, new JsonSerializerOptions { WriteIndented = true });
                  Clipboard.SetText(json);

                  DiffPreview = sb.ToString();
                  Status = "✅ Review packet copied to clipboard!";
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
      // File Selection Handler (Handles Race Conditions)
      // ═══════════════════════════════════════════════════════════
      partial void OnSelectedFileChanged(ChangedFile? value)
      {
            if (value == null) return;

            // Cancel previous load request to prevent race condition
            _diffLoadCts?.Cancel();
            _diffLoadCts?.Dispose();
            _diffLoadCts = new CancellationTokenSource();

            _ = LoadFileDiffAsync(value.Path, _diffLoadCts.Token);
      }

      private async Task LoadFileDiffAsync(string path, CancellationToken ct)
      {
            try
            {
                  DiffPreview = "Loading diff...";
                  var diff = await _git.GetFileDiffAsync(path, WorkingDirectory);

                  // Check cancellation
                  if (ct.IsCancellationRequested) return;

                  DiffPreview = string.IsNullOrEmpty(diff) ? "(No diff available)" : diff;
            }
            catch (OperationCanceledException)
            {
                  // Cancelled, ignore
            }
            catch
            {
                  if (!ct.IsCancellationRequested)
                        DiffPreview = "(Error loading diff)";
            }
      }
}
