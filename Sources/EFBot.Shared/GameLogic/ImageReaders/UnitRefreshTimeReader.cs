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
        
        public override void Refresh(Image<Bgr, byte> inputImage)
        {
            Clear();
            
            if (inputImage.Size.IsEmpty)
            {
                Text = "Window not found";
                return;
            }

            
            var imageToProcess = inputImage.GetSubRect(ImageSource.RefreshButtonArea);
            var refreshButtonRecognition = RecognitionEngine.Recognize(imageToProcess);
            if (refreshButtonRecognition.Text == "00 00" || refreshButtonRecognition.Text == "00:00")
            {
                Entity = TimeSpan.Zero;
            }
            
            inputImage.Draw(ImageSource.RefreshButtonArea, new Bgr(Color.Blue), 2);
            RecognitionResults.Add(refreshButtonRecognition);
            
            Text = $"Refresh button:\n{refreshButtonRecognition.Text}\n{refreshButtonRecognition.DebugData}";
        }
    }
}