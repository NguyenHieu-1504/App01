using App01.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace App01.Services
{
    /// <summary>
    /// Service để lấy danh sách Gate và Lane từ MongoDB
    /// </summary>
    public class GateLaneService
    {
        private IMongoDatabase? _database;

        /// <summary>
        /// Kết nối MongoDB
        /// </summary>
        public bool Connect(string connectionString, string dbName)
        {
            try
            {
                var client = new MongoClient(connectionString);
                _database = client.GetDatabase(dbName);

                // Test connection
                _database.RunCommandAsync((Command<MongoDB.Bson.BsonDocument>)"{ping:1}").Wait();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[GATE/LANE SERVICE] Connection error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Lấy danh sách tất cả Gates từ MongoDB
        /// </summary>
        public async Task<List<GateMongo>> GetAllGatesFromMongoAsync()
        {
            if (_database == null) return new List<GateMongo>();

            try
            {
                var collection = _database.GetCollection<GateMongo>("tblGate");
                var filter = Builders<GateMongo>.Filter.Eq(g => g.Inactive, false);
                var gates = await collection.Find(filter).ToListAsync();

                Debug.WriteLine($"[GATE/LANE SERVICE] Found {gates.Count} gates from MongoDB");
                return gates;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[GATE/LANE SERVICE] Error getting gates: {ex.Message}");
                return new List<GateMongo>();
            }
        }

        /// <summary>
        /// Lấy danh sách tất cả Lanes từ MongoDB
        /// </summary>
        public async Task<List<LaneMongo>> GetAllLanesFromMongoAsync()
        {
            if (_database == null) return new List<LaneMongo>();

            try
            {
                var collection = _database.GetCollection<LaneMongo>("tblLane");
                var filter = Builders<LaneMongo>.Filter.Eq(l => l.Inactive, false);
                var lanes = await collection.Find(filter).ToListAsync();

                Debug.WriteLine($"[GATE/LANE SERVICE] Found {lanes.Count} lanes from MongoDB");
                return lanes;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[GATE/LANE SERVICE] Error getting lanes: {ex.Message}");
                return new List<LaneMongo>();
            }
        }

        /// <summary>
        /// Lấy Lane theo GateID
        /// </summary>
        public async Task<List<LaneMongo>> GetLanesByGateIdAsync(string gateId)
        {
            if (_database == null) return new List<LaneMongo>();

            try
            {
                var collection = _database.GetCollection<LaneMongo>("tblLane");

                
                var filter = Builders<LaneMongo>.Filter.Eq(l => l.Inactive, false);
                var lanes = await collection.Find(filter).ToListAsync();

                return lanes;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[GATE/LANE SERVICE] Error getting lanes by gate: {ex.Message}");
                return new List<LaneMongo>();
            }
        }
    }
}