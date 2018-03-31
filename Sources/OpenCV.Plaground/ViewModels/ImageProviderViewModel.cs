using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using DynamicData;
using DynamicData.Binding;
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
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\PoeTestData"),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\GreenScreenTestData"),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\..\EFTestData"),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\..\PoeTestData"),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\..\GreenScreenTestData"),
        };
        
        public ImageProviderViewModel()
        {
            Observable.Timer(DateTimeOffset.Now, TimeSpan.FromSeconds(30))
                .Select(x => ImageDirectories.SelectMany(FindImages).ToArray())
                .WithPrevious(
                    (prev, curr) =>
                    {
                        var imagesToLoad = curr.EmptyIfNull().Except(prev.EmptyIfNull()).ToArray();
                        return imagesToLoad;
                    })
                .Where(x => x.Length > 0)
                .Subscribe(LoadImages)
                .AddTo(Anchors);

            images
                .Connect()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(out readonlyImages)
                .Subscribe()
                .AddTo(Anchors);

            readonlyImages
                .ToObservableChangeSet()
                .Where(x => SelectedImage == null)
                .OnItemAdded(x => SelectedImage = x)
                .Where(x => SelectedImage == null)
                .Subscribe()
                .AddTo(Anchors);
        }

        private BitmapSource selectedImage;
        private readonly ISourceList<BitmapSource> images = new SourceList<BitmapSource>();
        private readonly ReadOnlyObservableCollection<BitmapSource> readonlyImages;

        public BitmapSource SelectedImage
        {
            get { return selectedImage; }
            set { this.RaiseAndSetIfChanged(ref selectedImage, value); }
        }

        public ReadOnlyObservableCollection<BitmapSource> Images
        {
            get { return readonlyImages; }
        }

        private void LoadImages(IEnumerable<string> imagePaths)
        {
            Log.Instance.Debug($"New images detected:\n{imagePaths.DumpToTable()}");
            foreach (var imagePath in imagePaths)
            {
                var image = new Image<Rgb, byte>(imagePath);
                images.Add(image.ToBitmapSource());
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
            var knownImageFormats = new HashSet<string>(){ ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            return knownImageFormats.Contains(extension);
        }
        
    }
}