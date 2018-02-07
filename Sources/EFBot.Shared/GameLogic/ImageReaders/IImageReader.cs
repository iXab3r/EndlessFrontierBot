using EFBot.Shared.Services;

namespace EFBot.Shared.GameLogic.ImageReaders {
    internal abstract class ImageReaderBase<TEntity> : ImageReaderBase
    {
        protected readonly IRecognitionEngine RecognitionEngine;
        protected readonly IGameImageSource ImageSource;
        
        public ImageReaderBase(
            IRecognitionEngine recognitionEngine,
            IGameImageSource imgSource)
        {
            this.RecognitionEngine = recognitionEngine;
            this.ImageSource = imgSource;
        }
        
        public TEntity Entity { get; protected set; }

        public override void Clear()
        {
            base.Clear();
            Entity = default(TEntity);
        }
    }
}