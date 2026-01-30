namespace AITk.Core.Models;

/// <summary>
/// 命令执行结果
/// </summary>
public class ExecutionResult
{
    public bool Success { get; set; }
    public int ExitCode { get; set; }
    public string StdOut { get; set; } = string.Empty;
    public string StdErr { get; set; } = string.Empty;
    public long ElapsedMs { get; set; }
    public bool TimedOut { get; set; }
}

/// <summary>
/// 命令执行配置
/// </summary>
public class ExecutionConfig
{
    public string Command { get; set; } = string.Empty;
    public string WorkingDirectory { get; set; } = string.Empty;
    public int TimeoutMs { get; set; } = 30000;
}
