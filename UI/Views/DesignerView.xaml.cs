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

        // Constructor - commented out to help identify any other constructors
        /* 
        public DesignerView()
        {
            InitializeComponent();
            this.Loaded += DesignerView_Loaded;
        }
        */

        // Using a static constructor to initialize the component as a test
        static DesignerView()
        {
            // This is a static constructor that runs once per type
        }

        // THIS WILL RUN - We renamed the constructor to help identify duplicates
        public void InitializeView()
        {
            InitializeComponent();
            this.Loaded += DesignerView_Loaded;

            // Force initial setup
            _crosshairModel.Color = Colors.Green;
            _crosshairModel.Size = 20;
            _crosshairModel.Thickness = 2;
            _crosshairModel.Opacity = 1.0;

            // Call this immediately to try to draw the crosshair
            UpdateCrosshairPath();

            // Also force a redraw after a slight delay to ensure UI is ready
            Dispatcher.BeginInvoke(new Action(() => {
                _isInitialized = true;
                UpdateCrosshairPath();
            }), System.Windows.Threading.DispatcherPriority.Loaded);
        }

        private void UpdateCrosshair()
        {
            if (_crosshairModel == null)
            {
                _crosshairModel = new CrosshairModel();
            }

            _crosshairModel.Size = CrosshairSize;
            _crosshairModel.Thickness = CrosshairThickness;
            _crosshairModel.Opacity = CrosshairOpacity;
            _crosshairModel.X = CrosshairLeft;
            _crosshairModel.Y = CrosshairTop;

            UpdateCrosshairPath();
        }

        private void UpdateCrosshairPath()
        {
            // Debug check - remove this once it's working
            Console.WriteLine("UpdateCrosshairPath called");

            // Ensure model is initialized
            if (_crosshairModel == null)
            {
                _crosshairModel = new CrosshairModel();
                _crosshairModel.Color = Colors.Green; // Default color
            }

            // Debug check - this is necessary for first-time initialization
            if (CrosshairPreview == null)
            {
                Console.WriteLine("CrosshairPreview is null");
                return;
            }

            if (CrosshairPath == null)
            {
                Console.WriteLine("CrosshairPath is null");
                return;
            }

            // If preview has no size yet, set a default size
            if (CrosshairPreview.ActualWidth == 0 || CrosshairPreview.ActualHeight == 0)
            {
                // Use a default size if not yet rendered
                double centerX = 100 - 23.5; // Default width/2, shifted left by 23.5 pixels
                double centerY = 100; // Default height/2

                // Create geometry and draw crosshair using default center
                DrawCrosshairGeometry(centerX, centerY);
            }
            else
            {
                // Use actual size if available
                double centerX = (CrosshairPreview.ActualWidth / 2) - 23.5; // Shift left by 23.5 pixels
                double centerY = CrosshairPreview.ActualHeight / 2;

                // Create geometry and draw crosshair
                DrawCrosshairGeometry(centerX, centerY);
            }

            // Ensure path is visible
            CrosshairPath.Visibility = Visibility.Visible;
        }

        private void DrawCrosshairGeometry(double centerX, double centerY)
        {
            // Create a geometry for the crosshair
            var geometry = new StreamGeometry();

            using (StreamGeometryContext context = geometry.Open())
            {
                // Default Cross Style:
                double halfSize = _crosshairModel.Size / 2;
                double gap = 4; // Example gap, consider making this configurable

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

            // Update the path properties
            CrosshairPath.Data = geometry;
            CrosshairPath.StrokeThickness = _crosshairModel.Thickness;
            CrosshairPath.Opacity = _crosshairModel.Opacity;

            // Use the color from the model
            CrosshairPath.Stroke = new SolidColorBrush(_crosshairModel.Color);

            // Make sure path is visible and on top
            CrosshairPath.Visibility = Visibility.Visible;
            Panel.SetZIndex(CrosshairPath, 100);
        }

        // --- Event Handlers for Controls ---

        private void SizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_crosshairModel == null) return;
            CrosshairSize = e.NewValue; // Update the DependencyProperty which triggers UpdateCrosshair via AddValueChanged
            UpdateCrosshair(); // Explicitly update in case property change handler isn't working
        }

        private void ThicknessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_crosshairModel == null) return;
            CrosshairThickness = e.NewValue; // Update the DependencyProperty
            UpdateCrosshair(); // Explicitly update
        }

        private void OpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_crosshairModel == null) return;
            CrosshairOpacity = e.NewValue; // Update the DependencyProperty
            UpdateCrosshair(); // Explicitly update
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
                    var color = (Color)ColorConverter.ConvertFromString(hexColor);
                    // Update the model's color
                    if (_crosshairModel != null)
                    {
                        _crosshairModel.Color = color;
                    }
                    UpdateCrosshairPath(); // Redraw with the new model color
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error applying color swatch: {ex.Message}");
                    // Optionally show a message to the user
                }
            }
        }

        private void ApplyColor_Click(object sender, RoutedEventArgs e)
        {
            // This might be used if the popup had more complex controls (like RGB sliders)
            // Currently, color is applied directly on swatch click.
            ColorPickerPopup.IsOpen = false;
            UpdateCrosshairPath(); // Ensure redraw if color was changed in popup without immediate apply
        }

        private void CrosshairPreview_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Redraw when the preview area resizes
            UpdateCrosshairPath();
        }

        // --- Custom Style Dropdown Handlers ---

        private void StyleDropdownBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Toggle the style selection popup
            StylesPopup.IsOpen = !StylesPopup.IsOpen;
        }

        private void StyleOption_MouseEnter(object sender, MouseEventArgs e)
        {
            // Basic hover effect for style options
            if (sender is Border border)
            {
                // Consider using styles/triggers in XAML for this instead
                border.Background = new SolidColorBrush(Color.FromRgb(58, 84, 110)); // Example hover color
            }
        }

        private void StyleOption_MouseLeave(object sender, MouseEventArgs e)
        {
            // Remove hover effect
            if (sender is Border border)
            {
                // Consider using styles/triggers in XAML for this instead
                border.Background = Brushes.Transparent;
            }
        }

        private void StyleOption_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is string styleName)
            {
                // Update the selected style text/icon display
                SelectedStyleText.Text = styleName;
                UpdateSelectedStyleIcon(styleName);

                // Update the crosshair model's style property
                if (_crosshairModel != null)
                {
                    // Attempt to parse the style name string into the CrosshairStyle enum
                    if (Enum.TryParse<CrosshairStyle>(styleName, true, out var newStyle))
                    {
                        _crosshairModel.Style = newStyle;
                    }
                    else
                    {
                        // Handle cases where the tag might not match an enum value
                        Console.WriteLine($"Warning: Could not parse style '{styleName}'");
                        _crosshairModel.Style = CrosshairStyle.Default; // Fallback to default
                    }
                }

                // Redraw the main crosshair preview with the new style
                UpdateCrosshairPath();

                // Close the style selection popup
                StylesPopup.IsOpen = false;
            }
        }

        // Helper to update the small icon in the dropdown header based on selected style
        private void UpdateSelectedStyleIcon(string styleName)
        {
            if (SelectedStylePath == null) return;

            // Set geometry based on style name
            switch (styleName)
            {
                case "Standard":
                case "Cross":
                case "Default":
                    SelectedStylePath.Data = Geometry.Parse("M0,10 H20 M10,0 V20"); // Simple cross
                    SelectedStylePath.StrokeThickness = 1;
                    break;
                case "Dot":
                    SelectedStylePath.Data = new EllipseGeometry(new Point(10, 10), 3, 3);
                    SelectedStylePath.StrokeThickness = 1; // Small dot outline
                    break;
                case "Circle":
                    var circleGeometry = new EllipseGeometry(new Point(10, 10), 8, 8);
                    SelectedStylePath.Data = circleGeometry;
                    SelectedStylePath.StrokeThickness = 1; // Circle outline
                    break;
                case "Tactical":
                case "Classic":
                    SelectedStylePath.Data = Geometry.Parse("M10,0 V8 M10,12 V20 M0,10 H8 M12,10 H20"); // Classic style
                    SelectedStylePath.StrokeThickness = 1;
                    break;
                default:
                    SelectedStylePath.Data = Geometry.Parse("M0,10 H20 M10,0 V20"); // Default fallback
                    SelectedStylePath.StrokeThickness = 1;
                    break;
            }
        }


        // --- Initialization ---

        private void DesignerView_Loaded(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("DesignerView_Loaded event fired");

            if (!_isInitialized) // Prevent running multiple times if reloaded
            {
                // Initialize model properties from Dependency Properties
                if (_crosshairModel != null)
                {
                    _crosshairModel.Size = CrosshairSize;
                    _crosshairModel.Thickness = CrosshairThickness;
                    _crosshairModel.Opacity = CrosshairOpacity;
                    _crosshairModel.X = CrosshairLeft;
                    _crosshairModel.Y = CrosshairTop;
                    _crosshairModel.Color = Colors.Green; // Set a default color
                }

                // Register property change callbacks
                DependencyPropertyDescriptor.FromProperty(CrosshairSizeProperty, typeof(DesignerView))
                    .AddValueChanged(this, (s, args) => UpdateCrosshair());

                DependencyPropertyDescriptor.FromProperty(CrosshairThicknessProperty, typeof(DesignerView))
                    .AddValueChanged(this, (s, args) => UpdateCrosshair());

                DependencyPropertyDescriptor.FromProperty(CrosshairOpacityProperty, typeof(DesignerView))
                    .AddValueChanged(this, (s, args) => UpdateCrosshair());

                _isInitialized = true;

                // Explicitly update the crosshair
                UpdateCrosshairPath();

                // Schedule another update after everything is loaded
                Dispatcher.BeginInvoke(new Action(() => {
                    UpdateCrosshairPath();
                }), System.Windows.Threading.DispatcherPriority.Render);
            }
            else
            {
                // If already initialized, ensure redraw
                UpdateCrosshairPath();
            }
        }
    }
}