using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using PinPoint.UI.Services;
using PinPoint.UI.Commands;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using PinPoint.UI.Views;

namespace PinPoint.UI.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private bool _isOverlayVisible;
        private int _selectedTabIndex;
        private string _statusMessage = "Design your crosshair";
        private string _title = "PinPoint";

        public MainViewModel()
        {
            // Initialize commands
            ToggleOverlayCommand = new RelayCommand(ExecuteToggleOverlay);

            // Initialize views
            DesignerView = new DesignerView();
            SettingsView = new SettingsView();
            AboutView = new AboutView();
        }

        public bool IsOverlayVisible
        {
            get => _isOverlayVisible;
            set
            {
                if (_isOverlayVisible != value)
                {
                    _isOverlayVisible = value;
                    OnPropertyChanged();
                }
            }
        }

        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set
            {
                if (_selectedTabIndex != value)
                {
                    _selectedTabIndex = value;
                    OnPropertyChanged();
                    
                    // Update status message based on selected tab
                    switch (_selectedTabIndex)
                    {
                        case 0:
                            StatusMessage = "Design your crosshair";
                            break;
                        case 1:
                            StatusMessage = "Configure application settings";
                            break;
                        case 2:
                            StatusMessage = "About PinPoint";
                            break;
                    }
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (_statusMessage != value)
                {
                    _statusMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Title
        {
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged();
                }
            }
        }

        // Commands
        public ICommand ToggleOverlayCommand { get; }

        // Command implementations
        private void ExecuteToggleOverlay()
        {
            IsOverlayVisible = !IsOverlayVisible;
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public UserControl DesignerView { get; private set; }
        public UserControl SettingsView { get; private set; }
        public UserControl AboutView { get; private set; }
    }
}
