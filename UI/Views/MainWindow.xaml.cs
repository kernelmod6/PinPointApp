using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using PinPoint.UI.ViewModels;
using System.Linq;

namespace PinPoint.UI.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            // Set the DataContext
            DataContext = new MainViewModel();
            
            // Default to Designer tab
            designerViewHost.Visibility = Visibility.Visible;
            settingsViewHost.Visibility = Visibility.Collapsed;
            aboutViewHost.Visibility = Visibility.Collapsed;

            // Create view instances
            settingsViewHost.Content = new SettingsView();
            aboutViewHost.Content = new AboutView();
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
            if (sender is RadioButton radioButton)
            {
                string tag = radioButton.Tag?.ToString();

                // Hide all views first
                designerViewHost.Visibility = Visibility.Collapsed;
                settingsViewHost.Visibility = Visibility.Collapsed;
                aboutViewHost.Visibility = Visibility.Collapsed;

                // Find the main grid that contains our layout
                Grid mainGrid = FindMainGrid();
                
                // Find the content grid in row 1 (below title bar)
                var contentGrid = mainGrid?.Children.OfType<Grid>()
                    .FirstOrDefault(g => Grid.GetRow(g) == 1);
                    
                if (contentGrid == null)
                    return;
                    
                // Get the action panel (first column of content grid)
                var actionPanel = contentGrid.Children.OfType<FrameworkElement>()
                    .FirstOrDefault(c => Grid.GetColumn(c) == 0);
                    
                // Get the content panel (second column of content grid)
                var contentPanel = contentGrid.Children.OfType<FrameworkElement>()
                    .FirstOrDefault(c => Grid.GetColumn(c) == 1);
                    
                // Show the selected view
                switch (tag)
                {
                    case "Designer":
                        designerViewHost.Visibility = Visibility.Visible;
                        
                        // Show action panel for Designer tab
                        if (actionPanel != null)
                        {
                            actionPanel.Visibility = Visibility.Visible;
                        }
                        
                        // Reset content panel to original width
                        if (contentPanel != null && contentGrid.ColumnDefinitions.Count >= 2)
                        {
                            Grid.SetColumnSpan(contentPanel, 1);
                            contentGrid.ColumnDefinitions[0].Width = new GridLength(250);
                            contentGrid.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
                        }
                        break;

                    case "Settings":
                        settingsViewHost.Visibility = Visibility.Visible;
                        
                        // Hide action panel for Settings tab
                        if (actionPanel != null)
                        {
                            actionPanel.Visibility = Visibility.Collapsed;
                        }
                        
                        // Make content panel span both columns
                        if (contentPanel != null && contentGrid.ColumnDefinitions.Count >= 2)
                        {
                            Grid.SetColumnSpan(contentPanel, 2);
                            contentGrid.ColumnDefinitions[0].Width = new GridLength(0);
                            contentGrid.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
                        }
                        break;

                    case "About":
                        aboutViewHost.Visibility = Visibility.Visible;
                        
                        // Hide action panel for About tab
                        if (actionPanel != null)
                        {
                            actionPanel.Visibility = Visibility.Collapsed;
                        }
                        
                        // Make content panel span both columns
                        if (contentPanel != null && contentGrid.ColumnDefinitions.Count >= 2)
                        {
                            Grid.SetColumnSpan(contentPanel, 2);
                            contentGrid.ColumnDefinitions[0].Width = new GridLength(0);
                            contentGrid.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
                        }
                        break;
                }
            }
        }

        // Helper method to find the main grid
        private Grid FindMainGrid()
        {
            // First try to find by name if you happen to have a grid named "MainGrid"
            var namedGrid = this.FindName("MainGrid") as Grid;
            if (namedGrid != null)
                return namedGrid;
            
            // Otherwise find the root content grid
            return this.Content as Grid;
        }

        // Helper method to find the action panel
        private FrameworkElement FindActionPanel(Grid mainGrid)
        {
            if (mainGrid != null && mainGrid.Children.Count > 0)
            {
                // First try to find a Border or Panel in column 0
                foreach (FrameworkElement child in mainGrid.Children)
                {
                    if ((child is Border || child is Panel) && Grid.GetColumn(child) == 0)
                    {
                        return child;
                    }
                }
                
                // If not found, just return the first child
                return mainGrid.Children[0] as FrameworkElement;
            }
            return null;
        }
    }
}
