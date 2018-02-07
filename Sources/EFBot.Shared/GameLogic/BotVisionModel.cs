using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using DynamicData;
using DynamicData.Binding;
using EFBot.Shared.GameLogic.ImageReaders;
using EFBot.Shared.Prism;
using EFBot.Shared.Scaffolding;
using EFBot.Shared.Services;
using Emgu.CV;
using Emgu.CV.Structure;
using ReactiveUI;

namespace EFBot.Shared.GameLogic {
    internal sealed class BotVisionModel : DisposableReactiveObject, IBotVisionModel
    {
        private IImage botImage;
        private UnitShopUnit[] availableUnits;
        private string text;
        private TimeSpan? timeLeftTillRefresh;
        private string error;

        private readonly ReadOnlyObservableCollection<RecognitionResult> readonlyBotVision;

        private readonly UnitRefreshTimeReader unitRefreshTimeReader;
        private readonly UnitsListReader unitListReader;

        public BotVisionModel(
            IGameImageSource gameSource,
            IRecognitionEngine recognitionEngine,
            IFactory<UnitRefreshTimeReader, IRecognitionEngine, IGameImageSource> unitRefreshReaderFactory,
            IFactory<UnitsListReader, IRecognitionEngine, IGameImageSource> unitListReaderFactory)
        {
            unitListReader = unitListReaderFactory.Create(recognitionEngine, gameSource);
            unitRefreshTimeReader = unitRefreshReaderFactory.Create(recognitionEngine, gameSource);
            
            gameSource
                .WhenAnyValue(x => x.Source)
                .Subscribe(Recalculate)
                .AddTo(Anchors);
            
            var lists = new SourceList<ISourceList<RecognitionResult>>();
            lists.Add(unitListReader.Results.ToObservableChangeSet().ToSourceList());
            lists.Add(unitRefreshTimeReader.Results.ToObservableChangeSet().ToSourceList());
            lists.Or()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out readonlyBotVision)
                .Subscribe()
                .AddTo(Anchors);
        }

        public IImage BotImage
        {
            get { return botImage; }
            set { this.RaiseAndSetIfChanged(ref botImage, value); }
        }

        public UnitShopUnit[] AvailableUnits
        {
            get { return availableUnits; }
            set { this.RaiseAndSetIfChanged(ref availableUnits, value); }
        }

        public TimeSpan? TimeLeftTillRefresh
        {
            get { return timeLeftTillRefresh; }
            set { this.RaiseAndSetIfChanged(ref timeLeftTillRefresh, value); }
        }

        public string Text
        {
            get { return text; }
            set { this.RaiseAndSetIfChanged(ref text, value); }
        }

        public string Error
        {
            get { return error; }
            set { this.RaiseAndSetIfChanged(ref error, value); }
        }

        public ReadOnlyObservableCollection<RecognitionResult> BotVision
        {
            get { return readonlyBotVision; }
        }

        private void Recalculate(Bitmap activeImage)
        {
            var readersToRefresh = new ImageReaderBase[] { unitListReader, unitRefreshTimeReader };
            
            if (activeImage == null)
            {
                BotImage = null;
                Text = "No image";
                Error = null;
                readersToRefresh.ForEach(x => x.Clear());
                return;
            }

            var resultingImage = new Image<Rgb, byte>(activeImage);
            readersToRefresh.ForEach(x => x.Refresh(resultingImage));

            TimeLeftTillRefresh = unitRefreshTimeReader.Entity;
            AvailableUnits = unitListReader.Entity;

            resultingImage.ROI = Rectangle.Empty;
            BotImage = resultingImage;
            Text = readersToRefresh.Select(x => x.Text).DumpToTable("\n");
            Error = readersToRefresh.Select(x => x.Error).DumpToTable("\n");
        }
    }
}