using RGB_HSV.Models.Filters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGB_HSV.Models.Morphology
{
    class DistanceTransform
    {
        private double[,] squarePrimitive = new double[,]
        {
            {0, 1, 0 },
            {1, 1, 1 },
            {0, 1, 0 }
        };

        public Bitmap ApplyDistanceTransform(Bitmap srcImage)
        {
            ImageUtils image = new ImageUtils();
            var buffer = image.BitmapToBytes(srcImage);
            var width = image.Width;
            var height = image.Height;
            var bytes = image.Bytes;
            var result = new byte[bytes];

            var distances = new byte[bytes];
            for(var i = 0; i < distances.Length; ++i)
            {
                distances[i] = 0;
            }

            var filterOffsetY = 1;
            var filterOffsetX = 1;
            var calcOffset = 0;
            var byteOffset = 0;
            var hasBlackPixels = true;
            var blackPixels = 0;

            var count = 0;
            while (hasBlackPixels)
            {
                blackPixels = 0;///почему у резалте хранятся всякие разные значения?
                ///сделать так, чтобы выполнялось условие выхода из цикла
                for (var offsetY = filterOffsetY; offsetY < height - filterOffsetY; ++offsetY)
                {
                    for (var offsetX = filterOffsetX; offsetX < width - filterOffsetX; ++offsetX)
                    {
                        var max = 0;
                        byteOffset = offsetY * 4 * width + offsetX * 4;
                        if (buffer[byteOffset] == 0)///то есть фон - это 0(чёрный)
                        {
                            blackPixels++;
                        }

                        for (var filterY = -filterOffsetY; filterY <= filterOffsetY; filterY++)
                        {
                            for (var filterX = -filterOffsetX; filterX <= filterOffsetX; filterX++)
                            {
                                calcOffset = byteOffset + filterX * 4 + filterY * 4 * width;
                                if (squarePrimitive[filterY + filterOffsetY, filterX + filterOffsetX] == 1
                                    && buffer[calcOffset] > max)
                                {
                                    max = buffer[calcOffset];
                                }
                            }
                        }////понять, где заполнять расстояниями
                        ///может смотреть: если был чёрный, а стал белым, то заполнять?
                        var previousResult = buffer[byteOffset];//резалт же всегда ноль
                        result[byteOffset] = (byte)(max);
                        result[byteOffset + 1] = (byte)(max);
                        result[byteOffset + 2] = (byte)(max);///видимо тут те самые границы, которые сокращаются
                        result[byteOffset + 3] = 255;
                        if (previousResult == 0)
                        {
                            distances[byteOffset] = (byte)(distances[byteOffset] + 1);
                            distances[byteOffset + 1] = distances[byteOffset];
                            distances[byteOffset + 2] = distances[byteOffset];
                            distances[byteOffset + 3] = 255;
                        }
                    }
                }
                result.CopyTo(buffer, 0);
                if(blackPixels == 0)
                {
                    hasBlackPixels = false;
                }
                for (var i = 0; i < result.Length; ++i)
                {
                    result[i] = 0;
                }
                count++;//отладочная переменная
            }
            var maxDistance = 0;
            for (var i = 0; i < distances.Length; ++i)
            {
                if (distances[i] > maxDistance && distances[i] != 255)
                {
                    maxDistance = distances[i];
                }
            }

            var coast = 255.0 / maxDistance;
            for(var i = 0; i < distances.Length; i+=4)
            {
                distances[i] = (byte)(coast * distances[i]);
                distances[i + 1] = distances[i];
                distances[i + 2] = distances[i];
            }
            return image.BytesToBitmap(distances);
        }
    }
}
