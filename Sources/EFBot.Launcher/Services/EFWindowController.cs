using System;
using System.Runtime.InteropServices;
using EFBot.Shared.Scaffolding;
using ReactiveUI;

namespace EFBot.Launcher.Services
{
    internal sealed class EFWindowController : DisposableReactiveObject, IDisposable
    {
        private IntPtr windowHandle;
        public IntPtr WindowHandle
        {
            get { return windowHandle; }
            set { this.RaiseAndSetIfChanged(ref windowHandle, value); }
        }
        
        public void SendMouseClick(int x, int y)
        {
            PostMessage(WindowHandle, WM_LBUTTONDOWN, 1, MakeLParam(x, y));
        }
        
        public int MakeLParam(int LoWord, int HiWord)
        {
            return (int)((HiWord << 16) | (LoWord & 0xFFFF));
        }

            
        [DllImport("user32.dll")]
        static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        const int WM_LBUTTONDOWN = 0x0201;
        const int WM_LBUTTONUP = 0x0202;
    }
}