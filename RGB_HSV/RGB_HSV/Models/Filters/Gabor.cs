using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Data;

namespace RGB_HSV.Models.Filters
{
    class Gabor
    {
        private static double _sigma { get; set; }
        private static double _gamma { get; set; }
        private static int _size { get; set; }

        private static double GaborFilterValue(int x, int y, double theta, double phi, double lambda, double gamma)
        {
            double rad = Math.PI / 180 * theta;
            double xx = x * Math.Cos(rad) + y * Math.Sin(rad);
            double yy = -x * Math.Sin(rad) + y * Math.Cos(rad);
            double sigma = lambda * 0.56;

            double envelopeVal = Math.Exp(-((xx * xx + gamma * gamma * yy * yy) / (2.0f * sigma * sigma)));

            double carrierVal = Math.Cos(2.0f * (float)Math.PI * xx / lambda + phi);

            double g = envelopeVal * carrierVal;

            return g;
        }

        public static double[,] CreateGaborFilter(int i)
        {
            _size = 3;

            var filter = new double[_size, _size];

            int windowSize = _size / 2;
            var sum = 0.0;
            for (int y = -windowSize; y <= windowSize; ++y)
            {
                for (int x = -windowSize; x <= windowSize; ++x)
                {
                    int dy = windowSize + y;
                    int dx = windowSize + x;


                    filter[dx, dy] = GaborFilterValue(x, y, i, 0, 2.0, 0.1);
                    sum += filter[dx, dy];

                }
            }

            for (int y = -windowSize; y <= windowSize; ++y)
            {
                for (int x = -windowSize; x <= windowSize; ++x)
                {
                    int dy = windowSize + y;
                    int dx = windowSize + x;

                    filter[dx, dy] /= sum;
                }
            }
            return filter;
        }


        public static Bitmap ApplyGabor(Bitmap sourceImage)
        {
            var width = sourceImage.Width;
            var height = sourceImage.Height;
            var srcData = sourceImage.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var bytes = srcData.Stride * srcData.Height;
            var pixelBuffer1 = new byte[bytes];
            var pixelBuffer = new byte[bytes];

            var resultBuffer = new byte[bytes];
            var srcScan0 = srcData.Scan0;
            Marshal.Copy(srcScan0, pixelBuffer, 0, bytes);
            sourceImage.UnlockBits(srcData);

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

            for (int i = 0; i < pixelBuffer.Length; i ++)
            {
                pixelBuffer1[i] = 0;
            }

            for (int i = 0; i < 180; i += 45)
            {
                var kernel = CreateGaborFilter(i);
                var mid = (kernel.GetLength(0) - 1) / 2;
                var colorChannels = 3;
                var kcenter = 0;
                var kpixel = 0;
                var rgbBuffer = new double[colorChannels];

                for (var y = mid; y < height - mid; ++y)
                {
                    for (var x = mid; x < width - mid; ++x)
                    {
                        for (var c = 0; c < colorChannels; ++c)
                        {
                            rgbBuffer[c] = 0.0;
                        }
                        kcenter = y * srcData.Stride + x * 4;
                        for (var fy = -mid; fy <= mid; ++fy)
                        {
                            for (var fx = -mid; fx <= mid; ++fx)
                            {
                                kpixel = kcenter + fy * srcData.Stride + fx * 4;
                                for (var c = 0; c < colorChannels; ++c)
                                {
                                    var a = (double)(pixelBuffer[kpixel + c]);
                                    rgbBuffer[c] += (double)(pixelBuffer[kpixel + c]) * kernel[fy + mid, fx + mid];
                                }
                            }
                        }
                        for (var c = 0; c < colorChannels; ++c)
                        {
                            if (rgbBuffer[c] > 255)
                            {
                                rgbBuffer[c] = 255;
                            }
                            else if (rgbBuffer[c] < 0)
                            {
                                rgbBuffer[c] = 0;
                            }
                        }
                        for (var c = 0; c < colorChannels; ++c)
                        {
                            pixelBuffer[kcenter + c] = (byte)rgbBuffer[c];
                            var value = pixelBuffer1[kcenter + c] + pixelBuffer[kcenter + c];
                                pixelBuffer1[kcenter + c] = (byte)value;
                        }
                        pixelBuffer[kcenter + 3] = 255;
                        pixelBuffer1[kcenter + 3] = 255;
                    }
                }
            }
            Bitmap resultImage = new Bitmap(width, height);
            BitmapData resultData = resultImage.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(pixelBuffer1, 0, resultData.Scan0, bytes);
            resultImage.UnlockBits(resultData);
            return resultImage;
        }
    }
}
