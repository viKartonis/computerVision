using RGB_HSV.Models.Filters;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace RGB_HSV.Models
{
    class CountingObjects
    {
        public int[,] getBytesToInts(byte[] srcImage, int width, int height)
        {
            var result = new int[height, width];
            var h = 0;
            var w = 0;
            for (var i = 0; i < srcImage.Length; i += 4)
            {
                h = i / width / 4;
                w = i / 4 - h * width;
                result[h, w] = srcImage[i] == 255 ? 1 : 0;
            }
            return result;
        }

        public int CountObjects(Bitmap srcImage)
        {
            ImageUtils image = new ImageUtils();
            var buffer = image.BitmapToBytes(srcImage);
            var width = image.Width;
            var height = image.Height;
            var bytes = image.Bytes;
            var bufferInts = getBytesToInts(buffer, width, height);
            var labels = new byte[bufferInts.Length];

            var A = 0;
            var B = 0;
            var C = 0;
            var objects = 0;
            for (var i = 0; i < height; ++i)
            {
                for (var j = 0; j < width; ++j)
                {
                    var bJIndex = j - 1;
                    if (bJIndex < 0)
                    {
                        bJIndex = 1;
                        B = 0;
                    }
                    else
                    {
                        B = bufferInts[i, bJIndex];
                    }
                    var cIIndex = i - 1;
                    if (cIIndex < 0)
                    {
                        cIIndex = 1;
                        C = 0;
                    }
                    else
                    {
                        C = bufferInts[cIIndex, j];
                    }
                    A = bufferInts[i, j];
                    if (A == 0)
                    {
                    }
                    else if (B == 0 && C == 0)
                    {
                        objects++;
                        bufferInts[i, j] = objects;
                    }
                    else if (B != 0 && C == 0)
                    {
                        bufferInts[i, j] = B;
                    }
                    else if (C != 0 && B == 0)
                    {
                        bufferInts[i, j] = C;
                    }
                    else if (B != 0 && C != 0)
                    {
                        if (B == C)
                        {
                            bufferInts[i, j] = B;
                        }
                        else
                        {
                            bufferInts[i, j] = C;
                            var k = 1;
                            while (j - k >= 0 && bufferInts[i, j - k] != 0)
                            {
                                bufferInts[i, j - k] = C;
                                k++;
                            }
                        }
                    }
                }
            }
            var list = new LinkedList<int>();
            for (var i = 0; i < height; ++i)
            {
                for (var j = 0; j < width; ++j)
                {
                    Console.Write(bufferInts[i, j]);
                    if(!list.Contains(bufferInts[i, j]))
                    {
                        list.AddLast(bufferInts[i, j]);
                    }
                }
                Console.Write("\n");
            }
            return list.Count - 1;
        }
    }
}
