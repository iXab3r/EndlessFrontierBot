using System;
using System.Drawing;

namespace EFBot.Shared.Native
{
    internal interface IWindowCaptureSource
    {
        Bitmap Capture(IntPtr hWnd);
    }
}