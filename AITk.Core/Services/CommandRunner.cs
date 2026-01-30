using System.Diagnostics;
using System.Text;
using AITk.Core.Models;

namespace AITk.Core.Services;

/// <summary>
/// Service for executing external commands.
/// </summary>
public class CommandRunner
{
      /// <summary>
      /// Executes a command asynchronously and returns the result.
      /// </summary>
      public async Task<ExecutionResult> ExecuteAsync(ExecutionConfig config)
      {
            var result = new ExecutionResult();
            var stopwatch = Stopwatch.StartNew();
            var stdOutBuilder = new StringBuilder();
            var stdErrBuilder = new StringBuilder();

            try
            {
                  using var process = new Process();
                  process.StartInfo = new ProcessStartInfo
                  {
                        FileName = "cmd.exe",
                        Arguments = $"/c chcp 65001 >nul && {config.Command}",
                        WorkingDirectory = string.IsNullOrEmpty(config.WorkingDirectory)
                          ? Directory.GetCurrentDirectory()
                          : config.WorkingDirectory,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        StandardOutputEncoding = Encoding.UTF8,
                        StandardErrorEncoding = Encoding.UTF8
                  };

                  process.OutputDataReceived += (_, e) =>
                  {
                        if (e.Data != null) stdOutBuilder.AppendLine(e.Data);
                  };

                  process.ErrorDataReceived += (_, e) =>
                  {
                        if (e.Data != null) stdErrBuilder.AppendLine(e.Data);
                  };

                  process.Start();
                  process.BeginOutputReadLine();
                  process.BeginErrorReadLine();

                  var completed = await Task.Run(() => process.WaitForExit(config.TimeoutMs));

                  if (!completed)
                  {
                        process.Kill(entireProcessTree: true);
                        result.TimedOut = true;
                        result.Success = false;
                        result.ExitCode = -1;
                  }
                  else
                  {
                        result.Success = process.ExitCode == 0;
                        result.ExitCode = process.ExitCode;
                  }

                  // Wait for output buffer to flush
                  await Task.Delay(100);
                  result.StdOut = stdOutBuilder.ToString().TrimEnd();
                  result.StdErr = stdErrBuilder.ToString().TrimEnd();
            }
            catch (Exception ex)
            {
                  result.Success = false;
                  result.ExitCode = -1;
                  result.StdErr = ex.Message;
            }
            finally
            {
                  stopwatch.Stop();
                  result.ElapsedMs = stopwatch.ElapsedMilliseconds;
            }

            return result;
      }

      /// <summary>
      /// Executes a command with real-time output streaming (for UI) and returns the result.
      /// </summary>
      public async Task<ExecutionResult> ExecuteWithCallbackAsync(
          ExecutionConfig config,
          Action<string> onOutput,
          Action<string> onError,
          CancellationToken cancellationToken = default)
      {
            var result = new ExecutionResult();
            var stopwatch = Stopwatch.StartNew();

            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                  FileName = "cmd.exe",
                  Arguments = $"/c chcp 65001 >nul && {config.Command}",
                  WorkingDirectory = string.IsNullOrEmpty(config.WorkingDirectory)
                    ? Directory.GetCurrentDirectory()
                    : config.WorkingDirectory,
                  RedirectStandardOutput = true,
                  RedirectStandardError = true,
                  UseShellExecute = false,
                  CreateNoWindow = true,
                  StandardOutputEncoding = Encoding.UTF8,
                  StandardErrorEncoding = Encoding.UTF8
            };

            process.OutputDataReceived += (_, e) =>
            {
                  if (e.Data != null) onOutput(e.Data);
            };

            process.ErrorDataReceived += (_, e) =>
            {
                  if (e.Data != null) onError(e.Data);
            };

            try
            {
                  process.Start();
                  process.BeginOutputReadLine();
                  process.BeginErrorReadLine();

                  await process.WaitForExitAsync(cancellationToken);

                  result.Success = process.ExitCode == 0;
                  result.ExitCode = process.ExitCode;
            }
            catch (OperationCanceledException)
            {
                  process.Kill(entireProcessTree: true);
                  result.Success = false;
                  result.ExitCode = -1;
                  throw;
            }
            finally
            {
                  stopwatch.Stop();
                  result.ElapsedMs = stopwatch.ElapsedMilliseconds;
            }

            return result;
      }
}
