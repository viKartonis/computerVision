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
            
            int[] hist = new int[256];

            for (int t = 0; t < 256; t++)
            {
                hist[t] = 0;
            }
            for (int i = 0; i < buffer.Length; i++)
            {
                hist[buffer[i]]++;
            }

            int m = 0; 
            int n = 0; 
            for (int t = 0; t <= 255; t++)
            {
                m += t * hist[t];
                n += hist[t];
            }

            float maxSigma = -1; 
            int threshold = 0; 

            int alpha1 = 0; 
            int beta1 = 0; 

            for (int t = 0; t < 255; t++)
            {
                alpha1 += t * hist[t];
                beta1 += hist[t];

                float w1 = (float)beta1 / n;

                float a = (float)alpha1 / beta1 - (float)(m - alpha1) / (n - beta1);

                float sigma = w1 * (1 - w1) * a * a;

                if (sigma > maxSigma)
                {
                    maxSigma = sigma;
                    threshold = t;
                }
            }

            threshold += 0;
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
