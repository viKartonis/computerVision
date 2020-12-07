using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace RGB_HSV.Models.Segmantation
{
    class SplitAndMerge
    {
        private struct BitmapPosition
        {
            public Bitmap BitmapImage { get; set; }
            public int X { get; set; }
            public int Y { get; set; }

            public BitmapPosition(Bitmap bitmap, int x, int y) 
            {
                BitmapImage = bitmap;
                X = x;
                Y = y;
            }
        }

        private bool Criteria(Color background, Color pixel)
        {
            var distance = Math.Sqrt(Math.Pow((background.R - pixel.R), 2) + Math.Pow((background.G - pixel.G), 2)
                + Math.Pow((background.B - pixel.B), 2));
            return distance < 5 ? true : false;
        }

        private bool CriteriaMerge(Color background, Color pixel)
        {
            var distance = Math.Sqrt(Math.Pow((background.R - pixel.R), 2) + Math.Pow((background.G - pixel.G), 2)
                + Math.Pow((background.B - pixel.B), 2));
            return distance < 25 ? true : false;
        }

        private double Distance(Color background, Color pixel)
        {
            var distance = Math.Sqrt(Math.Pow((background.R - pixel.R), 2) + Math.Pow((background.G - pixel.G), 2)
                + Math.Pow((background.B - pixel.B), 2));
            return distance;
        }

        private Bitmap ApplyCriteria(Bitmap currentImagePart, int beginI, int beginJ, int endI, int endJ)
        {
            for (var i = beginI; i < endI; ++i)
            {
                for (var j = beginJ; j < endJ; ++j)
                {
                    if (!Criteria(currentImagePart.GetPixel(beginJ, beginI), currentImagePart.GetPixel(j, i)))
                    {
                        return currentImagePart.Clone(
                            new RectangleF(beginJ, beginI, currentImagePart.Width / 2, currentImagePart.Height / 2),
                            System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    }
                }
            }
            return currentImagePart;
        }

        private void MakeBitmap(Bitmap currentImagePart, Bitmap resultImage, int beginI, int beginJ, int endI, int endJ)
        {
            var sumR = 0;
            var sumG = 0;
            var sumB = 0;
            var width = currentImagePart.Width;
            var height = currentImagePart.Height;
            for (var i = 0; i < currentImagePart.Height; ++i)
            {
                for (var j = 0; j < currentImagePart.Width; ++j)
                {
                    sumR += currentImagePart.GetPixel(j, i).R;
                    sumG += currentImagePart.GetPixel(j, i).G;
                    sumB += currentImagePart.GetPixel(j, i).B;
                }
            }
            sumR /= width * height;
            sumG /= width * height;
            sumB /= width * height;
            for (var i = beginI; i < endI; ++i)
            {
                for (var j = beginJ; j < endJ; ++j)
                {
                    try
                    {
                        resultImage.SetPixel(j, i, Color.FromArgb(sumR, sumG, sumB));
                    }
                    catch(ArgumentOutOfRangeException ex)
                    {

                    }
                }
            }
        }

        private Bitmap Merge(Bitmap image)
        {
            var height = image.Height;
            var width = image.Width;
            Color A;
            Color B;
            Color C;
            for (var i = 1; i < width; ++i)
            {
                for (var j = 1; j < height; ++j)
                {
                    var bJIndex = j - 1;
                    B = image.GetPixel(i, bJIndex);
                    var cIIndex = i - 1;
                    C = image.GetPixel(cIIndex, j);
                    A = image.GetPixel(i, j);
                    if (!CriteriaMerge(B, A) && !CriteriaMerge(C, A))
                    {
                    }
                    else if (!CriteriaMerge(B, A) && CriteriaMerge(C, A))
                    {
                        image.SetPixel(i, j, C);
                    }
                    else if (CriteriaMerge(B, A) && !CriteriaMerge(C, A))
                    {
                        image.SetPixel(i, j, B);
                    }
                    else if (CriteriaMerge(B, A) && CriteriaMerge(C, A))
                    {
                        if (CriteriaMerge(B, C))
                        {
                            var min = Distance(C, A) < Distance(B, A) ? C : B;
                            image.SetPixel(i, j, min);
                        }
                        else
                        {
                            image.SetPixel(i, j, Distance(C, A) < Distance(B, A) ? C : B);
                        }
                    }
                }
            }
            return image;
        }

        private BitmapPosition ApplyCriteria(BitmapPosition source, int beginI, int beginJ, int endI, int endJ)
        {
            var part = ApplyCriteria(source.BitmapImage, beginI, beginJ, endI, endJ);
            return new BitmapPosition(part, source.X + beginJ, source.Y + beginI);
        }

        private Bitmap Split(int width, int height, Bitmap srcImage)
        {
            var stackImages = new Stack<BitmapPosition>();
            var listImages = new List<BitmapPosition>();

            stackImages.Push(new BitmapPosition(srcImage, 0, 0));
            while (stackImages.Any())
            {
                var currentImagePart = stackImages.Pop();
                if (currentImagePart.BitmapImage.Width >= width)
                {
                    var halfHeight = currentImagePart.BitmapImage.Height / 2;
                    var halfWidth = currentImagePart.BitmapImage.Width / 2;
                   var i = 0;
                   var j = 0;
                   var ki = 0;
                   var kj = 0;
                    while ( ki < 2)
                    {
                        kj = 0;
                        j = 0;
                        while(kj < 2)
                        {
                            var image = ApplyCriteria(currentImagePart, i, j,
                                i == 0 ? halfHeight : currentImagePart.BitmapImage.Height,
                                j == 0 ? halfWidth : currentImagePart.BitmapImage.Width);
                            if(image.BitmapImage.Width == currentImagePart.BitmapImage.Width)
                            {
                                listImages.Add(image);
                            }
                            else
                            {
                                stackImages.Push(image);
                            }
                            j += halfWidth;
                            kj++;
                        }
                        i += halfHeight;
                        ki++;
                    }
                }
                else
                {
                    listImages.Add(currentImagePart);
                }
            }
            Bitmap resultImage = new Bitmap(srcImage.Width, srcImage.Height);

            foreach(BitmapPosition currentPart in listImages)
            {
                MakeBitmap(currentPart.BitmapImage, resultImage, currentPart.Y, currentPart.X,
                   currentPart.Y + currentPart.BitmapImage.Height , currentPart.X + currentPart.BitmapImage.Width);
            }
            return Merge(resultImage);
        }

        public Bitmap ApplyMethod(Bitmap srcImage)
        {
            Bitmap image = srcImage.Clone(
                            new RectangleF(0, 0, 1024, 1024),
                            System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            return Split(image.Width/64, image.Height/64, image);
        }
    }
}
