using System.Windows.Controls;

namespace AITk.App.Views;

public partial class ReviewView : UserControl
{
    public ReviewView()
    {
        InitializeComponent();
        DataContext = new ViewModels.ReviewViewModel();
    }
}
