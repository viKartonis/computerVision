using System;
using System.Drawing;

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

        public static Bitmap ApplySodel(Bitmap srcImage)
        {
            var xkernel = xSobel;
            var ykernel = ySobel;

            ImageUtils image = new ImageUtils();
            var buffer = image.BitmapToBytes(srcImage);
            var width = image.Width;
            var height = image.Height;
            var bytes = image.Bytes;
            var result = new byte[bytes];

            directions = new double[bytes/4];
            resultBuf = new byte[bytes / 4];

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
            var x = 0.0;
            var y = 0.0;
            var results = 0.0;

            var filterOffset = 1;
            var calcOffset = 0;
            var byteOffset = 0;

            for(var offsetY = filterOffset; offsetY < height - filterOffset; ++offsetY)
            {
                for (var offsetX = filterOffset; offsetX < width - filterOffset; ++offsetX)
                {
                    x = y = 0;
                    results = 0.0;

                    byteOffset = offsetY * 4*width + offsetX * 4;

                    for(var filterY = -filterOffset; filterY <= filterOffset; filterY++)
                    {
                        for (var filterX = -filterOffset; filterX <= filterOffset; filterX++)
                        {
                            calcOffset = byteOffset + filterX * 4 + filterY * 4*width;
                            x += (double)(buffer[calcOffset]) * xkernel[filterY + filterOffset, filterX + filterOffset];
                            y += (double)(buffer[calcOffset]) * ykernel[filterY + filterOffset, filterX + filterOffset];
                        }
                    }
                    results += Math.Sqrt((x * x) + (y * y));

                    if (results > 255)
                    {
                        results = 255;
                    }
                    else if (results < 0)
                    {
                        results = 0;
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
                    
                    resultBuf[byteOffset / 4] = (byte)results;

                    result[byteOffset] = (byte)(results);
                    result[byteOffset + 1] = (byte)(results);
                    result[byteOffset + 2] = (byte)(results);
                    result[byteOffset + 3] = 255;
                }
            }
            return image.BytesToBitmap(result);
        }
    }
}
