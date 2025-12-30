using SQLite;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace App01.Models
{
    /// <summary>
    /// Model Gate trong SQLite (local config)
    /// Mapping với tblGate trong MongoDB
    /// </summary>
    [Table("Gate")]
    public class Gate
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        /// <summary>
        /// GateID từ MongoDB (ObjectId dạng string)
        /// Ví dụ: "682dac49415460cb3a459c45"
        /// </summary>
        public string GateIdMongo { get; set; } = "";

        /// <summary>
        /// GateCode từ MongoDB
        /// Ví dụ: "123", "G01"
        /// </summary>
        public string GateCode { get; set; } = "";

        /// <summary>
        /// GateName từ MongoDB
        /// Ví dụ: "Cổng A", "Gate 1"
        /// </summary>
        public string GateName { get; set; } = "";

        /// <summary>
        /// Gán cho khu vực nào trong hệ thống local (FK -> Area.Id)
        /// </summary>
        public int AreaId { get; set; }

        /// <summary>
        /// Có đang sử dụng Gate này để đếm xe không
        /// </summary>
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Model để đọc từ MongoDB collection tblGate
    /// </summary>
    [BsonIgnoreExtraElements]
    public class GateMongo
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public string Id { get; set; } = "";

        [BsonElement("GateID")]
        public string GateID { get; set; } = "";

        [BsonElement("GateCode")]
        public string GateCode { get; set; } = "";

        [BsonElement("GateName")]
        public string GateName { get; set; } = "";

        [BsonElement("Inactive")]
        public bool Inactive { get; set; } = false;
    }
}