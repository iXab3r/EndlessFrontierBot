﻿using System;
using System.Reactive.Disposables;
using System.Runtime.InteropServices;

namespace EFBot.Shared.Services {
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
}