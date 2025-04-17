using System.Windows.Media;

namespace PinPoint.UI.Models
{
    public enum CrosshairStyle
    {
        Cross = 0,
        Dot = 1,
        Circle = 2
    }
    
    public class CrosshairModel
    {
        public Color Color { get; set; }
        public double Size { get; set; }
        public double Thickness { get; set; }
        public double Opacity { get; set; }
        public CrosshairStyle Style { get; set; }
    }
}
