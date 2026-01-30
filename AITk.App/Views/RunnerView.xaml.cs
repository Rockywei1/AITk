using System.Windows.Controls;
using AITk.App.ViewModels;

namespace AITk.App.Views;

public partial class RunnerView : UserControl
{
      public RunnerView()
      {
            InitializeComponent();
            var vm = new RunnerViewModel();
            DataContext = vm;

            // Subscribe to OutputChanged event for auto-scroll
            vm.OutputChanged += () =>
            {
                  // Auto-scroll to bottom when new output is added
                  OutputScroller?.ScrollToEnd();
            };
      }
}
