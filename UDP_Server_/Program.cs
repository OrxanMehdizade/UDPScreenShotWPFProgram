using System.Drawing;
using System.Drawing.Imaging;
using System.Management;
using System.Net.Sockets;
using System.Net;
using System.Text;


var server = new Socket(
    AddressFamily.InterNetwork,
    SocketType.Dgram,
    ProtocolType.Udp
    );

IPAddress.TryParse("127.0.0.1", out var ip);
var listenerEP = new IPEndPoint(ip, 27001);

server.Bind(listenerEP);

var encryptedMessage = new byte[ushort.MaxValue];

EndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);

while (true)
{
    var len = server.ReceiveFrom(encryptedMessage, ref endPoint);
    var message = Encoding.Default.GetString(encryptedMessage, 0, len);
    Console.WriteLine(message);
    if (message.Contains("screenshot"))
    {
        WindowMinimizer.MinimizeWindowByProcessName("ScreenShotNetwork");
        Thread.Sleep(500);
        var ImageBytes = CaptureScreenShot();
        int arrayCount = (int)Math.Ceiling((double)ImageBytes.Length / 500);

        byte[][] bytes = new byte[arrayCount][];
        for (int i = 0; i < arrayCount; i++)
        {
            int remainingElements = ImageBytes.Length - i * 500;
            int currentArraySize = Math.Min(500, remainingElements);
            bytes[i] = new byte[currentArraySize];
            Array.Copy(ImageBytes, i * 500, bytes[i], 0, currentArraySize);
        }

        for (int i = 0; i < bytes.Length; i++)
            server.SendTo(bytes[i], endPoint);
        WindowMinimizer.ShowMinimizedWindowByProcessName("ScreenShotNetwork");
    }
}


byte[] CaptureScreenShot()
{
    try
    {

        ManagementObjectSearcher mydisplayResolution = new ManagementObjectSearcher("SELECT CurrentHorizontalResolution, CurrentVerticalResolution FROM Win32_VideoController");

        int height = 0, width = 0;

        foreach (ManagementObject record in mydisplayResolution.Get())
        {
            height = Convert.ToInt32(record["CurrentHorizontalResolution"]);
            width = Convert.ToInt32(record["CurrentVerticalResolution"]);
        }

        using (Bitmap bitmap = new Bitmap(height, width))
        {
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                graphics.CopyFromScreen(0, 0, 0, 0, bitmap.Size);
            }
            return BitmapToByteArray(bitmap);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
        var bytes = Encoding.Default.GetBytes("Error capturing the screenshot: " + ex.Message);
        return bytes;
    }
}
byte[] BitmapToByteArray(Bitmap bitmap)
{
    using (MemoryStream ms = new MemoryStream())
    {
        bitmap.Save(ms, ImageFormat.Jpeg);
        return ms.ToArray();
    }
}