using System;
using EFBot.Shared.Scaffolding;
using EFBot.Shared.Services;
using ReactiveUI;

namespace EFBot.Shared.GameLogic
{
    internal sealed class EFUnitScreenReader : DisposableReactiveObject, IDisposableReactiveObject
    {
        private readonly IRecognitionEngine recognitionEngine;
        
        public EFUnitScreenReader(
            IGameImageSource gameSource,
            IRecognitionEngine recognitionEngine)
        {
            this.recognitionEngine = recognitionEngine;

            gameSource.WhenAnyValue(x => x.Source)
                .Subscribe(Recalculate)
                .AddTo(Anchors);
        }
        
        private void Recalculate()
        {
            
        }
    }
}