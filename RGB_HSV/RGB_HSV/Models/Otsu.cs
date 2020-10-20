using RGB_HSV.Models.Filters;
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

        public static Bitmap ApplyOtsu(Bitmap srcImage)
        {
            ImageUtils image = new ImageUtils();
            var buffer = image.BitmapToBytes(srcImage);
            var width = image.Width;
            var height = image.Height;
            var bytes = image.Bytes;
            var result = new byte[bytes];

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
            var value = 0;
            var intensivity = new Dictionary<int, int>();
            var frequancy = new Dictionary<int, double>();
            var sigma = new Dictionary<int, double>();

            for (var i = 0; i < buffer.Length; i += 4)
            {
                value = buffer[i];
                intensivity[value] = intensivity.TryGetValue(value, out int val) ? val + 1 : 1;
            }
            foreach (var i in intensivity.Keys)
            {
                frequancy[i] = intensivity[i] / (double)buffer.Length;
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

            for (int i = 0; i < buffer.Length; i ++)
            {
                if (buffer[i] >= threshold)
                {
                    buffer[i] = 255;
                }
                else
                {
                    buffer[i] = 0;
                }
            }

            result = buffer;

            return image.BytesToBitmap(result);
        }
    }
}
