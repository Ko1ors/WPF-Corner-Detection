using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.Win32;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Wpf_Corner_Detection
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string imagePath;

        private RadioButton radioButton;


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
            if (radioButton.Content.ToString().Contains("Harris"))
                HarrisDetector();
            else if (radioButton.Content.ToString().Contains("FAST"))
                FASTFeatureDetector();
        }

        public void HarrisDetector()
        {
            try
            {
                var img = new Bitmap(imagePath);
                var outputImg = img.ToImage<Bgr, byte>();

                var gray = outputImg.Convert<Gray, byte>();

                var corners = new Mat();

                CvInvoke.CornerHarris(gray, corners, 2, k: Convert.ToDouble(textBoxK.Text));
                CvInvoke.Normalize(corners, corners, 255, 0, Emgu.CV.CvEnum.NormType.MinMax);

                Matrix<float> matrix = new Matrix<float>(corners.Rows, corners.Cols);
                corners.CopyTo(matrix);

                int threshold = Convert.ToInt32(thresholdSlider.Value);
                using (Graphics g = Graphics.FromImage(img))
                {
                    var pen = new Pen(Color.DarkViolet, 1);
                    for (int i = 0; i < corners.Rows; i++)
                    {
                        for (int j = 0; j < corners.Cols; j++)
                        {
                            if (matrix[i, j] > threshold)
                            {
                                g.DrawEllipse(pen, new Rectangle(j - 3, i - 3, 6, 6));
                            }
                        }
                    }
                }

                imageResult.Source = BitmapToBitmapImage(img);
            }
            catch (Exception e1)
            {
                MessageBox.Show("Error: " + e1.Message);
            }
        }


        public void FASTFeatureDetector()
        {
            try
            {
                var img = new Bitmap(imagePath);
                var outputImg = img.ToImage<Bgr, byte>();
                var gray = outputImg.Convert<Gray, byte>();

                var detector = new Emgu.CV.Features2D.FastFeatureDetector();

                var keyPoints = detector.Detect(gray);

                using (Graphics g = Graphics.FromImage(img))
                {
                    var pen = new Pen(Color.DarkViolet, 1);

                    foreach (var kp in keyPoints)
                    {
                        g.DrawEllipse(pen, new RectangleF(kp.Point.X-3, kp.Point.Y-3, 6, 6));
                    }
                }

                imageResult.Source = BitmapToBitmapImage(img);
            }
            catch (Exception e1)
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

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            var rb = sender as RadioButton;
            if (rb.IsChecked.Value)
                radioButton = rb;
        }
    }
}
