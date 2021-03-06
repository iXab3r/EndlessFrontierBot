﻿using System.Reactive.Disposables;
using ReactiveUI;

namespace EFBot.Shared.Scaffolding
{
    public abstract class DisposableReactiveObject : ReactiveObject, IDisposableReactiveObject
    {
        public CompositeDisposable Anchors { get; } = new CompositeDisposable();

        public virtual void Dispose()
        {
            Anchors.Dispose();
        }
    }
}