using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using RGB_HSV.Models;
using RGB_HSV.Models.Filters;
using RGB_HSV.Models.FindFigures;
using RGB_HSV.Models.LocalFeatures;
using RGB_HSV.Models.Morphology;
using RGB_HSV.Models.Segmantation;

namespace RGB_HSV.ViewModels
{
    class ViewModel : INotifyPropertyChanged
    {
        private BitmapImage _imageSource;
        private Histogram _histogram = new Histogram();
        private BitmapImage _barChart;
        private string _pixelInfo;

        public Bitmap BitmapProperty { get; set; }
        public BitmapImage ImageSource
        {
            get => _imageSource;
            private set
            {
                _imageSource = value;
                OnPropertyChanged(nameof(ImageSource));
            }
        }
        public BitmapImage BarChart
        {
            get => _barChart;
            private set
            {
                _barChart = value;
                OnPropertyChanged(nameof(BarChart));
            }
        }
        public string PixelInfo
        {
            get => _pixelInfo;
            private set
            {
                _pixelInfo = value;
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

        public event PropertyChangedEventHandler PropertyChanged;

        public void LoadImage(string fileName)
        {
            BitmapProperty = new Bitmap(fileName);
            BarChart = updateBitmap(_histogram.showBarChart(BitmapProperty));
            ImageSource = new BitmapImage(new Uri(fileName));
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

        public void getPixelFormats(System.Windows.Point point)
        {
            Color color = new Color();
            try
            {
               color = BitmapProperty.GetPixel(Convert.ToInt32(point.X), Convert.ToInt32(point.Y));
            }
            catch(ArgumentOutOfRangeException ex)
            {

            }
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
            HSV hsv = new HSV();
            hsv.changeH(BitmapProperty, HValue);
            ImageSource = updateBitmap(BitmapProperty);
            BarChart = updateBitmap(_histogram.showBarChart(BitmapProperty));
            BitmapProperty = hsv.PreviousBitmap;
        }

        public void changeS()
        {
            HSV hsv = new HSV();
            hsv.changeS(BitmapProperty, SValue);
            ImageSource = updateBitmap(BitmapProperty);
            BarChart = updateBitmap(_histogram.showBarChart(BitmapProperty));
            BitmapProperty = hsv.PreviousBitmap;
        }

        public void changeV()
        {
            HSV hsv = new HSV();
            hsv.changeV(BitmapProperty, VValue);
            ImageSource = updateBitmap(BitmapProperty);
            BarChart = updateBitmap(_histogram.showBarChart(BitmapProperty));
            BitmapProperty = hsv.PreviousBitmap;
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
            Bitmap blur = Blur.ApplyMethod(BitmapProperty, value);
            BitmapProperty = blur;
            ImageSource = updateBitmap(blur);
            BarChart = updateBitmap(_histogram.showBarChart(BitmapProperty));
        }

        public void ApplySobel()
        {
            Bitmap sobel = Sodel.ApplySodel(BitmapProperty);
            BitmapProperty = sobel;
            ImageSource = updateBitmap(sobel);
            BarChart = updateBitmap(_histogram.showBarChart(BitmapProperty));
        }

        public void ApplyCanny()
        {
            Bitmap canny = Canny.ApplyMethod(BitmapProperty);
            BitmapProperty = canny;
            ImageSource = updateBitmap(canny);
            BarChart = updateBitmap(_histogram.showBarChart(BitmapProperty));
        }

        public void SplitAndMerge()
        {
            SplitAndMerge segmantationMethod = new SplitAndMerge();
            Bitmap sam = segmantationMethod.ApplyMethod(BitmapProperty);
            BitmapProperty = sam;
            ImageSource = updateBitmap(sam);
            BarChart = updateBitmap(_histogram.showBarChart(BitmapProperty));
        }

        public void ApplyGabor()
        {
            Gabor gaborFilter = new Gabor(0.1, 3, 2.0, 0);
            Bitmap gabor = gaborFilter.ApplyGabor(BitmapProperty);
            BitmapProperty = gabor;
            ImageSource = updateBitmap(gabor);
            BarChart = updateBitmap(_histogram.showBarChart(BitmapProperty));
        }


        public void ApplyOtsu()
        {
            Bitmap otsu = Otsu.ApplyOtsu(BitmapProperty);
            BitmapProperty = otsu;
            ImageSource = updateBitmap(otsu);
            BarChart = updateBitmap(_histogram.showBarChart(BitmapProperty));
        }

        public void ApplyErosion()
        {
            Erosion erosionMethod = new Erosion();
            Bitmap erosion = erosionMethod.ApplyErosion(BitmapProperty);
            BitmapProperty = erosion;
            ImageSource = updateBitmap(erosion);
            BarChart = updateBitmap(_histogram.showBarChart(BitmapProperty));
        }

        public void ApplyRansac()
        {
            Ransac ransacMethod = new Ransac();
            Bitmap ransac = ransacMethod.ApplyMethod(BitmapProperty);
            BitmapProperty = ransac;
            ImageSource = updateBitmap(ransac);
            BarChart = updateBitmap(_histogram.showBarChart(BitmapProperty));
        }
       
        public void ApplyHough()
        {
            HoughTransform houghMethod = new HoughTransform();
            Bitmap hough = houghMethod.ApplyMethod(BitmapProperty);
            BitmapProperty = hough;
            ImageSource = updateBitmap(hough);
            BarChart = updateBitmap(_histogram.showBarChart(BitmapProperty));
        }

        public void ApplyDilatation()
        {
            Dilatation dilatationMethod = new Dilatation();
            Bitmap dilatation = dilatationMethod.ApplyDilatation(BitmapProperty);
            BitmapProperty = dilatation;
            ImageSource = updateBitmap(dilatation);
            BarChart = updateBitmap(_histogram.showBarChart(BitmapProperty));
        }

        public void ApplyOpening()
        {
            Erosion erosionMethod = new Erosion();
            Bitmap erosion = erosionMethod.ApplyErosion(BitmapProperty);
            BitmapProperty = erosion;
            Dilatation dilatationMethod = new Dilatation();
            Bitmap dilatation = dilatationMethod.ApplyDilatation(BitmapProperty);
            BitmapProperty = dilatation;
            ImageSource = updateBitmap(dilatation);
            BarChart = updateBitmap(_histogram.showBarChart(BitmapProperty));
        }

        public void ApplyEdgeDetecting()
        {
            var erosionMethod = new Erosion();
            var erosion = erosionMethod.ApplyErosion(BitmapProperty);

            var weight = erosion.Width;
            var height = erosion.Height;
            var result = new Bitmap(weight, height);

            for (var i = 0; i < height; ++i)
            {
                for(var j = 0; j < weight; ++j)
                {
                    if (erosion.GetPixel(j, i) != BitmapProperty.GetPixel(j, i))
                    {
                        result.SetPixel(j, i, Color.White);
                    }
                }
            }

            BitmapProperty = result;
            ImageSource = updateBitmap(result);
            BarChart = updateBitmap(_histogram.showBarChart(BitmapProperty));
        }

        public void ApplyClosing()
        {
            Dilatation dilatationMethod = new Dilatation();
            Bitmap dilatation = dilatationMethod.ApplyDilatation(BitmapProperty);
            BitmapProperty = dilatation;
            Erosion erosionMethod = new Erosion();
            Bitmap erosion = erosionMethod.ApplyErosion(BitmapProperty);
            BitmapProperty = erosion;
            ImageSource = updateBitmap(erosion);
            BarChart = updateBitmap(_histogram.showBarChart(BitmapProperty));
        }

        public void ApplyFilling()
        {
            Filling fillingMethod = new Filling();

            foreach (var filling in fillingMethod.ApplyFilling(BitmapProperty))
            {
                BitmapProperty = filling;
                ImageSource = updateBitmap(filling);
            }
            BarChart = updateBitmap(_histogram.showBarChart(BitmapProperty));
        }
        
        public void ApplyDistanceTransform()
        {
            DistanceTransform distanseTransformMethod = new DistanceTransform();
            Bitmap distanceTransform = distanseTransformMethod.ApplyDistanceTransform(BitmapProperty);
            BitmapProperty = distanceTransform;
            ImageSource = updateBitmap(distanceTransform);
            BarChart = updateBitmap(_histogram.showBarChart(BitmapProperty));
        }

        public void ApplyKMeans()
        {
            KMeans kmeans = new KMeans();
            Bitmap kmeansBitmap = kmeans.ApplyMethod(BitmapProperty);
            BitmapProperty = kmeansBitmap;
            ImageSource = updateBitmap(kmeansBitmap);
            BarChart = updateBitmap(_histogram.showBarChart(BitmapProperty));
        }

        public void ApplyNormCut()
        {
            NormCut cutted = new NormCut();
            Bitmap cuttedBitmap = cutted.ApplyMethod(BitmapProperty);
            BitmapProperty = cuttedBitmap;
            ImageSource = updateBitmap(cuttedBitmap);
            BarChart = updateBitmap(_histogram.showBarChart(BitmapProperty));
        }

        public void ApplyCornersDetector(int flag)
        {
            CornersDetector corners = new CornersDetector(flag);
            Bitmap bitmapWithCorners = corners.ApplyMethod(BitmapProperty);
            BitmapProperty = bitmapWithCorners;
            ImageSource = updateBitmap(bitmapWithCorners);
            BarChart = updateBitmap(_histogram.showBarChart(BitmapProperty));
        }

        public void ApplySIFT()
        {
            SIFT features = new SIFT();
            Bitmap bitmapWithFeatures = features.ApplyMethod(BitmapProperty);
            BitmapProperty = bitmapWithFeatures;
            ImageSource = updateBitmap(bitmapWithFeatures);
            BarChart = updateBitmap(_histogram.showBarChart(BitmapProperty));
        }

        public int ApplyCountingObjects()
        {
            CountingObjects countigObjectsMethod = new CountingObjects();
            var count = countigObjectsMethod.CountObjects(BitmapProperty);
            return count;
        }

        public void ApplyIntensityTransform()
        {
            var threshold = 60;
            var width = BitmapProperty.Width;
            var height = BitmapProperty.Height;
            for (var i = 0; i < height; ++i)
            {
                for (var j = 0; j < width; ++j)
                {
                    if(BitmapProperty.GetPixel(j, i).R > threshold)
                    {
                        var color = Color.FromArgb(255, 255, 255);
                        BitmapProperty.SetPixel(j, i, color);
                    }
                    else
                    {
                        var color = Color.FromArgb(0, 0, 0);
                        BitmapProperty.SetPixel(j, i, color);
                    }
                }
            }
            ImageSource = updateBitmap(BitmapProperty);
        }
    }
}
