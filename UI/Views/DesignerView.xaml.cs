using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using PinPoint.UI.Models;
using System.ComponentModel;
using System.Windows.Input;

namespace PinPoint.UI.Views
{
    public partial class DesignerView : UserControl
    {
        private CrosshairModel _crosshairModel = new CrosshairModel();
        private bool _isInitialized = false;
        
        public static readonly DependencyProperty CrosshairSizeProperty =
            DependencyProperty.Register("CrosshairSize", typeof(double), typeof(DesignerView), new PropertyMetadata(20.0));
        
        public static readonly DependencyProperty CrosshairThicknessProperty =
            DependencyProperty.Register("CrosshairThickness", typeof(double), typeof(DesignerView), new PropertyMetadata(2.0));
        
        public static readonly DependencyProperty CrosshairOpacityProperty =
            DependencyProperty.Register("CrosshairOpacity", typeof(double), typeof(DesignerView), new PropertyMetadata(1.0));
        
        public static readonly DependencyProperty CrosshairLeftProperty =
            DependencyProperty.Register("CrosshairLeft", typeof(double), typeof(DesignerView), new PropertyMetadata(0.0));
        
        public static readonly DependencyProperty CrosshairTopProperty =
            DependencyProperty.Register("CrosshairTop", typeof(double), typeof(DesignerView), new PropertyMetadata(0.0));
        
        public double CrosshairSize
        {
            get { return (double)GetValue(CrosshairSizeProperty); }
            set { SetValue(CrosshairSizeProperty, value); }
        }
        
        public double CrosshairThickness
        {
            get { return (double)GetValue(CrosshairThicknessProperty); }
            set { SetValue(CrosshairThicknessProperty, value); }
        }
        
        public double CrosshairOpacity
        {
            get { return (double)GetValue(CrosshairOpacityProperty); }
            set { SetValue(CrosshairOpacityProperty, value); }
        }
        
        public double CrosshairLeft
        {
            get { return (double)GetValue(CrosshairLeftProperty); }
            set { SetValue(CrosshairLeftProperty, value); }
        }
        
        public double CrosshairTop
        {
            get { return (double)GetValue(CrosshairTopProperty); }
            set { SetValue(CrosshairTopProperty, value); }
        }
        
        private void UpdateCrosshair()
        {
            if (_crosshairModel == null) 
            {
                _crosshairModel = new CrosshairModel();
            }
            
            if (!_isInitialized) return;
            
            _crosshairModel.Size = CrosshairSize;
            _crosshairModel.Thickness = CrosshairThickness;
            _crosshairModel.Opacity = CrosshairOpacity;
            _crosshairModel.X = CrosshairLeft;
            _crosshairModel.Y = CrosshairTop;
            
            UpdateCrosshairPath();
        }
        
        private void UpdateCrosshairPath()
        {
            if (_crosshairModel == null || CrosshairPreview == null || CrosshairPath == null) 
                return;
            
            double centerX = CrosshairPreview.ActualWidth / 2;
            double centerY = CrosshairPreview.ActualHeight / 2;
            
            // Create a geometry for the crosshair
            var geometry = new StreamGeometry();
            
            using (StreamGeometryContext context = geometry.Open())
            {
                // Horizontal line
                double halfSize = _crosshairModel.Size / 2;
                double gap = 4; // Gap in the center
                
                // Left horizontal line
                context.BeginFigure(new Point(centerX - halfSize, centerY), false, false);
                context.LineTo(new Point(centerX - gap, centerY), true, false);
                
                // Right horizontal line
                context.BeginFigure(new Point(centerX + gap, centerY), false, false);
                context.LineTo(new Point(centerX + halfSize, centerY), true, false);
                
                // Top vertical line
                context.BeginFigure(new Point(centerX, centerY - halfSize), false, false);
                context.LineTo(new Point(centerX, centerY - gap), true, false);
                
                // Bottom vertical line
                context.BeginFigure(new Point(centerX, centerY + gap), false, false);
                context.LineTo(new Point(centerX, centerY + halfSize), true, false);
            }
            
            geometry.Freeze();
            
            // Update the path
            CrosshairPath.Data = geometry;
            CrosshairPath.StrokeThickness = _crosshairModel.Thickness;
            CrosshairPath.Opacity = _crosshairModel.Opacity;
            CrosshairPath.Stroke = new SolidColorBrush(Colors.LightGreen);
            
            // Position the crosshair
            Canvas.SetLeft(CrosshairPath, centerX);
            Canvas.SetTop(CrosshairPath, centerY);
        }
        
        private void StyleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StyleComboBox == null || SelectedStylePath == null) return;
            
            // Get the selected style
            var selectedItem = StyleComboBox.SelectedItem as ComboBoxItem;
            if (selectedItem == null) return;
            
            string styleName = selectedItem.Content.ToString();
            
            // Update the preview based on selected style
            switch (styleName)
            {
                case "Standard":
                    SelectedStylePath.Data = new GeometryGroup
                    {
                        Children = new GeometryCollection
                        {
                            new LineGeometry(new Point(15, 0), new Point(15, 30)),
                            new LineGeometry(new Point(0, 15), new Point(30, 15))
                        }
                    };
                    break;
                    
                case "Dot":
                    var dotGeometry = new EllipseGeometry(new Point(15, 15), 3, 3);
                    SelectedStylePath.Data = dotGeometry;
                    break;
                    
                case "Circle":
                    var circleGeometry = new EllipseGeometry(new Point(15, 15), 10, 10);
                    SelectedStylePath.Data = circleGeometry;
                    break;
                    
                case "Tactical":
                    SelectedStylePath.Data = Geometry.Parse("M15,2 L15,28 M2,15 L28,15 M8,8 L22,22 M22,8 L8,22");
                    break;
            }
        }
        
        private void SizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_isInitialized || _crosshairModel == null) return;
            
            _crosshairModel.Size = e.NewValue;
            UpdateCrosshairPath();
        }
        
        private void ThicknessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_isInitialized || _crosshairModel == null) return;
            
            _crosshairModel.Thickness = e.NewValue;
            UpdateCrosshairPath();
        }
        
        private void OpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!_isInitialized || _crosshairModel == null) return;
            
            _crosshairModel.Opacity = e.NewValue;
            UpdateCrosshairPath();
        }
        
        private void ChooseColor_Click(object sender, RoutedEventArgs e)
        {
            ColorPickerPopup.PlacementTarget = ChooseColorButton;
            ColorPickerPopup.Placement = System.Windows.Controls.Primitives.PlacementMode.Right;
            ColorPickerPopup.IsOpen = true;
        }
        
        private void ColorSwatch_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string hexColor)
            {
                try
                {
                    // Apply color
                    var color = (Color)ColorConverter.ConvertFromString(hexColor);
                    CrosshairPath.Stroke = new SolidColorBrush(color);
                }
                catch { }
            }
        }
        
        private void ApplyColor_Click(object sender, RoutedEventArgs e)
        {
            ColorPickerPopup.IsOpen = false;
        }
        
        private void CrosshairPreview_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateCrosshairPath();
        }

        private void StyleDropdownBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Toggle the popup
            StylesPopup.IsOpen = !StylesPopup.IsOpen;
        }

        private void StyleOption_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Border border)
            {
                border.Background = new SolidColorBrush(Color.FromRgb(58, 84, 110)); // #3a546e
            }
        }

        private void StyleOption_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is Border border)
            {
                border.Background = new SolidColorBrush(Colors.Transparent);
            }
        }

        private void StyleOption_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is string styleName)
            {
                // Update the selected style text
                SelectedStyleText.Text = styleName;
                
                // Update the selected style icon using a NEW method
                // This is a totally different method with different logic
                UpdateSelectedStyleIcon(styleName);
                
                // Close the popup
                StylesPopup.IsOpen = false;
            }
        }

        // BRAND NEW METHOD with NO references to StylePreviewPath
        private void UpdateSelectedStyleIcon(string styleName)
        {
            switch (styleName)
            {
                case "Standard":
                    SelectedStylePath.Data = new GeometryGroup
                    {
                        Children = new GeometryCollection
                        {
                            new LineGeometry(new Point(10, 0), new Point(10, 20)),
                            new LineGeometry(new Point(0, 10), new Point(20, 10))
                        }
                    };
                    break;
                    
                case "Dot":
                    SelectedStylePath.Data = new EllipseGeometry(new Point(10, 10), 3, 3);
                    break;
                    
                case "Circle":
                    SelectedStylePath.Data = new EllipseGeometry(new Point(10, 10), 7, 7);
                    break;
                    
                case "Tactical":
                    SelectedStylePath.Data = Geometry.Parse("M10,2 L10,18 M2,10 L18,10 M6,6 L14,14 M14,6 L6,14");
                    break;
            }
        }

        private void DesignerView_Loaded(object sender, RoutedEventArgs e)
        {
            // This will only run after the XAML is fully loaded
            if (_crosshairModel != null)
            {
                // Now update with actual property values if available
                _crosshairModel.Size = CrosshairSize;
                _crosshairModel.Thickness = CrosshairThickness;
                _crosshairModel.Opacity = CrosshairOpacity;
                _crosshairModel.X = CrosshairLeft;
                _crosshairModel.Y = CrosshairTop;
            }
            
            // Make sure the canvas is properly sized
            if (CrosshairPreview != null)
            {
                CrosshairPreview.Width = 400;
                CrosshairPreview.Height = 400;
            }
            
            // Register property change callbacks
            DependencyPropertyDescriptor.FromProperty(CrosshairSizeProperty, typeof(DesignerView))
                .AddValueChanged(this, (s, e) => UpdateCrosshair());
            
            DependencyPropertyDescriptor.FromProperty(CrosshairThicknessProperty, typeof(DesignerView))
                .AddValueChanged(this, (s, e) => UpdateCrosshair());
            
            DependencyPropertyDescriptor.FromProperty(CrosshairOpacityProperty, typeof(DesignerView))
                .AddValueChanged(this, (s, e) => UpdateCrosshair());
            
            _isInitialized = true;
            
            // Explicitly update the crosshair to ensure it's drawn
            UpdateCrosshairPath();
        }
    }
}