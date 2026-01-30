using System.Windows;
using System.Windows.Controls;

namespace AITk.App;

/// <summary>
/// MainWindow - The Application Shell with Navigation.
/// Caches views to preserve state during navigation.
/// </summary>
public partial class MainWindow : Window
{
      // ═══════════════════════════════════════════════════════════
      // View Cache - Preserves state during navigation
      // ═══════════════════════════════════════════════════════════
      private Views.DashboardView? _dashboardView;
      private Views.RunnerView? _runnerView;
      private Views.ReviewView? _reviewView;
      private Views.PolisherView? _polisherView;
      private Views.PRReviewView? _prReviewView;

      public MainWindow()
      {
            InitializeComponent();
            Loaded += OnLoaded;
      }

      private void OnLoaded(object sender, RoutedEventArgs e)
      {
            LoadDashboard();
      }

      // ═══════════════════════════════════════════════════════════
      // Window Control Handlers
      // ═══════════════════════════════════════════════════════════
      private void MinimizeClick(object sender, RoutedEventArgs e)
          => WindowState = WindowState.Minimized;

      private void MaximizeClick(object sender, RoutedEventArgs e)
          => WindowState = WindowState == WindowState.Maximized
              ? WindowState.Normal
              : WindowState.Maximized;

      private void CloseClick(object sender, RoutedEventArgs e)
          => Close();

      // ═══════════════════════════════════════════════════════════
      // Navigation Handler
      // ═══════════════════════════════════════════════════════════
      private void NavChanged(object sender, RoutedEventArgs e)
      {
            if (ContentArea == null) return;

            if (sender is RadioButton rb)
            {
                  switch (rb.Name)
                  {
                        case "NavDashboard": LoadDashboard(); break;
                        case "NavRunner": LoadRunner(); break;
                        case "NavReview": LoadReview(); break;
                        case "NavPolisher": LoadPolisher(); break;
                        case "NavPRReview": LoadPRReview(); break;
                  }
            }
      }

      // ═══════════════════════════════════════════════════════════
      // View Loaders - Use caching to preserve state
      // ═══════════════════════════════════════════════════════════
      private void LoadDashboard()
      {
            _dashboardView ??= new Views.DashboardView();
            ContentArea.Content = _dashboardView;
      }

      private void LoadRunner()
      {
            _runnerView ??= new Views.RunnerView();
            ContentArea.Content = _runnerView;
      }

      private void LoadReview()
      {
            _reviewView ??= new Views.ReviewView();
            ContentArea.Content = _reviewView;
      }

      private void LoadPolisher()
      {
            _polisherView ??= new Views.PolisherView();
            ContentArea.Content = _polisherView;
      }

      private void LoadPRReview()
      {
            _prReviewView ??= new Views.PRReviewView();
            ContentArea.Content = _prReviewView;
      }
}
