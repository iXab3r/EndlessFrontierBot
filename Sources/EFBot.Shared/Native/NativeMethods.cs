using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace EFBot.Shared.Native
{
    internal static class NativeMethods
    {
        /// <summary>
        ///     The GetForegroundWindow function returns a handle to the foreground window.
        /// </summary>
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
        
        [DllImport("user32.dll")]
        public static extern bool PrintWindow(IntPtr hWnd, IntPtr hdcBlt, int nFlags);

        /// &lt;summary&gt;
        /// Get a windows client rectangle in a .NET structure
        /// &lt;/summary&gt;
        /// &lt;param name="hwnd"&gt;The window handle to look up&lt;/param&gt;
        /// &lt;returns&gt;The rectangle&lt;/returns&gt;
        public static Rectangle GetClientRect(IntPtr hwnd)
        {
            var rect = new RECT();
            GetClientRect(hwnd, out rect);
            return rect.AsRectangle;
        }

        /// &lt;summary&gt;
        /// Get a windows rectangle in a .NET structure
        /// &lt;/summary&gt;
        /// &lt;param name="hwnd"&gt;The window handle to look up&lt;/param&gt;
        /// &lt;returns&gt;The rectangle&lt;/returns&gt;
        public static Rectangle GetWindowRect(IntPtr hwnd)
        {
            var rect = new RECT();
            GetWindowRect(hwnd, out rect);
            return rect.AsRectangle;
        }

        internal static Rectangle GetAbsoluteClientRect(IntPtr hWnd)
        {
            var windowRect = GetWindowRect(hWnd);
            var clientRect = GetClientRect(hWnd);

            // This gives us the width of the left, right and bottom chrome - we can then determine the top height
            var chromeWidth = (windowRect.Width - clientRect.Width) / 2;

            return new Rectangle(new Point(windowRect.X + chromeWidth, windowRect.Y + (windowRect.Height - clientRect.Height - chromeWidth)), clientRect.Size);
        }
    }
}