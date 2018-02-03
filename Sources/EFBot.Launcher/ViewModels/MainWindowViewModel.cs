using System;
using System.Reactive.Linq;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using EFBot.Shared;
using EFBot.Shared.Scaffolding;
using EFBot.Shared.Services;
using ReactiveUI;

namespace EFBot.Launcher.ViewModels
{
    internal sealed class MainWindowViewModel : ReactiveObject
    {
        private readonly GameImageSource gameSource;
        private readonly BotStrategy strategy;

        public MainWindowViewModel()
        {
            gameSource = new GameImageSource();
            
            gameSource.WhenAnyValue(x => x.Source)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(img => this.RaisePropertyChanged(nameof(ActiveImage)));

            Controller = new BotController(gameSource);

            Controller.WhenAnyValue(x => x.BotImage)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => this.RaisePropertyChanged(nameof(BotImage)));

            strategy = new BotStrategy(gameSource, Controller);
        }

        public BitmapSource ActiveImage => gameSource.Source?.ToBitmapSource();
        public BitmapSource BotImage => Controller.BotImage?.ToBitmapSource();

        public BotController Controller { get; }
    }
}