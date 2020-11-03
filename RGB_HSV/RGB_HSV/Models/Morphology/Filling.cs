using RGB_HSV.Models.Filters;
using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace RGB_HSV.Models.Morphology
{
    class Filling
    {
        private double[,] squarePrimitive = new double[,]
        {
            {1, 1, 1 },
            {1, 1, 1 },
            {1, 1, 1 }
        };


        private byte[] invertImage(byte[] image)
        {
            var invertedImage = new byte[image.Length];
            for(var i = 0; i < image.Length; ++i)
            {
                invertedImage[i] = (byte)((image[i] == 1) ? 0 : 1);
            }
            return invertedImage;
        }

        private byte[] invertEdgeImage(byte[] image, int width)
        {
            var invertedImage = new byte[image.Length];
            for(var i = 0; i < image.Length; ++i)
            {
                invertedImage[i] = 0;
            }

            for (var i = 0; i < width; ++i)
            {
                invertedImage[i] = (byte)((image[i] == 0) ? 255 : 0);
            }

            for (var i = image.Length - width; i < image.Length; ++i)
            {
                invertedImage[i] = (byte)((image[i] == 0) ? 255 : 0);
            }

            for (var i = width; i < image.Length - width; i+=width)
            {
                for (var k = 0; k < 2; k++)
                {
                    for (var j = 0; j < 4; j++)
                    {
                        invertedImage[i + j + k * (width - 4)] = (byte)((image[i + j + k * (width - 4)] == 0) ? 255 : 0);
                    }
                }
            }


            return invertedImage;
        }

        private byte[] dilatation(int height, int width, byte[] markerImage, byte[] invertedImage)
        {
            var filterOffsetY = 1;
            var filterOffsetX = 1;
            var calcOffset = 0;
            var byteOffset = 0;
            var result = new byte[markerImage.Length];
            for (var offsetY = 0; offsetY < height; ++offsetY)
            {
                for (var offsetX = 0; offsetX < width; ++offsetX)
                {
                    var max = 0;
                    byteOffset = offsetY * 4 * width + offsetX * 4;

                    for (var filterY = -filterOffsetY; filterY <= filterOffsetY; filterY++)
                    {
                        for (var filterX = -filterOffsetX; filterX <= filterOffsetX; filterX++)
                        {
                            calcOffset = byteOffset + filterX * 4 + filterY * 4 * width;
                            try
                            {
                                if (squarePrimitive[filterY + filterOffsetY, filterX + filterOffsetX] == 1
                                    && markerImage[calcOffset] > max)
                                {
                                    max = markerImage[calcOffset];
                                }
                            }
                            catch(IndexOutOfRangeException ex)
                            {

                            }
                        }
                    }
                    result[byteOffset] = (byte)(max);
                    result[byteOffset + 1] = (byte)(max);
                    result[byteOffset + 2] = (byte)(max);
                    result[byteOffset + 3] = 255;
                }
            }
            for (var i = 0; i < result.Length; ++i)
            {
                var max = 0;
                if (result[i] == invertedImage[i])
                {
                    max = result[i];
                }
                result[i] = (byte)max;
            }
            return result;
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
                    byteOffset = offsetY * width + offsetX;///13
                    if(markerImage[byteOffset] == 1)
                    {
                        for (var filterY = -filterOffsetY; filterY <= filterOffsetY; filterY++)
                        {
                            for (var filterX = -filterOffsetX; filterX <= filterOffsetX; filterX++)
                            {
                                calcOffset = (offsetY + filterY) * width + offsetX + filterX;
                                try
                                {
                                    if (squarePrimitive[filterY + filterOffsetY, filterX + filterOffsetX] == 1 )
                                    {
                                        result[calcOffset] = 1;
                                    }
                                }
                                catch (IndexOutOfRangeException ex)
                                {

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
            var result = new int[srcImage.Length];
            for(var i = 0; i < srcImage.Length; i++)
            {
                result[i] = srcImage[i] == 255 ? 1 : 0; 
            }
            return result;
        }

        public byte[] getByteImage(int[] srcImage)
        {
            var result = new byte[srcImage.Length];
            for (var i = 0; i < srcImage.Length; i++)
            {
                result[i] = (byte)(srcImage[i] == 1 ? 255 : 0);
                //result[i + 1] = result[i];
                //result[i + 2] = result[i];
                //result[i + 3] = 255;
            }
            return result;
        }

        public Bitmap ApplyFilling(Bitmap srcImage)
        {
            ImageUtils image = new ImageUtils();
            //  var buffer = image.BitmapToBytes(srcImage);
            var buffer = new byte[]
            { 1, 1, 1, 1, 1, 1,
             1, 0, 0, 0, 0, 1,
             1, 0, 1, 1, 0, 1,
             1, 0, 1, 1, 0, 1,
             1, 0, 1, 1, 0, 1,
             1, 0, 1, 1, 0, 1,
             1, 0, 0, 0, 0, 1,
             1, 1, 1, 1, 1, 1,
             1, 0, 1, 1, 1, 0
            };
            var width = image.Width;
            var height = image.Height;
            var bytes = image.Bytes;
            var result = new byte[bytes];

            //var rgb = 0.0;
            //for (int i = 0; i < buffer.Length; i += 4)
            //{
            //    rgb = buffer[i] * .3f;
            //    rgb += buffer[i + 1] * .6f;
            //    rgb += buffer[i + 2] * .1f;
            //    buffer[i] = (byte)rgb;
            //    buffer[i + 1] = buffer[i];
            //    buffer[i + 2] = buffer[i];
            //    buffer[i + 3] = 255;
            //}

            buffer = invertImage(buffer);
           
            var invertedImage = invertImage(buffer);
            var markerImage = invertEdgeImage(buffer, 6);

            var markerImage4 = get4Byte(markerImage);
            var invertedImage4 = get4Byte(invertedImage);

            var image1 = dilatation4(9, 6, markerImage4, invertedImage4);
            var image2 = dilatation4(9, 6, image1, invertedImage4);
            
            bool isequal = false;

            while (!isequal)
            {
                isequal = true;
                for (var i = 0; i < 54; ++i)
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
                    image2 = dilatation4(height, width, image1, invertedImage4);
                }
            }
            var byteImage = getByteImage(image2);
            result = invertImage(byteImage);
            return image.BytesToBitmap(result);
        }
    }
}
