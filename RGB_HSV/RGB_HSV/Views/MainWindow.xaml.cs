using RGB_HSV.ViewModels;
using System.Windows;
namespace RGB_HSV.Views
{

    public partial class MainWindow : Window
    {
        private ViewModel viewModel;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = viewModel = new ViewModel();
        }

        private void Load_Image_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            bool? result = openFileDialog.ShowDialog();
            if (result == true)
            {
                string fileName = openFileDialog.FileName;
                viewModel.LoadImage(fileName);
            }
        }

        private void ApplySobel(object sender, RoutedEventArgs e)
        {
            viewModel.ApplySobel();
        }

        private void ApplyCanny(object sender, RoutedEventArgs e)
        {
            viewModel.ApplyCanny();
        }

        private void ApplyGabor(object sender, RoutedEventArgs e)
        {
            viewModel.ApplyGabor() ;
        }

        private void ApplyOtsu(object sender, RoutedEventArgs e)
        {
            viewModel.ApplyOtsu();
        }

        private void ApplyIntensityTransform(object sender, RoutedEventArgs e)
        {
            viewModel.ApplyIntensityTransform();
        }

        private void ApplyDistanceTransform(object sender, RoutedEventArgs e)
        {

            viewModel.ApplyDistanceTransform();
        }

        private void ApplyCountingObjects(object sender, RoutedEventArgs e)
        {
            var count = viewModel.ApplyCountingObjects();
            MessageBox.Show($"Detected {count} cells on image", "Number of objects", MessageBoxButton.OK);
        }

        private void ApplyFilling(object sender, RoutedEventArgs e)
        {
            viewModel.ApplyFilling();
        }

        private void ApplyErosion(object sender, RoutedEventArgs e)
        {
            viewModel.ApplyErosion();
        }

        private void ApplyDilatation(object sender, RoutedEventArgs e)
        {
            viewModel.ApplyDilatation();
        }

        private void ApplyClosing(object sender, RoutedEventArgs e)
        {
            viewModel.ApplyClosing();
        }

        private void ApplyOpening(object sender, RoutedEventArgs e)
        {
            viewModel.ApplyOpening();
        }

        private void mainImage_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var point = e.GetPosition(mainImage);
            if (point.X >= 0 && point.Y >= 0 && point.X < viewModel.BitmapProperty.Width
                && point.Y < viewModel.BitmapProperty.Height)
            {
                viewModel.getPixelFormats(point);
            }
            hsvText.Visibility = Visibility.Visible;
        }
    }
}
