using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace AITk.Core.Services;

/// <summary>
/// Loads embedded Skill specification files (.md).
/// Automatically strips YAML frontmatter to make skills model-agnostic.
/// </summary>
public partial class SkillLoader
{
      private readonly string _skillsDirectory;

      // Regex to match YAML frontmatter
      [GeneratedRegex(@"^---\s*\n(.*?)\n---\s*\n", RegexOptions.Singleline)]
      private static partial Regex FrontmatterRegex();

      public SkillLoader()
      {
            // Default to 'Skills' folder in application directory
            var exeDir = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) ?? "";
            _skillsDirectory = Path.Combine(exeDir, "Skills");
      }

      /// <summary>
      /// Gets all available Skill names.
      /// </summary>
      public List<string> GetAvailableSkills()
      {
            if (!Directory.Exists(_skillsDirectory))
                  return new List<string>();

            return Directory.GetFiles(_skillsDirectory, "*.md")
                .Select(f => Path.GetFileNameWithoutExtension(f))
                .ToList();
      }

      /// <summary>
      /// Loads specific Skill content (stripping YAML frontmatter).
      /// </summary>
      public async Task<string?> LoadSkillAsync(string skillName)
      {
            var path = Path.Combine(_skillsDirectory, $"{skillName}.md");
            if (!File.Exists(path))
                  return null;

            var content = await File.ReadAllTextAsync(path);
            return StripFrontmatter(content);
      }

      /// <summary>
      /// Loads corresponding Skill based on module type (auto-mapping).
      /// </summary>
      public async Task<string?> LoadSkillForModuleAsync(string moduleName)
      {
            // Mapping from module name to Skill file
            var mapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "Polisher", "code-simplifier" },
            { "CodePolisher", "code-simplifier" },
            { "Review", "code-reviewer" },
            { "SmartReview", "code-reviewer" },
            { "CodeReview", "code-review-command" }
        };

            if (mapping.TryGetValue(moduleName, out var skillName))
            {
                  return await LoadSkillAsync(skillName);
            }

            return null;
      }

      /// <summary>
      /// Removes YAML frontmatter, keeping only the actual Prompt content.
      /// This allows Skill files to be used with any AI model.
      /// </summary>
      private static string StripFrontmatter(string content)
      {
            // Check if starts with --- (YAML frontmatter marker)
            if (!content.TrimStart().StartsWith("---"))
                  return content;

            // Remove frontmatter using regex
            var match = FrontmatterRegex().Match(content);
            if (match.Success)
            {
                  return content.Substring(match.Length).TrimStart();
            }

            return content;
      }
}
