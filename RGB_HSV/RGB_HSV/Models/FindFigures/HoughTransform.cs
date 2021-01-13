using RGB_HSV.Models.Morphology;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace RGB_HSV.Models.FindFigures
{
    class HoughTransform
    {
        public struct PointInfo
        {
            public int F;
            public int R;

            public PointInfo(int f, int r)
            {
                F = f;
                R = r;
            }

        }

        private int FindFirstGreen(Bitmap src)
        {
            var h = src.Height;
            var w = src.Width;
            for (var i = 0; i < h; ++i)
            {
                for (var j = 0; j < w; ++j)
                {
                    if (src.GetPixel(j, i).G == 255 && src.GetPixel(j, i).R != 255)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        private bool Criteria(Color background, Color pixel)
        {
            var distance = Math.Sqrt(Math.Pow((background.R - pixel.R), 2) + Math.Pow((background.G - pixel.G), 2)
                + Math.Pow((background.B - pixel.B), 2));
            return distance < 15 ? true : false;
        }

        private Dictionary<Point, List<PointInfo>> _pointsInfo = new Dictionary<Point, List<PointInfo>>();
        private List<Point> dots = new List<Point>();

        public Bitmap ApplyMethod(Bitmap src)
        {
            var width = src.Width;
            var height = src.Height;

            var otsu = Otsu.ApplyOtsu(src);
            var erosionMethod = new Erosion();
            var erosion = erosionMethod.ApplyErosion(otsu);
            var result = new Bitmap(width, height);

            for (var i = 0; i < height; ++i)
            {
                for (var j = 0; j < width; ++j)
                {
                    if (erosion.GetPixel(j, i) != otsu.GetPixel(j, i))
                    {
                        result.SetPixel(j, i, Color.White);
                    }
                }
            }
            for (var i = 2; i < height - 2; ++i)
            {
                for (var j = 2; j < width - 2; ++j)
                {
                    if (result.GetPixel(j, i).R != 0)
                    {
                        dots.Add(new Point(j, i));
                    }
                }
            }

            var maxR = (int)Math.Sqrt(width*width + height*height);
            var phaseImage = new int[maxR, 180]; 
            var accuraccy = 1;

            for(var i = 0; i < dots.Count; i+=1)
            {
                var infos = new List<PointInfo>();
                
                for (var f = 0; f < 180; ++f)
                {
                    for(var r = 0; r < maxR; ++r)
                    {
                        var rad = f * Math.PI / 180.0;
                        if(Math.Abs(dots[i].Y * Math.Sin(rad) + dots[i].X * Math.Cos(rad) - r) < accuraccy)
                        {
                            phaseImage[r, f]++;
                            infos.Add(new PointInfo(f, r));
                        }
                    }
                }
                _pointsInfo[dots[i]] = infos;
            }

            var maxPhaseValues = new List<int>();
            var thetas = new List<double>();
            var Rs = new List<int>();
            for (var f = 60; f < 120; f+=30)
            {
                for (var r = 0; r < maxR; ++r)
                {
                    var a = phaseImage[r, f];
                    if (a > 1)
                    {
                        maxPhaseValues.Add(phaseImage[r, f]);
                        thetas.Add(f);
                        Rs.Add(r);
                    }
                }
            }

            var RsUniq = new List<double>();
            var horisontalValue = new Point(0, 0);

            for (var k = 0; k < maxPhaseValues.Count; ++k)
            { 
                if(RsUniq.Contains(Rs[k]))
                {
                    continue;
                }

                RsUniq.Add(Rs[k]);
                var theta = (thetas[k] * Math.PI / 180.0);

                for (var i = 0; i < height; ++i)
                {
                    for (var j = 0; j < width; ++j)
                    {
                        var a = i * Math.Sin(theta) + j * Math.Cos(theta);
                        var line = (a * 10) % 10 > 5 ? (int)(a+1) : (int)a;
                        if (line == Rs[k])
                        {
                            var point = new Point(j, i);
                            if (dots.Contains(point) && Criteria(src.GetPixel(j, i), Color.FromArgb(255, 99, 78, 88)))
                            {
                                result.SetPixel(j, i, Color.FromArgb(255, 0, 255, 0));
                                if(thetas[k] == 90)
                                {
                                    horisontalValue = point;
                                }
                            }
                        }
                    }
                }
            }
            var yellowPen = new Pen(Color.Yellow, 3);
            using (var graphics = Graphics.FromImage(src))
            {
                var firstGreenY = FindFirstGreen(result);
                graphics.DrawRectangle(yellowPen, new Rectangle(new Point(horisontalValue.X - 20, firstGreenY - 10),
                    new Size(85, 85)));
                graphics.DrawRectangle(yellowPen, new Rectangle(new Point(horisontalValue.X - 40, horisontalValue.Y),
                    new Size(55, 55)));
            }
            return src;
        }
    }
}
