using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Point = System.Drawing.Point;

namespace EFBot.Shared.Scaffolding
{
    public static class BitmapSourceExtensions
    {
        public static Bitmap ToBitmap(this BitmapSource source)
        {
            var bmp = new Bitmap(
                source.PixelWidth,
                source.PixelHeight,
                PixelFormat.Format32bppPArgb);
            var data = bmp.LockBits(
                new Rectangle(Point.Empty, bmp.Size),
                ImageLockMode.WriteOnly,
                PixelFormat.Format32bppPArgb);
            source.CopyPixels(
                Int32Rect.Empty,
                data.Scan0,
                data.Height * data.Stride,
                data.Stride);
            bmp.UnlockBits(data);
            return bmp;
        }

        public static Bitmap CropImage(this Image img, Rectangle cropArea)
        {
            // An empty bitmap which will hold the cropped image
            var bmp = new Bitmap(cropArea.Width, cropArea.Height);
            var g = Graphics.FromImage(bmp);
            // Draw the given area (section) of the source image
            // at location 0,0 on the empty bitmap (bmp)
            g.DrawImage(img, 0, 0, cropArea, GraphicsUnit.Pixel);

            g.Dispose();

            return bmp;
        }
        
        public static BitmapSource ToBitmapSource(this Bitmap bitmap)
        {
            var hBitmap = bitmap.GetHbitmap();

            var result = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero, Int32Rect.Empty,
                System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            DeleteObject(hBitmap);

            return result;
        }

        /// <summary>
        ///     Delete a GDI object
        /// </summary>
        /// <param name="o">The poniter to the GDI object to be deleted</param>
        /// <returns></returns>
        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);

        public static Image<TColor, TDepth> Duplicate<TColor, TDepth>(this Image<TColor, TDepth> image) where TColor : struct, IColor where TDepth : new()
        {
            var result = new Image<TColor,TDepth>(image.ToBitmap());
            return result;
        }

        public static BitmapSource ToBitmapSource(this IImage image)
        {
            using (var source = image.Bitmap)
            {
                var ptr = source.GetHbitmap(); //obtain the Hbitmap

                var bs = Imaging.CreateBitmapSourceFromHBitmap(
                    ptr,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
                bs.Freeze();

                DeleteObject(ptr); //release the HBitmap
                return bs;
            }
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
        public static VectorOfVectorOfPoint FindContours(this Image<Gray, byte> image, ChainApproxMethod method = ChainApproxMethod.ChainApproxSimple,
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