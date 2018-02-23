using System.Drawing;
using EFBot.Shared.Services;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace EFBot.Shared.GameLogic.ImageReaders {
    internal sealed class GoldChestReader : ImageReaderBase<Rectangle?>
    {
        private readonly Image<Rgb, byte> chestImg;
        
        public GoldChestReader(IRecognitionEngine recognitionEngine, IGameImageSource imgSource) : base(recognitionEngine, imgSource)
        {
            chestImg = new Image<Rgb, byte>(@"C:\Work\EndlessFrontierBot\EFTestData\goldchest\template.jpg");
        }
        
        public override void Refresh(Image<Rgb, byte> inputImage)
        {
            Clear();
            
            if (inputImage.Size.IsEmpty)
            {
                Text = "Window not found";
                return;
            }

            inputImage.ROI = ImageSource.GameFieldArea;
            var match = inputImage.MatchTemplate(chestImg, TemplateMatchingType.CcoeffNormed);
            

            double[] minValues, maxValues;
            Point[] minLocations, maxLocations;
            match.MinMax(out minValues, out maxValues, out minLocations, out maxLocations);

            if (maxValues[0] < 0.8)
            {
                Entity = null;
            }
            else
            {
                Entity = new Rectangle(maxLocations[0], chestImg.Size);
                inputImage.Draw(Entity.Value, new Rgb(255, 0,0), 2);
            }
        }
    }
}