using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using EFBot.Shared.Scaffolding;
using Emgu.CV;
using Emgu.CV.Structure;
using ReactiveUI;

namespace OpenCV.Plaground.ViewModels
{
    internal sealed class ImageProviderViewModel : DisposableReactiveObject
    {
        private readonly string[] ImageDirectories = new[]
        {
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"images"),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\images"),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\EFTestData"),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\EFTestData"),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\EFTestData"),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\..\EFTestData"),
        };
        
        public ImageProviderViewModel()
        {
            Observable.Timer(DateTimeOffset.Now, TimeSpan.FromSeconds(30))
                .Select(x => ImageDirectories.SelectMany(FindImages))
                .WithPrevious(
                    (prev, curr) =>
                    {
                        var imagesToLoad = curr.EmptyIfNull().Except(prev.EmptyIfNull()).ToArray();
                        return imagesToLoad;
                    })
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(LoadImages)
                .AddTo(Anchors);

            Images.ItemsAdded
                .Where(x => SelectedImage == null)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => SelectedImage = x)
                .AddTo(Anchors);
        }

        private BitmapSource selectedImage;

        public BitmapSource SelectedImage
        {
            get { return selectedImage; }
            set { this.RaiseAndSetIfChanged(ref selectedImage, value); }
        }

        public ReactiveList<BitmapSource> Images { get; } = new ReactiveList<BitmapSource>();

        private void LoadImages(IEnumerable<string> imagePaths)
        {
            Log.Instance.Debug($"New images detected:\n{imagePaths.DumpToTable()}");
            foreach (var imagePath in imagePaths)
            {
                var image = new Image<Rgb, byte>(imagePath);
                Images.Add(image.ToBitmapSource());
            }
        }
        
        private IEnumerable<string> FindImages(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                yield break;
            }

            var images = Directory.GetFiles(directoryPath, "*.*").Where(IsImage);
            foreach (var imagePath in images)
            {
                yield return imagePath;
            }
        }

        private bool IsImage(string filePath)
        {
            var extension = Path.GetExtension(filePath)?.ToLowerInvariant().Trim();
            var knownImageFormats = new HashSet<string>(){ ".jpg", ".png", ".gif", ".bmp" };
            return knownImageFormats.Contains(extension);
        }
        
    }
}