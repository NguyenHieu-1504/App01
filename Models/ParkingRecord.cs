using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace App01.Models
{

    [BsonIgnoreExtraElements]
    public class ParkingRecord
    {

        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public required string Id { get; set; }

        [BsonElement("DatetimeIn")]
        public DateTime? DatetimeIn { get; set; }

        [BsonElement("DateTimeOut")]
        public DateTime? DateTimeOut { get; set; }


        [BsonElement("PlateIn")]
        public string PlateNumber { get; set; }

        // Trạng thái xóa
        [BsonElement("IsDelete")]
        public bool IsDelete { get; set; }

        [BsonElement("EventCode")]
        public string EventCode { get; set; }

        [BsonElement("VehicleGroupID")]
        public string VehicleGroupID { get; set; }


        [BsonElement("LaneIDIn")]
        public string? LaneId { get; set; }

        [BsonElement("LaneIDOut")]
        public string? LaneIDOut { get; set; }


    }
}