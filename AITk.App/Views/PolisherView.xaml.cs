using System.Windows.Controls;

namespace AITk.App.Views;

public partial class PolisherView : UserControl
{
    public PolisherView()
    {
        InitializeComponent();
        DataContext = new ViewModels.PolisherViewModel();
    }
}
