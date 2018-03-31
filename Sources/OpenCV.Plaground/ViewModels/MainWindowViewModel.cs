using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;
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
            CanvasEffectCommand = CommandWrapper.Create<double>(
                args => ExecuteImageOperation(x => new CanvasEffectImageOperation().Process(x, args)));
            CannyCommand = CommandWrapper.Create<object[]>(
                args => ExecuteImageOperation(x => new CannyImageOperation().Process(x, Convert.ToDouble(args[0]), Convert.ToDouble(args[1]))));
            GaussianBlurCommand = CommandWrapper.Create<double>(
                args => ExecuteImageOperation(x => new GaussianBlueImageOperation().Process(x, Convert.ToInt32(args))));
            DilateCommand = CommandWrapper.Create<double>(
                args => ExecuteImageOperation(x => new DilateImageOperation().Process(x, Convert.ToInt32(args))));
            ErodeCommand = CommandWrapper.Create<double>(
                args => ExecuteImageOperation(x => new ErodeImageOperation().Process(x, Convert.ToInt32(args))));
            FindContoursCommand = CommandWrapper.Create(
                () => ExecuteImageOperation(x => new FindContoursImageOperation().Process(x)));      
            ColorRemovalCommand = CommandWrapper.Create<object[]>(
                args => ExecuteImageOperation(x => new ColorRemovalOperation().Process(x, Convert.ToDouble(args[0]), Convert.ToDouble(args[1]))));
            TemplateDetectionCommand = CommandWrapper.Create<object[]>(
                args => ExecuteImageOperation(x => new TemplateDetectionOperation().Process(x, Convert.ToString(args[0]).Trim('"'))));
        
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
        public ICommand DilateCommand { get; }
        public ICommand ErodeCommand { get; }
        public ICommand ColorRemovalCommand { get; }
        public ICommand TemplateDetectionCommand { get; }
        public ICommand CanvasEffectCommand { get; }
        
        public ReactiveList<LogRecord> ExecutionLog { get; } = new ReactiveList<LogRecord>();

        private async Task ExecuteImageOperation(Func<Image<Rgb, byte>, OperationResult> executor)
        {
            ExecutionLog.Clear();
            
            var image = ProcessedImage.Clone();
            ExecutionLog.Add(new LogRecord(){ Text = $"Initializing operation..." });

            var timer = Stopwatch.StartNew();
            var result = await Task.Run(() => executor(image));
            timer.Stop();

            result.Log.ForEach(ExecutionLog.Add);
            ExecutionLog.Add(new LogRecord(){ Text = $"Operation took {timer.ElapsedMilliseconds}ms" });

            var folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tmp");
            if (Directory.Exists(folderPath))
            {
                Directory.EnumerateFiles(folderPath).ForEach(
                    x =>
                    {
                        try
                        {
                            File.Delete(x);
                        }
                        catch (Exception e)
                        {
                            ExecutionLog.Add(new LogRecord(){ Text = $"Failed to remove file {x} - {e.Message}" });
                        }
                        
                    });
            } 
            else 
            {
                Directory.CreateDirectory(folderPath);
            }
            
            image.Save(Path.Combine(folderPath, $"Source.png"));
            foreach (var logRecord in 
                ExecutionLog.Where(x => x.Image != null))
            {
                var fileName = $"{Guid.NewGuid().ToString().Replace("-", "")}.png";
                var filePath = Path.Combine(folderPath, fileName);
                logRecord.Image.Save(filePath);
            }
            
            ExecutionLog.Add(new LogRecord(){ Text = $"Operation + logging took {timer.ElapsedMilliseconds}ms" });
            if (result.Result != null)
            {
                result.Result.Save(Path.Combine(folderPath, $"Result.png"));
                ProcessedImage = result.Result;
            }
        }
        
        private async Task ResetImageCommandExecuted()
        {
            ProcessedImage = LoadedImage.Clone();
        }
    }
}