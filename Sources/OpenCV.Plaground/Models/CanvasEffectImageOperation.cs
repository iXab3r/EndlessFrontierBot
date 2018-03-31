using System.Drawing;
using System.Windows.Controls;
using Emgu.CV;
using Emgu.CV.Structure;

namespace OpenCV.Plaground.Models {
    internal sealed class CanvasEffectImageOperation : ImageOperationBase<double> {
        public override OperationResult Process(Image<Rgb, byte> source, double canvasAlpha)
        {
            var result = new OperationResult();

            var original = source;

            result.LogRecord("Original", original);

            var noiseStd = 0.05;
            var motionBlurKernelSize = 15;

            var noiseLayer1 = CreateNoiseBackground(source.Size, noiseStd);
            noiseLayer1 = noiseLayer1 * ConstructKernel(motionBlurKernelSize, Orientation.Horizontal);
            result.LogRecord(noiseLayer1);
            
            var noiseLayer2 = CreateNoiseBackground(source.Size, noiseStd);
            noiseLayer2 = noiseLayer2 * ConstructKernel(motionBlurKernelSize, Orientation.Vertical);
            result.LogRecord(noiseLayer2);

            var alpha = 0.5;
            var noiseLayer = noiseLayer1.AddWeighted(noiseLayer2, alpha: alpha, beta: 1.0f - alpha, gamma: 0).Convert<Rgb, byte>();
            result.LogRecord("Noise layer", noiseLayer);

            result.Result = original.AddWeighted(noiseLayer, 1.0f - canvasAlpha, canvasAlpha, 0).Convert<Rgb, byte>();
            result.LogRecord("Result", result.Result);

            return result;
        }

        private Image<Gray, float> CreateNoiseBackground(Size size, double noiseStd)
        {
            var result = new Image<Gray, float>(size);
            result.Draw(new Rectangle(0, 0, result.Width, result.Height), new Gray(255), -1);
            result.SetRandNormal(new MCvScalar(0), new MCvScalar(noiseStd));
            return result;
        }

        private ConvolutionKernelF ConstructKernel(int size, Orientation orientation)
        {
            var k = new float[size,size];
            
            for (var i = 0; i < size; i++)
            {
                if (orientation == Orientation.Vertical)
                {
                    k[i, size / 2] = 1;
                }
                else
                {
                    k[size / 2, i] = 1;
                }
            }
            
            return new ConvolutionKernelF(k);
        }
    }
}