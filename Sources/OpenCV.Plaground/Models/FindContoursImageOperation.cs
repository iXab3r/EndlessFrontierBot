using System;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;

namespace OpenCV.Plaground.Models
{
    internal sealed class FindContoursImageOperation : ImageOperationBase
    {
        public override OperationResult Process(Image<Rgb, byte> source)
        {
            var result = new OperationResult();
            var img = source.Convert<Gray, byte>().PyrDown().PyrUp();

            var resultingImg = source.Clone();
            var contours = FindContours(img, ChainApproxMethod.ChainApproxSimple, RetrType.Tree);
            var contCount = contours.Size;
            for (var i = 0; i < contCount; i++)
            {
                CvInvoke.DrawContours(resultingImg, contours, i, new MCvScalar(255, 0, 0));
            }

            for (var i = 0; i < contCount; i++)
            {
                CvInvoke.DrawContours(resultingImg, contours, i, new MCvScalar(255, 0, 0));

                using (var contour = contours[i])
                {
                    using (var approxContour = new VectorOfPoint())
                    {
                        CvInvoke.ApproxPolyDP(contour, approxContour, CvInvoke.ArcLength(contour, true) * 0.05, true);
                        
                        var area = CvInvoke.MinAreaRect(approxContour).Size;
                        var relativeArea = area.Height * area.Width / (img.Height * img.Width) * 100;
                        
                        if (approxContour.Size == 4 && relativeArea > 5) //The contour has 4 vertices.
                        {
                            var isRectangle = true;
                            var pts = approxContour.ToArray();
                            var edges = PointCollection.PolyLine(pts, true);
                            
                            for (var j = 0; j < edges.Length; j++)
                            {
                                var angle = Math.Abs(
                                    edges[(j + 1) % edges.Length].GetExteriorAngleDegree(edges[j]));
                                if (angle < 80 || angle > 100)
                                {
                                    isRectangle = false;
                                    break;
                                }
                            }

                            if (isRectangle)
                            {
                                

                                result.LogRecord($"Rectangle\n\t{ToString(approxContour)}\n\tArea: {area} ({relativeArea}%)");

                                resultingImg.Draw(CvInvoke.BoundingRectangle(approxContour), new Rgb(0, 255, 0), 3);
                            }
                        }
                    }
                }
            }

            result.Result = resultingImg;
            return result;
        }

        private string ToString(VectorOfPoint vector)
        {
            return string.Join(" ", vector.ToArray());
        }

        /// <summary>
        ///     Find contours using the specific memory storage
        /// </summary>
        /// <param name="method">The type of approximation method</param>
        /// <param name="type">The retrieval type</param>
        /// <param name="stor">The storage used by the sequences</param>
        /// <returns>
        ///     Contour if there is any;
        ///     null if no contour is found
        /// </returns>
        public static VectorOfVectorOfPoint FindContours(Image<Gray, byte> image, ChainApproxMethod method = ChainApproxMethod.ChainApproxSimple,
            RetrType type = RetrType.List)
        {
            //Check that all parameters are valid.
            var result = new VectorOfVectorOfPoint();

            if (method == ChainApproxMethod.ChainCode)
            {
                throw new NotImplementedException();
            }

            CvInvoke.FindContours(image, result, null, type, method);
            return result;
        }
    }
}