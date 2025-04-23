using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using PinPoint.UI.ViewModels;

namespace PinPoint.UI.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            // Keep this if you have a MainViewModel class
            // this.DataContext = new MainViewModel();
            
            // Load views programmatically
            LoadViews();
        }
        
        private void LoadViews()
        {
            // Create the views
            var designerView = new DesignerView();
            var settingsView = new TextBlock { Text = "Settings View Content" }; // Placeholder
            var aboutView = new TextBlock { Text = "About View Content" }; // Placeholder
            
            // Load views into their respective hosts
            designerViewHost.Content = designerView;
            settingsViewHost.Content = settingsView;
            aboutViewHost.Content = aboutView;
            
            // Make sure designer view is visible by default
            designerViewHost.Visibility = Visibility.Visible;
            settingsViewHost.Visibility = Visibility.Collapsed;
            aboutViewHost.Visibility = Visibility.Collapsed;
        }

        // Allow window to be dragged when clicking on the title bar
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                try
                {
                    this.DragMove();
                }
                catch { }
            }
        }
        
        // Alternative method to handle mouse drag on title bar
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                try
                {
                    this.DragMove();
                }
                catch { }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        
        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized 
                ? WindowState.Normal 
                : WindowState.Maximized;
        }
        
        // Add the missing NavigationButton_Click method
        private void NavigationButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton button && button.Tag != null)
            {
                string viewName = button.Tag.ToString();
                
                // Hide all views first
                designerViewHost.Visibility = Visibility.Collapsed;
                settingsViewHost.Visibility = Visibility.Collapsed;
                aboutViewHost.Visibility = Visibility.Collapsed;
                
                // Show the selected view
                switch (viewName)
                {
                    case "Designer":
                        designerViewHost.Visibility = Visibility.Visible;
                        break;
                    case "Settings":
                        settingsViewHost.Visibility = Visibility.Visible;
                        break;
                    case "About":
                        aboutViewHost.Visibility = Visibility.Visible;
                        break;
                }
            }
        }
    }
}
