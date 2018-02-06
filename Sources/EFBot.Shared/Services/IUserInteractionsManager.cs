using System;
using System.Drawing;
using WindowsInput.Native;
using EFBot.Shared.Scaffolding;

namespace EFBot.Shared.Services {
    internal interface IUserInteractionsManager : IDisposableReactiveObject {
        void SendControlLeftClick();
        void SendClick();
        void SendKey(VirtualKeyCode keyCode);
        void MoveMouseTo(Point location);
        void Delay(TimeSpan delay);
        void Delay();
    }
}