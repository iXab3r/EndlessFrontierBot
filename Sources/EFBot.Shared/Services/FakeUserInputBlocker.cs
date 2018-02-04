using System;
using System.Reactive.Disposables;

namespace EFBot.Shared.Services {
    internal sealed class FakeUserInputBlocker : IUserInputBlocker
    {
        public IDisposable Block()
        {
            return Disposable.Empty;
        }
    }
}