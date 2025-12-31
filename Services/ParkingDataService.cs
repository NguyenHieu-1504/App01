using App01.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace App01.Services
{
    public class ParkingDataService
    {
        private IMongoCollection<ParkingRecord>? _collection;

        // Hàm kết nối
        public bool Connect(string connectionString, string dbName)
        {
            try
            {
                var client = new MongoClient(connectionString);
                var db = client.GetDatabase(dbName);
                
                _collection = db.GetCollection<ParkingRecord>("tblCardEventDay");

                
                db.RunCommandAsync((Command<BsonDocument>)"{ping:1}").Wait();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LỖI KẾT NỐI]: {ex.Message}");
                return false;
            }
        }

        // Hàm đếm số xe đang gửi
        public async Task<long> CountParkedCarsAsync(List<string>? laneIds = null,
            string? vehicleGroupId = null)
        {
            if (_collection == null) return -1;

            var builder = Builders<ParkingRecord>.Filter;

            // Filter cơ bản
            var filter = builder.Eq(x => x.EventCode, "1") &
                         builder.Eq(x => x.IsDelete, false);
            builder.Eq(x => x.VehicleGroupID, "84A002E0-34F3-46DA-9B69-1FA76FCF4C91");

            // Thêm filter theo LaneId 
            if (laneIds != null && laneIds.Count > 0)
            {
                Debug.WriteLine($"[PARKING SERVICE] Filtering by {laneIds.Count} lanes:");
                foreach (var laneId in laneIds)
                {
                    Debug.WriteLine($"  - {laneId}");
                }

                var laneFilter = builder.In(x => x.LaneId, laneIds);
                filter = filter & laneFilter;
            }
            else
            {
                Debug.WriteLine("[PARKING SERVICE] ⚠️ No lane filter - counting ALL cars!");
            }
            //  FILTER THEO VEHICLEGROUPID (loại xe: ô tô, xe máy, etc.)
            if (!string.IsNullOrWhiteSpace(vehicleGroupId))
            {
                Debug.WriteLine($"[PARKING SERVICE] Filtering by VehicleGroupID: {vehicleGroupId}");
                var vehicleFilter = builder.Eq(x => x.VehicleGroupID, vehicleGroupId);
                filter = filter & vehicleFilter;
            }
            try
            {
                long count = await _collection.CountDocumentsAsync(filter);
                Debug.WriteLine($"[PARKING SERVICE] Found {count} cars");
                return count;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LỖI TRUY VẤN]: {ex.Message}");
                Debug.WriteLine($"[STACK TRACE]: {ex.StackTrace}");
                return 0;
            }
        }
    }
}