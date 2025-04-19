using System.Windows;
using PinPoint.UI.Services;

namespace PinPoint.UI.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ShowOverlay_Checked(object sender, RoutedEventArgs e)
        {
            OverlayService.ShowOverlay(true);
        }

        private void ShowOverlay_Unchecked(object sender, RoutedEventArgs e)
        {
            OverlayService.ShowOverlay(false);
        }
    }
}
