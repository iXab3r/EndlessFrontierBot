using System;
using System.Collections.ObjectModel;
using DynamicData;
using EFBot.Shared.Scaffolding;
using EFBot.Shared.Services;
using Emgu.CV;
using Emgu.CV.Structure;

namespace EFBot.Shared.GameLogic.ImageReaders {
    internal abstract class ImageReaderBase : DisposableReactiveObject
    {
        public string Error { get; protected set;}
        
        public string Text { get; protected set;}

        public ReadOnlyObservableCollection<RecognitionResult> Results
        {
            get { return readonlyResults; }
        }

        public abstract void Refresh(Image<Rgb, byte> inputImage);
        
        protected readonly SourceList<RecognitionResult> RecognitionResults = new SourceList<RecognitionResult>();
        
        private readonly ReadOnlyObservableCollection<RecognitionResult> readonlyResults;

        protected ImageReaderBase()
        {
            RecognitionResults
                .Connect()
                .Bind(out readonlyResults)
                .Subscribe()
                .AddTo(Anchors);
        }

        public virtual void Clear()
        {
            Text = null;
            Error = null;
            RecognitionResults.Clear();
        }
    }
}