using System;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using EFBot.Shared.Scaffolding;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using OpenTK.Graphics.ES11;
using OpenTK.Platform.Windows;
using RaysVideoMixer.Services;
using ReactiveUI;

namespace RaysVideoMixer.ViewModels
{
    internal sealed class MainWindowViewModel : DisposableReactiveObject
    {
        private BitmapSource loadedImage;
        private BitmapSource processedImage;
        private bool previewLoadedImage;
        private bool previewProcessedImage;
        private BitmapSource backgroundImage;

        private IDisposable activeOperationAnchor;
        
        public MainWindowViewModel()
        {
            PlayCommand = CommandWrapper.Create<object[]>(
                args => PlayCommandExecuted(Convert.ToDouble(args[0]), Convert.ToDouble(args[1])));
            ShowStaticCommand = CommandWrapper.Create<object[]>(
                args => ShowStaticCommandExecuted(Convert.ToDouble(args[0]), Convert.ToDouble(args[1])));

            StopCommand = CommandWrapper.Create(ReactiveCommand.Create(() => {  activeOperationAnchor?.Dispose(); }));
        }

        public BitmapSource LoadedImage
        {
            get { return loadedImage; }
            set { this.RaiseAndSetIfChanged(ref loadedImage, value); }
        }

        public BitmapSource BackgroundImage
        {
            get { return backgroundImage; }
            set { this.RaiseAndSetIfChanged(ref backgroundImage, value); }
        }

        public BitmapSource ProcessedImage
        {
            get { return processedImage; }
            set { this.RaiseAndSetIfChanged(ref processedImage, value); }
        }

        public bool PreviewLoadedImage
        {
            get { return previewLoadedImage; }
            set { this.RaiseAndSetIfChanged(ref previewLoadedImage, value); }
        }


        public bool PreviewProcessedImage
        {
            get { return previewProcessedImage; }
            set { this.RaiseAndSetIfChanged(ref previewProcessedImage, value); }
        }
        
        public ICommand PlayCommand { get; }
        
        public ICommand ShowStaticCommand { get; }
        
        public ICommand StopCommand { get; }

        private async Task PlayCommandExecuted(double min, double max)
        {
            var isCancelled = false;
            activeOperationAnchor = Disposable.Create(() => isCancelled = true);
            
            var staticImg = new Image<Bgr, byte>(@"C:\Work\EndlessFrontierBot\GreenScreenTestData\Dream-Green-Forest-Stock-Photo.jpg");
            
            var imgProcessor = new ChromaKeyImageProcessor();
            imgProcessor.SetBackground(staticImg.Bitmap);
            imgProcessor.SetChromaKey(min, max);
            
            using ( var capture = new Capture(@"C:\Users\mailx\Downloads\Cogs Turning 2.mov"))
            using ( var bgCapture = new Capture(@"C:\Users\mailx\Downloads\Cogs Turning 2.mov"))
            {
                var fps = capture.GetCaptureProperty(CapProp.Fps);
                var delay = TimeSpan.FromMilliseconds(1000 / fps);

                var sw = new Stopwatch();
                while (!isCancelled)
                {
                    sw.Restart();
                    var frame = capture.QueryFrame()?.ToImage<Bgr, byte>();
                    if (frame == null)
                    {
                        break;
                    }

                    if (staticImg.Width != frame.Width || staticImg.Height != frame.Height)
                    {
                        staticImg = staticImg.Resize(frame.Width, frame.Height, Inter.Cubic);
                    }
                    
                    var bgFrame = bgCapture.QueryFrame()?.ToImage<Bgr, byte>().Flip(FlipType.Horizontal);

                    if (previewLoadedImage)
                    {
                        LoadedImage = frame.ToBitmapSource();
                        BackgroundImage = bgFrame.ToBitmapSource();
                    }

                    var processedImg = await Task.Run(() => imgProcessor.Process(frame.Bitmap));
                    if (previewProcessedImage)
                    {
                        ProcessedImage = processedImg.ToBitmapSource();
                    }
                    
                    var calculatedDelay = delay - sw.Elapsed;
                    if (calculatedDelay > TimeSpan.Zero)
                    {
                        await Task.Delay(delay);
                    }
                }
            }
        }
        
        
        private async Task ShowStaticCommandExecuted(double min, double max)
        {
            var isCancelled = false;
            activeOperationAnchor = Disposable.Create(() => isCancelled = true);
            
            var staticImg = new Image<Bgr, byte>(@"C:\Work\EndlessFrontierBot\GreenScreenTestData\Dream-Green-Forest-Stock-Photo.jpg");
            
            var imgProcessor = new ChromaKeyImageProcessor();
            imgProcessor.SetBackground(staticImg.Bitmap);
            imgProcessor.SetChromaKey(min, max);
            
            var frame = new Image<Bgr, byte>(@"C:\Work\EndlessFrontierBot\GreenScreenTestData\IMG_0001.JPG");
            
            if (previewLoadedImage)
            {
                LoadedImage = frame.ToBitmapSource();
                BackgroundImage = staticImg.ToBitmapSource();
            }

            var processedImg = await Task.Run(() => imgProcessor.Process(frame.Bitmap));
            if (previewProcessedImage)
            {
                ProcessedImage = processedImg.ToBitmapSource();
            }
        }
        
        private IImage Process(
            Image<Bgr, byte> source, 
            Image<Bgr, byte> bgSource, 
            Image<Bgr, byte> staticSource, 
            double min, double max)
        {
            if (bgSource == null)
            {
                return source;
            }
            
            var fgMask = source.Convert<Hsv, byte>().InRange(new Hsv(min, 40, 40), new Hsv(max, 255, 255));
            var bgMask = bgSource.Convert<Hsv, byte>().InRange(new Hsv(min, 40, 40), new Hsv(max, 255, 255));
            

            if (bgSource.Width != source.Width || bgSource.Height != source.Height)
            {
                bgSource = bgSource.Resize(source.Width, source.Height, Inter.Cubic);
            }

            var result = staticSource.Clone();
            bgSource.Copy(result, bgMask.Not());
            source.Copy(result, fgMask.Not());
            
            return result;
        }
    }
}