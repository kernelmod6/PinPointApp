using System.Windows;

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
            Services.OverlayService.ShowOverlay(true);
        }
        
        private void ShowOverlay_Unchecked(object sender, RoutedEventArgs e)
        {
            Services.OverlayService.ShowOverlay(false);
        }
    }
} 