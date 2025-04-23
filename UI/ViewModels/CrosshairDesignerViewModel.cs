using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;
using PinPoint.UI.Commands;
using PinPoint.UI.Models;

namespace PinPoint.UI.ViewModels
{
    public class CrosshairDesignerViewModel : INotifyPropertyChanged
    {
        private CrosshairModel _crosshairModel;
        
        public CrosshairDesignerViewModel()
        {
            _crosshairModel = new CrosshairModel
            {
                Color = Colors.Green,
                Size = 20,
                Thickness = 2,
                Opacity = 1.0,
                Style = CrosshairStyle.Default
            };
            
            SetColorCommand = new RelayCommand<string>(ExecuteSetColor);
            ChooseColorCommand = new RelayCommand(ExecuteChooseColor);
        }
        
        public CrosshairModel CrosshairModel
        {
            get => _crosshairModel;
            set
            {
                _crosshairModel = value;
                OnPropertyChanged();
            }
        }
        
        // Commands
        public ICommand SetColorCommand { get; }
        public ICommand ChooseColorCommand { get; }
        
        private void ExecuteSetColor(string hexColor)
        {
            if (string.IsNullOrEmpty(hexColor)) return;
            
            try
            {
                var color = (Color)ColorConverter.ConvertFromString(hexColor);
                _crosshairModel.Color = color;
                OnPropertyChanged(nameof(CrosshairModel));
            }
            catch (Exception ex)
            {
                // Log or handle the exception
                Console.WriteLine($"Error setting color: {ex.Message}");
            }
        }
        
        private void ExecuteChooseColor()
        {
            // Implement color picker dialog logic
            // For now just set a default color
            _crosshairModel.Color = Colors.Red;
            OnPropertyChanged(nameof(CrosshairModel));
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 