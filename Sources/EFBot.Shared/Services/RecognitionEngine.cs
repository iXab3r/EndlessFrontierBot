﻿using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.OCR;
using Emgu.CV.Structure;

namespace EFBot.Shared.Services
{
    public sealed class RecognitionEngine : IRecognitionEngine
    {
        private readonly Tesseract tesseractEngine;
        
        public RecognitionEngine()
        {
            tesseractEngine = new Tesseract();
            tesseractEngine.Init(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata"), "eng", OcrEngineMode.TesseractOnly);
            tesseractEngine.SetVariable("tessedit_char_whitelist", "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890:-.");
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public RecognitionResult Recognize(Image<Bgr, byte> source)
        {
            if (source.Width == 0 || source.Height == 0)
            {
                return new RecognitionResult();
            }
            
            var img = source.Convert<Gray, byte>();

            img = img.Resize(3, Inter.Cubic);

            //img = img.Not();

            //img = img.ThresholdBinary(new Gray(0xA5), new Gray(0xFF));
            
           // var mask = img.SmoothGaussian(3, 3, 9, 9);
           // img = img.AddWeighted(mask, 2, -1, 0.5);

            tesseractEngine.PageSegMode = PageSegMode.SparseTextOsd;
            tesseractEngine.Recognize(img);
            var text = tesseractEngine.GetText();

            source = img.Convert<Bgr, byte>();

            var charactes = tesseractEngine.GetCharacters();
            foreach (var recognizedCharacter in charactes)
            {
                source.Draw(recognizedCharacter.Region, new Bgr(Color.Red), 1, LineType.AntiAlias);
            }
            
            
            return new RecognitionResult()
            {
                Text = CleanupText(text),
                DebugData = string.Join("\n", charactes.Select(x => new { x.Text, x.Cost })),
                Image = source,
            };
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
}