using System;
using System.Drawing;
using DynamicData;
using EFBot.Shared.Services;
using Emgu.CV;
using Emgu.CV.Structure;

namespace EFBot.Shared.GameLogic.ImageReaders {
    internal sealed class UnitRefreshTimeReader : ImageReaderBase<TimeSpan?>
    {
        public UnitRefreshTimeReader(IRecognitionEngine recognitionEngine, IGameImageSource imgSource) : base(recognitionEngine, imgSource) { }
        
        public override void Refresh(Image<Rgb, byte> inputImage)
        {
            Clear();
            
            if (inputImage.Size.IsEmpty)
            {
                Text = "Window not found";
                return;
            }

            
            var imageToProcess = inputImage.GetSubRect(ImageSource.RefreshButtonArea);
            var refreshButtonRecognition = RecognitionEngine.Recognize(imageToProcess);

            if (!TimeSpan.TryParseExact(refreshButtonRecognition.Text.Replace(" ", string.Empty),  "mm\\:ss", null, out var result))
            {
                Entity = null;
            }
            else
            {
                Entity = result;
            }
            
            if (Entity != null)
            {
                inputImage.Draw(ImageSource.RefreshButtonArea, new Rgb(Color.Blue), 1);
                RecognitionResults.Add(refreshButtonRecognition);
            }
            else
            {
                inputImage.Draw(ImageSource.RefreshButtonArea, new Rgb(Color.IndianRed), 1);
            }
            
            Text = $"Refresh button:\n{refreshButtonRecognition.Text}";
        }
    }
}