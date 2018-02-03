using System;
using System.Reactive.Disposables;

namespace EFBot.Shared.Scaffolding
{
    public static class DisposableExtensions
    {
        public static T AssignTo<T>(this T instance, SerialDisposable anchor) where T : IDisposable
        {
            anchor.Disposable = instance;
            return instance;
        }
    }
}
