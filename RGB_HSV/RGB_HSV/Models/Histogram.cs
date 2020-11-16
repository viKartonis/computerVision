using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace RGB_HSV.Models
{
    class Histogram
    {
        private const int widthBarChart = 200;
        private const int heightBarChart = 100; 

        private Dictionary<int, int> getDictionary(Bitmap bitmap)
        {
            var width = bitmap.Width;
            var height = bitmap.Height;
            Color pixel;
            var lValues = new Dictionary<int, int>();
            Lab lab;
            for (var i = 0; i < height; ++i)
            {
                for (var j = 0; j < width; ++j)
                {
                    pixel = bitmap.GetPixel(j, i);
                    var xyz = XYZ.XyzFromColor(pixel);
                    lab = Lab.LabFromXYZ(xyz);
                    var l = (int)lab.L;
                    lValues[l] = lValues.TryGetValue(l, out int val) ? val + 1 : 1;
                }
            }
            return lValues;
        }

        public Bitmap showBarChart(Bitmap bitmap)
        {
            var width = bitmap.Width;
            var height = bitmap.Height;

            var image = bitmap.Clone() as Bitmap;
            var lValues = getDictionary(image);
            var max = lValues.Max(kv => kv.Value);
            var lValuesCount = lValues.Count;

            var extandBarChart = new Bitmap(widthBarChart, heightBarChart);
            var color = new Color();
            color = Color.FromArgb(Byte.MaxValue, Byte.MaxValue, Byte.MaxValue);

            for (var i = 0; i < widthBarChart; ++i)
            {
                for (var j = 0; j < heightBarChart; ++j)
                {
                    extandBarChart.SetPixel(i, j, color);
                }
            }
            color = Color.FromArgb(0, 0, 0);

            for (var i = 0; i < widthBarChart; i += 2)
            {
                if (lValues.ContainsKey(i / 2))
                {
                    var currentHeight = (int)Math.Floor(1.0 * lValues[i / 2] / max * 100);
                    for (var j = heightBarChart - currentHeight; j < heightBarChart; ++j)
                    {
                        extandBarChart.SetPixel(i, j, color);
                        extandBarChart.SetPixel(i + 1, j, color);
                    }
                }
            }
            return extandBarChart;
            //BarChart = updateBitmap(extandBarChart);
        }
    }
}
