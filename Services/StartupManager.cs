using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace App01.Services
{
    /// <summary>
    /// Quản lý tính năng khởi động cùng Windows
    /// </summary>
    public static class StartupManager
    {
        private const string APP_NAME = "ParkingMonitor"; // Tên app hiển thị trong Registry
        private static readonly string REGISTRY_KEY = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

        /// <summary>
        /// Kiểm tra app có tự khởi động cùng Windows không
        /// </summary>
        public static bool IsStartupEnabled()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(REGISTRY_KEY, false);
                if (key == null) return false;

                var value = key.GetValue(APP_NAME) as string;
                return !string.IsNullOrEmpty(value);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[STARTUP] Error checking startup: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Bật/tắt khởi động cùng Windows
        /// </summary>
        /// <param name="enable">true = bật, false = tắt</param>
        /// <returns>true nếu thành công</returns>
        public static bool SetStartup(bool enable)
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(REGISTRY_KEY, true);
                if (key == null)
                {
                    Debug.WriteLine("[STARTUP] Cannot open registry key");
                    return false;
                }

                if (enable)
                {
                    // Lấy đường dẫn exe hiện tại
                    string exePath = GetExecutablePath();

                    // Thêm vào registry
                    key.SetValue(APP_NAME, $"\"{exePath}\"");
                    Debug.WriteLine($"[STARTUP] Enabled: {exePath}");
                    return true;
                }
                else
                {
                    // Xóa khỏi registry
                    key.DeleteValue(APP_NAME, false);
                    Debug.WriteLine("[STARTUP] Disabled");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[STARTUP] Error setting startup: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Lấy đường dẫn file .exe của app
        /// </summary>
        private static string GetExecutablePath()
        {
            // Cách 1: Dùng cho published app
            string exePath = Environment.ProcessPath;

            if (!string.IsNullOrEmpty(exePath) && File.Exists(exePath))
            {
                return exePath;
            }

            // Cách 2: Dùng cho development (fallback)
            var assembly = Assembly.GetExecutingAssembly();
            return assembly.Location.Replace(".dll", ".exe");
        }

        /// <summary>
        /// Lấy tên app hiển thị
        /// </summary>
        public static string GetAppName() => APP_NAME;
    }
}