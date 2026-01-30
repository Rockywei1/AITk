using System.Diagnostics;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AITk.Core.Models;
using AITk.Core.Services;

namespace AITk.App.ViewModels;

/// <summary>
/// Task Runner ViewModel - Supports auto-loop mode (<300 lines).
/// </summary>
public partial class RunnerViewModel : ObservableObject
{
      private readonly CommandRunner _runner = new();
      private CancellationTokenSource? _cts;
      private readonly Stopwatch _stopwatch = new();

      // ═══════════════════════════════════════════════════════════
      // Observable Properties
      // ═══════════════════════════════════════════════════════════
      [ObservableProperty] private string _command = "dotnet build";
      [ObservableProperty] private string _status = "Ready";
      [ObservableProperty] private string _elapsedTime = "0.0s";
      [ObservableProperty] private string _exitCodeDisplay = "";
      [ObservableProperty] private Brush _statusColor = Brushes.Gray;
      [ObservableProperty] private bool _isRunning;

      // Output with performance optimization
      private readonly System.Text.StringBuilder _outputBuilder = new();
      private const int MaxOutputLength = 500_000;  // 500KB limit to prevent OOM
      [ObservableProperty] private string _output = "Ready to execute commands...\n";

      // Event for auto-scroll notification
      public event Action? OutputChanged;

      // Loop Mode Properties
      [ObservableProperty] private int _maxRetries = 5;
      [ObservableProperty] private int _currentAttempt;
      [ObservableProperty] private int _delayBetweenRetries = 2;
      [ObservableProperty] private bool _isLoopMode;

      partial void OnMaxRetriesChanged(int value)
      {
            if (value < 1) MaxRetries = 1;
            else if (value > 100) MaxRetries = 100;
      }

      partial void OnDelayBetweenRetriesChanged(int value)
      {
            if (value < 0) DelayBetweenRetries = 0;
            else if (value > 60) DelayBetweenRetries = 60;
      }

      // ═══════════════════════════════════════════════════════════
      // Single Run Command
      // ═══════════════════════════════════════════════════════════
      [RelayCommand(CanExecute = nameof(CanRun))]
      private async Task RunAsync()
      {
            await ExecuteOnceAsync();
      }

      // ═══════════════════════════════════════════════════════════
      // Auto-Fix Loop Command
      // ═══════════════════════════════════════════════════════════
      [RelayCommand(CanExecute = nameof(CanRun))]
      private async Task RunLoopAsync()
      {
            if (string.IsNullOrWhiteSpace(Command)) return;

            IsRunning = true;
            IsLoopMode = true;
            _cts = new CancellationTokenSource();
            CurrentAttempt = 0;

            Output = $"🔄 Starting Auto-Fix Loop (max {MaxRetries} attempts)\n";
            Output += $"   Command: {Command}\n";
            Output += new string('─', 50) + "\n\n";

            try
            {
                  while (CurrentAttempt < MaxRetries && !_cts.Token.IsCancellationRequested)
                  {
                        CurrentAttempt++;
                        AppendOutput($"📍 Attempt {CurrentAttempt}/{MaxRetries}\n");

                        var result = await ExecuteCommandInternalAsync();

                        if (result.Success)
                        {
                              AppendOutput($"\n✅ SUCCESS on attempt {CurrentAttempt}!\n");
                              Status = $"Loop Complete ({CurrentAttempt}/{MaxRetries})";
                              StatusColor = Brushes.LimeGreen;
                              break;
                        }

                        if (CurrentAttempt < MaxRetries)
                        {
                              AppendOutput($"\n❌ Failed. Waiting {DelayBetweenRetries}s before retry...\n\n");
                              await Task.Delay(DelayBetweenRetries * 1000, _cts.Token);
                        }
                        else
                        {
                              AppendOutput($"\n💀 All {MaxRetries} attempts failed.\n");
                              AppendOutput("\n📋 Copy the output above and send to AI for analysis.\n");
                              Status = "Loop Failed";
                              StatusColor = Brushes.OrangeRed;
                        }
                  }
            }
            catch (OperationCanceledException)
            {
                  Status = "Loop Cancelled";
                  StatusColor = Brushes.Yellow;
                  AppendOutput("\n[Loop cancelled by user]\n");
            }
            finally
            {
                  IsRunning = false;
                  IsLoopMode = false;
                  _cts?.Dispose();
                  _cts = null;
            }
      }

      // ═══════════════════════════════════════════════════════════
      // Internal Execution
      // ═══════════════════════════════════════════════════════════
      private async Task ExecuteOnceAsync()
      {
            if (string.IsNullOrWhiteSpace(Command)) return;

            IsRunning = true;
            _cts = new CancellationTokenSource();
            _stopwatch.Restart();

            Output = $"$ {Command}\n\n";
            Status = "Running...";
            StatusColor = Brushes.Orange;

            try
            {
                  var result = await ExecuteCommandInternalAsync();
                  _stopwatch.Stop();
                  ElapsedTime = $"{_stopwatch.ElapsedMilliseconds / 1000.0:F1}s";

                  Status = result.Success ? "Completed" : "Failed";
                  StatusColor = result.Success ? Brushes.LimeGreen : Brushes.OrangeRed;
                  ExitCodeDisplay = $"Exit: {result.ExitCode}";
            }
            catch (OperationCanceledException)
            {
                  Status = "Cancelled";
                  StatusColor = Brushes.Yellow;
                  AppendOutput("\n[Cancelled]\n");
            }
            finally
            {
                  IsRunning = false;
                  _cts?.Dispose();
                  _cts = null;
            }
      }

      private async Task<ExecutionResult> ExecuteCommandInternalAsync()
      {
            var config = new ExecutionConfig { Command = Command, TimeoutMs = 60000 };
            var result = new ExecutionResult();

            try
            {
                  result = await _runner.ExecuteWithCallbackAsync(
                      config,
                      onOutput: line => AppendOutput(line),
                      onError: line => AppendOutput($"[ERR] {line}"),
                      cancellationToken: _cts?.Token ?? CancellationToken.None
                  );
            }
            catch (OperationCanceledException)
            {
                  result.Success = false;
                  result.ExitCode = -1;
            }

            return result;
      }

      // ═══════════════════════════════════════════════════════════
      // Stop & Copy Commands
      // ═══════════════════════════════════════════════════════════
      [RelayCommand(CanExecute = nameof(CanStop))]
      private void Stop() => _cts?.Cancel();

      [RelayCommand]
      private void CopyOutput()
      {
            var json = JsonSerializer.Serialize(new
            {
                  Command,
                  Output,
                  Status,
                  Attempts = IsLoopMode ? CurrentAttempt : 1
            }, new JsonSerializerOptions { WriteIndented = true });

            Clipboard.SetText(json);
            Status = "Copied to clipboard!";
      }

      private bool CanRun() => !IsRunning;
      private bool CanStop() => IsRunning;

      private void AppendOutput(string line)
      {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                  _outputBuilder.AppendLine(line);

                  // Limit output size to prevent memory issues
                  if (_outputBuilder.Length > MaxOutputLength)
                  {
                        var truncated = _outputBuilder.ToString()
                              .Substring(_outputBuilder.Length - MaxOutputLength / 2);
                        _outputBuilder.Clear();
                        _outputBuilder.AppendLine("... (older output truncated) ...");
                        _outputBuilder.Append(truncated);
                  }

                  Output = _outputBuilder.ToString();
                  OutputChanged?.Invoke();
            });
      }



      partial void OnIsRunningChanged(bool value)
      {
            RunCommand.NotifyCanExecuteChanged();
            RunLoopCommand.NotifyCanExecuteChanged();
            StopCommand.NotifyCanExecuteChanged();
      }
}
