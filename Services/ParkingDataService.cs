using App01.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
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
                // Tên collection phải chuẩn xác
                _collection = db.GetCollection<ParkingRecord>("tblCardEventDay");

                // Ping thử server 1 cái cho chắc
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
        public async Task<long> CountParkedCarsAsync()
        {
            if (_collection == null) return -1;

            //  EventCode = 1 VÀ IsDelete = false
            var builder = Builders<ParkingRecord>.Filter;
            var filter = builder.Eq(x => x.EventCode, "1") &
                         builder.Eq(x => x.IsDelete, false);

            try
            {
                long count = await _collection.CountDocumentsAsync(filter);
                return count;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LỖI TRUY VẤN]: {ex.Message}");
                return 0;
            }
        }
    }
}