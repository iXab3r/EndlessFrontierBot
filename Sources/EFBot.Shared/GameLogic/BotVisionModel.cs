using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
using MathNet.Numerics.Statistics;
using ReactiveUI;
using SlimDX.Direct3D9;

namespace EFBot.Shared.GameLogic
{
    internal sealed class BotVisionModel : DisposableReactiveObject, IBotVisionModel
    {
        private readonly GoldChestReader goldChestReader;

        private readonly ReadOnlyObservableCollection<RecognitionResult> readonlyBotVision;
        private readonly UnitAcquisitionConfirmationDialogReader unitAcquisitionDialogReader;
        private readonly UnitsListReader unitListReader;

        private readonly UnitRefreshTimeReader unitRefreshTimeReader;
        private UnitShopUnit[] availableUnits;
        private IImage botImage;
        private string error;

        private string text;
        private TimeSpan? timeLeftTillRefresh;
        
        private readonly MovingStatistics fpsStats = new MovingStatistics(10);

        public BotVisionModel(
            IGameImageSource gameSource,
            IRecognitionEngine recognitionEngine,
            IFactory<UnitAcquisitionConfirmationDialogReader, IRecognitionEngine, IGameImageSource> unitAcquisitionDialogReaderFactory,
            IFactory<UnitRefreshTimeReader, IRecognitionEngine, IGameImageSource> unitRefreshReaderFactory,
            IFactory<GoldChestReader, IRecognitionEngine, IGameImageSource> goldChestReaderFactory,
            IFactory<UnitsListReader, IRecognitionEngine, IGameImageSource> unitListReaderFactory)
        {
            unitAcquisitionDialogReader = unitAcquisitionDialogReaderFactory.Create(recognitionEngine, gameSource);
            unitListReader = unitListReaderFactory.Create(recognitionEngine, gameSource);
            unitRefreshTimeReader = unitRefreshReaderFactory.Create(recognitionEngine, gameSource);
            goldChestReader = goldChestReaderFactory.Create(recognitionEngine, gameSource);

            gameSource
                .WhenAnyValue(x => x.Source)
                .Select(x => x != null ? new Image<Rgb, byte>(x) : null)
                .Subscribe(Recalculate, Log.HandleUiException)
                .AddTo(Anchors);

            var lists = new SourceList<ISourceList<RecognitionResult>>();
            lists.Add(unitAcquisitionDialogReader.Results.ToObservableChangeSet().ToSourceList());
            lists.Add(unitListReader.Results.ToObservableChangeSet().ToSourceList());
            lists.Add(unitRefreshTimeReader.Results.ToObservableChangeSet().ToSourceList());
            lists.Or()
                .Bind(out readonlyBotVision)
                .Subscribe()
                .AddTo(Anchors);
        }

        public double FramesPerSecond => fpsStats.Mean;

        public IImage BotImage
        {
            get => botImage;
            set => this.RaiseAndSetIfChanged(ref botImage, value);
        }

        public UnitShopUnit[] AvailableUnits
        {
            get => availableUnits;
            set => this.RaiseAndSetIfChanged(ref availableUnits, value);
        }

        public TimeSpan? TimeLeftTillRefresh
        {
            get => timeLeftTillRefresh;
            set => this.RaiseAndSetIfChanged(ref timeLeftTillRefresh, value);
        }

        public string Text
        {
            get => text;
            set => this.RaiseAndSetIfChanged(ref text, value);
        }

        public string Error
        {
            get => error;
            set => this.RaiseAndSetIfChanged(ref error, value);
        }

        public ReadOnlyObservableCollection<RecognitionResult> BotVision => readonlyBotVision;

        private DateTime lastFrameTime;

        private void Recalculate(Image<Rgb, byte> activeImage)
        {
            var now = DateTime.Now;
            if (lastFrameTime != DateTime.MinValue)
            {
                var delta = now - lastFrameTime;
                
                var fps = 1000 / delta.TotalMilliseconds;
                fpsStats.Push(fps);

                this.RaisePropertyChanged(nameof(FramesPerSecond));
            }
            
            lastFrameTime = now;

            var readersToRefresh = new ImageReaderBase[]
            {
                unitAcquisitionDialogReader, goldChestReader, unitListReader, unitRefreshTimeReader
            };

            if (activeImage == null)
            {
                BotImage = null;
                Text = "No image";
                Error = null;
                readersToRefresh.ForEach(x => x.Clear());
                return;
            }

            var debugText = new StringBuilder();
            var timingsText = new List<Tuple<string, TimeSpan>>();

            var all = new OperationTimer(x => timingsText.Add(new Tuple<string, TimeSpan>($"All: {x.TotalMilliseconds}ms", x)));
            using (all)
            {
                foreach (var imageReaderBase in readersToRefresh)
                {
                    var readerName = imageReaderBase.GetType().Name;
                    using (new OperationTimer(x => timingsText.Add(new Tuple<string, TimeSpan>($"{readerName}: {x.TotalMilliseconds}ms", x))))
                    {
                        imageReaderBase.Refresh(activeImage);
                        activeImage.ROI = Rectangle.Empty;
                    }

                    debugText.Append($"{readerName}\n{imageReaderBase.Text}\n");
                }
            }

            var maxFps = 1000f / all.Elapsed.TotalMilliseconds;

            TimeLeftTillRefresh = unitRefreshTimeReader.Entity;
            AvailableUnits = unitListReader.Entity;

            BotImage = activeImage;

            Text = $"Time elapsed: {all.Elapsed.TotalMilliseconds:F0}ms (max fps {maxFps:F1})\n\t{timingsText.Select(x => $"{x.Item1}").DumpToTable("\n\t")}\n\n{debugText}";
            Error = readersToRefresh.Select(x => x.Error).DumpToTable("\n");
        }

        private class OperationTimer : IDisposable
        {
            private readonly Stopwatch sw;

            private readonly Action<TimeSpan> endAction;

            public OperationTimer(Action<TimeSpan> endAction)
            {
                this.endAction = endAction;
                sw = Stopwatch.StartNew();
            }

            public TimeSpan Elapsed => sw.Elapsed;

            public void Dispose()
            {
                sw.Stop();
                endAction(sw.Elapsed);
            }
        }
    }
}