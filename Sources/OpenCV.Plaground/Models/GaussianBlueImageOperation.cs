using Emgu.CV;
using Emgu.CV.Structure;

namespace OpenCV.Plaground.Models {
    internal sealed class GaussianBlueImageOperation : ImageOperationBase<int> {
        public override OperationResult Process(Image<Rgb, byte> source, int kernelSize)
        {
            var img = source.Clone();
            
            var result = new OperationResult();
            result.LogRecord($"Kernel size: {kernelSize}");
            result.Result = img.SmoothGaussian(kernelSize);
            return result.LogRecord("Operation completed successfully");
        }
    }
}