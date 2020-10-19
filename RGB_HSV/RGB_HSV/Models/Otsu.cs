using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RGB_HSV.Models
{
    class Otsu
    {

        public static Bitmap ApplyOtsu(Bitmap image)
        {
            var width = image.Width;
            var height = image.Height;
            var srcData = image.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var bytes = srcData.Stride * srcData.Height;
            var pixelBuffer = new byte[bytes];

            var resultBuffer = new byte[bytes];
            var srcScan0 = srcData.Scan0;
            Marshal.Copy(srcScan0, pixelBuffer, 0, bytes);
            image.UnlockBits(srcData);

            var rgb = 0.0;
            for (int i = 0; i < pixelBuffer.Length; i += 4)
            {
                rgb = pixelBuffer[i] * .3f;
                rgb += pixelBuffer[i + 1] * .6f;
                rgb += pixelBuffer[i + 2] * .1f;
                pixelBuffer[i] = (byte)rgb;
                pixelBuffer[i + 1] = pixelBuffer[i];
                pixelBuffer[i + 2] = pixelBuffer[i];
                pixelBuffer[i + 3] = 255;
            }
            var value = 0;
            var intensivity = new Dictionary<int, int>();
            var frequancy = new Dictionary<int, double>();
            var sigma = new Dictionary<int, double>();

            for (var i = 0; i < pixelBuffer.Length; i += 4)
            {
                value = pixelBuffer[i];
                intensivity[value] = intensivity.TryGetValue(value, out int val) ? val + 1 : 1;
            }
            foreach (var i in intensivity.Keys)
            {
                frequancy[i] = intensivity[i] / (double)pixelBuffer.Length;
            }
            var keys = intensivity.ToList();
            keys.Sort((a, b) => a.Key.CompareTo(b.Key));

            foreach (var k in keys)
            {
                var probabilityPart1 = 0.0;
                var m1 = 0.0;
                var probabilityPart2 = 0.0;
                var mg = 0.0;
                var count = 0;
                foreach (var i in keys)
                {
                    if (i.Key < k.Key)
                    {
                        probabilityPart1 += frequancy[k.Key];
                        m1 += k.Key * frequancy[k.Key];
                        count++;
                    }
                    mg += k.Key * frequancy[k.Key];
                }
                probabilityPart2 = 1 - probabilityPart1;

                sigma[k.Key] = probabilityPart1 * (1 - probabilityPart1) * 
                    Math.Pow((m1/count - (mg - m1)/(keys.Count - count)), 2);
            }

            var list = sigma.Values.ToList();
            var max = list.Max();

            var threshold = 0.0;
            foreach (var k in keys)
            {
                if(sigma[k.Key] == max)
                {
                    threshold = k.Key;
                }
            }

            for (int i = 0; i < pixelBuffer.Length; i ++)
            {
                if (pixelBuffer[i] >= threshold)
                {
                    pixelBuffer[i] = 255;
                }
                else
                {
                    pixelBuffer[i] = 0;
                }
            }

            resultBuffer = pixelBuffer;
        
            Bitmap resultImage = new Bitmap(width, height);
            BitmapData resultData = resultImage.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(resultBuffer, 0, resultData.Scan0, bytes);
            resultImage.UnlockBits(resultData);
            return resultImage;
        }
    }
}
