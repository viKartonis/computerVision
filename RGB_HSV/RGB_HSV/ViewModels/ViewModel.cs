using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using RGB_HSV.Models;
using RGB_HSV.Models.Filters;

namespace RGB_HSV.ViewModels
{
    class ViewModel : INotifyPropertyChanged
    {
        private BitmapImage imageSource;
        private BitmapImage barChart;
        private string pixelInfo;

        public Bitmap bitmap;
        public BitmapImage ImageSource
        {
            get => imageSource;
            private set
            {
                imageSource = value;
                OnPropertyChanged(nameof(ImageSource));
            }
        }
        public BitmapImage BarChart
        {
            get => barChart;
            private set
            {
                barChart = value;
                OnPropertyChanged(nameof(BarChart));
            }
        }
        public string PixelInfo
        {
            get => pixelInfo;
            private set
            {
                pixelInfo = value;
                OnPropertyChanged(nameof(PixelInfo));
            }
        }

        public string HValue { get; set; }
        public string SValue { get; set; }
        public string VValue { get; set; }

        public string Sigma { get; set; }

        public ICommand UpdateHCommand { get; }
        public ICommand UpdateSCommand { get; }
        public ICommand UpdateVCommand { get; }
        public ICommand BluringCommand { get; }

        public ViewModel()
        {
            UpdateHCommand = new Command(changeH);
            UpdateSCommand = new Command(changeS);
            UpdateVCommand = new Command(changeV);
            BluringCommand = new Command(blurImage);
        }

        private const int widthBarChart = 200;
        private const int heightBarChart = 100;

        public event PropertyChangedEventHandler PropertyChanged;

        public void LoadImage(string fileName)
        {
            bitmap = new Bitmap(fileName);
            showBarChart();
            ImageSource = new BitmapImage(new Uri(fileName));
        }

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
        private BitmapImage updateBitmap(Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Bmp);
                memory.Position = 0;
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = new MemoryStream(memory.ToArray());
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }

        private void showBarChart()
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
            BarChart = updateBitmap(extandBarChart);
        }

        public void getPixelFormats(System.Windows.Point point)
        {
            Color color = bitmap.GetPixel(Convert.ToInt32(point.X), Convert.ToInt32(point.Y));
            byte red = color.R, green = color.G, blue = color.B;
            HSV hsv = HSV.HsvFromColor(color);
            XYZ xyz = XYZ.XyzFromColor(color);
            Lab lab = Lab.LabFromXYZ(xyz);
            PixelInfo = $"R: {color.R} G: {color.G} B: {color.B}\n"
                + $"H: {hsv.H}\nS: {hsv.S}\nV: {hsv.V}"
                + $"\nL: {lab.L}\na: {lab.A}\nb: {lab.B}";
        }

        public void changeH()
        {
            if (!double.TryParse(HValue, out var value))
            {
                return;
            }
            int width = bitmap.Width;
            int height = bitmap.Height;
            Bitmap trueBitmap = (Bitmap)bitmap.Clone();
            for (int i = 0; i < width; ++i)
            {
                for (int j = 0; j < height; ++j)
                {
                    Color pixel = bitmap.GetPixel(i, j);
                    HSV hsv = HSV.HsvFromColor(pixel);
                    hsv.H += value;
                    if (hsv.H > 360 || hsv.H < 0)
                    {
                        hsv.H -= value;
                        continue;
                    }
                    Color color = hsv.ToColor();
                    bitmap.SetPixel(i, j, color);
                }
            }
            ImageSource = updateBitmap(bitmap);

            showBarChart();
            bitmap = trueBitmap;
        }

        public void changeS()
        {
            if (!double.TryParse(SValue, out var value))
            {
                return;
            }
            int width = bitmap.Width;
            int height = bitmap.Height;
            Bitmap trueBitmap = (Bitmap)bitmap.Clone();
            for (int i = 0; i < width; ++i)
            {
                for (int j = 0; j < height; ++j)
                {
                    Color pixel = bitmap.GetPixel(i, j);
                    HSV hsv = HSV.HsvFromColor(pixel);
                    hsv.S += value;
                    if (hsv.S > 100 || hsv.S < 0)
                    {
                        hsv.S -= value;
                        continue;
                    }
                    Color color = hsv.ToColor();
                    bitmap.SetPixel(i, j, color);
                }
            }
            ImageSource = updateBitmap(bitmap);

            showBarChart();
            bitmap = trueBitmap;
        }

        public void changeV()
        {
            if (!double.TryParse(VValue, out var value))
            {
                return;
            }
            int width = bitmap.Width;
            int height = bitmap.Height;
            Bitmap trueBitmap = (Bitmap)bitmap.Clone();
            for (int i = 0; i < width; ++i)
            {
                for (int j = 0; j < height; ++j)
                {
                    Color pixel = bitmap.GetPixel(i, j);
                    HSV hsv = HSV.HsvFromColor(pixel);
                    hsv.V += value;
                    if (hsv.V > 100 || hsv.V < 0)
                    {
                        hsv.V -= value;
                        continue;
                    }
                    Color color = hsv.ToColor();
                    bitmap.SetPixel(i, j, color);
                }
            }
            ImageSource = updateBitmap(bitmap);

            showBarChart();
            bitmap = trueBitmap;
        }

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void blurImage()
        {
            if (!double.TryParse(Sigma, out var value))
            {
                return;
            }
            Bitmap blur = Blur.blurImage(bitmap, value);
            bitmap = blur;
            ImageSource = updateBitmap(blur);
            showBarChart();
        }

        public void ApplySobel()
        {
            Bitmap sobel = Sodel.ApplySodel(bitmap);
            bitmap = sobel;
            ImageSource = updateBitmap(sobel);
            showBarChart();
        }

        public void ApplyCanny()
        {
            Bitmap canny = Canny.ApplyCanny(bitmap);
            bitmap = canny;
            ImageSource = updateBitmap(canny);
            showBarChart();
        }

        public void ApplyGabor()
        {
            Gabor gaborFilter = new Gabor(0.1, 3, 2.0, 0);
            Bitmap gabor = gaborFilter.ApplyGabor(bitmap);
            bitmap = gabor;
            ImageSource = updateBitmap(gabor);
            showBarChart();
        }


        public void ApplyOtsu()
        {
            Bitmap otsu = Otsu.ApplyOtsu(bitmap);
            bitmap = otsu;
            ImageSource = updateBitmap(otsu);
            showBarChart();
        }

        public void RemoveBackground()
        {
            var width = bitmap.Width;
            var height = bitmap.Height;

            var hValues = new Dictionary<int, int>();
            var hsvImage = new Dictionary<Point, HSV>();
            var hue = 0;
            Color pixel;
            for (var i = 0; i < height; ++i)
            {
                for (var j = 0; j < width; ++j)
                {

                    pixel = bitmap.GetPixel(j, i);
                    var hsv = HSV.HsvFromColor(bitmap.GetPixel(j, i));
                    hue = (int)hsv.H;
                    if (hue < 0)
                    {
                        continue;
                    }
                    hsvImage[new Point(j, i)] = hsv;
                    hValues[hue] = hValues.TryGetValue(hue, out int val) ? val + 1 : 1;
                }
            }
            KMeans.Segmentation(hsvImage);
            foreach (Point point in hsvImage.Keys)
            {
                Color color = hsvImage[point].ToColor();
                bitmap.SetPixel(point.X, point.Y, color);
            }
            ImageSource = updateBitmap(bitmap);
        }
    }
}
