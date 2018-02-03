using System;
using System.ComponentModel;
using System.Linq.Expressions;
using JetBrains.Annotations;
using ReactiveUI;

namespace EFBot.Shared.Scaffolding
{
    public static class ReactiveObjectExtensions
    {
        public static IDisposable BindPropertyTo<TSource, TTarget, TSourceProperty, TTargetProperty>(
            [NotNull] this TTarget instance,
            [NotNull] Expression<Func<TTarget, TTargetProperty>> instancePropertyExtractor,
            [NotNull] TSource source,
            [NotNull] Expression<Func<TSource, TSourceProperty>> sourcePropertyExtractor)
              where TSource : INotifyPropertyChanged
              where TTarget : IReactiveObject
        {
            var instancePropertyName = new Lazy<string>(() => Reflection.ExpressionToPropertyNames(instancePropertyExtractor.Body));

            return source
                .WhenAnyValue(sourcePropertyExtractor)
                .Subscribe(x => instance.RaisePropertyChanged(instancePropertyName.Value));
        }

        public static IDisposable LinkObjectProperties<TSource, TSourceProperty, TTargetProperty>(
            [NotNull] this TSource instance,
            [NotNull] Expression<Func<TSource, TTargetProperty>> instancePropertyExtractor,
            [NotNull] Expression<Func<TSource, TSourceProperty>> sourcePropertyExtractor)
            where TSource : IReactiveObject
        {
            var instancePropertyName = new Lazy<string>(() => Reflection.ExpressionToPropertyNames(instancePropertyExtractor.Body));

            return instance
                .WhenAnyValue(sourcePropertyExtractor)
                .Subscribe(x => instance.RaisePropertyChanged(instancePropertyName.Value));
        }
    }
}
