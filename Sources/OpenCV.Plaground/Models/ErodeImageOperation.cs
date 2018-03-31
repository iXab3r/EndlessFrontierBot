using Emgu.CV;
using Emgu.CV.Structure;

namespace OpenCV.Plaground.Models {
    internal sealed class ErodeImageOperation : ImageOperationBase<int> {
        public override OperationResult Process(Image<Rgb, byte> source, int iterationsCount)
        {
            var img = source.Clone();
            
            var result = new OperationResult();
            result.LogRecord($"Erode Iteratons count: {iterationsCount}");
            result.Result = img.Erode(iterationsCount);
            return result.LogRecord("Operation completed successfully");
        }
    }
}