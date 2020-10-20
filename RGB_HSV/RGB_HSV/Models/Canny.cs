using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace RGB_HSV.Models.Filters
{
    class Canny
    {
        public static Bitmap ApplyCanny(Bitmap sourceImage)
        {
            Bitmap bitmapImage = Sodel.ApplySodel(Blur.blurImage(sourceImage, 1.0));
            var width = bitmapImage.Width;
            var height = bitmapImage.Height;
            var values = Sodel.resultBuf;
            var directions = Sodel.directions;
            var supressionEdge = new byte[values.Length];
            var bigImage = new byte[values.Length * 4];
            var left = 0;
            var right = 0;
            for (var y = 0; y < height; ++y)
            {
                for (var x = 0; x < width; ++x)
                {
                    if (Math.Abs(directions[y * width + x]) == 0)
                    {
                        if ((x + 1 < width) && (x - 1 >= 0))
                        {
                            left = values[y * width + x + 1];
                            right = values[y * width + x - 1];
                        }
                        else
                        {
                            supressionEdge[y * width + x] = values[y * width + x];
                            continue;
                        }
                    }
                    else if (Math.Abs(directions[y * width + x]) == 45)
                    {
                        if ((x + 1 < width) && (x - 1 >= 0) || (y - 1 >= 0) || (y + 1 < height))
                        {
                            left = values[(y - 1) * width + x + 1];
                            right = values[(y + 1) * width + x - 1];
                        }
                        else
                        {
                            supressionEdge[y * width + x] = values[y * width + x];
                            continue;
                        }
                    }
                    else if (Math.Abs(directions[y * width + x]) == 90)
                    {
                        if ((y + 1 < width) && (y - 1 >= 0))
                        {
                            left = values[(y - 1) * width + x];
                            right = values[(y + 1) * width + x];
                        }
                        else
                        {
                            supressionEdge[y * width + x] = values[y * width + x];
                            continue;
                        }
                    }
                    else if (Math.Abs(directions[y * width + x]) == 135)
                    {
                        if ((x + 1 < width) && (x - 1 >= 0) || (y - 1 >= 0) || (y + 1 < height))
                        {
                            left = values[(y - 1) * width + (x - 1)];
                            right = values[(y + 1) * width + (x + 1)];
                        }
                        else
                        {
                            supressionEdge[y * width + x] = values[y * width + x];
                            continue;
                        }
                    }
                    else if (Math.Abs(directions[y * width + x]) == 180)
                    {
                        if ((x + 1 < width) && (x - 1 >= 0))
                        {
                            left = values[y * width + x + 1];
                            right = values[y * width + x - 1];
                        }
                        else
                        {
                            supressionEdge[y * width + x] = values[y * width + x];
                            continue;
                        }
                    }

                    if (values[y * width + x] >= left && values[y * width + x] >= right)
                    {
                        supressionEdge[y * width + x] = values[y * width + x];
                    }
                    else
                    {
                        supressionEdge[y * width + x] = 0;
                    }
                }
            }

            var commulate = 0;
            for (var y = 0; y < height; ++y)
            {
                for (var x = 0; x < width; ++x)
                {
                    commulate += values[(y) * width + x];
                }
            }
            commulate /= values.Length;

            var minValue = 10;
            var maxValue = 100;

            for (var y = 0; y < height; ++y)
            {
                for (var x = 0; x < width; ++x)
                {
                    if (supressionEdge[y * width + x] >= maxValue)
                    {
                        supressionEdge[y * width + x] = 255;
                    }
                    else if (supressionEdge[y * width + x] <= minValue)
                    {
                        supressionEdge[y * width + x] = 0;
                    }
                    else
                    {
                        supressionEdge[y * width + x] = 127;
                    }
                }
            }

            for (var y = 0; y < height; ++y)
            {
                for (var x = 0; x < width; ++x)
                {
                    if(supressionEdge[y * width + x] == 127)
                    {
                        if(x + 1 < width && x - 1 >= 0 && y + 1 < height && y - 1 >= 0)
                        {
                            if(supressionEdge[y * width + x + 1] == 255 ||
                                supressionEdge[(y - 1) * width + x + 1] == 255 ||
                                supressionEdge[(y - 1) * width + x] == 255 ||
                                supressionEdge[(y-1) * width + x - 1] == 255 ||
                                supressionEdge[y * width + x - 1] == 255||
                                supressionEdge[(y+1) * width + x - 1] == 255 ||
                                supressionEdge[(y+1) * width + x] == 255 ||
                                supressionEdge[(y+1) * width + x + 1] == 255)
                            {
                                supressionEdge[y * width + x] = 255;
                            }
                            else
                            {
                                supressionEdge[y * width + x] = 0;
                            }
                        }
                    }
                }
            }

            for (var y = 0; y < height; ++y)
            {
                for (var x = 0; x < width; ++x)
                {
                    bigImage[4 * y * width + 4 * x] = supressionEdge[y * width + x];
                    bigImage[4 * y * width + 4 * x + 1] = supressionEdge[y * width + x];
                    bigImage[4 * y * width + 4 * x + 2] = supressionEdge[y * width + x];
                    bigImage[4 * y * width + 4 * x + 3] = 255;
                }
            }

            Bitmap resultImage = new Bitmap(sourceImage.Width, sourceImage.Height);
            BitmapData resultData = resultImage.LockBits(new Rectangle(0, 0, sourceImage.Width, sourceImage.Height),
                ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(bigImage, 0, resultData.Scan0, bigImage.Length);
            resultImage.UnlockBits(resultData);

            return resultImage;
        }
    }
}
