using RGB_HSV.Models.Morphology;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace RGB_HSV.Models.FindFigures
{
    class Ransac
    {
        private List<Point> _points;
        private Dictionary<Point, Point> _lineSegments = new Dictionary<Point, Point>();
        private int _greenColor = 128;

        private int FindFirstGreen(Bitmap src)
        {
            var h = src.Height;
            var w = src.Width;
            for (var i = 0; i < h; ++i)
            {
                for (var j = 0; j < w; ++j)
                {
                    if (src.GetPixel(j, i).G == _greenColor)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        private bool AddWithCondition(Point p, Bitmap src)
        {
            if (src.GetPixel(p.X, p.Y).G != 0)
            {
                _points.Add(p);
                return true;
            }
            return false;
        }

        public List<Point> GetPoints(Point p1, Point p2, Bitmap src)
        {
            _points = new List<Point>();
            if (p1.X == p2.X)
            {
                for (int y = p1.Y; y <= p2.Y; y++)
                {
                    Point p = new Point(p1.X, y);
                    if(!AddWithCondition(p, src))
                    {
                        break;
                    }
                }
            }
            else
            {
                if (p2.X < p1.X)
                {
                    Point temp = p1;
                    p1 = p2;
                    p2 = temp;
                }

                double deltaX = p2.X - p1.X;
                double deltaY = p2.Y - p1.Y;
                
                double deltaErr = deltaY / deltaX;
                double error = deltaErr >= 0 ? -1.0f : 1.0f;

                int y = p1.Y;
                for (int x = p1.X; x <= p2.X; x++)
                {
                    Point p = new Point(x, y);
                    if(!AddWithCondition(p, src))
                    {
                        break;
                    }

                    error += deltaErr;
                    if(deltaErr == 0)
                    {
                        continue;
                    }
                    if(error > 0)
                    {
                        while (error >= 0.0f)
                        {
                            y++;
                            if(!AddWithCondition(new Point(x, y), src))
                            {
                                break;
                            }
                            error -= 1.0f;
                        }
                    }
                    else if(error < 0)
                    {
                        while (error <= 0.0f)
                        {
                            y--;
                            if(y < 0)
                            {
                                error += 1.0f;
                                continue;
                            }
                            if(!AddWithCondition(new Point(x, y), src))
                            {
                                break;
                            }
                            error += 1.0f;
                        }
                    }
                }
                if (_points.Last() != p2)
                {
                    int index = _points.IndexOf(p2);
                    _points.RemoveRange(index + 1, _points.Count - index - 1);
                }
            }
            return _points;
        }


        private List<Point> dots = new List<Point>();

        private int CountingVotes(Point p1, Point p2)
        {
            var count = 0;
            foreach (var dot in dots)
            {
                var A = dot.X - p1.X;
                var B = dot.Y - p1.Y;
                var C = p2.X - p1.X;
                var D = p2.Y - p1.Y;

                var A1 = dot.X - p2.X;
                var B1 = dot.Y - p2.Y;

                double point = A * C + B * D;
                var len_sq = C * C + D * D;
                var param = -1.0;
                if(len_sq != 0)
                {
                    param = point / len_sq;
                }
                var xx = 0;
                var yy = 0;
                if(param < 0)
                {
                    xx = p1.X;
                    yy = p1.Y;
                }
                else if(param > 1)
                {
                    xx = p2.X;
                    yy = p2.Y;
                }
                else
                {
                    xx = (int)Math.Ceiling(p1.X + param * C);
                    yy = (int)Math.Ceiling(p1.Y + param * D);
                }
                var dx = dot.X - xx;
                var dy = dot.Y - yy;

                var dist = Math.Sqrt(dx * dx + dy * dy);
                if (dist == 0)
                {
                    count++;
                }
            }
            return count;
        }

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
                for(var j = 2; j < width - 2; ++j)
                {
                    if(result.GetPixel(j, i).R != 0)
                    {
                        dots.Add(new Point(j, i));
                    }
                }
            }
            
            for (var i = 0; i < dots.Count; i+=50)
            {
                var counts = new Dictionary<int, int>();
                for(var j = -i; j < dots.Count; j++)
                {
                    if (i + j < dots.Count && i+j != i && Math.Abs(dots[i].X - dots[j + i].X) < 25 && Math.Abs(dots[i].Y - dots[j + i].Y) < 25)
                    {
                        var neighbor = dots[i + j];
                        counts[j+i] = CountingVotes(dots[i], neighbor);
                    }
                }

                var greenPen = new Pen(Color.Green, 3);
                if(counts.Count == 0)
                {
                    continue;
                }
                var max = counts.Max(x => x.Value);
                if(max == 0)
                {
                    continue;
                }
                using (var graphics = Graphics.FromImage(result))
                {
                    var index = counts.First(x => x.Value == max).Key;
                    var points = GetPoints(dots[i], dots[index], result);

                    if (points.Count > 0)
                    {
                        var last = points.Last();
                        var first = points.First();

                        if (last.Y < first.Y)
                        {
                            Point temp = last;
                            last = first;
                            first = temp;
                        }

                        double tg = (last.X - first.X) != 0 ? (double)(Math.Abs(last.Y - first.Y)) / Math.Abs(last.X - first.X) : double.MaxValue;
                        if ((last.X - first.X) != 0 && ((tg == 0 && (last.X - first.X)>12)
                            || Math.Abs(tg - Math.Sqrt(3)) < 0.4))
                        {
                            graphics.DrawLine(greenPen, first, last);
                            _lineSegments[first] = last;
                        }
                    }
                }
            }

            var ind = 0;
            var prevYHorisontal = 0;
            var prev = new Point();
            foreach (var point in _lineSegments.Keys)
            {
                if ((ind == 0 || (point.Y - prev.Y > 10)) && ((point.Y - _lineSegments[point].Y) / (point.X - _lineSegments[point].X) == 0)
                    && (point.X - _lineSegments[point].X) < 0)
                {
                    var yellowPen = new Pen(Color.Yellow, 3);
                    using (var graphics = Graphics.FromImage(src))
                    {
                        if (ind == 0)
                        {
                            var firstGreenY = FindFirstGreen(result);
                            graphics.DrawRectangle(yellowPen, new Rectangle(new Point(point.X - 20, firstGreenY - 10), new Size(100, point.Y - firstGreenY + 10)));
                            prevYHorisontal = point.Y;
                        }
                        else
                        {
                            graphics.DrawRectangle(yellowPen, new Rectangle(new Point(point.X - 40, prevYHorisontal), new Size(75, point.Y - prevYHorisontal + 10)));
                        }
                        ind++;
                    }
                }
                prev = point;
            }
            return src;
        }
    }
}
