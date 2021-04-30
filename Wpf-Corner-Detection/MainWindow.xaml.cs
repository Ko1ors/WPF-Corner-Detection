using Emgu.CV;
using Emgu.CV.Features2D;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using static Emgu.CV.Features2D.FastFeatureDetector;

namespace Wpf_Corner_Detection
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string imagePath;

        private DetectionMethod method;


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
            if (method == DetectionMethod.HarrisDetector)
                HarrisDetector();
            else if (method == DetectionMethod.FASTDetector)
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

                int threshold = Convert.ToInt32(thresholdSlider.Value);
                var nonmaxSupression = checkBoxSuppresion.IsChecked.Value;
                var useFiltering = checkBoxUseAngleFiltering.IsChecked.Value;

                var lowerLimit = Convert.ToDouble(textBoxLowerLimit.Text);
                var upperLimit = Convert.ToDouble(textBoxUpperLimit.Text);
                Feature2D detector;
                if (useFiltering)
                    detector = new Emgu.CV.Features2D.ORBDetector(fastThreshold: threshold);
                else
                    detector = new Emgu.CV.Features2D.FastFeatureDetector(threshold: threshold, nonmaxSupression: nonmaxSupression);

                var keyPoints = detector.Detect(gray);

                using (Graphics g = Graphics.FromImage(img))
                {
                    var pen = new Pen(Color.DarkViolet, 1);

                    foreach (var kp in keyPoints)
                    {
                            if (!useFiltering || (kp.Angle >= lowerLimit && kp.Angle <= upperLimit))
                                g.DrawEllipse(pen, new RectangleF(kp.Point.X - 3, kp.Point.Y - 3, 6, 6));
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
            if (rb.IsChecked.Value && rb.Content is not null)
            {
                var content = rb.Content.ToString();
                if (content.Contains("Harris"))
                    method = DetectionMethod.HarrisDetector;
                else if (content.Contains("FAST"))
                    method = DetectionMethod.FASTDetector;
                ChangeSettingsBox();
            }
        }

        private void ChangeSettingsBox()
        {
            switch (method)
            {
                case DetectionMethod.HarrisDetector:
                    groupBoxHarrisSettings.Visibility = Visibility.Visible;
                    groupBoxFASTSettings.Visibility = Visibility.Collapsed;
                    break;
                case DetectionMethod.FASTDetector:
                    groupBoxFASTSettings.Visibility = Visibility.Visible;
                    groupBoxHarrisSettings.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private void checkBoxUseAngleFiltering_Checked(object sender, RoutedEventArgs e)
        {
            stackPanelLimit.Visibility = Visibility.Visible;

        }

        private void checkBoxUseAngleFiltering_Unchecked(object sender, RoutedEventArgs e)
        {
            stackPanelLimit.Visibility = Visibility.Collapsed;
        }
    }
}
