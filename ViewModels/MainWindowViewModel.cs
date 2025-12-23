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
                // 1. Lấy cấu hình
                var config = _configService.GetConfig();
                var areas = _configService.GetActiveAreas();

                if (areas.Count == 0)
                {
                    ConnectionStatus = "⚠️ Chưa có khu vực! Vào Settings để tạo.";
                    StatusColor = "Orange";
                    Debug.WriteLine("[MAIN] Không có khu vực nào.");
                    return;
                }

                var area = areas.First();
                Debug.WriteLine($"[MAIN] Khởi động khu vực: {area.Name}");


                var templates = _configService.GetAllDisplayTemplates();
                foreach (var t in templates)
                {
                    Debug.WriteLine($"[DEBUG] Template: Id={t.Id}, Name={t.Name}, FontSize={t.FontSize}, ColorRules Count={t.ColorRules.Count}");
                }

                // 2. Kết nối MongoDB
                ConnectionStatus = "Đang kết nối MongoDB...";
                StatusColor = "Orange";

                _parkingService = new ParkingDataService();
                string connString = area.MongoConnectionString ?? config.MongoConnectionString;
                string dbName = area.DatabaseName ?? config.DatabaseName;

                bool mongoConnected = _parkingService.Connect(connString, dbName);

                if (!mongoConnected)
                {
                    ConnectionStatus = "❌ MongoDB lỗi!";
                    StatusColor = "Red";
                    Debug.WriteLine("[MAIN] Không thể kết nối MongoDB!");
                    return;
                }

                ConnectionStatus = "✅ MongoDB OK";
                StatusColor = "Green";

                // 3. Kết nối LED boards
                var ledBoards = _configService.GetActiveLedBoardsByArea(area.Id);
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

                // 4. Bắt đầu polling
                _cts = new CancellationTokenSource();
                _isRunning = true;
                int interval = area.RefreshIntervalSeconds;

                Debug.WriteLine($"[MAIN] Bắt đầu polling {interval}s");

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
                ConnectionStatus = $"❌ Lỗi: {ex.Message}";
                StatusColor = "Red";
                Debug.WriteLine($"[MAIN FATAL] {ex}");
            }
        }

        /// <summary>
        /// Dừng monitoring
        /// </summary>
        public void StopMonitoring()
        {
            _cts?.Cancel();
            _isRunning = false;

            foreach (var led in _ledServices.Values)
            {
                led.Dispose();
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

            long parkedCount = await _parkingService.CountParkedCarsAsync();

            if (parkedCount < 0) return;

            int currentParked = (int)parkedCount;
            int currentAvailable = area.MaxCapacity - currentParked;

            ParkedCount = currentParked.ToString();
            AvailableCount = currentAvailable.ToString();

            Debug.WriteLine($"[MAIN] Xe: {currentParked}, Chỗ: {currentAvailable}");

            // Cập nhật LED
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