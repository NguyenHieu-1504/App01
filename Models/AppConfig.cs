using SQLite;

namespace App01.Models
{
    public class AppConfig
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        // Cấu hình MongoDB
        public string MongoConnectionString { get; set; } = "mongodb://kztek:Kztek123456@103.127.207.247:21210/event?authSource=admin&directConnection=true";
        public string DatabaseName { get; set; } = "MPARKINGEVENT-AOENBD";
        public string CollectionName { get; set; } = "tblCardEventDay";

        // Cấu hình Khu vực
        public int MaxCapacity { get; set; } = 100; // Tổng số chỗ
        public int RefreshIntervalSeconds { get; set; } = 5; // Polling 5s/lần

        // Cấu hình LED 
        public string LedIpAddress { get; set; } = "192.168.1.250";
        public int LedPort { get; set; } = 100;
    }
}