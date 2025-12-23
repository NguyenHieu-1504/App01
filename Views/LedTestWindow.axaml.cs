using App01.Services;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.TextFormatting;
using System;

namespace App01.Views
{
    public partial class LedTestWindow : Window
    {
        private LedService? _ledService;

        public LedTestWindow()
        {
            InitializeComponent();
        }

        private void LogMessage(string message)
        {
            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            txtLog.Text += $"\n[{timestamp}] {message}";
        }

        private async void BtnConnect_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                string ip = txtIpAddress.Text ?? "192.168.1.100";
                int port = int.Parse(txtPort.Text ?? "100");

                LogMessage($"Đang kết nối đến {ip}:{port}...");

                _ledService = new LedService();
                bool connected = _ledService.Connect(ip, port);

                if (connected)
                {
                    txtStatus.Text = $"✓ Đã kết nối {ip}:{port}";
                    txtStatus.Foreground = Avalonia.Media.Brushes.Green;
                    btnConnect.IsEnabled = false;
                    btnDisconnect.IsEnabled = true;
                    btnSend.IsEnabled = true;
                    LogMessage("✓ Kết nối thành công!");

                    // Test lấy firmware version
                    var version = await _ledService.GetFirmwareVersionAsync();
                    if (!string.IsNullOrEmpty(version))
                    {
                        LogMessage($"Firmware: {version}");
                    }
                }
                else
                {
                    txtStatus.Text = "✗ Kết nối thất bại";
                    txtStatus.Foreground = Avalonia.Media.Brushes.Red;
                    LogMessage("✗ Không thể kết nối đến LED!");
                    _ledService?.Dispose();
                    _ledService = null;
                }
            }
            catch (Exception ex)
            {
                LogMessage($"✗ LỖI: {ex.Message}");
                txtStatus.Text = "✗ Lỗi kết nối";
                txtStatus.Foreground = Avalonia.Media.Brushes.Red;
            }
        }

        private void BtnDisconnect_Click(object? sender, RoutedEventArgs e)
        {
            _ledService?.Disconnect();
            _ledService = null;

            txtStatus.Text = "Đã ngắt kết nối";
            txtStatus.Foreground = Avalonia.Media.Brushes.Gray;
            btnConnect.IsEnabled = true;
            btnDisconnect.IsEnabled = false;
            btnSend.IsEnabled = false;
            LogMessage("Đã ngắt kết nối");
        }

        private async void BtnSend_Click(object? sender, RoutedEventArgs e)
        {
            if (_ledService == null)
            {
                LogMessage("✗ Chưa kết nối LED!");
                return;
            }

            try
            {
                string line1 = txtLine1.Text ?? "";
                string line2 = txtLine2.Text ?? "";
                int color1 = (cboColor1.SelectedIndex >= 0 ? cboColor1.SelectedIndex : 0) + 1;
                int color2 = (cboColor2.SelectedIndex >= 0 ? cboColor2.SelectedIndex : 0) + 1;
                int effect = cboEffect.SelectedIndex >= 0 ? cboEffect.SelectedIndex : 0;

                LogMessage($"Gửi: Dòng1='{line1}' (Màu {color1}), Dòng2='{line2}' (Màu {color2})");

                bool success = await _ledService.DisplayTwoLinesAsync(
                    line1, line2,
                    color1, color2,
                    effect, 10
                );

                if (success)
                {
                    LogMessage("✓ Gửi thành công!");
                }
                else
                {
                    LogMessage("✗ Gửi thất bại!");
                }
            }
            catch (Exception ex)
            {
                LogMessage($"✗ LỖI: {ex.Message}");
            }
        }

        private async void BtnTestRed_Click(object? sender, RoutedEventArgs e)
        {
            if (_ledService == null) return;
            LogMessage("Test màu ĐỎ...");
            await _ledService.DisplayTwoLinesAsync("HET CHO", "0 CHO", 1, 1, 3, 10);
        }

        private async void BtnTestGreen_Click(object? sender, RoutedEventArgs e)
        {
            if (_ledService == null) return;
            LogMessage("Test màu XANH...");
            await _ledService.DisplayTwoLinesAsync("CON NHIEU", "100 CHO", 2, 2, 0, 10);
        }

        private async void BtnTestYellow_Click(object? sender, RoutedEventArgs e)
        {
            if (_ledService == null) return;
            LogMessage("Test màu VÀNG...");
            await _ledService.DisplayTwoLinesAsync("SAP HET", "20 CHO", 3, 3, 0, 10);
        }

        private async void BtnDefault_Click(object? sender, RoutedEventArgs e)
        {
            if (_ledService == null) return;
            LogMessage("Trở về màn hình mặc định...");
            await _ledService.ReturnToDefaultScreenAsync();
        }

        private void BtnClose_Click(object? sender, RoutedEventArgs e)
        {
            _ledService?.Dispose();
            Close();
        }
    }
}