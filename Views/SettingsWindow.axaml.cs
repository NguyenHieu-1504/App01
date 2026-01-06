using App01.Models;
using App01.Services;
using App01.ViewModels;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace App01.Views
{
    public partial class SettingsWindow : Window
    {
        private readonly ConfigService _configService;
        private bool _isDataLoading = false;

        // ObservableCollection giúp UI tự động cập nhật khi list thay đổi
        private ObservableCollection<Area> _areas = new();
        private ObservableCollection<LedBoard> _ledBoards = new();
        private ObservableCollection<DisplayTemplate> _templates = new();

        // Collections cho Gates & Lanes
        private ObservableCollection<Gate> _gates = new();
        private ObservableCollection<Lane> _lanes = new();
        private ObservableCollection<Area> _areasForGate = new();

        public SettingsWindow()
        {
            InitializeComponent();
            _configService = new ConfigService();

            // Gán nguồn dữ liệu cho DataGrid
            dgAreas.ItemsSource = _areas;
            dgLedBoards.ItemsSource = _ledBoards;
            dgTemplates.ItemsSource = _templates;

            LoadData();
            Debug.WriteLine($"[DEBUG] After LoadData - Areas: {_areas.Count}");
            Debug.WriteLine($"[DEBUG] After LoadData - LedBoards: {_ledBoards.Count}");
            Debug.WriteLine($"[DEBUG] After LoadData - Templates: {_templates.Count}");
        }

        private void LoadData()
        {
            _isDataLoading = true; // Bắt đầu load, chặn sự kiện

            try
            {
                // 1. Load MongoDB Config
                var config = _configService.GetConfig();
                if (config != null)
                {
                    txtMongoConnection.Text = config.MongoConnectionString;
                    txtDatabaseName.Text = config.DatabaseName;
                    txtCollectionName.Text = "tblCardEventDay";
                }

                // 2. Load Lists
                ReloadAreas();
                ReloadLedBoards();
                ReloadTemplates();
                ReloadGatesAndLanes();
                ReloadStartupSettings(); //  thay đổi CheckBox, nhưng cờ đang bật
            }
            finally
            {
                _isDataLoading = false; // Load xong, cho phép sự kiện chạy bình thường
            }
        }

        private void ReloadStartupSettings()
        {
            var chkStartup = this.FindControl<CheckBox>("ChkStartWithWindows");
            if (chkStartup != null)
            {
                // Kiểm tra trạng thái thực tế từ Registry
                bool isEnabled = StartupManager.IsStartupEnabled();
                chkStartup.IsChecked = isEnabled;

                Debug.WriteLine($"[SETTINGS] Startup with Windows: {isEnabled}");
            }
        }

        private void ChkStartWithWindows_Changed(object? sender, RoutedEventArgs e)
        {
            // Nếu đang load dữ liệu thì không làm gì cả (tránh hiện thông báo lặp lại)
            if (_isDataLoading) return;

            var chkStartup = sender as CheckBox;
            if (chkStartup == null) return;

            bool enable = chkStartup.IsChecked ?? false;

            // Cập nhật Registry
            bool success = StartupManager.SetStartup(enable);

            if (success)
            {
                // Lưu vào config
                var config = _configService.GetConfig();
                config.StartWithWindows = enable;
                _configService.UpdateConfig(config);

                string message = enable
                    ? " Đã bật tự động khởi động cùng hệ thống!\n\nỨng dụng sẽ tự động chạy khi bạn khởi động máy tính."
                    : " Đã tắt tự động khởi động!\n\nỨng dụng sẽ không tự chạy khi hệ thống khởi động nữa.";

                ShowMessage(message);
                Debug.WriteLine($"[SETTINGS] Startup with Windows: {enable}");
            }
            else
            {
                // Rollback checkbox nếu thất bại
                chkStartup.IsChecked = !enable;
                ShowMessage("❌ Không thể thay đổi cài đặt!\n\nVui lòng chạy ứng dụng với quyền Administrator.");
            }
        }

        // --- Helpers Load Data ---
        private void ReloadAreas()
        {
            _areas.Clear();
            var list = _configService.GetAllAreas();
            foreach (var item in list) _areas.Add(item);
        }

        private void ReloadLedBoards()
        {
            _ledBoards.Clear();
            var list = _configService.GetAllLedBoards();
            foreach (var item in list) _ledBoards.Add(item);
        }

        private void ReloadTemplates()
        {
            _templates.Clear();
            var list = _configService.GetAllDisplayTemplates();
            foreach (var item in list) _templates.Add(item);
            Debug.WriteLine($"[DEBUG] ReloadTemplates: Loaded {list.Count} items from DB");
            if (list.Count > 0)
            {
                Debug.WriteLine($"[DEBUG] First item: Id={list[0].Id}, Name={list[0].Name}");
            }
        }

        // Reload Gates & Lanes
        private void ReloadGatesAndLanes()
        {
            // Load Areas cho ComboBox
            var areas = _configService.GetAllAreas();
            _areasForGate.Clear();
            foreach (var area in areas)
                _areasForGate.Add(area);

            var cbAreaForGate = this.FindControl<ComboBox>("CbAreaForGate");
            if (cbAreaForGate != null)
            {
                cbAreaForGate.ItemsSource = _areasForGate;
                cbAreaForGate.DisplayMemberBinding = new Avalonia.Data.Binding("Name");
            }

            // Load Gates
            var gates = _configService.GetAllGates();
            _gates.Clear();
            foreach (var gate in gates)
                _gates.Add(gate);

            var gatesGrid = this.FindControl<DataGrid>("GatesDataGrid");
            if (gatesGrid != null)
                gatesGrid.ItemsSource = _gates;

            // Load Gates cho ComboBox của Lane
            var cbGateForLane = this.FindControl<ComboBox>("CbGateForLane");
            if (cbGateForLane != null)
            {
                cbGateForLane.ItemsSource = _gates;
                cbGateForLane.DisplayMemberBinding = new Avalonia.Data.Binding("GateName"); // ✅ Sửa từ "Name" → "GateName"
            }

            // Load Lanes
            var lanes = _configService.GetAllLanes();
            _lanes.Clear();
            foreach (var lane in lanes)
                _lanes.Add(lane);

            var lanesGrid = this.FindControl<DataGrid>("LanesDataGrid");
            if (lanesGrid != null)
                lanesGrid.ItemsSource = _lanes;

            Debug.WriteLine($"[SETTINGS] Loaded {gates.Count} gates, {lanes.Count} lanes");
        }

        // ================= EVENTS: MONGODB =================
        private async void BtnTestMongo_Click(object? sender, RoutedEventArgs e)
        {
            txtMongoTestResult.Text = "Đang kết nối...";
            try
            {
                var service = new ParkingDataService();
                bool ok = service.Connect(txtMongoConnection.Text, txtDatabaseName.Text);
                if (ok)
                {
                    long count = await service.CountParkedCarsAsync();
                    txtMongoTestResult.Text = $"✅ OK! Tìm thấy {count} xe.";
                    txtMongoTestResult.Foreground = Avalonia.Media.Brushes.Green;
                }
                else
                {
                    txtMongoTestResult.Text = "❌ Kết nối thất bại!";
                    txtMongoTestResult.Foreground = Avalonia.Media.Brushes.Red;
                }
            }
            catch (Exception ex)
            {
                txtMongoTestResult.Text = $"Lỗi: {ex.Message}";
            }
        }

        // ================= EVENTS: AREAS =================
        private void DgAreas_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            bool hasSelect = dgAreas.SelectedItem != null;
            if (btnEditArea != null) btnEditArea.IsEnabled = hasSelect;
            if (btnDeleteArea != null) btnDeleteArea.IsEnabled = hasSelect;
        }

        private async void BtnAddArea_Click(object? sender, RoutedEventArgs e)
        {
            var dialog = new AreaEditDialog();
            var result = await dialog.ShowDialog<Area?>(this);
            if (result != null)
            {
                _configService.InsertArea(result);
                ReloadAreas();
                ReloadGatesAndLanes(); // Cập nhật ComboBox Area
            }
        }

        private async void BtnEditArea_Click(object? sender, RoutedEventArgs e)
        {
            if (dgAreas.SelectedItem is Area item)
            {
                var dialog = new AreaEditDialog(item);
                var result = await dialog.ShowDialog<Area?>(this);
                if (result != null)
                {
                    _configService.UpdateArea(result);
                    ReloadAreas();
                    ReloadGatesAndLanes(); // Cập nhật ComboBox Area
                }
            }
        }

        private async void BtnDeleteArea_Click(object? sender, RoutedEventArgs e)
        {
            if (dgAreas.SelectedItem is Area item)
            {
                _configService.DeleteArea(item.Id);
                ReloadAreas();
                ReloadGatesAndLanes(); // Cập nhật ComboBox Area
            }
        }

        // ================= EVENTS: LED BOARDS =================
        private void DgLedBoards_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            bool hasSelect = dgLedBoards.SelectedItem != null;
            if (btnEditLed != null) btnEditLed.IsEnabled = hasSelect;
            if (btnDeleteLed != null) btnDeleteLed.IsEnabled = hasSelect;
            if (btnTestLed != null) btnTestLed.IsEnabled = hasSelect;
        }

        private async void BtnAddLed_Click(object? sender, RoutedEventArgs e)
        {
            var areas = _configService.GetAllAreas();
            var templates = _configService.GetActiveDisplayTemplates();

            var dialog = new LedBoardEditDialog(areas, templates);
            var result = await dialog.ShowDialog<LedBoard?>(this);
            if (result != null)
            {
                _configService.InsertLedBoard(result);
                ReloadLedBoards();
            }
        }

        private async void BtnEditLed_Click(object? sender, RoutedEventArgs e)
        {
            if (dgLedBoards.SelectedItem is LedBoard item)
            {
                var areas = _configService.GetAllAreas();
                var templates = _configService.GetAllDisplayTemplates();

                var dialog = new LedBoardEditDialog(areas, templates, item);
                var result = await dialog.ShowDialog<LedBoard?>(this);
                if (result != null)
                {
                    _configService.UpdateLedBoard(result);
                    ReloadLedBoards();
                }
            }
        }

        private void BtnDeleteLed_Click(object? sender, RoutedEventArgs e)
        {
            if (dgLedBoards.SelectedItem is LedBoard item)
            {
                _configService.DeleteLedBoard(item.Id);
                ReloadLedBoards();
            }
        }

        private async void BtnTestLed_Click(object? sender, RoutedEventArgs e)
        {
            if (dgLedBoards.SelectedItem is LedBoard item)
            {
                using var led = new LedService();
                if (led.Connect(item.IpAddress, item.Port))
                {
                    await led.DisplayTwoLinesAsync("TEST", "OK", 1, 1, 0, 10);
                }
            }
        }

        // ================= EVENTS: TEMPLATES =================
        private void DgTemplates_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            bool hasSelect = dgTemplates.SelectedItem != null;
            if (btnEditTemplate != null) btnEditTemplate.IsEnabled = hasSelect;
            if (btnDeleteTemplate != null) btnDeleteTemplate.IsEnabled = hasSelect;
        }

        private async void BtnAddTemplate_Click(object? sender, RoutedEventArgs e)
        {
            var dialog = new TemplateEditDialog();
            var result = await dialog.ShowDialog<DisplayTemplate?>(this);
            if (result != null)
            {
                _configService.InsertDisplayTemplate(result);
                ReloadTemplates();
            }
        }

        private async void BtnEditTemplate_Click(object? sender, RoutedEventArgs e)
        {
            if (dgTemplates.SelectedItem is DisplayTemplate item)
            {
                var dialog = new TemplateEditDialog(item);
                var result = await dialog.ShowDialog<DisplayTemplate?>(this);
                if (result != null)
                {
                    _configService.UpdateDisplayTemplate(result);
                    ReloadTemplates();
                }
            }
        }

        private void BtnDeleteTemplate_Click(object? sender, RoutedEventArgs e)
        {
            if (dgTemplates.SelectedItem is DisplayTemplate item)
            {
                _configService.DeleteDisplayTemplate(item.Id);
                ReloadTemplates();
            }
        }

        // ================= GATES & LANES =================

        /// <summary>
        /// Load Gates từ MongoDB vào hệ thống
        /// </summary>
        private async void BtnLoadGatesFromMongo_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                var config = _configService.GetConfig();

                if (string.IsNullOrWhiteSpace(config.ResourceDatabaseName))
                {
                    await ShowMessageDialog("Lỗi cấu hình",
                        "⚠️ Chưa cấu hình Resource Database!");
                    return;
                }

                var service = new GateLaneService();

                if (!service.Connect(config.MongoConnectionString, config.ResourceDatabaseName))
                {
                    await ShowMessageDialog("Lỗi", "❌ Không thể kết nối Resource Database!");
                    return;
                }

                var gatesFromMongo = await service.GetAllGatesFromMongoAsync();

                if (gatesFromMongo.Count == 0)
                {
                    await ShowMessageDialog("Thông báo", "⚠️ Không tìm thấy Gate nào trong MongoDB!");
                    return;
                }

                //  Hiển thị dialog import
                var areas = _configService.GetAllAreas();
                var dialog = new GateImportDialog(gatesFromMongo, areas);
                var result = await dialog.ShowDialog<GateImportDialog?>(this);

                if (result != null && result.SelectedGates.Count > 0)
                {
                    // Lưu vào database
                    foreach (var gate in result.SelectedGates)
                    {
                        _configService.InsertGate(gate);
                        _gates.Add(gate);
                    }

                    ReloadGatesAndLanes();

                    await ShowMessageDialog("Thành công",
                        $"✅ Đã import {result.SelectedGates.Count} Gates vào '{result.SelectedArea?.Name}'!");
                }
            }
            catch (Exception ex)
            {
                await ShowMessageDialog("Lỗi", $"❌ {ex.Message}");
            }
        }

        /// <summary>
        /// Load Lanes từ MongoDB vào hệ thống
        /// </summary>
        private async void BtnLoadLanesFromMongo_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                var config = _configService.GetConfig();

                if (string.IsNullOrWhiteSpace(config.ResourceDatabaseName))
                {
                    await ShowMessageDialog("Lỗi cấu hình",
                        "⚠️ Chưa cấu hình Resource Database!");
                    return;
                }

                var service = new GateLaneService();

                if (!service.Connect(config.MongoConnectionString, config.ResourceDatabaseName))
                {
                    await ShowMessageDialog("Lỗi", "❌ Không thể kết nối Resource Database!");
                    return;
                }

                var lanesFromMongo = await service.GetAllLanesFromMongoAsync();

                if (lanesFromMongo.Count == 0)
                {
                    await ShowMessageDialog("Thông báo", "⚠️ Không tìm thấy Lane nào trong MongoDB!");
                    return;
                }

                // ✨ Hiển thị dialog import
                var gates = _configService.GetAllGates();

                if (gates.Count == 0)
                {
                    await ShowMessageDialog("Lỗi",
                        "⚠️ Chưa có Gate nào!\n\n" +
                        "Vui lòng import Gates trước, sau đó mới import Lanes.");
                    return;
                }

                var dialog = new LaneImportDialog(lanesFromMongo, gates);
                var result = await dialog.ShowDialog<LaneImportDialog?>(this);

                if (result != null && result.SelectedLanes.Count > 0)
                {
                    // Lưu vào database
                    foreach (var lane in result.SelectedLanes)
                    {
                        _configService.InsertLane(lane);
                        _lanes.Add(lane);
                    }

                    ReloadGatesAndLanes();

                    await ShowMessageDialog("Thành công",
                        $"✅ Đã import {result.SelectedLanes.Count} Lanes vào '{result.SelectedGate?.GateName}'!");
                }
            }
            catch (Exception ex)
            {
                await ShowMessageDialog("Lỗi", $"❌ {ex.Message}");
            }
        }

        private void BtnAddGate_Click(object? sender, RoutedEventArgs e)
        {
            var cbAreaForGate = this.FindControl<ComboBox>("CbAreaForGate");
            var txtGateIdMongo = this.FindControl<TextBox>("TxtGateIdMongo");
            var txtGateCode = this.FindControl<TextBox>("TxtGateCode");
            var txtGateName = this.FindControl<TextBox>("TxtGateName");

            if (cbAreaForGate?.SelectedItem is not Area selectedArea)
            {
                ShowMessage("⚠️ Vui lòng chọn khu vực!");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtGateIdMongo?.Text))
            {
                ShowMessage("⚠️ Vui lòng nhập Gate ID từ MongoDB!");
                return;
            }

            var gate = new Gate
            {
                GateIdMongo = txtGateIdMongo.Text.Trim(),
                GateCode = txtGateCode?.Text?.Trim() ?? "",
                GateName = txtGateName?.Text?.Trim() ?? "",
                AreaId = selectedArea.Id,
                IsActive = true
            };

            _configService.InsertGate(gate);
            _gates.Add(gate);

            // Clear form
            if (txtGateIdMongo != null) txtGateIdMongo.Text = "";
            if (txtGateCode != null) txtGateCode.Text = "";
            if (txtGateName != null) txtGateName.Text = "";

            // Reload để cập nhật ComboBox CbGateForLane
            ReloadGatesAndLanes();

            Debug.WriteLine($"[SETTINGS] ✅ Added gate: {gate.GateName}");
        }

        private void BtnUpdateGate_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int gateId)
            {
                var gate = _gates.FirstOrDefault(g => g.Id == gateId);
                if (gate != null)
                {
                    _configService.UpdateGate(gate);
                    Debug.WriteLine($"[SETTINGS] ✅ Updated gate ID={gateId}");
                }
            }
        }

        private void BtnDeleteGate_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int gateId)
            {
                var gate = _gates.FirstOrDefault(g => g.Id == gateId);
                if (gate != null)
                {
                    _configService.DeleteGate(gateId);
                    _gates.Remove(gate);

                    // Xóa luôn các Lane thuộc Gate này
                    var lanesToRemove = _lanes.Where(l => l.GateId == gateId).ToList();
                    foreach (var lane in lanesToRemove)
                    {
                        _lanes.Remove(lane);
                    }

                    ReloadGatesAndLanes(); // Reload để cập nhật ComboBox
                    Debug.WriteLine($"[SETTINGS] ✅ Deleted gate ID={gateId}");
                }
            }
        }

        private void BtnAddLane_Click(object? sender, RoutedEventArgs e)
        {
            var cbGateForLane = this.FindControl<ComboBox>("CbGateForLane");
            var txtLaneIdMongo = this.FindControl<TextBox>("TxtLaneIdMongo");
            var txtLaneCode = this.FindControl<TextBox>("TxtLaneCode");
            var txtLaneName = this.FindControl<TextBox>("TxtLaneName");

            if (cbGateForLane?.SelectedItem is not Gate selectedGate)
            {
                ShowMessage("⚠️ Vui lòng chọn cổng!");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtLaneIdMongo?.Text))
            {
                ShowMessage("⚠️ Vui lòng nhập Lane ID từ MongoDB!");
                return;
            }

            var lane = new Lane
            {
                LaneIdMongo = txtLaneIdMongo.Text.Trim(),
                LaneCode = txtLaneCode?.Text?.Trim() ?? "",
                LaneName = txtLaneName?.Text?.Trim() ?? "",
                GateId = selectedGate.Id,
                IsActive = true
            };

            _configService.InsertLane(lane);
            _lanes.Add(lane);

            // Clear form
            if (txtLaneIdMongo != null) txtLaneIdMongo.Text = "";
            if (txtLaneCode != null) txtLaneCode.Text = "";
            if (txtLaneName != null) txtLaneName.Text = "";

            Debug.WriteLine($"[SETTINGS] ✅ Added lane: {lane.LaneName} (MongoDB ID: {lane.LaneIdMongo})");
        }

        private void BtnUpdateLane_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int laneId)
            {
                var lane = _lanes.FirstOrDefault(l => l.Id == laneId);
                if (lane != null)
                {
                    _configService.UpdateLane(lane);
                    Debug.WriteLine($"[SETTINGS] ✅ Updated lane ID={laneId}");
                }
            }
        }

        private void BtnDeleteLane_Click(object? sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int laneId)
            {
                var lane = _lanes.FirstOrDefault(l => l.Id == laneId);
                if (lane != null)
                {
                    _configService.DeleteLane(laneId);
                    _lanes.Remove(lane);
                    Debug.WriteLine($"[SETTINGS] ✅ Deleted lane ID={laneId}");
                }
            }
        }

        // ================= SAVE & CLOSE =================
        private async void BtnSave_Click(object? sender, RoutedEventArgs e)
        {
            var config = _configService.GetConfig();
            config.MongoConnectionString = txtMongoConnection.Text;
            config.DatabaseName = txtDatabaseName.Text;
            config.CollectionName = "tblCardEventDay";

            _configService.UpdateConfig(config);

            Debug.WriteLine("[SETTINGS] Đã lưu cấu hình!");

            // Reload MainWindow
            if (Owner is MainWindow mainWindow)
            {
                if (mainWindow.DataContext is MainWindowViewModel vm)
                {
                    Close();
                    await vm.ReloadConfigurationAsync();
                }
            }
            else
            {
                Close();
            }
        }

        private void BtnClose_Click(object? sender, RoutedEventArgs e)
        {
            Close();
        }

        // ================= HELPERS =================
        private async void ShowMessage(string message)
        {
            var msgBox = new Window
            {
                Title = "Thông báo",
                Width = 350,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Content = new StackPanel
                {
                    Margin = new Avalonia.Thickness(20),
                    Children =
                    {
                        new TextBlock
                        {
                            Text = message,
                            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                            Margin = new Avalonia.Thickness(0, 0, 0, 20)
                        },
                        new Button
                        {
                            Content = "OK",
                            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                            Width = 80
                        }
                    }
                }
            };

            var okButton = (msgBox.Content as StackPanel)?.Children[1] as Button;
            if (okButton != null)
            {
                okButton.Click += (s, e) => msgBox.Close();
            }

            await msgBox.ShowDialog(this);
        }

        private async Task ShowMessageDialog(string title, string message)
        {
            var dialog = new Window
            {
                Title = title,
                Width = 350,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false,
                Content = new StackPanel
                {
                    Margin = new Avalonia.Thickness(20),
                    Spacing = 20,
                    Children =
                    {
                        new TextBlock { Text = message, TextWrapping = Avalonia.Media.TextWrapping.Wrap },
                        new Button
                        {
                            Content = "OK",
                            Width = 80,
                            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right
                        }
                    }
                }
            };

            var btnOk = (dialog.Content as StackPanel)?.Children[1] as Button;
            if (btnOk != null)
            {
                btnOk.Click += (_, _) => dialog.Close();
            }

            await dialog.ShowDialog(this);
        }
    }
}