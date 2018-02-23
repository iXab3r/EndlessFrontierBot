using System;
using System.Windows.Controls;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using OpenTK.Graphics.ES11;

namespace OpenCV.Plaground.Models {
    internal sealed class ColorRemovalOperation : ImageOperationBase<double, double> {
        public override OperationResult Process(Image<Rgb, byte> source, double min, double max)
        {
            
            var result = new OperationResult();
            var saturation = 80;
            var value = 100;
            
            var hsv = source.Convert<Hsv, byte>();
            var mask = hsv.InRange(new Hsv(min, 40, 40), new Hsv(max, 255, 255));
            
            var bgImg = new Image<Rgb, byte>(@"C:\Work\EndlessFrontierBot\GreenScreenTestData\Dream-Green-Forest-Stock-Photo.jpg");
            bgImg = bgImg.Resize(source.Width, source.Height, Inter.Cubic);

            var resultingImg = source.Clone();
            bgImg.Copy(resultingImg, mask);
            result.Result = resultingImg;

            return result.LogRecord("Operation completed successfully");
        }
    }
}