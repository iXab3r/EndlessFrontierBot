using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Compression;
using System.Linq;
using DynamicData;
using EFBot.Shared.Scaffolding;
using EFBot.Shared.Services;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;

namespace EFBot.Shared.GameLogic.ImageReaders
{
    internal sealed class UnitAcquisitionConfirmationDialogReader : ImageReaderBase<IUnitAcquisitionGameDialog>
    {
        public UnitAcquisitionConfirmationDialogReader(IRecognitionEngine recognitionEngine, IGameImageSource imgSource) : base(recognitionEngine, imgSource) { }
        
        public override void Refresh(Image<Rgb, byte> inputImage)
        {
            RecognitionResults.Clear();
            var canvas = inputImage.Clone();

            var img = inputImage
                .SmoothGaussian(11)
                .Convert<Gray, byte>()
                .Canny(60, 90)
                .PyrDown().PyrUp();

            var dialogs = FindRectangle(canvas, img, x => x.Coverage >= 0.20).OrderByDescending(x => x.Coverage).ToArray();

            if (dialogs.Length == 0)
            {
                return;
            }

            var dialogArea = inputImage.Clone().GetSubRect(dialogs[0].Rect);

            var buttonAreas = FindRectangle(
                dialogArea,
                dialogArea
                    .SmoothGaussian(9)
                    .Convert<Gray, byte>()
                    .Canny(24, 35)
                    .PyrDown().PyrUp(),
                candidate => candidate.Coverage >= 0.05 && candidate.Coverage <= 0.2).ToArray();

            if (buttonAreas.Length == 0)
            {
                return;
            }

            var buttons = buttonAreas
                .Select(x => ToButton(dialogArea, x))
                .Where(x => !string.IsNullOrEmpty(x.Text))
                .ToArray();
            
            if (buttons.Length == 0)
            {
                return;
            }
            
            RecognitionResults.Add(new RecognitionResult()
            {
                Text = "Unit dialog",
                Image = dialogArea
            });
        }

        private IGameButton ToButton(Image<Rgb, byte> image, RectangleCandidate candidate)
        {
            var element = image.Clone().GetSubRect(candidate.Rect);
            var text = RecognitionEngine.Recognize(element);
            
            return new GameButton()
            {
                Area = candidate.Rect,
                Text = text.Text,
            };
        }
        
        private IEnumerable<RectangleCandidate> FindRectangle(
            Image<Rgb, byte> canvas, Image<Gray, byte> img, Predicate<RectangleCandidate> filter)
        {
            var contours = img.FindContours(ChainApproxMethod.ChainApproxSimple, RetrType.Tree);
            
            var contCount = contours.Size;

            for (var i = 0; i < contCount; i++)
            {
                CvInvoke.DrawContours(canvas, contours, i, new MCvScalar(255, 0, 0));
            }

            for (var i = 0; i < contCount; i++)
            {
                using (var contour = contours[i])
                using (var approxContour = new VectorOfPoint())
                {
                    CvInvoke.ApproxPolyDP(contour, approxContour, CvInvoke.ArcLength(contour, true) * 0.05, true);

                    var area = CvInvoke.MinAreaRect(approxContour).Size;
                    var relativeArea = area.Height * area.Width / (img.Height * img.Width);

                    if (approxContour.Size != 4 )
                    {
                        continue;
                    }

                    if (!IsRectangularArea(approxContour))
                    {
                        continue;
                    }
                    
                    var candidate = new RectangleCandidate()
                    {
                        Coverage = relativeArea,
                        Rect = CvInvoke.BoundingRectangle(approxContour)
                    };

                    if (!filter(candidate))
                    {
                        continue;
                    }
                    
                    canvas.Draw(CvInvoke.BoundingRectangle(approxContour), new Rgb(0, 255, 0), 3);
                    yield return candidate;
                }
            }
        }
        
        private bool IsRectangularArea(VectorOfPoint contour)
        {
            var pts = contour.ToArray();
            var edges = PointCollection.PolyLine(pts, true);

            for (var j = 0; j < edges.Length; j++)
            {
                var angle = Math.Abs(edges[(j + 1) % edges.Length].GetExteriorAngleDegree(edges[j]));
                if (angle < 80 || angle > 100)
                {
                    return false;
                }
            }

            return true;
        }

        private struct RectangleCandidate
        {
            public Rectangle Rect { get; set; }
            
            public double Coverage { get; set; }
        }
        
        private struct GameButton : IGameButton
        {
            public Rectangle Area { get; set; }
            public string Text { get; set; }
        }
    }
}