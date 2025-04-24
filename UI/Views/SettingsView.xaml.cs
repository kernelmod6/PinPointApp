using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Diagnostics;
using System;

namespace PinPoint.UI.Views
{
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
            
            // Initialize with Appearance section visible, others collapsed
            AppearanceSection.Visibility = Visibility.Visible;
            BehaviorSection.Visibility = Visibility.Collapsed;
            PerformanceSection.Visibility = Visibility.Collapsed;
            AdvancedSection.Visibility = Visibility.Collapsed;
            AboutSection.Visibility = Visibility.Collapsed;
            
            // Make sure Appearance tab is selected by default
            if (AppearanceNavButton != null)
                AppearanceNavButton.IsChecked = true;
            
            // Set up event handlers for navigation
            var navButtons = this.FindVisualChildren<RadioButton>()
                .Where(rb => rb.GroupName == "SettingsNav");
                
            foreach (var button in navButtons)
            {
                button.Checked += SettingsNavButton_Checked;
            }
        }
        
        private void SettingsNavButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb)
            {
                string tag = rb.Tag?.ToString();
                
                // Hide all sections first
                if (AppearanceSection != null) 
                    AppearanceSection.Visibility = Visibility.Collapsed;
                if (BehaviorSection != null) 
                    BehaviorSection.Visibility = Visibility.Collapsed;
                if (PerformanceSection != null)
                    PerformanceSection.Visibility = Visibility.Collapsed;
                if (AdvancedSection != null)
                    AdvancedSection.Visibility = Visibility.Collapsed;
                if (AboutSection != null)
                    AboutSection.Visibility = Visibility.Collapsed;
                
                // Show the selected section
                switch (tag)
                {
                    case "Appearance":
                        if (AppearanceSection != null)
                            AppearanceSection.Visibility = Visibility.Visible;
                        break;
                        
                    case "Behavior":
                        if (BehaviorSection != null)
                            BehaviorSection.Visibility = Visibility.Visible;
                        break;
                        
                    case "Performance":
                        if (PerformanceSection != null)
                            PerformanceSection.Visibility = Visibility.Visible;
                        break;
                        
                    case "Advanced":
                        if (AdvancedSection != null)
                            AdvancedSection.Visibility = Visibility.Visible;
                        break;
                        
                    case "About":
                        if (AboutSection != null)
                            AboutSection.Visibility = Visibility.Visible;
                        break;
                }
            }
        }
        
        private void UIScaleSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ScaleValueText != null)
            {
                // Round to nearest 5%
                int roundedValue = (int)System.Math.Round(e.NewValue / 5) * 5;
                ScaleValueText.Text = $"{roundedValue}%";
            }
        }
        
        private void ScalePreset_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string tagValue && UIScaleSlider != null)
            {
                if (int.TryParse(tagValue, out int scaleValue))
                {
                    UIScaleSlider.Value = scaleValue;
                }
            }
        }

        // This method would handle actual scaling implementation
        private void ApplyUIScaling(double scaleFactor)
        {
            // Implement UI scaling logic here
            // For example, you could adjust the ScaleTransform of the main UI container
            // Or set application-wide scaling parameters
        }
        
        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            // Open the URL in the default browser
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = e.Uri.AbsoluteUri,
                UseShellExecute = true
            });
            e.Handled = true;
        }

        private void ComboBox_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            // Mark the event as handled to prevent selection changes on scroll
            e.Handled = true;
        }

        // Add this method to handle the resource links
        private void ResourceLink_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string url)
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Unable to open link: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
    
    // Helper extension for finding visual children
    public static class VisualTreeHelpers
    {
        public static System.Collections.Generic.IEnumerable<T> FindVisualChildren<T>(this DependencyObject depObj)
            where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
    }
} 