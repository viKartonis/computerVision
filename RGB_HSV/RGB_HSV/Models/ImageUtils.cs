using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace RGB_HSV.Models.Filters
{
    class ImageUtils
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int Bytes { get; set; }

        public byte[] BitmapToBytes(Bitmap srcImage)
        {
            Width = srcImage.Width;
            Height = srcImage.Height;
            var bitmapData = srcImage.LockBits(new Rectangle(0, 0, Width, Height),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            Bytes = bitmapData.Stride * bitmapData.Height;
            var buffer = new byte[Bytes];
            Marshal.Copy(bitmapData.Scan0, buffer, 0, Bytes);
            srcImage.UnlockBits(bitmapData);
            return buffer;
        }

        public Bitmap BytesToBitmap(byte[] buffer)
        {
            Bitmap resultImage = new Bitmap(Width, Height);
            BitmapData resultData = resultImage.LockBits(new Rectangle(0, 0, Width, Height),
                ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(buffer, 0, resultData.Scan0, Bytes);
            resultImage.UnlockBits(resultData);
            return resultImage;
        }

        public Bitmap ColorToGrayScale(Bitmap src)
        {
            ImageUtils image = new ImageUtils();
            var buffer = image.BitmapToBytes(src);
            var rgb = 0.0;
            for (int i = 0; i < buffer.Length; i += 4)
            {
                rgb = buffer[i] * .3f;
                rgb += buffer[i + 1] * .6f;
                rgb += buffer[i + 2] * .1f;
                buffer[i] = (byte)rgb;
                buffer[i + 1] = buffer[i];
                buffer[i + 2] = buffer[i];
                buffer[i + 3] = 255;
            }
            return image.BytesToBitmap(buffer);
        }

        public double[,] GetTwoDim(double[] srcImage, int width, int height)
        {
            var result = new double[height, width];
            var h = 0;
            var w = 0;
            for (var i = 0; i < srcImage.Length; i++)
            {
                h = i / width;
                w = i - h * width;
                result[h, w] = srcImage[i];
            }
            return result;
        }
    }
}
