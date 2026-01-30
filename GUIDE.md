# AITk Beginner's Guide

> **AITk (AI Toolkit)** - Desktop tool to make AI-assisted programming more efficient

---

## 📖 What is AITk?

AITk is a Windows desktop application designed to **enhance your AI-assisted programming experience**. It's not an AI itself, but a **prompt generator** — helping you quickly create structured Prompts so any AI (whether IDE plugins or web chats) can more accurately understand your code and requirements.

### Core Concept

```
Your Code → AITk generates structured Prompt → Copy to AI → AI gives precise answers
```

---

## 🎯 Five Core Modules

| Module | Function | Use Case |
|--------|----------|----------|
| **Dashboard** | Overview Panel | Quick project status check |
| **Task Runner** | Command Executor | Auto-retry loop mode |
| **Smart Review** | Git Change Review | Pre-commit code review |
| **Code Polisher** | Code Optimizer | Simplify complex code |
| **PR Review** | Comprehensive Review | Multi-dimensional code analysis |

---

## 🚀 Detailed Module Guide

### 1. Task Runner (Command Executor)

#### Description

Execute terminal commands with **auto-retry loop** ("persistence mode"), perfect for:

- Fixing compilation errors
- Running tests until they pass
- Executing lint/formatting

#### Usage Steps

1. **Enter command**: Type the command you want to execute

   ```
   dotnet build
   ```

2. **Single execution**: Click `▶ Run Once`

3. **Persistence mode**:
   - Set max retry count (default: 5)
   - Set retry interval (default: 2 seconds)
   - Click `🔄 Start Loop`
   - Command auto-repeats until success or max retries reached

4. **Copy output**: Click `📋 Copy` to copy results to clipboard

#### Using with AI

```
Scenario: Unable to resolve compilation error

1. Execute `dotnet build` in Task Runner
2. After seeing the error, click `📋 Copy`
3. Paste into AI chat window
4. AI receives formatted error info including command, output, and status
```

---

### 2. Smart Review (Intelligent Review)

#### Description

Scans **uncommitted changes** in Git repository and generates AI-readable review packets.

#### Usage Steps

1. **Select directory**: Click `📂 Browse` to choose project folder

2. **Scan changes**: Click `🔄 Scan` to detect Git changes

3. **View file list**: Left side shows all changed files
   - ✅ Check files to include in review
   - Click file to preview Diff

4. **Generate review packet**: Click `📋 Generate`
   - Automatically copied to clipboard
   - Includes branch info, file list, code diffs

#### Using with AI

```
Scenario: Have AI review code before committing

1. Open Smart Review after making code changes
2. Select project directory → Scan
3. Check files to review
4. Click Generate
5. Paste to AI: "Please review these code changes"
```

**Compatible AI Tools**:

- Cursor: Paste directly in chat
- Antigravity: Paste in dialog box
- ChatGPT/Claude web: Paste in input field

---

### 3. Code Polisher (Code Optimizer)

#### Description

Generate **simplification/optimization Prompts** for individual code files.

#### Usage Steps

1. **Select file**: Click `📂 Browse` to choose the code file to optimize

2. **Set optimization goals** (multiple selection):
   - ☑️ Remove dead code
   - ☑️ Simplify conditionals
   - ☑️ Extract methods
   - ☐ Improve naming
   - ☐ Add comments

3. **Set line limit**: Target max lines (default: 300)

4. **Generate Prompt**: Click `✨ Generate`

5. **Copy to use**: Click `📋 Copy Prompt`

#### Using with AI

```
Scenario: Code too complex, want to simplify

1. Open Code Polisher
2. Select complex source file
3. Check "Simplify conditionals" and "Extract methods"
4. Generate Prompt
5. Paste to AI, AI will provide simplified code
```

---

### 4. PR Review (Comprehensive Review Tool)

#### Description

The most powerful review tool, supporting **multiple AI Agent perspectives** for code analysis.

#### Usage Steps

1. **Select project**: Click `📂 Browse` to choose project directory

2. **Choose scan mode**:
   - 🔀 **Git Changes Only**: Review only uncommitted changes
   - 📁 **Scan All Files**: Scan all code files (max 100)

3. **Click scan**: `🔎 Scan Changes`

4. **Select review dimensions** (multiple selection):

   | Dimension | Description |
   |-----------|-------------|
   | 📖 Code Review | Standard code review |
   | 🧪 Test Coverage | Test coverage analysis |
   | ⚠️ Error Handling | Error handling checks |
   | 💬 Comments | Comment quality analysis |
   | 📐 Type Design | Type design analysis |
   | ✨ Simplify | Code simplification suggestions |

5. **Generate**: Click `📋 Generate & Copy`

#### Using with AI

```
Scenario: Comprehensive review of PR about to merge

1. Open PR Review
2. Select project directory
3. Use Git Changes mode to scan
4. Check all review dimensions
5. After generating, paste to AI
6. AI analyzes code from multiple angles
```

---

## 🤝 Working with Various AI Tools

### Cursor / Antigravity (AI IDE)

```
1. Generate Prompt in AITk
2. Open AI chat panel (Cmd+L / Ctrl+L)
3. Paste Prompt
4. AI understands code context and provides suggestions
5. Apply AI's code modifications directly in IDE
```

**Advantage**: AI responses can be directly applied as code edits

### VSCode + GitHub Copilot Chat

```
1. Generate Prompt in AITk
2. Open Copilot Chat (Ctrl+Shift+I)
3. Paste Prompt
4. Copilot analyzes and provides suggestions
```

### Web AI (ChatGPT / Claude / Gemini)

```
1. Generate Prompt in AITk
2. Open AI chat in browser
3. Paste entire Prompt directly
4. AI parses structured content and provides detailed analysis
```

**Tip**: Web AI typically supports longer context, suitable for large reviews

---

## 💡 Tips & Tricks

### 1. Auto-loading Project Guidelines

AITk automatically finds and loads from project root:

- `CLAUDE.md`
- `SKILL.md`
- `README.md`

This content is included in generated Prompts to help AI understand project standards.

### 2. Skills System

AITk includes professional AI Agent instructions (in `Skills/` folder):

- `code-reviewer.md` - Code Review Expert
- `code-simplifier.md` - Code Simplification Expert
- `silent-failure-hunter.md` - Silent Failure Hunter
- `pr-test-analyzer.md` - Test Coverage Analyst

These instructions are automatically injected into generated Prompts.

### 3. Output Format

AITk generates **structured Markdown** Prompts containing:

- Clear headings and sections
- Code blocks with syntax highlighting
- Diff-formatted change display

This format makes it easier for AI to understand and process.

---

## ⚠️ FAQ

### Q: Why can't I find any changes?

**A**: Ensure the directory is a Git repository with uncommitted changes.

### Q: What if the Prompt is too long?

**A**:

- Reduce the number of selected files
- Use Code Polisher for single-file analysis
- Some AI support long text (like Claude)

### Q: Which programming languages are supported?

**A**: Supports 30+ languages with syntax highlighting, including C#, Python, JavaScript, TypeScript, Java, Go, Rust, etc.

---

## 🎓 Quick Start Path

```
Recommended learning path for new users:

1. Task Runner → Experience command execution and output copying
2. Smart Review → Try scanning Git changes
3. Code Polisher → Select a complex file to generate optimization Prompt
4. PR Review → Perform complete multi-dimensional review
```

---

**Happy Coding with AI! 🚀**
