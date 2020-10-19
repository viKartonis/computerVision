using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace RGB_HSV.Models.Filters
{
    class Sodel
    {
        private static byte[] resultBuffer { get; set; }

        public static byte[] resultBuf { get; private set; }

        public static double[] directions { get; private set; }

        private static double[,] xSobel
        {
            get
            {
                return new double[,]
                {
                    {-1, 0, 1 },
                    {-2, 0, 2 },
                    {-1, 0, 1 }
                };
            }
        }

        private static double[,] ySobel
        {
            get
            {
                return new double[,]
                {
                    {1, 2, 1 },
                    {0, 0, 0 },
                    {-1, -2, -1 }
                };
            }
        }
        private static double NormalizeDirection(double value)
        {
            if(value > 10)
            {
                value *= 1;
            }
            if(value < -10)
            {
                value *= 1;
            }
            value %= 180;
            var normValue = (int) value / 45;
            var result = (Math.Abs(normValue * 45 - value) < Math.Abs(((value/2 >= 0 ? normValue + 1 : normValue - 1) * 45) - value))
                ? normValue * 45 : (value/2 >= 0 ? normValue + 1 : normValue - 1) * 45;
            return result;
        }

        public static Bitmap ApplySodel(Bitmap sourceImage)
        {
            var xkernel = xSobel;
            var ykernel = ySobel;
            var width = sourceImage.Width;
            var height = sourceImage.Height;
            var srcData = sourceImage.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var bytes = srcData.Stride * srcData.Height;
            var pixelBuffer = new byte[bytes];
            resultBuffer = new byte[bytes];
            directions = new double[bytes/4];
            resultBuf = new byte[bytes / 4];
            var srcScan0 = srcData.Scan0;
            Marshal.Copy(srcScan0, pixelBuffer, 0, bytes);
            sourceImage.UnlockBits(srcData);

            var rgb = 0.0;
            for(int i = 0; i < pixelBuffer.Length; i+=4)
            {
                rgb = pixelBuffer[i] * .3f;
                rgb += pixelBuffer[i + 1] * .6f;
                rgb += pixelBuffer[i + 2] * .1f;
                pixelBuffer[i] = (byte)rgb;
                pixelBuffer[i + 1] = pixelBuffer[i];
                pixelBuffer[i + 2] = pixelBuffer[i];
                pixelBuffer[i + 3] = 255;
            }
            var x = 0.0;
            var y = 0.0;
            var result = 0.0;

            var filterOffset = 1;
            var calcOffset = 0;
            var byteOffset = 0;

            for(var offsetY = filterOffset; offsetY < height - filterOffset; ++offsetY)
            {
                for (var offsetX = filterOffset; offsetX < width - filterOffset; ++offsetX)
                {
                    x = y = 0;
                    result = 0.0;

                    byteOffset = offsetY * srcData.Stride + offsetX * 4;

                    for(var filterY = -filterOffset; filterY <= filterOffset; filterY++)
                    {
                        for (var filterX = -filterOffset; filterX <= filterOffset; filterX++)
                        {
                            calcOffset = byteOffset + filterX * 4 + filterY * srcData.Stride;
                            x += (double)(pixelBuffer[calcOffset]) * xkernel[filterY + filterOffset, filterX + filterOffset];
                            y += (double)(pixelBuffer[calcOffset]) * ykernel[filterY + filterOffset, filterX + filterOffset];
                        }
                    }
                    result += Math.Sqrt((x * x) + (y * y));

                    if (result > 255)
                    {
                        result = 255;
                    }
                    else if (result < 0)
                    {
                        result = 0;
                    }

                    if (x != 0)
                    {
                        var dirNotNorm = 57.29 * 1.0 / Math.Tan(y / x);
                        if (Math.Tan(y / x) == 0)
                        {
                            directions[byteOffset / 4] = 0;
                        }
                        else
                        {
                            directions[byteOffset / 4] = NormalizeDirection(dirNotNorm);
                        }
                    }
                    else
                    {
                        directions[byteOffset / 4] = 0;
                    }
                    
                    resultBuf[byteOffset / 4] = (byte)result;

                    resultBuffer[byteOffset] = (byte)(result);
                    resultBuffer[byteOffset + 1] = (byte)(result);
                    resultBuffer[byteOffset + 2] = (byte)(result);
                    resultBuffer[byteOffset + 3] = 255;
                }
            }
            Bitmap resultImage = new Bitmap(width, height);
            BitmapData resultData = resultImage.LockBits(new Rectangle(0, 0, width, height),
                ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(resultBuffer, 0, resultData.Scan0, resultBuffer.Length);
            resultImage.UnlockBits(resultData);

            return resultImage;
        }
    }
}
