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
    }
}
