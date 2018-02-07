using Emgu.CV;
using Emgu.CV.Structure;

namespace EFBot.Shared.Services {
    public interface IRecognitionEngine {
        RecognitionResult Recognize(Image<Rgb, byte> source);
    }
}