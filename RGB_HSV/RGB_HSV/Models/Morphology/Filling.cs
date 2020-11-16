using RGB_HSV.Models.Filters;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace RGB_HSV.Models.Morphology
{
    class Filling
    {
        private double[,] squarePrimitive = new double[,]
        {
            {0, 1, 0 },
            {1, 1, 1 },
            {0, 1, 0 }
        };


        private int[] invertImage(int[] image)
        {
            var invertedImage = new int[image.Length];
            for(var i = 0; i < image.Length; ++i)
            {
                invertedImage[i] = (byte)((image[i] == 1) ? 0 : 1);
            }
            return invertedImage;
        }

        private byte[] invertImage(byte[] image)
        {
            var invertedImage = new byte[image.Length];
            for (var i = 0; i < image.Length; ++i)
            {
                invertedImage[i] = (byte)((image[i] == 255) ? 0 : 255);
            }
            return invertedImage;
        }

        private int[] invertEdgeImage(int[] image, int width)
        {
            var invertedImage = new int[image.Length];
            for(var i = 0; i < image.Length; ++i)
            {
                invertedImage[i] = 0;
            }

            for (var i = 0; i < width; ++i)
            {
                invertedImage[i] = (byte)((image[i] == 0) ? 1 : 0);
            }

            for (var i = image.Length - width; i < image.Length; ++i)
            {
                invertedImage[i] = (byte)((image[i] == 0) ? 1 : 0);
            }

            for (var i = width; i < image.Length - width; i+=width)
            {
                for (var k = 0; k < 2; k++)
                {
                   invertedImage[i + k * (width - 1)] = (byte)((image[i + k * (width - 1)] == 0) ? 1 : 0);
                }
            }


            return invertedImage;
        }

        private int[] dilatation4(int height, int width, int[] markerImage, int[] invertedImage)
        {
            var filterOffsetY = 1;
            var filterOffsetX = 1;
            var calcOffset = 0;
            var byteOffset = 0;
            var result = new int[markerImage.Length];
            for(var i = 0; i < result.Length; ++i)
            {
                result[i] = 0;
            }
            for (var offsetY = 0; offsetY < height; ++offsetY)
            {
                for (var offsetX = 0; offsetX < width; ++offsetX)
                {
                    byteOffset = offsetY * width + offsetX;
                    if(markerImage[byteOffset] == 1)
                    {
                        for (var filterY = -filterOffsetY; filterY <= filterOffsetY; filterY++)
                        {
                            for (var filterX = -filterOffsetX; filterX <= filterOffsetX; filterX++)
                            {
                                calcOffset = (offsetY + filterY) * width + offsetX + filterX;
                              if(filterY + filterOffsetY >= 0 && filterY + filterOffsetY < squarePrimitive.Length
                                    && filterX + filterOffsetX >= 0 && filterX + filterOffsetX < squarePrimitive.Length
                                    && calcOffset >= 0 && calcOffset < result.Length)
                                {
                                    if (squarePrimitive[filterY + filterOffsetY, filterX + filterOffsetX] == 1 )
                                    {
                                        result[calcOffset] = 1;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            for (var i = 0; i < result.Length; ++i)
            {
                var max = 0;
                if (result[i] == invertedImage[i])
                {
                    max = result[i];
                }
                result[i] = max;
            }
            return result;
        }

        public int[] get4Byte(byte[] srcImage)
        {
            var result = new int[srcImage.Length/4];
            for(var i = 0; i < srcImage.Length; i+=4)
            {
                result[i/4] = srcImage[i] == 255 ? 1 : 0; 
            }
            return result;
        }

        public byte[] getByteImage(int[] srcImage)
        {
            var result = new byte[srcImage.Length*4];
            for (var i = 0; i < srcImage.Length*4; i+=4)
            {
                result[i] = (byte)(srcImage[i/4] == 1 ? 255 : 0);
                result[i + 1] = result[i];
                result[i + 2] = result[i];
                result[i + 3] = 255;
            }
            return result;
        }

        public IEnumerable<Bitmap> ApplyFilling(Bitmap srcImage)
        {
            ImageUtils image = new ImageUtils();
            var buffer = image.BitmapToBytes(srcImage);
            var width = image.Width;
            var height = image.Height;
            var bytes = image.Bytes;

            var buffer4 = get4Byte(buffer);

            var invertedImage = buffer4;
            var markerImage = invertEdgeImage(buffer4, bytes/4/height);

            var image1 = dilatation4(width, height, markerImage, invertedImage);
            var image2 = dilatation4(width, height, image1, invertedImage);

            bool isequal = false;
            long iter = 1;
            while (!isequal)
            {
                if (iter % 2 == 0)
                {
                    var result6 = invertImage(image2);
                    var byteImage6 = getByteImage(result6);
                }
                isequal = true;
                for (var i = 0; i < image1.Length; ++i)
                {
                    if (image1[i] != image2[i])
                    {
                        isequal = false;
                        break;
                    }
                }
                if (!isequal)
                {
                    image1 = image2;
                    image2 = dilatation4(height, width, image1, invertedImage);
                }
                iter++;
            }
            var byteImage = getByteImage(image2);
            yield return image.BytesToBitmap(byteImage);
        }
    }
}
