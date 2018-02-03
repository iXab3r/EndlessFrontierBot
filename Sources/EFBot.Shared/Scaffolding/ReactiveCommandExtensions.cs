using System;
using ReactiveUI.Legacy;

namespace EFBot.Shared.Scaffolding
{
    public static class ReactiveCommandExtensions
    {
        public static ReactiveCommand<T> SubscribeToExceptions<T>(
            this ReactiveCommand<T> instance, 
            Action<Exception> onNextError,
            Action<IDisposable> anchorProcessor)
        {
            var anchor = instance.ThrownExceptions.Subscribe(onNextError);
            anchorProcessor(anchor);
            return instance;
        }
    }
}
