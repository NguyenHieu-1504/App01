using SQLite;
using System;

namespace App01.Models
{
    /// <summary>
    /// Model đại diện cho 1 bảng LED hiển thị
    /// </summary>
    public class LedBoard
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        /// <summary>
        /// Tên bảng LED (VD: "LED Cổng 1", "LED Tầng 2")
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Địa chỉ IP của bảng LED
        /// </summary>
        public string IpAddress { get; set; } = "192.168.1.250";

        /// <summary>
        /// Cổng TCP/IP
        /// </summary>
        public int Port { get; set; } = 100;

        /// <summary>
        /// Thuộc khu vực nào (Foreign Key)
        /// </summary>
        [Indexed]
        public int AreaId { get; set; }

        /// <summary>
        /// Kịch bản hiển thị (Foreign Key)
        /// </summary>
        [Indexed]
        public int DisplayTemplateId { get; set; }

        /// <summary>
        /// Kích hoạt bảng LED này không
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Timeout kết nối (ms)
        /// </summary>
        public int TimeoutMs { get; set; } = 3000;

        /// <summary>
        /// Ghi chú
        /// </summary>
        public string? Notes { get; set; } = string.Empty;

        /// <summary>
        /// Lần kết nối thành công cuối
        /// </summary>
        public DateTime? LastConnected { get; set; }

        /// <summary>
        /// Trạng thái kết nối hiện tại
        /// </summary>
        [Ignore] // Không lưu vào DB
        public bool IsConnected { get; set; } = false;
    }
}