using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using PinPoint.UI.Models;
using PinPoint.UI.Services;

namespace PinPoint.UI.Views
{
    public partial class DesignerView : UserControl
    {
        private CrosshairModel _crosshairModel;
        
        // Dependency properties for binding
        public static readonly DependencyProperty CrosshairColorProperty =
            DependencyProperty.Register("CrosshairColor", typeof(SolidColorBrush), typeof(DesignerView), 
                new PropertyMetadata(new SolidColorBrush(Colors.LimeGreen)));
        
        public static readonly DependencyProperty CrosshairSizeProperty =
            DependencyProperty.Register("CrosshairSize", typeof(double), typeof(DesignerView), 
                new PropertyMetadata(20.0));
        
        public static readonly DependencyProperty CrosshairThicknessProperty =
            DependencyProperty.Register("CrosshairThickness", typeof(double), typeof(DesignerView), 
                new PropertyMetadata(2.0));
        
        public static readonly DependencyProperty CrosshairOpacityProperty =
            DependencyProperty.Register("CrosshairOpacity", typeof(double), typeof(DesignerView), 
                new PropertyMetadata(1.0));
        
        public static readonly DependencyProperty CrosshairLeftProperty =
            DependencyProperty.Register("CrosshairLeft", typeof(double), typeof(DesignerView), 
                new PropertyMetadata(0.0));
        
        public static readonly DependencyProperty CrosshairTopProperty =
            DependencyProperty.Register("CrosshairTop", typeof(double), typeof(DesignerView), 
                new PropertyMetadata(0.0));
        
        public SolidColorBrush CrosshairColor
        {
            get { return (SolidColorBrush)GetValue(CrosshairColorProperty); }
            set { SetValue(CrosshairColorProperty, value); }
        }
        
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
        
        public DesignerView()
        {
            InitializeComponent();
            
            // Initialize the crosshair model
            _crosshairModel = new CrosshairModel
            {
                Color = Colors.LimeGreen,
                Size = 20.0,
                Thickness = 2.0,
                Opacity = 1.0,
                Style = CrosshairStyle.Cross
            };
            
            // Center the crosshair in the preview
            CrosshairPreview.SizeChanged += (s, e) => UpdateCrosshairPosition();
            
            // Apply initial settings
            ApplySettings();
        }
        
        private void UpdateCrosshairPosition()
        {
            CrosshairLeft = CrosshairPreview.ActualWidth / 2;
            CrosshairTop = CrosshairPreview.ActualHeight / 2;
        }
        
        private void StyleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StyleComboBox.SelectedIndex >= 0)
            {
                _crosshairModel.Style = (CrosshairStyle)StyleComboBox.SelectedIndex;
                UpdateCrosshairPreview();
            }
        }
        
        private void SizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_crosshairModel != null)
            {
                _crosshairModel.Size = e.NewValue;
                UpdateCrosshairPreview();
            }
        }
        
        private void ThicknessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_crosshairModel != null)
            {
                _crosshairModel.Thickness = e.NewValue;
                UpdateCrosshairPreview();
            }
        }
        
        private void OpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_crosshairModel != null)
            {
                _crosshairModel.Opacity = e.NewValue;
                UpdateCrosshairPreview();
            }
        }
        
        private void ChooseColor_Click(object sender, RoutedEventArgs e)
        {
            // Use standard WPF color picker dialog
            var dialog = new System.Windows.Forms.ColorDialog();
            dialog.Color = System.Drawing.Color.FromArgb(
                _crosshairModel.Color.A,
                _crosshairModel.Color.R,
                _crosshairModel.Color.G,
                _crosshairModel.Color.B);
            
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _crosshairModel.Color = Color.FromArgb(
                    dialog.Color.A,
                    dialog.Color.R,
                    dialog.Color.G,
                    dialog.Color.B);
                
                CrosshairColor = new SolidColorBrush(_crosshairModel.Color);
                UpdateCrosshairPreview();
            }
        }
        
        private void ApplySettings_Click(object sender, RoutedEventArgs e)
        {
            ApplySettings();
        }
        
        private void ApplySettings()
        {
            // Apply settings to the actual overlay
            OverlayService.SetCrosshairColor(
                (float)_crosshairModel.Color.R / 255.0f,
                (float)_crosshairModel.Color.G / 255.0f,
                (float)_crosshairModel.Color.B / 255.0f,
                (float)_crosshairModel.Opacity
            );
            
            OverlayService.SetCrosshairSize((float)_crosshairModel.Size);
            OverlayService.SetCrosshairThickness((float)_crosshairModel.Thickness);
            OverlayService.SetCrosshairStyle((int)_crosshairModel.Style);
        }
        
        private void UpdateCrosshairPreview()
        {
            CrosshairColor = new SolidColorBrush(_crosshairModel.Color) { Opacity = _crosshairModel.Opacity };
            CrosshairThickness = _crosshairModel.Thickness;
            
            // Update crosshair geometry based on style
            GeometryGroup geometry = new GeometryGroup();
            
            switch (_crosshairModel.Style)
            {
                case CrosshairStyle.Cross:
                    double size = _crosshairModel.Size / 2;
                    geometry.Children.Add(new LineGeometry(new Point(-size, 0), new Point(size, 0)));
                    geometry.Children.Add(new LineGeometry(new Point(0, -size), new Point(0, size)));
                    break;
                
                case CrosshairStyle.Dot:
                    geometry.Children.Add(new EllipseGeometry(new Point(0, 0), _crosshairModel.Size / 4, _crosshairModel.Size / 4));
                    break;
                
                case CrosshairStyle.Circle:
                    geometry.Children.Add(new EllipseGeometry(new Point(0, 0), _crosshairModel.Size / 2, _crosshairModel.Size / 2));
                    break;
            }
            
            CrosshairPath.Data = geometry;
        }
    }
} 