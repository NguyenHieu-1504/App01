using SQLite;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace App01.Models
{
    /// <summary>
    /// Model Lane trong SQLite (local config)
    /// Mapping với tblLane trong MongoDB
    /// </summary>
    [Table("Lane")]
    public class Lane
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        /// <summary>
        /// LaneID từ MongoDB (ObjectId dạng string)
        /// Ví dụ: "682e938098501042bc18bf85"
        /// ĐÂY LÀ GIÁ TRỊ DÙNG ĐỂ FILTER TRONG tblCardEventDay
        /// </summary>
        public string LaneIdMongo { get; set; } = "";

        /// <summary>
        /// LaneCode từ MongoDB
        /// Ví dụ: "3", "L01"
        /// </summary>
        public string LaneCode { get; set; } = "";

        /// <summary>
        /// LaneName từ MongoDB
        /// Ví dụ: "Ra", "Vào", "Lane A"
        /// </summary>
        public string LaneName { get; set; } = "";

        /// <summary>
        /// Thuộc Gate nào trong hệ thống local (FK -> Gate.Id)
        /// </summary>
        public int GateId { get; set; }

        /// <summary>
        /// Có đang sử dụng Lane này để đếm xe không
        /// </summary>
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Model để đọc từ MongoDB collection tblLane
    /// </summary>
    [BsonIgnoreExtraElements]
    public class LaneMongo
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string Id { get; set; } = "";

        [BsonElement("LaneID")]
        public string LaneID { get; set; } = "";

        [BsonElement("LaneCode")]
        public string LaneCode { get; set; } = "";

        [BsonElement("LaneName")]
        public string LaneName { get; set; } = "";

        [BsonElement("Inactive")]
        public bool Inactive { get; set; } = false;

        [BsonElement("Controller")]
        public string Controller { get; set; } = "";

        [BsonElement("LaneType")]
        public int LaneType { get; set; } = 0;
    }
}