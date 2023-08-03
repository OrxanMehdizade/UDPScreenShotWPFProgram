using Microsoft.Win32;
using System;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace UDPScreenShotWPFProgram
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<byte> imageBytes = new();
        Socket UDPClient;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Screenshotclcik(object sender, RoutedEventArgs e)
        {
            List<byte> receivedBytes = new List<byte>();

            UDPClient = new Socket( AddressFamily.InterNetwork,SocketType.Dgram,ProtocolType.Udp);
            var ip = IPAddress.Parse("127.0.0.1");

            var connectEP = new IPEndPoint(ip, 27001);

            EndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);

            var Bytes = Encoding.Default.GetBytes("screenshot");

            UDPClient.SendTo(Bytes, connectEP);

            Task.Run(() =>
            {
                int bytesReceived = 0;
                bool isEndOfMessage = false;

                while (!isEndOfMessage)
                {
                    byte[] buffer = new byte[500];

                    try
                    {
                        bytesReceived = UDPClient.ReceiveFrom(buffer, ref endPoint);
                    }
                    catch (SocketException ex)
                    {
                        Console.WriteLine($"Error receiving data: {ex.Message}");
                        break;
                    }

                    if (bytesReceived > 0)
                    {
                        receivedBytes.AddRange(buffer.Take(bytesReceived));

                        if (bytesReceived < 500)
                            isEndOfMessage = true;
                    }
                }

                Dispatcher.Invoke(() =>
                {
                    BitmapImage bitmapImage = ByteArrayToImageSource(receivedBytes.ToArray());
                    imageControl.Source = bitmapImage;
                    imageBytes.Clear();
                    imageBytes.AddRange(receivedBytes);
                });
            });
        }

        private BitmapImage ByteArrayToImageSource(byte[] byteArray)
        {
            using (MemoryStream ms = new MemoryStream(byteArray))
            {
                try
                {
                    BitmapImage image = new BitmapImage();
                    image.BeginInit();
                    image.StreamSource = ms;
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.EndInit();
                    return image;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error {ex.Message}");
                    return null!;
                }
            }
        }

        private void SaveButton(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            saveFileDialog.Filter = "JPEG Image|*.jpg";

            if (saveFileDialog.ShowDialog() == true)
            {
                if (imageControl.Source is BitmapImage bitmapImage)
                {
                    JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmapImage));

                    using (FileStream fileStream = new FileStream(saveFileDialog.FileName, FileMode.Create))
                    {
                        encoder.Save(fileStream);
                    }
                }
            }
        }

    }
}