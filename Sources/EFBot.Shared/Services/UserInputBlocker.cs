using System;
using System.Reactive.Disposables;
using System.Runtime.InteropServices;
using EFBot.Shared.Services;

namespace EFBot.Shared {
    internal sealed class UserInputBlocker : IUserInputBlocker
    {
        public IDisposable Block()
        {
            BlockInput(true);
            return Disposable.Create(() => BlockInput(false));
        }

        [DllImport("user32.dll")]
        static extern bool BlockInput(bool blockInput);
    }
    
    internal sealed class FakeUserInputBlocker : IUserInputBlocker
    {
        public IDisposable Block()
        {
            return Disposable.Empty;
        }
    }
}