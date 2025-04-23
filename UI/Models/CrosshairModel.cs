using System.Windows.Media;

namespace PinPoint.UI.Models
{
    public class CrosshairModel
    {
        public Color Color { get; set; } = Colors.Green;
        public double Size { get; set; } = 20;
        public double Thickness { get; set; } = 2;
        public double Opacity { get; set; } = 1.0;
        public CrosshairStyle Style { get; set; } = CrosshairStyle.Default;
        
        // Add the missing X and Y properties
        public double X { get; set; } = 0;
        public double Y { get; set; } = 0;
    }
    
    public enum CrosshairStyle
    {
        Default,
        Dot,
        Circle,
        Cross,
        Classic // Add the missing Classic style
    }
} 