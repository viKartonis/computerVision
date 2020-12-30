using RGB_HSV.Models.Filters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGB_HSV.Models.LocalFeatures
{
    class SIFT
    {
        private struct Descriptor
        {
            public Point _keyPoint;
            public Point _squarePointPosition;
            public double[] _gradients;

            public Descriptor(Point keyPoint, Point squarePointPosition, double[] gradients)
            {
                _keyPoint = keyPoint;
                _squarePointPosition = squarePointPosition;
                _gradients = gradients;
            }
        }

        private Bitmap DifferenceOfGaussians(Bitmap gaussian1, Bitmap gaussian2)
        {
            var width = gaussian1.Width;
            var height = gaussian1.Height;

            var resultImage = new Bitmap(width, height);

            for (var i = 0; i < height; ++i)
            {
                for (var j = 0; j < width; ++j)
                {
                    var pixelR = gaussian1.GetPixel(j, i).R - gaussian2.GetPixel(j, i).R;
                    resultImage.SetPixel(j, i, Color.FromArgb(Math.Abs(pixelR), Math.Abs(pixelR), Math.Abs(pixelR)));
                }
            }
            return resultImage;
        }

        public Bitmap ApplyMethod(Bitmap src)
        {
            var width = src.Width;
            var height = src.Height;
            var scales = new double[width, height];

            var gaussians = new List<Bitmap>();
            var laplasians = new List<Bitmap>();
            var features = new List<Point>();
            var descriptors = new List<Descriptor>();

            var utils = new ImageUtils();
            var bitmapImage = Sodel.ApplySodel(Blur.ApplyMethod(src, 1.0));
            var gradientDirections = utils.GetTwoDim(Sodel.directions, width, height);

            var grayScaleSrc = utils.ColorToGrayScale(src);
            for (var sigma = 0.5; sigma <= 2.5; sigma += 0.5)
            {
                gaussians.Add(Blur.ApplyMethod(grayScaleSrc, sigma));
            }

            for(var i = 0; i < gaussians.Count - 1; ++i)
            {
                laplasians.Add(DifferenceOfGaussians(gaussians[i + 1], gaussians[i]));
            }

            for (var i = 0; i < height; ++i)
            {
                for (var j = 0; j < width; ++j)
                {
                    var max = 0.0;
                    for (var k = 0; k < laplasians.Count; ++k)
                    {
                        var pixel = laplasians[k].GetPixel(j, i).R;
                        if (j - 1 >= 0 && i - 1 >= 0 && j + 1 < width && i + 1 < height && pixel >= 3)
                        {
                            if (pixel >= laplasians[k].GetPixel(j - 1, i).R
                               && pixel >= laplasians[k].GetPixel(j, i - 1).R
                               && pixel >= laplasians[k].GetPixel(j - 1, i - 1).R
                               && pixel >= laplasians[k].GetPixel(j + 1, i).R
                               && pixel >= laplasians[k].GetPixel(j, i + 1).R
                               && pixel >= laplasians[k].GetPixel(j + 1, i + 1).R
                               )
                            {
                                if (k - 1 >= 0 && pixel >= laplasians[k - 1].GetPixel(j - 1, i).R
                                   && pixel >= laplasians[k - 1].GetPixel(j, i - 1).R
                                   && pixel >= laplasians[k - 1].GetPixel(j, i).R
                                   && pixel >= laplasians[k - 1].GetPixel(j - 1, i - 1).R
                                   && pixel >= laplasians[k - 1].GetPixel(j + 1, i).R
                                   && pixel >= laplasians[k - 1].GetPixel(j, i + 1).R
                                   && pixel >= laplasians[k - 1].GetPixel(j + 1, i + 1).R
                                   )
                                {
                                    if (k + 1 < laplasians.Count && pixel >= laplasians[k + 1].GetPixel(j - 1, i).R
                                            && pixel >= laplasians[k - 1].GetPixel(j, i).R
                                  && pixel >= laplasians[k + 1].GetPixel(j, i - 1).R
                                  && pixel >= laplasians[k + 1].GetPixel(j - 1, i - 1).R
                                  && pixel >= laplasians[k + 1].GetPixel(j + 1, i).R
                                  && pixel >= laplasians[k + 1].GetPixel(j, i + 1).R
                                  && pixel >= laplasians[k + 1].GetPixel(j + 1, i + 1).R
                                  )
                                    {
                                        max = (k + 1) * 0.5;
                                    }
                                }
                            }
                        }
                    }
                    scales[j, i] = max;
                    if(max != 0)
                    {
                        features.Add(new Point(j, i));
                    }
                }
            }

            foreach(var point in features)
            {
                if(point.Y - 16 >= 0 && point.Y + 16 < height && point.X - 16 >= 0 && point.X + 16 < width)
                for(var i = point.Y - 8; i <= point.Y + 8; i+=4)
                {
                    for (var j = point.X - 8; j <= point.X + 8; j+=4)
                    {
                        var gradients = new double[8];
                        for( var sub_i = i; sub_i < i + 4; ++sub_i)
                        {
                            for (var sub_j = j; sub_j < j + 4; ++sub_j)
                            {
                                    //var coef = 1.0 / (2 * Math.PI) * Math.Exp(-(sub_i - sub_j) * (sub_i - sub_j) / 2);
                                    var a = gradientDirections[sub_i, sub_j] % 360;
                                    gradients[(int)a >= 0 ? (int)a/45 
                                        : (360 + (int)a)/45]++;
                            }
                        }
                        descriptors.Add(new Descriptor(point, new Point(j/4, i/4), gradients));
                    }
                }
            }

            for (var i = 0; i < height; i++)
            {
                for (var j = 0; j < width; j++)
                {
                    if (scales[j, i] > 0)
                    {
                        var threashold = scales[j, i];
                        for (var ii = -threashold; ii < threashold + 1; ++ii)
                        {
                            for (var jj = -threashold; jj < threashold + 1; ++jj)
                            {
                                if (i + ii < 0 || i + ii >= height || j + jj < 0 || j + jj >= width)
                                {
                                    continue;
                                }
                                src.SetPixel((int)(j + jj), (int)(i+ ii), Color.Green);
                            }
                        }
                    }
                }
            }

            return src;
        }
    }
}
