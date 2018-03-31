using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using DynamicData;
using DynamicData.Binding;
using EFBot.Launcher.Services;
using EFBot.Shared.GameLogic;
using EFBot.Shared.Prism;
using EFBot.Shared.Scaffolding;
using EFBot.Shared.Services;
using ReactiveUI;

namespace EFBot.Launcher.ViewModels
{
    internal sealed class MainWindowViewModel : DisposableReactiveObject
    {
        private readonly ReadOnlyObservableCollection<RecognitionResult> botVision;

        private bool previewActiveImage = true;

        private bool previewBotImage = true;

        private bool showBotDebugData;

        private double sourcePeriodInMilliseconds = 500;

        public MainWindowViewModel(
            IGameImageSource gameSource,
            IFactory<IBotVisionModel, IGameImageSource> botVisionModelFactory,
            IFactory<IBotStrategy, IGameImageSource, IBotVisionModel> botStrategyFactory)
        {
            GameSource = gameSource;
            Controller = botVisionModelFactory.Create(gameSource);

            this.WhenAnyValue(x => x.SourcePeriodInMilliseconds)
                .Subscribe(x => GameSource.Period = TimeSpan.FromMilliseconds(SourcePeriodInMilliseconds))
                .AddTo(Anchors);

            gameSource.WhenAnyValue(x => x.Source)
                .Where(x => PreviewActiveImage)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(img => this.RaisePropertyChanged(nameof(ActiveImage)));

            Controller.WhenAnyValue(x => x.BotImage)
                .Where(x => PreviewBotImage)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => this.RaisePropertyChanged(nameof(BotImage)));

            Controller.BotVision
                .ToObservableChangeSet()
                .Filter(x => ShowBotDebugData)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out botVision)
                .Subscribe()
                .AddTo(Anchors);

            ActiveStrategy = botStrategyFactory.Create(gameSource, Controller);

            SendClickCommand = CommandWrapper.Create(
                ReactiveCommand.Create(
                    () =>
                    {
                        var controller = new EFWindowController();
                        controller.WindowHandle = gameSource.WindowHandle;
                        controller.SendMouseClick(350, 460);
                    }));
        }

        public BitmapSource ActiveImage => GameSource.Source?.ToBitmapSource();

        public bool PreviewActiveImage
        {
            get => previewActiveImage;
            set => this.RaiseAndSetIfChanged(ref previewActiveImage, value);
        }

        public bool PreviewBotImage
        {
            get => previewBotImage;
            set => this.RaiseAndSetIfChanged(ref previewBotImage, value);
        }

        public bool ShowBotDebugData
        {
            get => showBotDebugData;
            set => this.RaiseAndSetIfChanged(ref showBotDebugData, value);
        }

        public BitmapSource BotImage => Controller.BotImage?.Bitmap?.ToBitmapSource();

        public ReadOnlyObservableCollection<RecognitionResult> BotVision => botVision;

        public IGameImageSource GameSource { get; }

        public IBotVisionModel Controller { get; }

        public IBotStrategy ActiveStrategy { get; }
        
        public ICommand SendClickCommand { get; }

        public double SourcePeriodInMilliseconds
        {
            get => sourcePeriodInMilliseconds;
            set => this.RaiseAndSetIfChanged(ref sourcePeriodInMilliseconds, value);
        }
    }
}