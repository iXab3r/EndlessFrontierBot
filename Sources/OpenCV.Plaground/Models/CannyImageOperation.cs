using Emgu.CV;
using Emgu.CV.Structure;

namespace OpenCV.Plaground.Models {
    internal sealed class CannyImageOperation : ImageOperationBase<double, double>
    {
        public override OperationResult Process(Image<Rgb, byte> source, double threshold, double thresholdLinking)
        {
            var img = source.Convert<Gray, byte>();
            
            var result = new OperationResult();
            result.LogRecord($"Args: {new { Threshold = threshold, ThresholdLinking = thresholdLinking }}");

            img = img.Canny(threshold, thresholdLinking);

            result.Result = img.Convert<Rgb, byte>();
            return result.LogRecord("Operation completed successfully");;
        }
    }
}