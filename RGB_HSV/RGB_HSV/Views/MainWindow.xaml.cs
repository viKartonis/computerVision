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

        private void RemoveBackground(object sender, RoutedEventArgs e)
        {
            viewModel.RemoveBackground();
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

        private void mainImage_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var point = e.GetPosition(mainImage);
            if (point.X >= 0 && point.Y >= 0 && point.X < viewModel.bitmap.Width 
                && point.Y < viewModel.bitmap.Height)
            {
                viewModel.getPixelFormats(point);
            }
            hsvText.Visibility = Visibility.Visible;
        }
    }
}
