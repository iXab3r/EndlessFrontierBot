using System;
using DynamicData;

namespace EFBot.Shared.Scaffolding {
    public static class ChangeSetExtensions
    {
        public static ISourceList<T> ToSourceList<T>(this IObservable<IChangeSet<T>> source)
        {
            return new SourceList<T>(source);
        }
    }
}