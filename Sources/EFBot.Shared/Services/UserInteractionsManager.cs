using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using WindowsInput;
using WindowsInput.Native;

namespace EFBot.Shared.Services
{
    internal sealed class UserInteractionsManager 
    {
        private TimeSpan actionDelay;
        private readonly IUserInputBlocker userInputBlocker;

        private readonly IInputSimulator inputSimulator;

        public UserInteractionsManager(
                IInputSimulator inputSimulator,
                IUserInputBlocker userInputBlocker)
        {
            this.inputSimulator = inputSimulator;
            this.userInputBlocker = userInputBlocker;
            actionDelay = TimeSpan.FromMilliseconds(500);
        }

        public void SendControlLeftClick()
        {
            using (userInputBlocker.Block())
            {
                Delay();
                inputSimulator.Keyboard.KeyDown(VirtualKeyCode.CONTROL);
                Delay();
                inputSimulator.Mouse.LeftButtonClick();
                Delay();
                inputSimulator.Keyboard.KeyUp(VirtualKeyCode.CONTROL);
            }
        }

        public void SendClick()
        {
            using (userInputBlocker.Block())
            {
                Delay();
                inputSimulator.Mouse.LeftButtonClick();
            }
        }

        public void SendKey(VirtualKeyCode keyCode)
        {
            inputSimulator.Keyboard.KeyPress(keyCode);
        }

        public void MoveMouseTo(Point location)
        {
            Delay();
            NativeMethods.SetCursorPos(location);
        }

        public void Delay(TimeSpan delay)
        {
            Thread.Sleep(delay);
        }

        public void Delay()
        {
            Delay(actionDelay);
        }

        private static class NativeMethods
        {
            [DllImport("user32.dll", CharSet = CharSet.Unicode)]
            static extern bool SetCursorPos(uint x, uint y);

            public static void SetCursorPos(Point location)
            {
                SetCursorPos((uint)location.X, (uint)location.Y);
            }
        }
    }
}