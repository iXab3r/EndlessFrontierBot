using System.Drawing;

namespace EFBot.Shared.Scaffolding
{
    internal static class RectangleExtensions
    {
        public static Rectangle ToRectangle(this RectangleF rect)
        {
            return new Rectangle((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height);
        }

        public static RectangleF OffsetBy(this RectangleF rect, float x, float y)
        {
            return new RectangleF(rect.X + x, rect.Y + y, rect.Width, rect.Height);
        }
        
        public static RectangleF Multiply(this RectangleF rect1, RectangleF rect2)
        {
            return new RectangleF(
                x: rect1.X * rect2.Width,
                y: rect1.Y * rect2.Height,
                width: rect1.Width * rect2.Width,
                height:rect1.Height * rect2.Height);
        }
    }
}