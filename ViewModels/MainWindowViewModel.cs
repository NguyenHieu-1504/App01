using App01.Models;
using App01.Services;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace App01.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly ConfigService _configService;
        private ParkingDataService? _parkingService;
        private readonly Dictionary<int, LedService> _ledServices;

        private CancellationTokenSource? _cts;
        private string _connectionStatus;
        private string _parkedCount;
        private string _availableCount;
        private string _statusColor;
        private string _ledStatus;
        private bool _isRunning;

        public string ConnectionStatus
        {
            get => _connectionStatus;
            set => this.RaiseAndSetIfChanged(ref _connectionStatus, value);
        }

        public string ParkedCount
        {
            get => _parkedCount;
            set => this.RaiseAndSetIfChanged(ref _parkedCount, value);
        }

        public string AvailableCount
        {
            get => _availableCount;
            set => this.RaiseAndSetIfChanged(ref _availableCount, value);
        }

        public string StatusColor
        {
            get => _statusColor;
            set => this.RaiseAndSetIfChanged(ref _statusColor, value);
        }

        public string LedStatus
        {
            get => _ledStatus;
            set => this.RaiseAndSetIfChanged(ref _ledStatus, value);
        }

        public bool IsRunning
        {
            get => _isRunning;
            set => this.RaiseAndSetIfChanged(ref _isRunning, value);
        }

        public MainWindowViewModel()
        {
            _configService = new ConfigService();
            _ledServices = new Dictionary<int, LedService>();

            // KHÔNG tự động connect - chỉ hiển thị trạng thái ban đầu
            _connectionStatus = "Chưa kết nối";
            _statusColor = "Gray";
            _parkedCount = "--";
            _availableCount = "--";
            _ledStatus = "LED: Chưa kết nối";
            _isRunning = false;

            Debug.WriteLine("[MAIN] App khởi động. Đợi cấu hình từ Settings.");
            // ⬇ TỰ ĐỘNG START SAU 1 GIÂY
            _ = AutoStartAsync();
        }

        private async Task AutoStartAsync()
        {
            // Delay 1 giây để UI load xong
            await Task.Delay(1000);

            Debug.WriteLine("[AUTO-START] Kiểm tra cấu hình...");

            // Kiểm tra đã cấu hình chưa
            var config = _configService.GetConfig();
            var areas = _configService.GetActiveAreas();

            if (areas.Count == 0)
            {
                ConnectionStatus = "⚠️ Chưa có cấu hình! Vui lòng vào Settings để cấu hình.";
                StatusColor = "Orange";
                Debug.WriteLine("[AUTO-START] Không có khu vực nào được cấu hình!");
                return;
            }

            // Kiểm tra connection string
            var area = areas.First();
            string connString = area.MongoConnectionString ?? config.MongoConnectionString;

            if (string.IsNullOrWhiteSpace(connString))
            {
                ConnectionStatus = "⚠️ Chưa cấu hình MongoDB! Vui lòng vào Settings.";
                StatusColor = "Orange";
                Debug.WriteLine("[AUTO-START] Connection string trống!");
                return;
            }

            // Nếu đã có cấu hình → Tự động start
            Debug.WriteLine("[AUTO-START] Đã có cấu hình, bắt đầu monitoring...");
            await StartMonitoringAsync();
        }

        /// <summary>
        /// Gọi từ MainWindow sau khi user cấu hình xong
        /// </summary>
        public async Task StartMonitoringAsync()
        {
            if (_isRunning)
            {
                Debug.WriteLine("[MAIN] Đã đang chạy!");
                return;
            }

            try
            {
                ConnectionStatus = "Đang kiểm tra cấu hình...";
                StatusColor = "Orange";

                // 1. Lấy cấu hình
                var config = _configService.GetConfig();
                var areas = _configService.GetActiveAreas();

                if (areas.Count == 0)
                {
                    ConnectionStatus = "⚠️ Chưa có khu vực!\nVui lòng vào Settings → Tab 'Khu Vực' để thêm khu vực mới.";
                    StatusColor = "Orange";
                    Debug.WriteLine("[MAIN] Không có khu vực nào.");
                    return;
                }

                var area = areas.First();
                Debug.WriteLine($"[MAIN] Khởi động khu vực: {area.Name}");

                // 2. Kết nối MongoDB
                ConnectionStatus = $"Đang kết nối MongoDB ({area.Name})...";
                StatusColor = "Orange";

                _parkingService = new ParkingDataService();
                string connString = area.MongoConnectionString ?? config.MongoConnectionString;
                string dbName = area.DatabaseName ?? config.DatabaseName;

                if (string.IsNullOrWhiteSpace(connString) || string.IsNullOrWhiteSpace(dbName))
                {
                    ConnectionStatus = "⚠️ Chưa cấu hình MongoDB!\nVui lòng vào Settings → Tab 'MongoDB' để cấu hình.";
                    StatusColor = "Orange";
                    Debug.WriteLine("[MAIN] Connection string hoặc DB name trống!");
                    return;
                }

                bool mongoConnected = _parkingService.Connect(connString, dbName);

                if (!mongoConnected)
                {
                    ConnectionStatus = "❌ Không thể kết nối MongoDB!\nKiểm tra: IP, Port, Username, Password";
                    StatusColor = "Red";
                    Debug.WriteLine("[MAIN] Không thể kết nối MongoDB!");
                    return;
                }

                ConnectionStatus = $"✅ MongoDB OK - {area.Name}";
                StatusColor = "Green";

                // 3. Kết nối LED boards
                var ledBoards = _configService.GetActiveLedBoardsByArea(area.Id);

                if (ledBoards.Count == 0)
                {
                    LedStatus = "⚠️ Chưa có bảng LED nào được cấu hình";
                    Debug.WriteLine("[MAIN] Không có LED board nào!");
                }
                else
                {
                    int connectedLeds = 0;

                    foreach (var board in ledBoards)
                    {
                        var ledService = new LedService();
                        bool ledConnected = ledService.Connect(board.IpAddress, board.Port);

                        if (ledConnected)
                        {
                            _ledServices[board.Id] = ledService;
                            connectedLeds++;
                            Debug.WriteLine($"[LED] Kết nối OK: {board.Name} ({board.IpAddress})");
                        }
                        else
                        {
                            Debug.WriteLine($"[LED] Kết nối THẤT BẠI: {board.Name}");
                            ledService.Dispose();
                        }
                    }

                    LedStatus = connectedLeds > 0
                        ? $"LED: {connectedLeds}/{ledBoards.Count} OK"
                        : "LED: Không kết nối được";
                }

                // 4. Bắt đầu polling
                _cts = new CancellationTokenSource();
                _isRunning = true;
                int interval = area.RefreshIntervalSeconds;

                Debug.WriteLine($"[MAIN] Bắt đầu polling mỗi {interval}s");

                _ = Task.Run(async () =>
                {
                    while (!_cts.Token.IsCancellationRequested)
                    {
                        try
                        {
                            await UpdateDataAsync(area, ledBoards);
                            await Task.Delay(interval * 1000, _cts.Token);
                        }
                        catch (TaskCanceledException)
                        {
                            break;
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[MAIN ERROR] {ex.Message}");
                            await Task.Delay(5000, _cts.Token);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                ConnectionStatus = $"❌ Lỗi khởi động: {ex.Message}";
                StatusColor = "Red";
                Debug.WriteLine($"[MAIN FATAL] {ex}");
            }
        }

        /// <summary>
        /// Reload toàn bộ cấu hình và restart monitoring
        /// </summary>
        public async Task ReloadConfigurationAsync()
        {
            Debug.WriteLine("[RELOAD] Đang reload cấu hình...");

            // 1. Dừng monitoring hiện tại (nếu đang chạy)
            if (_isRunning)
            {
                StopMonitoring();
                // Chờ 1 giây để cleanup hoàn tất
                await Task.Delay(1000);
            }

            // 2. Reset trạng thái
            ConnectionStatus = "Đang reload cấu hình...";
            StatusColor = "Orange";
            ParkedCount = "--";
            AvailableCount = "--";
            LedStatus = "LED: Chưa kết nối";

            // 3. Chờ 500ms cho UI cập nhật
            await Task.Delay(500);

            // 4. Start lại với cấu hình mới
            await StartMonitoringAsync();

            Debug.WriteLine("[RELOAD] Hoàn tất reload!");
        }

        /// <summary>
        /// Dừng monitoring
        /// </summary>
        public void StopMonitoring()
        {
            if (!_isRunning)
            {
                Debug.WriteLine("[MAIN] Chưa chạy, không cần stop!");
                return;
            }

            Debug.WriteLine("[MAIN] Đang dừng monitoring...");

            _cts?.Cancel();
            _isRunning = false;

            // Cleanup LED connections
            foreach (var led in _ledServices.Values)
            {
                try
                {
                    led.Dispose();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[LED CLEANUP ERROR] {ex.Message}");
                }
            }
            _ledServices.Clear();

            ConnectionStatus = "Đã dừng";
            StatusColor = "Gray";
            LedStatus = "LED: Ngắt kết nối";

            Debug.WriteLine("[MAIN] Đã dừng monitoring");
        }


        private async Task UpdateDataAsync(Area area, List<LedBoard> ledBoards)
        {
            if (_parkingService == null) return;

            //  Lấy danh sách Lane thuộc Area này
            var lanes = _configService.GetLanesByArea(area.Id);

            //  Nếu không có Lane → KHÔNG ĐẾM, hiển thị cảnh báo
            if (lanes.Count == 0)
            {
                Debug.WriteLine($"[MAIN] ⚠️ Area '{area.Name}' chưa gán Lane!");
                Debug.WriteLine($"[MAIN] → Vui lòng vào Settings → Gán Gate/Lane cho Area này");

                // Hiển thị trạng thái lỗi
                ParkedCount = "--";
                AvailableCount = "--";
                ConnectionStatus = $"⚠️ Area '{area.Name}' chưa gán Lane!\n" +
                                  "Vui lòng vào Settings để gán Gate và Lane.";
                StatusColor = "Orange";
                return;
            }

            //  Lấy danh sách LaneIdMongo
            var laneIds = lanes.Select(l => l.LaneIdMongo).ToList();

            Debug.WriteLine($"[MAIN] ════════════════════════════════════");
            Debug.WriteLine($"[MAIN] 🚗 Đếm xe cho Area: {area.Name}");
            Debug.WriteLine($"[MAIN] 📊 Số Gate: {_configService.GetGatesByArea(area.Id).Count}");
            Debug.WriteLine($"[MAIN] 🛣️ Số Lane: {laneIds.Count}");

            if (!string.IsNullOrWhiteSpace(area.VehicleGroupID))
            {
                Debug.WriteLine($"[MAIN] 🚙 Filter loại xe: {area.VehicleGroupID}");
            }

            Debug.WriteLine($"[MAIN] 🔍 Danh sách Lane IDs:");
            foreach (var laneId in laneIds)
            {
                var lane = lanes.First(l => l.LaneIdMongo == laneId);
                Debug.WriteLine($"     - {lane.LaneName} ({lane.LaneCode}): {laneId}");
            }

            //  Đếm xe với filter theo LaneIDIn + VehicleGroupID
            long parkedCount = await _parkingService.CountParkedCarsAsync(laneIds, area.VehicleGroupID);

            if (parkedCount < 0)
            {
                Debug.WriteLine("[MAIN] ❌ Lỗi khi đếm xe!");
                ConnectionStatus = "❌ Lỗi kết nối MongoDB!";
                StatusColor = "Red";
                return;
            }

            int currentParked = (int)parkedCount;
            int currentAvailable = area.MaxCapacity - currentParked;

            ParkedCount = currentParked.ToString();
            AvailableCount = currentAvailable.ToString();
            ConnectionStatus = $"✅ MongoDB OK - {area.Name}";
            StatusColor = "Green";

            Debug.WriteLine($"[MAIN] ✅ KẾT QUẢ: Xe đang gửi = {currentParked}, Chỗ trống = {currentAvailable}");
            Debug.WriteLine($"[MAIN] ════════════════════════════════════");

            // Cập nhật LED (giữ nguyên)
            var tasks = new List<Task>();
            foreach (var board in ledBoards)
            {
                if (!_ledServices.ContainsKey(board.Id)) continue;

                var ledService = _ledServices[board.Id];
                var template = _configService.GetDisplayTemplate(board.DisplayTemplateId);

                if (template != null)
                {
                    tasks.Add(DisplayToLedAsync(ledService, template, currentParked, currentAvailable, area.MaxCapacity));
                }
            }

            await Task.WhenAll(tasks);
        }

        private async Task DisplayToLedAsync(LedService ledService, DisplayTemplate template, int parked, int available, int max)
        {
            int percent = max > 0 ? (available * 100 / max) : 0;

            string line1 = template.Line1Format
                .Replace("{ParkedCount}", parked.ToString())
                .Replace("{AvailableCount}", available.ToString())
                .Replace("{MaxCapacity}", max.ToString())
                .Replace("{PercentAvailable}", percent.ToString());

            string line2 = template.Line2Format
                .Replace("{ParkedCount}", parked.ToString())
                .Replace("{AvailableCount}", available.ToString())
                .Replace("{MaxCapacity}", max.ToString())
                .Replace("{PercentAvailable}", percent.ToString());

            int colorCode = GetColorFromTemplate(template, percent);

            await ledService.DisplayTwoLinesAsync(line1, line2, colorCode, colorCode, 0, template.FontSize);
        }

        private int GetColorFromTemplate(DisplayTemplate template, int percentAvailable)
        {
            foreach (var rule in template.ColorRules)
            {
                if (rule.IsMatch(percentAvailable))
                {
                    return ConvertColorNameToCode(rule.Color);
                }
            }
            return ConvertColorNameToCode(template.DefaultColor);
        }

        private int ConvertColorNameToCode(string colorName)
        {
            return colorName.ToLower() switch
            {
                "red" => 1,
                "green" => 2,
                "yellow" => 3,
                "blue" => 4,
                "purple" => 5,
                "cyan" => 6,
                "white" => 7,
                _ => 2
            };
        }

        public void Cleanup()
        {
            StopMonitoring();
            _configService.CloseConnection();
        }
    }
}