using System;
using System.Windows.Controls;
using Emgu.CV;
using Emgu.CV.Cuda;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using OpenTK.Graphics.ES11;

namespace OpenCV.Plaground.Models {
    internal sealed class ColorRemovalOperation : ImageOperationBase<double, double> {
        public override OperationResult Process(Image<Rgb, byte> source, double min, double max)
        {
            var result = new OperationResult();
            var bgImg = new Image<Rgb, byte>(@"C:\Work\EndlessFrontierBot\GreenScreenTestData\bg\636566578613675735_1 (1).jpg");
            result.Result = Process(result, source, bgImg, new Hsv(min, 20, 40), new Hsv(max, 255, 255));

            return result.LogRecord("Operation completed successfully");
        }
        
        private static Image<Rgb, byte> Process(
            OperationResult log,
            Image<Rgb, byte> source,
            Image<Rgb, byte> bgSource,
            Hsv min, Hsv max)
        {
            if (bgSource == null)
            {
                return source.Clone();
            }

            var hsv = source.Convert<Hsv, byte>();
            var fgMask = hsv.InRange(min, max).Not();

            if (bgSource.Width != source.Width || bgSource.Height != source.Height)
            {
                bgSource = bgSource.Resize(source.Width, source.Height, Inter.Cubic);
            }
            log.LogRecord("Background", bgSource.Clone());

            var result = bgSource.Clone();
            
            log.LogRecord("Alpha(before)", fgMask.Clone());
            fgMask = fgMask.SmoothGaussian(5);
            fgMask = fgMask.Erode(3);
            fgMask = fgMask.Dilate(3);
            
            log.LogRecord("Alpha(after)", fgMask.Clone());

            source.Convert<Rgb, byte>().Copy(result, fgMask);

            return result;
        }
    }
}