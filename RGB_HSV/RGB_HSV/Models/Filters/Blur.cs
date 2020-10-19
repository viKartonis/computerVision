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
    class Blur
    {
        private static double[,] generateKernel(int kernelSize, double sigma)
        {
            var kernel = new double[kernelSize, kernelSize];
            var kernelSum = 0.0;
            var mid = (kernelSize - 1) / 2;
            var dist = 0.0;
            var constant = 1d / (2 * Math.PI * sigma * sigma);
            for(var y = -mid; y <= mid; ++y)
            {
                for(var x = -mid; x <= mid; ++x)
                {
                    dist = (x * x + y * y) / (2 * sigma * sigma);
                    kernel[y + mid, x + mid] = constant * Math.Exp(-dist);
                    kernelSum += kernel[y + mid, x + mid];
                }
            }
            for (var y = -mid; y <= mid; ++y)
            {
                for (var x = -mid; x <= mid; ++x)
                {
                    kernel[y + mid, x + mid] *= 1d / kernelSum;
                }
            }
            return kernel;
        }

        public static Bitmap blurImage(Bitmap srcImage, double sigma)
        {
            var width = srcImage.Width;
            var height = srcImage.Height;
            var bitmapData = srcImage.LockBits(new Rectangle(0,0,width, height),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var bytes = bitmapData.Stride * bitmapData.Height;
            var buffer = new byte[bytes];
            var result = new byte[bytes];
            Marshal.Copy(bitmapData.Scan0, buffer, 0, bytes);
            srcImage.UnlockBits(bitmapData);
            var colorChannels = 3;
            var rgb = new double[colorChannels];
            var kernel = generateKernel((int)Math.Ceiling(3*sigma), sigma);
            var mid = (kernel.GetLength(0) - 1) / 2;
            var kcenter = 0;
            var kpixel = 0;

            for(var y = mid; y < height - mid; ++y)
            {
                for(var x = mid; x < width - mid; ++x)
                {
                    for(var c = 0; c < colorChannels; ++c)
                    {
                        rgb[c] = 0.0;
                    }
                    kcenter = y * bitmapData.Stride + x * 4;
                    for (var fy = -mid; fy <= mid; ++fy)
                    {
                        for (var fx = -mid; fx <= mid; ++fx)
                        {
                            kpixel = kcenter + fy * bitmapData.Stride + fx * 4;
                            for (var c = 0; c < colorChannels; ++c)
                            {
                                rgb[c] += (double)(buffer[kpixel + c]) * kernel[fy + mid, fx + mid];
                            }
                        }
                    }
                    for (var c = 0; c < colorChannels; ++c)
                    {
                        if(rgb[c] > 255)
                        {
                            rgb[c] = 255;
                        }
                        else if(rgb[c] < 0)
                        {
                            rgb[c] = 0;
                        }
                    }
                    for (var c = 0; c < colorChannels; ++c)
                    {
                        result[kcenter + c] = (byte)rgb[c];
                    }
                    result[kcenter + 3] = 255;
                }
            }
            Bitmap resultImage = new Bitmap(width, height);
            BitmapData resultData = resultImage.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(result, 0, resultData.Scan0, bytes);
            resultImage.UnlockBits(resultData);
            return resultImage;
        }
    }
}
