using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace RGB_HSV.Models
{
    class KMeans
    {
        struct Pixel
        {
            public Color Color { get; set; }
            public int X { get; set; }
            public int Y { get; set; }

            public Pixel(Color color, int x, int y)
            {
                Color = color;
                X = x;
                Y = y;
            }
        }


        private List<Color> Centers = new List<Color>
        {
            //Color.FromArgb(66, 90, 157),
            //Color.FromArgb(105, 128, 180),
            //Color.FromArgb(85, 93, 139),
            //Color.FromArgb(139, 149, 183),
            //Color.FromArgb(191, 190, 204)
            Color.Blue,
            Color.Red,
            Color.Black,
            Color.White,
            Color.Green,
            Color.Yellow,
            Color.Purple,
            Color.AliceBlue,
            Color.Beige,
            Color.CadetBlue,
            Color.DarkBlue
        };

        private Dictionary<Color, List<Pixel>> MakeClusters(IReadOnlyCollection<Color> centers)
        {
            var clusters = new Dictionary<Color, List<Pixel>>();
            foreach (var center in centers)
            {
                clusters.Add(center, new List<Pixel>());
            }
            return clusters;
        }

        private void ApplyCriteria(Pixel pixel, Dictionary<Color, List<Pixel>> clusters)
        {
            var R = pixel.Color.R;
            var G = pixel.Color.G;
            var B = pixel.Color.B;

            var directions = new Dictionary<Color, double>();

            foreach (var center in clusters.Keys)
            {
                directions.Add(center, Math.Sqrt(Math.Pow((center.R - R), 2) + Math.Pow((center.G - G), 2) + Math.Pow((center.B - B), 2)));
            }
            var min = directions.Aggregate((a, b) => a.Value > b.Value ? b : a);
            clusters[min.Key].Add(pixel);
        }

        private Dictionary<Color, List<Pixel>> RebuildCenters(Dictionary<Color, List<Pixel>> clusters)
        {
            var tmp = new Dictionary<Color, List<Pixel>>();
            foreach (var cluster in clusters)
            {
                if (!cluster.Value.Any())
                {
                    continue;
                }
                var meanR = 0;
                var meanG = 0;
                var meanB = 0;
                foreach (var color in cluster.Value)
                {
                    meanR += color.Color.R;
                    meanG += color.Color.G;
                    meanB += color.Color.B;
                }
                meanR /= cluster.Value.Count;
                meanG /= cluster.Value.Count;
                meanB /= cluster.Value.Count;
                var center = Color.FromArgb(meanR, meanG, meanB);
                tmp.Add(center, cluster.Value);
            }
            return tmp;
        }

        public Bitmap ApplyMethod(Bitmap srcImage)
        {
            var clusters = MakeClusters(Centers);
            var width = srcImage.Width;
            var height = srcImage.Height;
            IReadOnlyCollection<Color> prevCenters;
            do
            {
                prevCenters = clusters.Keys;
                clusters = MakeClusters(clusters.Keys);
                for (var i = 0; i < height; i++)
                {
                    for (var j = 0; j < width; j++)
                    {
                        ApplyCriteria(new Pixel(srcImage.GetPixel(j, i), j, i), clusters);
                    }
                }
                clusters = RebuildCenters(clusters);
            
            }
            while (!prevCenters.SequenceEqual(clusters.Keys));

            var result = new Bitmap(width, height);
            foreach(var cluster in clusters)
            {
                foreach(var pixel in cluster.Value)
                {
                    result.SetPixel(pixel.X, pixel.Y, cluster.Key);
                }
            }
            return result;
        }
    }
}
