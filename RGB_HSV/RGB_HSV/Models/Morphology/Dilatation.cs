using RGB_HSV.Models.Filters;
using System.Drawing;

namespace RGB_HSV.Models.Morphology
{
    class Dilatation
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

        public Bitmap ApplyDilatation(Bitmap srcImage)
        {
            ImageUtils image = new ImageUtils();
            var buffer = image.BitmapToBytes(srcImage);
            var width = image.Width;
            var height = image.Height;
            var bytes = image.Bytes;
            var result = new byte[bytes];

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
            var filterOffsetY = 1;
            var filterOffsetX = 1;
            var calcOffset = 0;
            var byteOffset = 0;

            for (var offsetY = filterOffsetY; offsetY < height - filterOffsetY; ++offsetY)
            {
                for (var offsetX = filterOffsetX; offsetX < width - filterOffsetX; ++offsetX)
                {
                    var min = 255;
                    byteOffset = offsetY * 4 * width + offsetX * 4;

                    for (var filterY = -filterOffsetY; filterY <= filterOffsetY; filterY++)
                    {
                        for (var filterX = -filterOffsetX; filterX <= filterOffsetX; filterX++)
                        {
                            calcOffset = byteOffset + filterX * 4 + filterY * 4 * width;
                            if (squarePrimitive[filterY + filterOffsetY, filterX + filterOffsetX] == 1
                                && buffer[calcOffset] < min)
                            {
                                min = buffer[calcOffset];
                            }
                        }
                    }
                    result[byteOffset] = (byte)(min);
                    result[byteOffset + 1] = (byte)(min);
                    result[byteOffset + 2] = (byte)(min);
                    result[byteOffset + 3] = 255;
                }
            }

            return image.BytesToBitmap(result);
        }
    }
}
