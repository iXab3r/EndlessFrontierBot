using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace OpenCV.Plaground.Models {
    internal sealed class TemplateDetectionOperation : ImageOperationBase<string> {
        public override OperationResult Process(Image<Rgb, byte> source, string templatePath)
        {
            var result = new OperationResult();
            
            var templateImg = new Image<Rgb, byte>(templatePath);
            var match = source.MatchTemplate(templateImg, TemplateMatchingType.Ccoeff);

            double[] min, max;
            Point[] minLoc, maxLoc;
            match.MinMax(out min, out max, out minLoc, out maxLoc);

            var topLeft = maxLoc[0];

            var resultingImg = source.Clone();
            resultingImg.Draw(new Rectangle(topLeft, templateImg.Size), new Rgb(255, 0,0), 2);
            
            result.Result = resultingImg;
            
            return result.LogRecord("Operation completed successfully");
        }
    }
}