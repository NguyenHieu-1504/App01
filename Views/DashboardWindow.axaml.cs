using App01.Models;
using App01.Services;
using App01.ViewModels;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace App01.Views
{
    public partial class DashboardWindow : Window
    {
        private readonly ConfigService _configService;
        private readonly Dictionary<int, ParkingDataService> _parkingServices;
        private readonly Dictionary<int, List<string>> _areaLaneIds;

        private ObservableCollection<AreaCardViewModel> _areaCards;
        private CancellationTokenSource? _cts;
        private DispatcherTimer? _timer;
        private int _refreshInterval = 1; // giây

        
        public DashboardWindow()
        {
            InitializeComponent();

            _configService = new ConfigService();
            _parkingServices = new Dictionary<int, ParkingDataService>();
            _areaLaneIds = new Dictionary<int, List<string>>();
            _areaCards = new ObservableCollection<AreaCardViewModel>();

            AreasItemsControl.ItemsSource = _areaCards;

            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            await LoadAreasAsync();
            StartAutoRefresh();
        }

        /// <summary>
        /// Load tất cả Areas và khởi tạo connections
        /// </summary>
        private async Task LoadAreasAsync()
        {
            try
            {
                var config = _configService.GetConfig();
                var areas = _configService.GetActiveAreas();

                Debug.WriteLine($"[DASHBOARD] Loading {areas.Count} areas");

                foreach (var area in areas)
                {
                    // Tạo ParkingService cho mỗi Area
                    var service = new ParkingDataService();
                    string connString = area.MongoConnectionString ?? config.MongoConnectionString;
                    string dbName = area.DatabaseName ?? config.DatabaseName;

                    if (!service.Connect(connString, dbName))
                    {
                        Debug.WriteLine($"[DASHBOARD] ❌ Failed to connect: {area.Name}");
                        continue;
                    }

                    _parkingServices[area.Id] = service;
                    Debug.WriteLine($"[DASHBOARD] ✅ Connected to Area: {area.Name}");

                    //  Lấy danh sách LaneId và KIỂM TRA
                    var lanes = _configService.GetLanesByArea(area.Id);

                    if (lanes.Count == 0)
                    {
                        Debug.WriteLine($"[DASHBOARD] ⚠️ Area '{area.Name}' CHƯA GÁN LANE!");
                        _areaLaneIds[area.Id] = new List<string>(); // Empty list → Sẽ bị skip
                    }
                    else
                    {
                        _areaLaneIds[area.Id] = lanes.Select(l => l.LaneIdMongo).ToList();
                        Debug.WriteLine($"[DASHBOARD] ✅ Area '{area.Name}' có {lanes.Count} lanes");
                    }

                    // Tạo ViewModel
                    var card = new AreaCardViewModel
                    {
                        Name = lanes.Count > 0 ? area.Name : $"{area.Name} ⚠️", // Thêm icon nếu chưa có lane
                        MaxCapacity = area.MaxCapacity,
                        ParkedCount = 0,
                        AvailableCount = area.MaxCapacity
                    };
                    _areaCards.Add(card);
                }

                // Load dữ liệu ban đầu
                await RefreshDataAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DASHBOARD ERROR] {ex.Message}");
            }
        }

        /// <summary>
        /// Làm mới dữ liệu tất cả Areas
        /// </summary>
        private async Task RefreshDataAsync()
        {
            try
            {
                int totalParked = 0;
                int totalAvailable = 0;
                int totalCapacity = 0;

                var areas = _configService.GetActiveAreas();

                for (int i = 0; i < areas.Count && i < _areaCards.Count; i++)
                {
                    var area = areas[i];
                    var card = _areaCards[i];

                    if (!_parkingServices.ContainsKey(area.Id))
                    {
                        Debug.WriteLine($"[DASHBOARD] ⚠️ Area '{area.Name}' không có service");
                        continue;
                    }

                    // ✅ FIX: Kiểm tra xem Area có Lane không
                    if (!_areaLaneIds.ContainsKey(area.Id) || _areaLaneIds[area.Id].Count == 0)
                    {
                        Debug.WriteLine($"[DASHBOARD] ⚠️ Area '{area.Name}' chưa gán Lane → BỎ QUA");

                        // Hiển thị cảnh báo trên card
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            card.ParkedCount = 0;
                            card.AvailableCount = 0;
                            card.Name = $"{area.Name} ⚠️"; // Thêm icon cảnh báo
                        });
                        continue; //  BỎ QUA Area này, không đếm
                    }

                    var service = _parkingServices[area.Id];
                    var laneIds = _areaLaneIds[area.Id];

                    // Đếm xe
                    long count = await service.CountParkedCarsAsync(laneIds, area.VehicleGroupID);

                    if (count >= 0)
                    {
                        int parked = (int)count;
                        int available = area.MaxCapacity - parked;

                        // Cập nhật card
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            card.ParkedCount = parked;
                            card.AvailableCount = available;
                        });

                        totalParked += parked;
                        totalAvailable += available;
                        totalCapacity += area.MaxCapacity;

                        Debug.WriteLine($"[DASHBOARD] ✅ {area.Name}: {parked}/{area.MaxCapacity}");
                    }
                    else
                    {
                        Debug.WriteLine($"[DASHBOARD] ❌ {area.Name}: Lỗi đếm xe");
                    }
                }

                // Cập nhật tổng
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    TxtTotalParked.Text = totalParked.ToString();
                    TxtTotalAvailable.Text = totalAvailable.ToString();
                    TxtLastUpdate.Text = $"Cập nhật: {DateTime.Now:HH:mm:ss}";
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[DASHBOARD REFRESH ERROR] {ex.Message}");
            }
        }

        /// <summary>
        /// Bắt đầu auto-refresh
        /// </summary>
        private void StartAutoRefresh()
        {
            _timer?.Stop();
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(_refreshInterval)
            };
            _timer.Tick += async (s, e) => await RefreshDataAsync();
            _timer.Start();

            Debug.WriteLine($"[DASHBOARD] Auto-refresh started: {_refreshInterval}s");
        }

        /// <summary>
        /// Thay đổi interval
        /// </summary>
        private void CbRefreshInterval_Changed(object? sender, SelectionChangedEventArgs e)
        {
            if (CbRefreshInterval == null || CbRefreshInterval.SelectedIndex == -1) return;

            _refreshInterval = CbRefreshInterval.SelectedIndex switch
            {
                0 => 1,
                1 => 5,
                2 => 10,
                3 => 30,
                _ => 5
            };

            StartAutoRefresh();
            Debug.WriteLine($"[DASHBOARD] Refresh interval changed to {_refreshInterval}s");
        }

        /// <summary>
        /// Làm mới thủ công
        /// </summary>
        private async void BtnRefresh_Click(object? sender, RoutedEventArgs e)
        {
            BtnRefresh.IsEnabled = false;
            await RefreshDataAsync();
            await Task.Delay(500); // Debounce
            BtnRefresh.IsEnabled = true;
        }

        /// <summary>
        /// Mở Settings
        /// </summary>
        private void BtnSettings_Click(object? sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow();
            settingsWindow.ShowDialog(this);

            // Reload sau khi đóng settings
            _ = ReloadAfterSettingsAsync();
        }

        private async Task ReloadAfterSettingsAsync()
        {
            await Task.Delay(500);

            // Clear và reload
            _areaCards.Clear();
            _parkingServices.Clear();
            _areaLaneIds.Clear();

            await LoadAreasAsync();
        }

        /// <summary>
        /// Đóng window
        /// </summary>
        private void BtnClose_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Cleanup khi đóng
        /// </summary>
        protected override void OnClosing(WindowClosingEventArgs e)
        {
            _timer?.Stop();
            _cts?.Cancel();

            base.OnClosing(e);
        }
    }
}