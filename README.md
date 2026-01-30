# AITk (AI Toolkit)

> **Desktop tool to enhance AI-assisted programming.**
>
> **提升 AI 辅助编程体验的桌面工具。**

[English](#english) | [中文](#chinese)

---

<a name="english"></a>

## 🇬🇧 English

AITk is a Windows desktop application designed to bridge the gap between your code and AI. It generates structured Prompts from your codebase, allowing any AI (IDE plugins, web chats) to better understand your context and intent.

### 🚀 Key Features

| Module | Description | Use Case |
|--------|-------------|----------|
| **Dashboard** | Overview Panel | Quick project status check |
| **Task Runner** | Command Executor | "Auto-Fix Loop" for build/test errors |
| **Smart Review** | Git Change Scanner | Generate reviews for uncommitted changes |
| **Code Polisher** | Code Optimizer | Simplify/Refactor specific files |
| **PR Review** | Comprehensive Review | Multi-agent analysis for PRs |

### 🛠️ Getting Started

**Prerequisites**: .NET 8.0 SDK, Windows 10/11

```powershell
# Clone the repository
git clone https://github.com/yourusername/AITk.git

# Navigate to project directory
cd AITk

# Build
dotnet build

# Run
dotnet run --project AITk.App
```

### 📖 Module Guide

1. **Task Runner**: Execute terminal commands with **Auto-Fix Loop**. Great for fixing flaky tests or compilation errors.
2. **Smart Review**: Scan **uncommitted Git changes** and generate an AI-readable review packet.
3. **Code Polisher**: Generate **simplification prompts** for complex files (e.g. "Remove dead code", "Simplify conditionals").
4. **PR Review**: Comprehensive multi-agent review system analyzing code, tests, errors, and types.

### 📄 License

**GNU Affero General Public License v3.0 (AGPL-3.0)**

This project is licensed under the **AGPL-3.0**. This is the strictest Open Source license, ensuring that:

- Any modifications or derivative works must also be Open Source.
- Integration into proprietary (closed-source) software is strictly prohibited.
- If you use this software to provide a service over a network, you must release the source code.

**This project is not for sale and cannot be sub-licensed for proprietary commercial use without explicit permission.**

---

<a name="chinese"></a>

## 🇨🇳 中文

AITk 是一款 Windows 桌面应用程序，专为连接你的代码与 AI 而设计。它能根据你的代码库生成结构化的 Prompt（提示词），让任何 AI（无论是 IDE 插件还是网页聊天机器人）都能更准确地理解你的上下文和意图。

### 🚀 核心功能

| 模块 | 功能说明 | 适用场景 |
|------|----------|----------|
| **Dashboard** | 总览面板 | 快速查看项目状态 |
| **Task Runner** | 命令执行器 | "死磕模式" (自动重试) 修复编译/测试错误 |
| **Smart Review** | 智能审查 | 扫描 Git 未提交变更并生成审查包 |
| **Code Polisher** | 代码磨光机 | 简化、重构特定的复杂代码文件 |
| **PR Review** | PR 综合审查 | 多 Agent 视角的代码合并请求审查 |

### 🛠️ 快速开始

**前置要求**: .NET 8.0 SDK, Windows 10/11

```powershell
# 克隆仓库
git clone https://github.com/yourusername/AITk.git

# 进入目录
cd AITk

# 构建
dotnet build

# 运行
dotnet run --project AITk.App
```

### 📖 模块指南

1. **Task Runner (任务运行器)**: 支持 **Auto-Fix Loop (死磕模式)** 的终端命令执行器。非常适合自动修复不稳定的测试或编译错误。
2. **Smart Review (智能审查)**: 扫描 **Git 未提交的变更**，生成 AI 可读的代码审查包。
3. **Code Polisher (代码磨光机)**: 为复杂文件生成 **代码简化 Prompt**（支持"移除死代码"、"简化条件判断"等选项）。
4. **PR Review (PR 审查)**: 综合性的多 Agent 审查系统，从代码质量、测试覆盖、错误处理和类型设计等多个维度分析代码。

### 📄 许可证 (License)

**GNU Affero General Public License v3.0 (AGPL-3.0)**

本项目采用 **AGPL-3.0** 协议授权。这是最严格的开源协议，确保：

- 任何修改或衍生作品也都必须开源。
- 严禁集成到专有（闭源）软件中。
- 如果通过网络提供本软件的服务，必须公开源代码。

**本项目非卖品，未经明确许可，不得进行商业闭源授权或通过出售获利。**
