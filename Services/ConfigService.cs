
using App01.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json; 

namespace App01.Services
{
    public class ConfigService
    {
        private readonly SQLiteConnection _db;

        public ConfigService()
        {
            try
            {
                // Lưu DB vào thư mục LocalApplicationData để tránh lỗi quyền hạn
                var dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "parking_config.db");

                Debug.WriteLine($"[DEBUG] DB Path: {dbPath}");  // Log path để kiểm tra
                if (!File.Exists(dbPath))
                {
                    Debug.WriteLine("[DEBUG] Creating new DB file");
                    File.Create(dbPath).Close();  // Thử tạo file thủ công
                }

                //var dbPath = Path.Combine(Environment.CurrentDirectory, "parking_config.db");

                _db = new SQLiteConnection(dbPath);

                // Tạo bảng (nếu chưa có)
                _db.CreateTable<AppConfig>();
                _db.CreateTable<Area>();
                _db.CreateTable<LedBoard>();
                _db.CreateTable<DisplayTemplate>();

                InitializeDefaultData();

                Debug.WriteLine("DB Path: " + dbPath);

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] DB Init: {ex.Message}");
            }
        }
        private void InitializeDefaultData()
        {
            if (_db.Table<AppConfig>().Count() == 0)
            {
                _db.Insert(new AppConfig());
            }

            if (_db.Table<Area>().Count() == 0)
            {
                _db.Insert(new Area
                {
                    Name = "Khu vực mẫu",
                    MaxCapacity = 100,
                    RefreshIntervalSeconds = 5,
                    IsActive = true
                });
            }

            if (_db.Table<DisplayTemplate>().Count() == 0)
            {
                var template = new DisplayTemplate
                {
                    Name = "Mẫu mặc định",
                    Line1Format = "{ParkedCount} XE",
                    Line2Format = "{AvailableCount} CHO",
                    DefaultColor = "Green",
                    FontSize = 10,
                    ColorRules = new List<ColorRule>
                    {
                        new ColorRule { MinPercent = 0, MaxPercent = 20, Color = "Red" },
                        new ColorRule { MinPercent = 21, MaxPercent = 100, Color = "Green" }
                    }
                };
                // Serialize trước khi lưu
                InsertDisplayTemplate(template);
            }
        }

        // ============ AppConfig ============
        public AppConfig GetConfig()
        {
            return _db.Table<AppConfig>().FirstOrDefault() ?? new AppConfig();
        }

        public void UpdateConfig(AppConfig config)
        {
            //  check ID
            if (config.Id == 0) _db.Insert(config);
            else _db.Update(config);
        }

        // ============ Areas ============
        public List<Area> GetAllAreas() => _db.Table<Area>().ToList();

        public List<Area> GetActiveAreas() => _db.Table<Area>().Where(a => a.IsActive).ToList();

        public void InsertArea(Area area) => _db.Insert(area);

        public void UpdateArea(Area area) => _db.Update(area);

        public void DeleteArea(int id)
        {
            _db.Delete<Area>(id);
            // Xóa các LED thuộc Area này
            var leds = _db.Table<LedBoard>().Where(l => l.AreaId == id).ToList();
            foreach (var led in leds) _db.Delete<LedBoard>(led.Id);
        }

        // ============ LedBoards ============
        public List<LedBoard> GetAllLedBoards() => _db.Table<LedBoard>().ToList();

        public List<LedBoard> GetActiveLedBoardsByArea(int areaId)
            => _db.Table<LedBoard>().Where(b => b.AreaId == areaId && b.IsActive).ToList();

        public void InsertLedBoard(LedBoard board) => _db.Insert(board);

        public void UpdateLedBoard(LedBoard board) => _db.Update(board);

        public void DeleteLedBoard(int id) => _db.Delete<LedBoard>(id);

        // ============ Display Templates (Cần xử lý JSON) ============
        public List<DisplayTemplate> GetAllDisplayTemplates()
        {
            var list = _db.Table<DisplayTemplate>().ToList();
            // Deserialize JSON -> List<ColorRule>
            foreach (var item in list)
            {
                try
                {
                    if (!string.IsNullOrEmpty(item.ColorRulesJson))
                        item.ColorRules = JsonSerializer.Deserialize<List<ColorRule>>(item.ColorRulesJson) ?? new List<ColorRule>();
                }
                catch { item.ColorRules = new List<ColorRule>(); }
            }
            
            Debug.WriteLine($"[DEBUG] GetAllDisplayTemplates: Queried {list.Count} rows from DB");
            return list;
        }

        public DisplayTemplate? GetDisplayTemplate(int id)
        {
            var item = _db.Table<DisplayTemplate>().FirstOrDefault(t => t.Id == id);
            if (item != null && !string.IsNullOrEmpty(item.ColorRulesJson))
            {
                try
                {
                    item.ColorRules = JsonSerializer.Deserialize<List<ColorRule>>(item.ColorRulesJson) ?? new List<ColorRule>();
                }
                catch { }
            }
            return item;
        }

        public void InsertDisplayTemplate(DisplayTemplate template)
        {
            try
            {
                template.ColorRulesJson = JsonSerializer.Serialize(template.ColorRules);
                int rows = _db.Insert(template);
                Debug.WriteLine($"[DEBUG] Inserted rows: {rows}, New ID: {template.Id}");

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERROR] Insert: {ex.Message}");
            }
            
        }

        public void UpdateDisplayTemplate(DisplayTemplate template)
        {
            template.ColorRulesJson = JsonSerializer.Serialize(template.ColorRules);
            _db.Update(template);
        }

        public void DeleteDisplayTemplate(int id)
        {
            _db.Delete<DisplayTemplate>(id);
        }



        public void CloseConnection() => _db?.Close();
    }
}