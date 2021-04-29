using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Wpf_Corner_Detection
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string imagePath;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.jpe, *.jfif, *.png) | *.jpg; *.jpeg; *.jpe; *.jfif; *.png";

            if (openFileDialog.ShowDialog() == true)
            {
                imagePath = openFileDialog.FileName;
                image.Source = new BitmapImage(new Uri(imagePath));
                processButton.IsEnabled = true;
            }
        }

        private void processButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // var img = BitmapImageToBitmap(image.Source as BitmapImage);
                var img = new Bitmap(imagePath);
                var outputImg = img.ToImage<Bgr, byte>();

                //var gray = outputImg.Convert<Gray, byte>().ThresholdBinaryInv(new Gray(200), new Gray(255));
                var gray = outputImg.Convert<Gray, byte>();

                var corners = new Mat();
                CvInvoke.CornerHarris(gray, corners, 2);
                CvInvoke.Normalize(corners, corners, 255, 0, Emgu.CV.CvEnum.NormType.MinMax);

                Matrix<float> matrix = new Matrix<float>(corners.Rows, corners.Cols);
                corners.CopyTo(matrix);
                using (Graphics g = Graphics.FromImage(img))
                {
                    var pen = new System.Drawing.Pen(System.Drawing.Color.DarkViolet, 3);
                    for (int i = 0; i < matrix.Rows; i++)
                    {
                        for (int j = 0; j < matrix.Cols; j++)
                        {
                            if (matrix[i, j] > 100)
                            {
                                //CvInvoke.Circle(outputImg, new System.Drawing.Point(j, i), 1, new MCvScalar(0, 150, 255), 25);
                                g.DrawEllipse(pen, new System.Drawing.Rectangle(j - 3, i - 3, 6, 6));
                            }
                        }
                    }
                }
                //outputImg.AsBitmap().Save("test.png", ImageFormat.Png);
                //imageResult.Source = BitmapToBitmapImage(outputImg.AsBitmap());
                imageResult.Source = BitmapToBitmapImage(img);
            }
            catch(Exception e1)
            {
                MessageBox.Show("Error: " + e1.Message);
            }
        }

        private BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }

        private Bitmap BitmapImageToBitmap(BitmapImage bitmapImage)
        {

            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }
    }
}
