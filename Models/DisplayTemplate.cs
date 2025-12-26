using MongoDB.Bson.Serialization.Attributes;
using SQLite;
using System.Collections.Generic;
using System.Text.Json;

namespace App01.Models
{
    /// <summary>
    /// Model kịch bản hiển thị trên LED
    /// </summary>
    public class DisplayTemplate
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        /// <summary>
        /// Tên kịch bản (VD: "Mẫu 1", "Hiển thị đầy đủ")
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Dòng 1 - Format string
        /// VD: "{ParkedCount} xe đã gửi"
        /// Biến: {ParkedCount}, {AvailableCount}, {MaxCapacity}, {PercentFull}
        /// </summary>
        public string Line1Format { get; set; } = "{ParkedCount} XE";

        /// <summary>
        /// Dòng 2 - Format string
        /// VD: "{AvailableCount} chỗ còn lại"
        /// </summary>
        public string Line2Format { get; set; } = "{AvailableCount} CHO";

        /// <summary>
        /// Quy tắc màu sắc (JSON)
        /// VD: [{"MinPercent":0,"MaxPercent":30,"Color":"Red"},
        ///      {"MinPercent":31,"MaxPercent":70,"Color":"Yellow"},
        ///      {"MinPercent":71,"MaxPercent":100,"Color":"Green"}]
        /// </summary>
        public string ColorRulesJson { get; set; } = "[]";

        /// <summary>
        /// Màu mặc định nếu không khớp rule nào
        /// </summary>
        public string DefaultColor { get; set; } = "Green";

        /// <summary>
        /// Font size (nếu LED hỗ trợ)
        /// </summary>
        public int FontSize { get; set; } = 10;

        /// <summary>
        /// Căn chỉnh (Left, Center, Right)
        /// </summary>
        public string Alignment { get; set; } = "Center";

        /// <summary>
        /// Ghi chú
        /// </summary>
        public string? Notes { get; set; }

        [BsonElement("IsActive")]
        public bool IsActive { get; set; } = true; // Mặc định active

        /// <summary>
        /// Parse ColorRulesJson thành List<ColorRule>
        /// </summary>
        [Ignore]
        public List<ColorRule> ColorRules
        {
            get
            {
                try
                {
                    return JsonSerializer.Deserialize<List<ColorRule>>(ColorRulesJson)
                           ?? new List<ColorRule>();
                }
                catch
                {
                    return new List<ColorRule>();
                }
            }
            set
            {
                ColorRulesJson = JsonSerializer.Serialize(value);
            }
        }
    }

    /// <summary>
    /// Quy tắc màu sắc theo % chỗ còn lại
    /// </summary>
    public class ColorRule
    {
        /// <summary>
        /// % chỗ còn lại TỐI THIỂU (0-100)
        /// </summary>
        public int MinPercent { get; set; }

        /// <summary>
        /// % chỗ còn lại TỐI ĐA (0-100)
        /// </summary>
        public int MaxPercent { get; set; }

        /// <summary>
        /// Màu hiển thị (VD: "Red", "Green", "Yellow", "#FF0000")
        /// </summary>
        public string Color { get; set; } = "Green";

        /// <summary>
        /// Kiểm tra % có nằm trong khoảng này không
        /// </summary>
        public bool IsMatch(int percentAvailable)
        {
            return percentAvailable >= MinPercent && percentAvailable <= MaxPercent;
        }


    }
    

    }