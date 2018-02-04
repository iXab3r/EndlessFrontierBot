using System;
using Emgu.CV;

namespace EFBot.Shared.Services {
    public struct RecognitionResult
    {
        public IImage Image { get; set; }
        
        public String Text { get; set; }
        
        public String DebugData { get; set; }
    }
}