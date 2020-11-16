using RGB_HSV.Models.Filters;
using System.Drawing;

namespace RGB_HSV.Models.Morphology
{
    class Erosion
    {
        private double[,] squarePrimitive
        {
            get
            {
                return new double[,]
                {
                    {1, 1, 1 },
                    {1, 1, 1 },
                    {1, 1, 1 }
                };
            }
        }

        public Bitmap ApplyErosion(Bitmap srcImage)
        {
            ImageUtils image = new ImageUtils();
            var buffer = image.BitmapToBytes(srcImage);
            var width = image.Width;
            var height = image.Height;
            var bytes = image.Bytes;
            var result = new byte[bytes];

            var filterOffsetY = 1;
            var filterOffsetX = 1;
            var calcOffset = 0;
            var byteOffset = 0;

            for (var offsetY = filterOffsetY; offsetY < height - filterOffsetY; ++offsetY)
            {
                for (var offsetX = filterOffsetX; offsetX < width - filterOffsetX; ++offsetX)
                {
                    var max = 0;
                    byteOffset = offsetY * 4 * width + offsetX * 4;
                    
                    for (var filterY = -filterOffsetY; filterY <= filterOffsetY; filterY++)
                    {
                        for (var filterX = -filterOffsetX; filterX <= filterOffsetX; filterX++)
                        {   
                            calcOffset = byteOffset + filterX * 4 + filterY * 4 * width;
                            if(squarePrimitive[filterY + filterOffsetY, filterX + filterOffsetX] == 1
                                && buffer[calcOffset] > max)
                            {
                                max = buffer[calcOffset];
                            }
                        }
                    }
                    result[byteOffset] = (byte)(max);
                    result[byteOffset + 1] = (byte)(max);
                    result[byteOffset + 2] = (byte)(max);
                    result[byteOffset + 3] = 255;
                }
            }

            return image.BytesToBitmap(result);
        }
    }
}
