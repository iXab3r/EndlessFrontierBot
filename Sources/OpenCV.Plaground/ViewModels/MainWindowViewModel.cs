using System;
using System.Diagnostics;
using System.IO;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using EFBot.Shared.Scaffolding;
using Emgu.CV;
using Emgu.CV.Structure;
using OpenCV.Plaground.Models;
using ReactiveUI;

namespace OpenCV.Plaground.ViewModels
{
    internal sealed class MainWindowViewModel : DisposableReactiveObject
    {
        private Image<Rgb,byte> loadedImage;
        private Image<Rgb,byte> processedImage;

        public MainWindowViewModel()
        {
            this.WhenAnyValue(x => x.LoadedImage)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(() => this.RaisePropertyChanged(nameof(LoadedBitmap)))
                .AddTo(Anchors);
            
            this.WhenAnyValue(x => x.ProcessedImage)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(() => this.RaisePropertyChanged(nameof(ProcessedBitmap)))
                .AddTo(Anchors);

            ResetImageCommand = CommandWrapper.Create(ResetImageCommandExecuted);
            
            ApplyNotCommand = CommandWrapper.Create(
                () => ExecuteImageOperation(x => new NegateImageOperation().Process(x)));
            CannyCommand = CommandWrapper.Create<object[]>(
                args => ExecuteImageOperation(x => new CannyImageOperation().Process(x, Convert.ToDouble(args[0]), Convert.ToDouble(args[1]))));
            GaussianBlurCommand = CommandWrapper.Create<double>(
                args => ExecuteImageOperation(x => new GaussianBlueImageOperation().Process(x, Convert.ToInt32(args))));
            FindContoursCommand = CommandWrapper.Create(
                () => ExecuteImageOperation(x => new FindContoursImageOperation().Process(x)));
            
            ImageProvider = new ImageProviderViewModel();
            ImageProvider.WhenAnyValue(x => x.SelectedImage)
                .Where(x => x != null)
                .Subscribe(x => LoadedImage = new Image<Rgb, byte>(x.ToBitmap()))
                .AddTo(Anchors);

            this.WhenAnyValue(x => x.LoadedImage)
                .Subscribe(x => ResetImageCommandExecuted())
                .AddTo(Anchors);
        }
        
        public ImageProviderViewModel ImageProvider { get; }

        public BitmapSource LoadedBitmap => LoadedImage?.Bitmap?.ToBitmapSource();
        
        public BitmapSource ProcessedBitmap => ProcessedImage?.Bitmap?.ToBitmapSource();

        public Image<Rgb,byte> LoadedImage
        {
            get { return loadedImage; }
            set { this.RaiseAndSetIfChanged(ref loadedImage, value); }
        }

        public Image<Rgb,byte> ProcessedImage
        {
            get { return processedImage; }
            set { this.RaiseAndSetIfChanged(ref processedImage, value); }
        }
        
        public ICommand ResetImageCommand { get; }
        public ICommand ApplyNotCommand { get; }
        public ICommand CannyCommand { get; }
        public ICommand FindContoursCommand { get; }
        public ICommand GaussianBlurCommand { get; }
        
        public ReactiveList<string> ExecutionLog { get; } = new ReactiveList<string>();

        private async Task ExecuteImageOperation(Func<Image<Rgb, byte>, OperationResult> executor)
        {
            ExecutionLog.Clear();
            
            var image = ProcessedImage.Clone();
            ExecutionLog.Add($"Initializing operation...");

            var timer = Stopwatch.StartNew();
            var result = await Task.Run(() => executor(image));
            timer.Stop();

            result.Log.ForEach(ExecutionLog.Add);
            ExecutionLog.Add($"Operation took {timer.ElapsedMilliseconds}ms");

            ProcessedImage = result.Result;
        }
        
        private async Task ResetImageCommandExecuted()
        {
            ProcessedImage = LoadedImage.Clone();
        }
    }
}