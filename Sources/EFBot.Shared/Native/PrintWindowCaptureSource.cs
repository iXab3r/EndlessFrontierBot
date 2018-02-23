using System;
using System.Drawing;
using System.Drawing.Imaging;
using EFBot.Shared.Scaffolding;

namespace EFBot.Shared.Native
{
    public sealed class PrintWindowCaptureSource : IWindowCaptureSource
    {
        public Bitmap Capture(IntPtr hwnd)
        {
            if (hwnd == IntPtr.Zero)
            {
                return null;
            }

            try
            {
                var region = NativeMethods.GetAbsoluteClientRect(hwnd);
                if (region.IsEmpty || region.Width == 0 || region.Height == 0)
                {
                    return null;
                }
                
                var bmp = new Bitmap(region.Width, region.Height, PixelFormat.Format32bppArgb);
                var gfxBmp = Graphics.FromImage(bmp);
                var hdcBitmap = gfxBmp.GetHdc();

                NativeMethods.PrintWindow(hwnd, hdcBitmap, 0);

                gfxBmp.ReleaseHdc(hdcBitmap);
                gfxBmp.Dispose();

                return bmp;
            }
            catch (Exception e)
            {
                Log.HandleException(e);
            }

            return null;
        }
    }
}