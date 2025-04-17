using System;
using System.Windows;

namespace PinPoint.UI
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Check architecture
            if (!Environment.Is64BitProcess)
            {
                MessageBox.Show(
                    "This application must run in 64-bit mode. Please rebuild in x64 configuration.",
                    "Architecture Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                
                Shutdown(1);
                return;
            }
            
            // Continue normal startup
            Services.OverlayService.Initialize();
            
            // Make sure to clean up when the application exits
            Current.Exit += (s, args) => Services.OverlayService.Shutdown();
        }
    }
} 