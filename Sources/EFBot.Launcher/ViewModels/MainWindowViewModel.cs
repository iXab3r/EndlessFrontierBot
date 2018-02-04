using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using DynamicData;
using DynamicData.Binding;
using EFBot.Shared;
using EFBot.Shared.GameLogic;
using EFBot.Shared.Prism;
using EFBot.Shared.Scaffolding;
using EFBot.Shared.Services;
using ReactiveUI;
using Unity;

namespace EFBot.Launcher.ViewModels
{
    internal sealed class MainWindowViewModel : DisposableReactiveObject
    {
        private readonly IGameImageSource gameSource;
        private readonly ReadOnlyObservableCollection<BitmapSource> botVision;

        public MainWindowViewModel(
                IGameImageSource gameSource,
                IFactory<BotController, IGameImageSource> botControllerFactory,
                IFactory<IBotStrategy, IGameImageSource, BotController> botStrategyFactory)
        {
            this.gameSource = gameSource;
            
            gameSource.WhenAnyValue(x => x.Source)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(img => this.RaisePropertyChanged(nameof(ActiveImage)));

            Controller = botControllerFactory.Create(gameSource);

            Controller.WhenAnyValue(x => x.BotImage)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => this.RaisePropertyChanged(nameof(BotImage)));

            Controller.BotVision
                .ToObservableChangeSet()
                .Transform(x => x.Image?.Bitmap?.ToBitmapSource())
                .Bind(out botVision)
                .Subscribe()
                .AddTo(Anchors);
            
            botStrategyFactory.Create(gameSource, Controller);
        }

        public BitmapSource ActiveImage => gameSource.Source?.ToBitmapSource();
        
        public BitmapSource BotImage => Controller.BotImage?.Bitmap?.ToBitmapSource();

        public ReadOnlyObservableCollection<BitmapSource> BotVision
        {
            get { return botVision; }
        }

        public BotController Controller { get; }
    }
}