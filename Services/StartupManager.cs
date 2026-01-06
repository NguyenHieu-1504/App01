using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices; //  thư viện để check OS
using System.Text;

namespace App01.Services
{
    /// <summary>
    /// Quản lý tính năng khởi động cùng OS (Windows & Linux)
    /// </summary>
    public static class StartupManager
    {
        private const string APP_NAME = "ParkingMonitor"; // Tên app

        // Windows Registry Key
        private static readonly string REGISTRY_KEY = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

        // Linux Autostart Path: /home/user/.config/autostart/ParkingMonitor.desktop
        private static string GetLinuxAutostartPath()
        {
            string homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(homeDir, ".config", "autostart", $"{APP_NAME}.desktop");
        }

        /// <summary>
        /// Kiểm tra app có tự khởi động không
        /// </summary>
        public static bool IsStartupEnabled()
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    
                    using var key = Registry.CurrentUser.OpenSubKey(REGISTRY_KEY, false);
                    if (key == null) return false;

                    var value = key.GetValue(APP_NAME) as string;
                    return !string.IsNullOrEmpty(value);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    
                    string path = GetLinuxAutostartPath();
                    return File.Exists(path);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[STARTUP] Error checking startup: {ex.Message}");
            }
            return false;
        }

        /// <summary>
        /// Bật/tắt khởi động cùng hệ điều hành
        /// </summary>
        public static bool SetStartup(bool enable)
        {
            try
            {
                string exePath = GetExecutablePath();
                if (string.IsNullOrEmpty(exePath)) return false;

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return SetStartupWindows(enable, exePath);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    return SetStartupLinux(enable, exePath);
                }

                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[STARTUP] Error setting startup: {ex.Message}");
                return false;
            }
        }

        // --- Logic riêng cho Windows ---
        private static bool SetStartupWindows(bool enable, string exePath)
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(REGISTRY_KEY, true);
                if (key == null) return false;

                if (enable)
                {
                    key.SetValue(APP_NAME, $"\"{exePath}\"");
                    Debug.WriteLine($"[STARTUP-WIN] Enabled: {exePath}");
                }
                else
                {
                    key.DeleteValue(APP_NAME, false);
                    Debug.WriteLine("[STARTUP-WIN] Disabled");
                }
                return true;
            }
            catch { return false; }
        }

        // --- Logic riêng cho Linux ---
        private static bool SetStartupLinux(bool enable, string exePath)
        {
            try
            {
                string autostartPath = GetLinuxAutostartPath();
                string autostartDir = Path.GetDirectoryName(autostartPath)!;

                if (enable)
                {
                    // 1. Tạo thư mục nếu chưa có
                    if (!Directory.Exists(autostartDir))
                    {
                        Directory.CreateDirectory(autostartDir);
                    }

                    // 2. Nội dung file .desktop chuẩn
                    // Exec: Đường dẫn file thực thi
                    // Path: Đường dẫn thư mục làm việc (quan trọng để load config/db)
                    string workingDir = Path.GetDirectoryName(exePath)!;

                    var sb = new StringBuilder();
                    sb.AppendLine("[Desktop Entry]");
                    sb.AppendLine("Type=Application");
                    sb.AppendLine($"Name={APP_NAME}");
                    sb.AppendLine($"Exec=\"{exePath}\"");
                    sb.AppendLine($"Path={workingDir}");
                    sb.AppendLine("Terminal=false");
                    sb.AppendLine("Hidden=false");
                    sb.AppendLine("X-GNOME-Autostart-enabled=true");

                    File.WriteAllText(autostartPath, sb.ToString());

                    // (Optional) Cấp quyền thực thi cho file .desktop (thường không bắt buộc)
                    // SetExecutePermission(autostartPath); 

                    Debug.WriteLine($"[STARTUP-LINUX] Created: {autostartPath}");
                }
                else
                {
                    if (File.Exists(autostartPath))
                    {
                        File.Delete(autostartPath);
                        Debug.WriteLine($"[STARTUP-LINUX] Deleted: {autostartPath}");
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[STARTUP-LINUX] Error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Lấy đường dẫn file .exe /.dll
        /// </summary>
        private static string GetExecutablePath()
        {
            string path = Environment.ProcessPath;
            if (string.IsNullOrEmpty(path))
            {
                path = Assembly.GetExecutingAssembly().Location;
                // Trên Windows thì đổi .dll -> .exe, trên Linux giữ nguyên hoặc bỏ đuôi tùy cách build
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    path = path.Replace(".dll", ".exe");
                else
                    path = path.Replace(".dll", ""); // Linux binary thường không có đuôi
            }
            return path;
        }
    }
}