using Emgu.CV;
using Emgu.CV.Structure;

namespace OpenCV.Plaground.Models {
    internal sealed class LogRecord
    {
        public string Text { get; set; }
        
        public IImage Image { get; set; }
    }
}