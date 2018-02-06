using Emgu.CV;
using Emgu.CV.Structure;

namespace OpenCV.Plaground.Models {
    internal sealed class NegateImageOperation : ImageOperationBase {
        public override OperationResult Process(Image<Rgb, byte> source)
        {
            return new OperationResult()
            {
                Result = source.Not(),
            }.LogRecord("Operation completed successfully");
        }
    }
}