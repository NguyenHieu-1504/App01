using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;
using App01.Models;

namespace App01.Services
{
    /// <summary>
    /// Service điều khiển bảng LED qua UDP
    /// </summary>
    public class LedService : IDisposable
    {
        private Socket? _udpSocket;
        private IPEndPoint? _endpoint;
        private readonly int _timeout = 3000; // 3 giây timeout

        /// <summary>
        /// Kết nối đến bảng LED
        /// </summary>
        public bool Connect(string ipAddress, int port = 100)
        {
            try
            {
                // Đóng socket cũ nếu có
                Disconnect();

                // Tạo socket UDP mới
                _udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                _endpoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
                _udpSocket.ReceiveTimeout = _timeout;
                Debug.WriteLine($"[LED] Attempting connect to {ipAddress}:{port}");

                // Test kết nối bằng AutoDetect
                string testCmd = "AutoDetect?";
                byte[] testData = Encoding.UTF8.GetBytes(testCmd);
                _udpSocket.SendTo(testData, _endpoint);

                // Đợi response
                byte[] recvBuffer = new byte[1024];
                int recv = _udpSocket.Receive(recvBuffer);

                if (recv > 0)
                {
                    string response = Encoding.UTF8.GetString(recvBuffer, 0, recv);
                    Debug.WriteLine($"[LED CONNECTED] {ipAddress}:{port} - {response}");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LED CONNECT ERROR] {ipAddress}:{port} - {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Ngắt kết nối
        /// </summary>
        public void Disconnect()
        {
            try
            {
                _udpSocket?.Close();
                _udpSocket?.Dispose();
                _udpSocket = null;
            }
            catch { }
        }

        /// <summary>
        /// Hiển thị nội dung lên LED (2 dòng) - SetScreenCurrent
        /// </summary>
        public async Task<bool> DisplayTwoLinesAsync(
            string line1Text,
            string line2Text,
            int line1Color = 1,
            int line2Color = 2,
            int effect = 0,
            int fontSize = 10)  //  FontSize mặc định = 10
        {
            if (_udpSocket == null || _endpoint == null)
            {
                Debug.WriteLine("[LED ERROR] Chưa kết nối!");
                return false;
            }

            try
            {
                // BỎ DẤU tiếng Việt để LED hiển thị được
                //line1Text = RemoveVietnameseTones(line1Text);
                //line2Text = RemoveVietnameseTones(line2Text);

                // Xây dựng command SetScreenCurrent
                string command = BuildSetScreenCurrentCommandRaw(
                    line1Text, line2Text,
                    line1Color, line2Color,
                    effect, fontSize
                );

                Debug.WriteLine($"[LED COMMAND] {command}");

                // Gửi với UTF-8
                byte[] data = Encoding.UTF8.GetBytes(command);

                Debug.WriteLine($"[LED BYTES] {BitConverter.ToString(data).Substring(0, Math.Min(150, BitConverter.ToString(data).Length))}");

                await Task.Run(() => _udpSocket.SendTo(data, _endpoint));

                // Đợi response
                try
                {
                    _udpSocket.ReceiveTimeout = 2000;
                    byte[] recvBuffer = new byte[256];
                    int recv = await Task.Run(() => _udpSocket.Receive(recvBuffer));
                    string response = Encoding.UTF8.GetString(recvBuffer, 0, recv);

                    if (response.Contains("/OK"))
                    {
                        Debug.WriteLine($"[LED OK] {response}");
                        return true;
                    }
                    else
                    {
                        Debug.WriteLine($"[LED ERROR] {response}");
                        return false;
                    }
                }
                catch (SocketException)
                {
                    // Timeout - có thể LED đã nhận
                    Debug.WriteLine("[LED] Timeout (có thể đã nhận)");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LED SEND ERROR] {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Hiển thị theo template đã cấu hình
        /// </summary>
        public async Task<bool> DisplayFromTemplateAsync(
            DisplayTemplate template,
            int parkedCount,
            int availableCount,
            int maxCapacity)
        {
            // Tính % chỗ còn lại
            int percentAvailable = maxCapacity > 0
                ? (availableCount * 100 / maxCapacity)
                : 0;

            // Chọn màu dựa trên ColorRules
            string color = GetColorFromTemplate(template, percentAvailable);
            int colorCode = ConvertColorNameToCode(color);

            // Format text với placeholders
            string line1 = FormatText(template.Line1Format, parkedCount, availableCount, maxCapacity, percentAvailable);
            string line2 = FormatText(template.Line2Format, parkedCount, availableCount, maxCapacity, percentAvailable);

            // Gửi lên LED
            return await DisplayTwoLinesAsync(
                line1, line2,
                colorCode, colorCode,
                0, // Effect = 0 (đứng yên)
                template.FontSize
            );
        }

        /// <summary>
        /// Xây dựng command SetScreenDefault (lưu vào EEPROM)
        /// </summary>
        private string BuildSetScreenDefaultCommand(
            string line1Text,
            string line2Text,
            int line1Color,
            int line2Color,
            int effect,
            int fontSize)
        {
            // CHỈ escape ký tự /
            line1Text = line1Text.Replace("/", " ");
            line2Text = line2Text.Replace("/", " ");

            return $"SetScreenDefault?/" +
                   $"NumLine=2/" +
                   $"Effect1={effect}/" +
                   $"Speed1=5/" +
                   $"FontSize1={fontSize}/" +
                   $"Text1=<Colour1={line1Color}>{line1Text}/" +
                   $"Effect2={effect}/" +
                   $"Speed2=5/" +
                   $"FontSize2={fontSize}/" +
                   $"Text2=<Colour2={line2Color}>{line2Text}";
        }

        /// <summary>
        /// Xây dựng command SetScreenCurrent - GIỮ NGUYÊN TEXT
        /// </summary>
        private string BuildSetScreenCurrentCommandRaw(
            string line1Text,
            string line2Text,
            int line1Color,
            int line2Color,
            int effect,
            int fontSize)
        {
            // CHỈ escape ký tự /
            line1Text = line1Text.Replace("/", " ");
            line2Text = line2Text.Replace("/", " ");

            return $"SetScreenCurrent?/" +
                   $"NumLine=2/" +
                   $"Effect1={effect}/" +
                   $"Speed1=5/" +
                   $"FontSize1={fontSize}/" +
                   $"Text1=<Colour1={line1Color}>{line1Text}/" +
                   $"Effect2={effect}/" +
                   $"Speed2=5/" +
                   $"FontSize2={fontSize}/" +
                   $"Text2=<Colour2={line2Color}>{line2Text}";
        }

        /// <summary>
        /// Xây dựng command SetScreenCurrent theo protocol
        /// </summary>
        private string BuildSetScreenCurrentCommand(
            string line1Text,
            string line2Text,
            int line1Color,
            int line2Color,
            int effect,
            int fontSize)
        {
            // Xử lý ký tự đặc biệt
            line1Text = SanitizeText(line1Text);
            line2Text = SanitizeText(line2Text);

            return $"SetScreenCurrent?/" +
                   $"NumLine=2/" +
                   $"Effect1={effect}/" +
                   $"Speed1=5/" +
                   $"FontSize1={fontSize}/" +
                   $"Text1=<Colour1={line1Color}>{line1Text}/" +
                   $"Effect2={effect}/" +
                   $"Speed2=5/" +
                   $"FontSize2={fontSize}/" +
                   $"Text2=<Colour2={line2Color}>{line2Text}";
        }

        /// <summary>
        /// Làm sạch text - xóa dấu tiếng Việt và ký tự đặc biệt
        /// </summary>
        private string SanitizeText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            // Xóa ký tự / (conflict với protocol)
            text = text.Replace("/", " ");

            // Bỏ dấu tiếng Việt
            //text = RemoveVietnameseTones(text);

            return text;
        }

        /// <summary>
        /// Bỏ dấu tiếng Việt
        /// </summary>
        private string RemoveVietnameseTones(string text)
        {
            string[] vietnameseSigns = new string[]
            {
                "aAeEoOuUiIdDyY",
                "áàạảãâấầậẩẫăắằặẳẵ",
                "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",
                "éèẹẻẽêếềệểễ",
                "ÉÈẸẺẼÊẾỀỆỂỄ",
                "óòọỏõôốồộổỗơớờợởỡ",
                "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",
                "úùụủũưứừựửữ",
                "ÚÙỤỦŨƯỨỪỰỬỮ",
                "íìịỉĩ",
                "ÍÌỊỈĨ",
                "đ",
                "Đ",
                "ýỳỵỷỹ",
                "ÝỲỴỶỸ"
            };

            for (int i = 1; i < vietnameseSigns.Length; i++)
            {
                for (int j = 0; j < vietnameseSigns[i].Length; j++)
                {
                    text = text.Replace(vietnameseSigns[i][j], vietnameseSigns[0][i - 1]);
                }
            }

            return text;
        }

        /// <summary>
        /// Format text với placeholders
        /// </summary>
        private string FormatText(string format, int parked, int available, int max, int percent)
        {
            return format
                .Replace("{ParkedCount}", parked.ToString())
                .Replace("{AvailableCount}", available.ToString())
                .Replace("{MaxCapacity}", max.ToString())
                .Replace("{PercentFull}", (100 - percent).ToString())
                .Replace("{PercentAvailable}", percent.ToString());
        }

        /// <summary>
        /// Lấy màu từ template dựa trên %
        /// </summary>
        private string GetColorFromTemplate(DisplayTemplate template, int percentAvailable)
        {
            foreach (var rule in template.ColorRules)
            {
                if (rule.IsMatch(percentAvailable))
                {
                    return rule.Color;
                }
            }
            return template.DefaultColor;
        }

        /// <summary>
        /// Convert tên màu sang mã LED (1-7)
        /// </summary>
        private int ConvertColorNameToCode(string colorName)
        {
            return colorName.ToLower() switch
            {
                "red" => 1,
                "green" => 2,
                "yellow" => 3,
                "blue" => 4,
                "purple" => 5,
                "cyan" => 6,
                "white" => 7,
                _ => 2 // Mặc định xanh lá
            };
        }

        /// <summary>
        /// Lấy firmware version của LED
        /// </summary>
        public async Task<string?> GetFirmwareVersionAsync()
        {
            if (_udpSocket == null || _endpoint == null)
                return null;

            try
            {
                string cmd = "GetFirmwareVersion?/";
                byte[] data = Encoding.UTF8.GetBytes(cmd);
                await Task.Run(() => _udpSocket.SendTo(data, _endpoint));

                byte[] recvBuffer = new byte[256];
                int recv = await Task.Run(() => _udpSocket.Receive(recvBuffer));
                return Encoding.UTF8.GetString(recvBuffer, 0, recv);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Trở về màn hình mặc định
        /// </summary>
        public async Task<bool> ReturnToDefaultScreenAsync()
        {
            if (_udpSocket == null || _endpoint == null)
                return false;

            try
            {
                string cmd = "ReturnDefaultScreen?/";
                byte[] data = Encoding.UTF8.GetBytes(cmd);
                await Task.Run(() => _udpSocket.SendTo(data, _endpoint));
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            Disconnect();
        }
    }
}