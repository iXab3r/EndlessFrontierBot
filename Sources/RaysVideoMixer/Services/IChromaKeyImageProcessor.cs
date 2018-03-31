using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

namespace RaysVideoMixer.Services
{
    public interface IChromaKeyImageProcessor
    {
        void SetBackground(Bitmap source);

        void SetChromaKey(double minValue, double maxValue);

        Bitmap Process(Bitmap source);
    }

    public sealed class ChromaKeyImageProcessor : IChromaKeyImageProcessor
    {
        private Image<Rgb, byte> backgroundImage;
        private Image<Rgb, byte> scaledBackgroundImage;

        private Size resolution;
        private Hsv backgroundMin;
        private Hsv backgroundMax;

        public void SetResolution(Size resolution)
        {
            this.resolution = resolution;
        }

        private static Image<Rgb, byte> ScaleIfNeeded(Image<Rgb, byte>  source, Size targetResolution)
        {
            if (targetResolution.IsEmpty || targetResolution == source.Size)
            {
                return source;
            }
            return source.Resize(targetResolution.Width, targetResolution.Height, Inter.Cubic);
        }

        public Bitmap Process(Bitmap source)
        {
            if (backgroundImage == null)
            {
                return source;
            }
            var img = new Image<Rgb, byte>(source);
            img = ScaleIfNeeded(img, resolution);
            var result = Process(img, scaledBackgroundImage, backgroundMin, backgroundMax);
            return result.Bitmap;
        }

        public void SetBackground(Bitmap source)
        {
            if (source == null)
            {
                backgroundImage = null;
                scaledBackgroundImage = null;
                return;
            }
            backgroundImage = new Image<Rgb, byte>(source);
            scaledBackgroundImage = ScaleIfNeeded(backgroundImage, resolution).Clone();
        }

        public void SetChromaKey(double minValue, double maxValue)
        {
            backgroundMin = new Hsv(minValue, 40, 40);
            backgroundMax = new Hsv(maxValue, 255, 255);
        }

        private static IImage Process(
            Image<Rgb, byte> source,
            Image<Rgb, byte> bgSource,
            Hsv min, Hsv max)
        {
            if (bgSource == null)
            {
                return source.Clone();
            }

            var hsv = source.Convert<Hsv, byte>();
            var topLeft = hsv[10, 10];
            var fgMask = hsv.InRange(min, max);

            if (bgSource.Width != source.Width || bgSource.Height != source.Height)
            {
                bgSource = bgSource.Resize(source.Width, source.Height, Inter.Cubic);
            }

            var result = bgSource.Clone();
            source.Copy(result, fgMask.Not());

            return result;
        }
    }
}