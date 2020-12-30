using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Factorization;
using RGB_HSV.Models.Formats;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RGB_HSV.Models.Segmantation
{
    class NormCut
    {
        private double[,] diagonal;

        private double Distance(LCH first, LCH second)
        {
            var lab1 = LCH.LCHToLab(first);
            var lab2 = LCH.LCHToLab(second);
            var deltaL = second.L - first.L;
            var lRoof = (first.L + second.L) / 2;
            var sl = 1 + 0.015 * Math.Pow(lRoof - 50, 2) / Math.Sqrt(20 + Math.Pow(lRoof - 50, 2));
            var added1 = Math.Pow(deltaL / sl, 2);            
            var deltaC = second.C - first.C;
            var cRoof = (first.C + second.C) / 2;
            var sc = 1 + 0.045 * cRoof;
            var added2 = Math.Pow(deltaC / sc, 2);
            var smallDeltaH = 0.0;
            var a1 = lab1.A + lab1.A * (1 - Math.Sqrt(Math.Pow(cRoof, 7)/ (Math.Pow(cRoof, 7) + Math.Pow(25, 7))));
            var a2 = lab2.A + lab2.A * (1 - Math.Sqrt(Math.Pow(cRoof, 7) / (Math.Pow(cRoof, 7) + Math.Pow(25, 7))));
            var c1 = Math.Sqrt(Math.Pow(a1, 2) + Math.Pow(a2, 2));
            var h1 = Math.Atan2(lab1.B, a1) % 360;
            var h2 = Math.Atan2(lab2.B, a2) % 360;
            if (Math.Abs(h1 - h2) <= 180)
            {
                smallDeltaH = h2 - h1;
            }
            else if(Math.Abs(h1 - h2) > 180 && h2 <= h1)
            {
                smallDeltaH = h2 - h1 + 360;
            }
            else if(Math.Abs(h1 - h2) > 180 && h2 > h1)
            {
                smallDeltaH = h2 - h1 - 360;
            }
            var deltaH = 2 * Math.Sqrt(first.C*second.C) *Math.Sin(smallDeltaH/2);
            var hRoof = 0.0;
            if (Math.Abs(h1 - h2) <= 180)
            {
                hRoof = (h2 + h1)/2;
            }
            else if (Math.Abs(h1 - h2) > 180 && h2 + h1 < 360)
            {
                hRoof = (h2 + h1 + 360)/2;
            }
            else if (Math.Abs(h1 - h2) > 180 && h2 + h1 >= 360)
            {
                hRoof = (h2 + h1 - 360) / 2;
            }
            var t = 1 - 0.17 * Math.Cos(hRoof - 30) + 0.24 * Math.Cos(2 * hRoof) 
                + 0.32 * Math.Cos(2 * hRoof + 6) - 0.2 * Math.Cos(4 * hRoof - 63);
            var sh = 1 + 0.015*cRoof*t;
            var rt = -2 * Math.Sqrt(Math.Pow(cRoof, 7) / (Math.Pow(cRoof, 7) + Math.Pow(25, 7)))
                * Math.Sin(60 * Math.Exp(-Math.Pow((hRoof - 275) / 25, 2)));
            var added3 = Math.Pow(deltaH/sh, 2);
            var added4 = rt * deltaC/sc * deltaH/sh;
            return Math.Sqrt(added1 + added2 + added3 + added4);
        }

       private double[,] MakeMatrixes(LCH[,] srcImage)
        {
            var height = srcImage.GetUpperBound(0) + 1;
            var width = srcImage.Length / height;
            var matrix = new double[width * height, width * height];
            diagonal = new double[width * height, width * height];
            for (var y = 0; y < height; ++y)
            {
                for (var x = 0; x < width; ++x)
                {
                    var weight = 0.0;

                    if (y - 1 >= 0)
                    {
                        matrix[(y - 1) * width + x, y * width + x] = Distance(srcImage[y - 1, x], srcImage[y, x]);
                        matrix[y * width + x, (y - 1) * width + x] = matrix[(y - 1) * width + x, y * width + x];
                        weight += matrix[y * width + x, (y - 1) * width + x];
                    }
                    if (y - 1 >= 0 && x - 1 >= 0)
                    {
                        matrix[(y - 1) * width + x - 1, y * width + x] = Distance(srcImage[y - 1, x - 1], srcImage[y, x]);
                        matrix[y * width + x, (y - 1) * width + x - 1] = matrix[(y - 1) * width + x - 1, y * width + x];
                        weight += matrix[y * width + x, (y - 1) * width + x - 1];
                    }
                    if (y - 1 >= 0 && x + 1 < width)
                    {
                        matrix[(y - 1) * width + x + 1, y * width + x] = Distance(srcImage[y - 1, x + 1], srcImage[y, x]);
                        matrix[y * width + x, (y - 1) * width + x + 1] = matrix[(y - 1) * width + x + 1, y * width + x];
                        weight += matrix[y * width + x, (y - 1) * width + x + 1];
                    }
                    if (x + 1 < width)
                    {
                        matrix[y * width + x + 1, y * width + x] = Distance(srcImage[y, x + 1], srcImage[y, x]);
                        matrix[y * width + x, y * width + x + 1] = matrix[y * width + x + 1, y * width + x];
                        weight += matrix[y * width + x, y * width + x + 1];
                    }
                    if (y + 1 < height && x + 1 < width)
                    {
                        matrix[(y + 1) * width + x + 1, y * width + x] = Distance(srcImage[y + 1, x + 1], srcImage[y, x]);
                        matrix[y * width + x, (y + 1) * width + x + 1] = matrix[(y + 1) * width + x + 1, y * width + x];
                        weight += matrix[y * width + x, (y + 1) * width + x + 1];
                    }
                    if (y + 1 < height)
                    {
                        matrix[(y + 1) * width + x, y * width + x] = Distance(srcImage[y + 1, x], srcImage[y, x]);
                        matrix[y * width + x, (y + 1) * width + x] = matrix[(y + 1) * width + x, y * width + x];
                        weight += matrix[y * width + x, (y + 1) * width + x];
                    }
                    if (y + 1 < height && x - 1 >= 0)
                    {
                        matrix[(y + 1) * width + x - 1, y * width + x] = Distance(srcImage[y + 1, x - 1], srcImage[y, x]);
                        matrix[y * width + x, (y + 1) * width + x - 1] = matrix[(y + 1) * width + x - 1, y * width + x];
                        weight += matrix[y * width + x, (y + 1) * width + x - 1];
                    }
                    if (x - 1 >= 0)
                    {
                        matrix[y * width + x - 1, y * width + x] = Distance(srcImage[y, x - 1], srcImage[y, x]);
                        matrix[y * width + x, y * width + x - 1] = matrix[y * width + x - 1, y * width + x];
                        weight += matrix[y * width + x, y * width + x - 1];
                    }
                    diagonal[y * width + x, y * width + x] = weight;
                }
            }
            return matrix;
        }

        public Bitmap ApplyMethod(Bitmap srcImage)
        {
            var width = srcImage.Width;
            var height = srcImage.Height;
            var lhcImage = LCH.RGBToLch(srcImage);
            var W = MakeMatrixes(lhcImage);
            var matrixDif = new double[width * height, width * height];
            for (var i = 0; i < width * height; ++i)
            {
                for (var j = 0; j < width * height; ++j)
                {
                    matrixDif[i, j] = W[i, j] - diagonal[i, j];
                }
            }
            Matrix<double> matrix = DenseMatrix.OfArray(matrixDif);
            Evd<double> eigen = matrix.Evd();
            Vector<Complex> eigenvector = eigen.EigenValues;

            var resultBitmap = new Bitmap(width, height);
            var firstRSum = 0.0;
            var secondRSum = 0.0;
            var firstGSum = 0.0;
            var secondGSum = 0.0;
            var firstBSum = 0.0;
            var secondBSum = 0.0;
            var firstGeneralSum = 0;
            var secondGeneralSum = 0;

            for (var i = 0; i < width; ++i)
            {
                for (var j = 0; j < height; ++j)
                {
                    if (eigenvector[i].Real < 0)
                    {
                        firstRSum += srcImage.GetPixel(i, j).R;
                        firstGSum += srcImage.GetPixel(i, j).G;
                        firstBSum += srcImage.GetPixel(i, j).B;
                        firstGeneralSum++;
                    }
                    else
                    {
                        secondRSum += srcImage.GetPixel(i, j).R;
                        secondGSum += srcImage.GetPixel(i, j).G;
                        secondBSum += srcImage.GetPixel(i, j).B;
                        secondGeneralSum++;
                    }
                }
            }
            for (var i = 0; i < width; ++i)
            {
                for (var j = 0; j < height; ++j)
                {
                    if (eigenvector[i].Real < 0)
                    {
                        resultBitmap.SetPixel(i, j, Color.FromArgb((int)firstRSum/firstGeneralSum, (int)firstGSum / firstGeneralSum, (int)firstBSum / firstGeneralSum));
                    }
                    else
                    {
                        resultBitmap.SetPixel(i, j, Color.FromArgb((int)secondRSum / secondGeneralSum, (int)secondGSum / secondGeneralSum, (int)secondBSum / secondGeneralSum));
                    }
                }
            }
            return resultBitmap;
        }
    }
}
