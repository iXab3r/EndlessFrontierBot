﻿using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using DynamicData;
using DynamicData.Binding;
using EFBot.Shared;
using EFBot.Shared.Scaffolding;
using EFBot.Shared.Services;
using ReactiveUI;

namespace EFBot.Launcher.ViewModels
{
    internal sealed class MainWindowViewModel : DisposableReactiveObject
    {
        private readonly GameImageSource gameSource;
        private readonly BotStrategy strategy;
        private readonly ReadOnlyObservableCollection<BitmapSource> botVision;

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

            Controller.BotVision
                .ToObservableChangeSet()
                .Transform(x => x.Image?.Bitmap?.ToBitmapSource())
                .Bind(out botVision)
                .Subscribe()
                .AddTo(Anchors);
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