using System.Windows.Media;

namespace FancyCandles.Model
{
    public static class Config
    {
        public static Brush UpBrush => new SolidColorBrush(Color.FromRgb(131, 194, 118));
        public static Brush DownBrush => new SolidColorBrush(Color.FromRgb(184, 41, 32));
    }
}
