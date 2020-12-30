using RGB_HSV.Models.Filters;
using System;
using System.Drawing;

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

        public static Bitmap ApplyMethod(Bitmap srcImage, double sigma)
        {
            ImageUtils image = new ImageUtils();
            var buffer = image.BitmapToBytes(srcImage);
            var width = image.Width;
            var height = image.Height;
            var bytes = image.Bytes;
            var result = new byte[bytes];

            var colorChannels = 3;
            var rgb = new double[colorChannels];
            var kernel = generateKernel((int)Math.Ceiling(3*sigma), sigma);
            var mid = (kernel.GetLength(0) - 1) / 2;
            
            var kcenter = 0;
            var kpixel = 0;

            for(var y = 0; y < height; ++y)
            {
                for(var x = 0; x < width; ++x)
                {
                    for(var c = 0; c < colorChannels; ++c)
                    {
                        rgb[c] = 0.0;
                    }
                    kcenter = y * width*4 + x * 4;
                    for (var fy = -mid; fy <= mid; ++fy)
                    {
                        for (var fx = -mid; fx <= mid; ++fx)
                        {
                            kpixel = kcenter + fy * width * 4 + fx * 4;
                            if (kpixel >= 0 && kpixel < bytes)
                            {
                                for (var c = 0; c < colorChannels; ++c)
                                {
                                    rgb[c] += (double)(buffer[kpixel + c]) * kernel[fy + mid, fx + mid];
                                }
                            }
                            else
                            {
                                for (var c = 0; c < colorChannels; ++c)
                                {
                                    rgb[c] += (double)(buffer[kcenter + c]) * kernel[fy + mid, fx + mid];
                                }
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
                        result[kcenter + c] = (byte)(rgb[c]);
                    }
                    result[kcenter + 3] = 255;
                }
            }
            return image.BytesToBitmap(result);
        }
    }
}
