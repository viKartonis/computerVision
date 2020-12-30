using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Factorization;
using RGB_HSV.Models.Filters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RGB_HSV.Models.LocalFeatures
{
    class CornersDetector
    {
        private int _flag;
        private Dictionary<int, double[,]> _pixelMapM { get; set; }
        private Dictionary<int, Vector<Complex>> _pixelMapEigenValues { get; set; }
        private Dictionary<int, double> _pixelMapR { get; set; }

        public CornersDetector(int flag)
        {
            _flag = flag;
        }

        private double[,] getTwoDim(double[] srcImage, int width, int height)
        {
            var result = new double[height, width];
            var h = 0;
            var w = 0;
            for (var i = 0; i < srcImage.Length; i++)
            {
                h = i / width;
                w = i - h * width;
                result[h, w] = srcImage[i];
            }
            return result;
        }

        private int[] get4Byte(byte[] srcImage)
        {
            var result = new int[srcImage.Length / 4];
            for (var i = 0; i < srcImage.Length; i += 4)
            {
                result[i / 4] = srcImage[i];
            }
            return result;
        }

        private bool Condition(double _pixelMapR_i)
        {
            return (_flag == 0) ? (_pixelMapR_i > 5e-12) : (Math.Abs(_pixelMapR_i) < 1 && (Math.Abs(_pixelMapR_i) != 0));
        }

        public Bitmap ApplyMethod(Bitmap src)
        {
            Bitmap bitmapImage = Sodel.ApplySodel(Blur.ApplyMethod(src, 1.0));
            var gradientXValues = Sodel.xGradients;
            var gradientYValues = Sodel.yGradients;

            ImageUtils image = new ImageUtils();
            var buffer = image.BitmapToBytes(src);
            var width = image.Width;
            var height = image.Height;
            var bytes = image.Bytes;
            var cuttedImage = get4Byte(buffer);

            _pixelMapM = new Dictionary<int, double[,]>();
            _pixelMapEigenValues = new Dictionary<int, Vector<Complex>>();
            _pixelMapR = new Dictionary<int, double>();

            var gradXValuesTwoDim = getTwoDim(gradientXValues, width, height);
            var gradYValuesTwoDim = getTwoDim(gradientYValues, width, height);

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; ++j)
                {
                    var xGrad = 0.0;
                    var yGrad = 0.0;

                    xGrad += gradXValuesTwoDim[i, j];
                    yGrad += gradYValuesTwoDim[i, j];
                    var M = new double[,]
                    {
                    { xGrad * xGrad,
                     xGrad * yGrad },
                    { xGrad * yGrad,
                    yGrad * yGrad }
                    };
                    _pixelMapM[i * width + j] = M;
                }
            }

            for (int i = 0; i < cuttedImage.Length; ++i)
            {
                var M_i = _pixelMapM[i];
                Matrix<double> matrix = DenseMatrix.OfArray(M_i);
                Evd<double> eigen = matrix.Evd();
                Vector<Complex> eigenvector = eigen.EigenValues;
                _pixelMapEigenValues[i] = eigenvector;
            }

            for (int i = 0; i < cuttedImage.Length; ++i)
            {
                var arrayEigen = _pixelMapEigenValues[i];
                
                var det = arrayEigen[0].Real * arrayEigen[1].Real;
                var k = 0.05;
                var traceM = arrayEigen[0].Real + arrayEigen[1].Real;
                var b = (det - k * (traceM * traceM));
                _pixelMapR[i] = (_flag == 0) ? (det / traceM) : b;
            }

            var step = 24;
            for (var i = 0; i < height; i += step)
            {
                for (var j = 0; j < width; j += step)
                {
                    var max = Double.MinValue;
                    var maxII = 0;
                    var maxJJ = 0;
                    for (var ii = 0; ii < step; ++ii)
                    {
                        for (var jj = 0; jj < step; ++jj)
                        {
                            if (i + ii < 0 || i + ii >= height || j + jj < 0 || j + jj >= width)
                            {
                                continue;
                            }
                            if (_pixelMapR[(i + ii) * width + j + jj] > max)
                            {
                                maxII = ii;
                                maxJJ = jj;
                                max = _pixelMapR[(i + ii) * width + j + jj];
                            }

                        }
                    }
                    for (var ii = 0; ii < step; ++ii)
                    {
                        for (var jj = 0; jj < step; ++jj)
                        {
                            if (i + ii < 0 || i + ii >= height || j + jj < 0 || j + jj >= width)
                            {
                                continue;
                            }
                            if (ii != maxII || jj != maxJJ)
                            {
                                _pixelMapR[(i + ii) * width + j + jj] = 0;
                            }
                        }
                    }
                }
            }

            for (var i = 0; i < height; i++)
            {
                for (var j = 0; j < width; j++)
                {
                    if (Condition(_pixelMapR[i * width + j]))
                    {
                        for (var ii = -1; ii < 2; ++ii)
                        {
                            for (var jj = -1; jj < 2; ++jj)
                            {
                                if (i + ii < 0 || i + ii >= height || j + jj < 0 || j + jj >= width)
                                {
                                    continue;
                                }
                                buffer[4 * ((i + ii) * width + j + jj)] = 0;
                                buffer[4 * ((i + ii) * width + j + jj) + 1] = 255;
                                buffer[4 * ((i + ii) * width + j + jj) + 2] = 0;
                                buffer[4 * ((i + ii) * width + j + jj) + 3] = 255;
                            }
                        }
                    }
                }
            }
            Bitmap resultImage = new Bitmap(src.Width, src.Height);
            BitmapData resultData = resultImage.LockBits(new Rectangle(0, 0, src.Width, src.Height),
                ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            Marshal.Copy(buffer, 0, resultData.Scan0, buffer.Length);
            resultImage.UnlockBits(resultData);
            return resultImage;
        }
    }
}
