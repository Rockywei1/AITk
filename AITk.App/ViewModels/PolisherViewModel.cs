using System.IO;
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
/// Code Polisher ViewModel - Generates code simplification Prompt (<300 lines).
/// </summary>
public partial class PolisherViewModel : ObservableObject
{
      private readonly GitContextService _git = new();
      private readonly SkillLoader _skillLoader = new();

      // Supported language mapping
      private static readonly Dictionary<string, string> LanguageMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { ".cs", "csharp" },
        { ".py", "python" },
        { ".js", "javascript" },
        { ".ts", "typescript" },
        { ".jsx", "jsx" },
        { ".tsx", "tsx" },
        { ".java", "java" },
        { ".go", "go" },
        { ".rs", "rust" },
        { ".cpp", "cpp" },
        { ".c", "c" },
        { ".h", "c" },
        { ".hpp", "cpp" },
        { ".rb", "ruby" },
        { ".php", "php" },
        { ".swift", "swift" },
        { ".kt", "kotlin" },
        { ".scala", "scala" },
        { ".lua", "lua" },
        { ".sql", "sql" },
        { ".sh", "bash" },
        { ".ps1", "powershell" },
        { ".yaml", "yaml" },
        { ".yml", "yaml" },
        { ".json", "json" },
        { ".xml", "xml" },
        { ".html", "html" },
        { ".css", "css" },
    };
      // ═══════════════════════════════════════════════════════════
      // Observable Properties
      // ═══════════════════════════════════════════════════════════
      [ObservableProperty] private string _targetPath = "";
      [ObservableProperty] private string _status = "Select a file or folder to polish";
      [ObservableProperty] private string _promptPreview = "Generated prompt will appear here...";
      [ObservableProperty] private Brush _statusColor = Brushes.Gray;

      // Options
      [ObservableProperty] private bool _removeDeadCode = true;
      [ObservableProperty] private bool _simplifyConditionals = true;
      [ObservableProperty] private bool _extractMethods = true;
      [ObservableProperty] private bool _improveNaming = false;
      [ObservableProperty] private bool _addComments = false;
      [ObservableProperty] private int _maxLinesPerFile = 300;

      partial void OnMaxLinesPerFileChanged(int value)
      {
            if (value < 1) MaxLinesPerFile = 1;
            else if (value > 10000) MaxLinesPerFile = 10000;
      }

      // Busy flag to prevent race conditions
      [ObservableProperty]
      [NotifyCanExecuteChangedFor(nameof(GenerateCommand))]
      private bool _isBusy = false;

      private bool CanExecuteCommand() => !IsBusy;

      // ═══════════════════════════════════════════════════════════
      // Commands
      // ═══════════════════════════════════════════════════════════
      [RelayCommand]
      private void Browse()
      {
            var dialog = new OpenFileDialog
            {
                  Title = "Select file to polish",
                  Filter = "All Code Files|*.cs;*.py;*.js;*.ts;*.jsx;*.tsx;*.java;*.go;*.rs;*.cpp;*.c;*.rb;*.php;*.swift;*.kt;*.scala|C#|*.cs|Python|*.py|JavaScript/TypeScript|*.js;*.ts;*.jsx;*.tsx|Java|*.java|Go|*.go|Rust|*.rs|C/C++|*.c;*.cpp;*.h;*.hpp|All Files|*.*",
                  CheckFileExists = true
            };

            if (dialog.ShowDialog() == true)
            {
                  TargetPath = dialog.FileName;
                  Status = $"Selected: {Path.GetFileName(TargetPath)}";
                  StatusColor = Brushes.LimeGreen;
            }
      }

      [RelayCommand(CanExecute = nameof(CanExecuteCommand))]
      private async Task GenerateAsync()
      {
            if (string.IsNullOrWhiteSpace(TargetPath))
            {
                  Status = "Please select a file first";
                  StatusColor = Brushes.Yellow;
                  return;
            }

            if (!File.Exists(TargetPath))
            {
                  Status = "File not found";
                  StatusColor = Brushes.Red;
                  return;
            }

            IsBusy = true;
            Status = "Generating prompt...";
            StatusColor = Brushes.Orange;

            try
            {
                  var fileContent = await File.ReadAllTextAsync(TargetPath);

                  // Get project guidelines (CLAUDE.md / SKILL.md)
                  var repoRoot = await _git.GetRepositoryRootAsync(TargetPath);
                  var guidelines = await _git.FindProjectGuidelinesAsync(repoRoot);

                  // Load built-in code-simplifier Skill
                  var skill = await _skillLoader.LoadSkillForModuleAsync("Polisher");

                  var prompt = BuildPrompt(fileContent, guidelines, skill);
                  PromptPreview = prompt;

                  Status = "✅ Prompt generated! Click Copy to use with AI.";
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

      [RelayCommand]
      private void Copy()
      {
            if (string.IsNullOrEmpty(PromptPreview) || PromptPreview.StartsWith("Generated"))
            {
                  Status = "Generate a prompt first!";
                  StatusColor = Brushes.Yellow;
                  return;
            }

            Clipboard.SetText(PromptPreview);
            Status = "✅ Prompt copied to clipboard!";
            StatusColor = Brushes.LimeGreen;
      }

      // ═══════════════════════════════════════════════════════════
      // Prompt Builder
      // ═══════════════════════════════════════════════════════════
      private string BuildPrompt(string code, string? guidelines, string? skill)
      {
            var goals = new List<string>();
            if (RemoveDeadCode) goals.Add("Remove dead/unreachable code");
            if (SimplifyConditionals) goals.Add("Simplify nested conditionals");
            if (ExtractMethods) goals.Add("Extract long methods into smaller ones");
            if (ImproveNaming) goals.Add("Improve variable/method naming");
            if (AddComments) goals.Add("Add clarifying comments");

            var sb = new StringBuilder();

            // If Skill is defined, prioritize it as system instructions
            if (!string.IsNullOrEmpty(skill))
            {
                  sb.AppendLine("## Expert Agent Instructions");
                  sb.AppendLine(skill.Trim());
                  sb.AppendLine();
            }

            sb.AppendLine("# Code Simplification Request");
            sb.AppendLine();
            sb.AppendLine("## Goals");
            foreach (var goal in goals)
            {
                  sb.AppendLine($"- {goal}");
            }
            sb.AppendLine();
            sb.AppendLine("## Constraints");
            sb.AppendLine($"- Target max {MaxLinesPerFile} lines per file");
            sb.AppendLine("- Preserve existing functionality");
            sb.AppendLine("- Follow project coding standards");

            if (!string.IsNullOrEmpty(guidelines))
            {
                  sb.AppendLine();
                  sb.AppendLine("## Project Context (CLAUDE.md/SKILL.md)");
                  sb.AppendLine("```");
                  sb.AppendLine(guidelines.Length > 2000 ? guidelines.Substring(0, 2000) + "..." : guidelines.Trim());
                  sb.AppendLine("```");
            }

            sb.AppendLine();
            var lang = DetectLanguage(TargetPath);
            sb.AppendLine($"## File: {Path.GetFileName(TargetPath)} ({lang})");
            sb.AppendLine($"```{lang}");
            sb.AppendLine(code.Length > 5000 ? code.Substring(0, 5000) + "\n// ... (truncated)" : code);
            sb.AppendLine("```");
            sb.AppendLine();
            sb.AppendLine("Please simplify this code according to the goals and instructions above.");

            return sb.ToString();
      }

      /// <summary>
      /// Detects programming language based on file extension.
      /// </summary>
      private static string DetectLanguage(string filePath)
      {
            var ext = Path.GetExtension(filePath);
            return LanguageMap.TryGetValue(ext, out var lang) ? lang : "text";
      }
}
