using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace RGB_HSV.Models.Filters
{
    class Gabor
    {
        private double _sigma { get; set; }
        private double _gamma { get; set; }
        private int _size { get; set; }
        private double _lambda { get; set; }
        private double _phi { get; set; }

        public Gabor(double gamma, int size, double lambda, double phi)
        {
            _gamma = gamma;
            _size = size;
            _lambda = lambda;
            _phi = phi;
        }

        private double GaborFilterValue(int x, int y, double theta)
        {
            double rad =theta / 180 * Math.PI;
            double xx = x * Math.Cos(rad) + y * Math.Sin(rad);
            double yy = -x * Math.Sin(rad) + y * Math.Cos(rad);
            _sigma = _lambda * 0.56;

            double envelopeVal = Math.Exp(-((xx * xx + _gamma * _gamma * yy * yy) / (2.0f * _sigma * _sigma)));

            double carrierVal = Math.Cos(2.0f * (float)Math.PI * xx / _lambda + _phi);

            double g = envelopeVal * carrierVal;

            return g;
        }

        public double[,] CreateGaborFilter(int i)
        {
            var filter = new double[_size, _size];

            int windowSize = _size / 2;
            var sum = 0.0;
            for (int y = -windowSize; y <= windowSize; ++y)
            {
                for (int x = -windowSize; x <= windowSize; ++x)
                {
                    int dy = windowSize + y;
                    int dx = windowSize + x;

                    filter[dx, dy] = GaborFilterValue(x, y, i);
                    sum += filter[dx, dy];

                }
            }

            return filter;
        }


        public Bitmap ApplyGabor(Bitmap sourceImage)
        {
            ImageUtils image = new ImageUtils();
            var width = image.Width;
            var height = image.Height;
            var bytes = image.Bytes;
            var result = new byte[bytes];

            var mid = (_size) / 2;
            var values = new double[width, height];

            var kernel0 = CreateGaborFilter(0);
            var kernel45 = CreateGaborFilter(45);
            var kernel135 = CreateGaborFilter(135);
            var kernel90 = CreateGaborFilter(90);

            for (var y = 0; y < height; ++y)
            {
                for (var x = 0; x < width; ++x)
                {
                    var gaborValue = 0.0;
                    for (var fy = 0; fy < _size; ++fy)
                    {
                        for (var fx = 0; fx < _size; ++fx)
                        {
                            var value = 0.0;
                            if (x + fx - mid >= 0 && x + fx - mid < width && y + fy - mid >= 0 && y + fy - mid < height)
                            {
                                value = HSV.HsvFromColor(sourceImage.GetPixel(x + fx - mid, y + fy - mid)).V;
                            }
                            else
                            {
                                value = HSV.HsvFromColor(sourceImage.GetPixel(x, y)).V;
                            }

                            gaborValue += kernel0[fy, fx] * value;
                            gaborValue += kernel45[fy, fx] * value;
                            gaborValue += kernel90[fy, fx] * value;
                            gaborValue += kernel135[fy, fx] * value;
                        }
                    }
                    values[x, y] = gaborValue;
                }
            }

            var max = values[0, 0];
            var min = values[0,0];
            for (var y = 0; y < height; ++y)
            {
                for (var x = 0; x < width; ++x)
                {
                    if (values[x, y] > max)
                    {
                        max = values[x, y];
                    }
                    if (values[x, y] < min)
                    {
                        min = values[x, y];
                    }
                }
            }
            for (var y = 0; y < height; ++y)
            {
                for (var x = 0; x < width; ++x)
                {
                    if (min < 0)
                    {
                        if (max >= 0)
                        {
                            values[x, y] -= min;
                            values[x, y] /= -min + max;
                            values[x, y] *= 100;
                        }
                        else
                        {
                            values[x, y] -= min;
                            values[x, y] /= -min - max;
                            values[x, y] *= 100;
                        }
                    }
                    else
                    {
                        values[x, y] += min;
                        values[x, y] /= min + max;
                        values[x, y] *= 100;
                    }
                }
            }

            for (var i = 0; i < bytes; i += 4)
            {
                HSV hsv = new HSV { H = 0, S = 0, V = values[(i / 4) % width, (i / 4) / width] };
                hsv.ToColor();
                result[i + 0] = hsv.ToColor().R;
                result[i + 1] = hsv.ToColor().G;
                result[i + 2] = hsv.ToColor().B;
                result[i + 3] = 255;
            }
            return image.BytesToBitmap(result);
        }
    }
}
