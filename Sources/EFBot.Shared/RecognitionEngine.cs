using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Imaging;
using EFBot.Shared.Scaffolding;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.OCR;
using Emgu.CV.Structure;

namespace EFBot.Shared
{
    public sealed class RecognitionEngine
    {
        private readonly Tesseract tesseractEngine;
        
        public RecognitionEngine()
        {
            tesseractEngine = new Tesseract();
            tesseractEngine.Init(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata"), "eng", OcrEngineMode.Default);
            tesseractEngine.SetVariable("tessedit_char_whitelist", "1234567890:-");
        }


        [MethodImpl(MethodImplOptions.Synchronized)]
        public RecognitionResult Recognize(Image<Bgr, byte> source)
        {
            source = source.Convert<Gray, byte>().Convert<Bgr, byte>();

            var mask = source.SmoothGaussian(3, 3, 9, 9);
            source = source.AddWeighted(mask, 2, -1, 0.5);
            
            tesseractEngine.PageSegMode = PageSegMode.SparseTextOsd;
            tesseractEngine.Recognize(source);
            var text = tesseractEngine.GetText();
            foreach (var recognizedCharacter in tesseractEngine.GetCharacters())
            {
                source.Draw(recognizedCharacter.Region, new Bgr(Color.Red), 1, LineType.AntiAlias);
            }
            
            return new RecognitionResult()
            {
                Text = CleanupText(text),
                Image = source.ToBitmap(),
            };
        }

        public RecognitionResult Recognize(Bitmap image)
        {
            var source = new Image<Bgr, byte>(image);
            return Recognize(source);
        }

        private string CleanupText(string text)
        {
            var lines = text.Split(new[]{ Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            lines = lines.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
            lines = lines.Where(x => x.Length >= 1).ToArray();
            lines = lines.Select(x => x.Trim()).ToArray();
            
            return String.Join(Environment.NewLine, lines);
        }
    }

    public struct RecognitionResult
    {
        public Bitmap Image { get; set; }
        
        public String Text { get; set; }
    }
}