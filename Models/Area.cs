using SQLite;
using System.Collections.Generic;

namespace App01.Models
{
    /// <summary>
    /// Model đại diện cho 1 khu vực đỗ xe
    /// </summary>
    public class Area
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        /// <summary>
        /// Tên khu vực (VD: "Tầng 1", "Khu A", "VIP")
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Sức chứa tối đa (số chỗ đỗ)
        /// </summary>
        public int MaxCapacity { get; set; } = 100;

        /// <summary>
        /// Filter MongoDB (JSON) để lọc xe theo khu vực
        /// VD: {"AreaCode": "A"} hoặc {"Gate": "Gate1"}
        /// Để null nếu đếm toàn bộ
        /// </summary>
        public string? MongoFilter { get; set; }

        /// <summary>
        /// Tần suất polling (giây)
        /// </summary>
        public int RefreshIntervalSeconds { get; set; } = 5;

        /// <summary>
        /// Có dùng MongoDB Change Stream không (realtime)
        /// </summary>
        public bool UseChangeStream { get; set; } = false;

        /// <summary>
        /// Kích hoạt khu vực này không
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Connection string riêng (nếu null dùng global)
        /// </summary>
        public string? MongoConnectionString { get; set; }

        /// <summary>
        /// Database name riêng (nếu null dùng global)
        /// </summary>
        public string? DatabaseName { get; set; }

        /// <summary>
        /// Collection name riêng (nếu null dùng global)
        /// </summary>
        public string? CollectionName { get; set; }
        /// <summary>
        /// Ghi chú về khu vực (tùy chọn)
        /// </summary>
        public string? Notes { get; set; }

        [Ignore] // Không lưu vào SQLite, chỉ dùng runtime
        public List<Gate> Gates { get; set; } = new List<Gate>();
    }
}