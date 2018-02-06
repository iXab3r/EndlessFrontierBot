using System.Drawing;
using EFBot.Shared.Scaffolding;
using Emgu.CV;
using Emgu.CV.Structure;

namespace OpenCV.Plaground.Models
{
    internal abstract class ImageOperationBase : DisposableReactiveObject
    {
        public abstract OperationResult Process(Image<Rgb, byte> source);
    }
    
    internal abstract class ImageOperationBase<TParam> : DisposableReactiveObject
    {
        public abstract OperationResult Process(Image<Rgb, byte> source, TParam param);
    }
    
    internal abstract class ImageOperationBase<TParam1, TParam2> : DisposableReactiveObject
    {
        public abstract OperationResult Process(Image<Rgb, byte> source, TParam1 param1, TParam2 param2);
    }
}