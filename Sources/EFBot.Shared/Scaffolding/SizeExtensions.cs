using System.Drawing;

namespace EFBot.Shared.Scaffolding
{
    internal static class SizeExtensions
    {
        public static Size Scale(this Size size, double scaleFactor)
        {
            return new Size((int)(size.Width * scaleFactor), (int)(size.Height * scaleFactor));
            
        }
    }
}