//using System;
//using System.Collections.ObjectModel; 
//using System.Linq;
//using Avalonia.Controls;
//using Avalonia.Interactivity;
//using App01.Models;
//using App01.Services;

//namespace App01.Views
//{
//    public partial class SettingsWindow : Window
//    {
//        private readonly ConfigService _configService;

//        // Khai báo ObservableCollection để UI tự động cập nhật
//        private ObservableCollection<Area> _areas = new();
//        private ObservableCollection<LedBoard> _ledBoards = new();
//        private ObservableCollection<DisplayTemplate> _templates = new();

//        public SettingsWindow()
//        {
//            InitializeComponent();
//            _configService = new ConfigService();

//            // Gán DataContext hoặc ItemsSource 1 lần duy nhất ở đây
//            dgAreas.ItemsSource = _areas;
//            dgLedBoards.ItemsSource = _ledBoards;
//            dgTemplates.ItemsSource = _templates;

//            LoadData();
//        }

//        private void LoadData()
//        {
//            // Tab 1: MongoDB Config
//            var config = _configService.GetConfig();
//            if (config != null)
//            {
//                txtMongoConnection.Text = config.MongoConnectionString;
//                txtDatabaseName.Text = config.DatabaseName;
//                txtCollectionName.Text = config.CollectionName;
//            }

//            // Load dữ liệu cho các Tab khác
//            ReloadAreas();
//            ReloadLedBoards();
//            ReloadTemplates();
//        }

//        // --- Hàm hỗ trợ Load lại dữ liệu từ DB ---
//        private void ReloadAreas()
//        {
//            _areas.Clear();
//            var list = _configService.GetAllAreas();
//            foreach (var item in list) _areas.Add(item);
//        }

//        private void ReloadLedBoards()
//        {
//            _ledBoards.Clear();
//            var list = _configService.GetAllLedBoards();
//            foreach (var item in list) _ledBoards.Add(item);
//        }

//        private void ReloadTemplates()
//        {
//            _templates.Clear();
//            var list = _configService.GetAllDisplayTemplates();
//            foreach (var item in list) _templates.Add(item);
//        }

//        // ==================== TAB 1: MONGODB ====================
//        private async void BtnTestMongo_Click(object? sender, RoutedEventArgs e)
//        {
//            try
//            {
//                txtMongoTestResult.Text = "Đang kiểm tra...";
//                txtMongoTestResult.Foreground = Avalonia.Media.Brushes.Gray;

//                string connString = txtMongoConnection.Text ?? "";
//                string dbName = txtDatabaseName.Text ?? "";

//                // Kiểm tra null để tránh lỗi
//                if (string.IsNullOrWhiteSpace(connString) || string.IsNullOrWhiteSpace(dbName))
//                {
//                    txtMongoTestResult.Text = "❌ Vui lòng nhập Connection String và Database Name";
//                    return;
//                }

//                var testService = new ParkingDataService();
//                bool connected = testService.Connect(connString, dbName);

//                if (connected)
//                {
//                    long count = await testService.CountParkedCarsAsync();
//                    txtMongoTestResult.Text = $"✅ Kết nối thành công! Tìm thấy {count} xe.";
//                    txtMongoTestResult.Foreground = Avalonia.Media.Brushes.Green;
//                }
//                else
//                {
//                    txtMongoTestResult.Text = "❌ Không thể kết nối (Sai thông tin hoặc Firewall chặn)!";
//                    txtMongoTestResult.Foreground = Avalonia.Media.Brushes.Red;
//                }
//            }
//            catch (Exception ex)
//            {
//                txtMongoTestResult.Text = $"❌ Lỗi Exception: {ex.Message}";
//                txtMongoTestResult.Foreground = Avalonia.Media.Brushes.Red;
//            }
//        }

//        // ==================== TAB 2: AREAS ====================
//        private void DgAreas_SelectionChanged(object? sender, SelectionChangedEventArgs e)
//        {
//            // Logic: Chỉ Enable nút khi có dòng được chọn
//            bool hasSelection = dgAreas.SelectedItem != null;
//            if (btnEditArea != null) btnEditArea.IsEnabled = hasSelection;
//            if (btnDeleteArea != null) btnDeleteArea.IsEnabled = hasSelection;
//        }

//        private async void BtnAddArea_Click(object? sender, RoutedEventArgs e)
//        {
//            var dialog = new AreaEditDialog();
//            var result = await dialog.ShowDialog<Area?>(this);

//            if (result != null)
//            {
//                // 1. Lưu vào DB
//                _configService.InsertArea(result);
//                // 2. Load lại từ DB để đảm bảo có ID mới nhất
//                ReloadAreas();
//            }
//        }

//        private async void BtnEditArea_Click(object? sender, RoutedEventArgs e)
//        {
//            if (dgAreas.SelectedItem is not Area selected) return;

//            var dialog = new AreaEditDialog(selected);
//            var result = await dialog.ShowDialog<Area?>(this);

//            if (result != null)
//            {
//                _configService.UpdateArea(result);
//                ReloadAreas();
//            }
//        }

//        private async void BtnDeleteArea_Click(object? sender, RoutedEventArgs e)
//        {
//            if (dgAreas.SelectedItem is not Area selected) return;

//            if (await ShowConfirmDialog($"Xóa khu vực '{selected.Name}'?"))
//            {
//                _configService.DeleteArea(selected.Id);
//                ReloadAreas();
//            }
//        }

//        // ==================== TAB 3: LED BOARDS ====================
//        private void DgLedBoards_SelectionChanged(object? sender, SelectionChangedEventArgs e)
//        {
//            bool hasSelection = dgLedBoards.SelectedItem != null;
//            if (btnEditLed != null) btnEditLed.IsEnabled = hasSelection;
//            if (btnDeleteLed != null) btnDeleteLed.IsEnabled = hasSelection;
//            if (btnTestLed != null) btnTestLed.IsEnabled = hasSelection;
//        }

//        private async void BtnAddLed_Click(object? sender, RoutedEventArgs e)
//        {
//            // Lấy danh sách mới nhất để truyền vào dialog
//            var areas = _configService.GetAllAreas();
//            var templates = _configService.GetAllDisplayTemplates();

//            var dialog = new LedBoardEditDialog(areas, templates);
//            var result = await dialog.ShowDialog<LedBoard?>(this);

//            if (result != null)
//            {
//                _configService.InsertLedBoard(result);
//                ReloadLedBoards();
//            }
//        }

//        private async void BtnEditLed_Click(object? sender, RoutedEventArgs e)
//        {
//            if (dgLedBoards.SelectedItem is not LedBoard selected) return;

//            var areas = _configService.GetAllAreas();
//            var templates = _configService.GetAllDisplayTemplates();

//            var dialog = new LedBoardEditDialog(areas, templates, selected);
//            var result = await dialog.ShowDialog<LedBoard?>(this);

//            if (result != null)
//            {
//                _configService.UpdateLedBoard(result);
//                ReloadLedBoards();
//            }
//        }

//        private async void BtnDeleteLed_Click(object? sender, RoutedEventArgs e)
//        {
//            if (dgLedBoards.SelectedItem is not LedBoard selected) return;

//            if (await ShowConfirmDialog($"Xóa bảng LED '{selected.Name}'?"))
//            {
//                _configService.DeleteLedBoard(selected.Id);
//                ReloadLedBoards();
//            }
//        }

//        private async void BtnTestLed_Click(object? sender, RoutedEventArgs e)
//        {
//            if (dgLedBoards.SelectedItem is not LedBoard selected) return;

//            try
//            {
//                using var ledService = new LedService();
//                bool connected = ledService.Connect(selected.IpAddress, selected.Port);

//                if (connected)
//                {
//                    await ledService.DisplayTwoLinesAsync("TEST OK", "CONNECTION", 2, 2, 0, 10);
//                    await ShowMessageDialog("✅ Thành công", $"Đã gửi tín hiệu đến {selected.IpAddress}");
//                }
//                else
//                {
//                    await ShowMessageDialog("❌ Thất bại", $"Không thể kết nối {selected.IpAddress}:{selected.Port}");
//                }
//            }
//            catch (Exception ex)
//            {
//                await ShowMessageDialog("❌ Lỗi", ex.Message);
//            }
//        }

//        // ==================== TAB 4: TEMPLATES ====================
//        private void DgTemplates_SelectionChanged(object? sender, SelectionChangedEventArgs e)
//        {
//            bool hasSelection = dgTemplates.SelectedItem != null;
//            if (btnEditTemplate != null) btnEditTemplate.IsEnabled = hasSelection;
//            if (btnDeleteTemplate != null) btnDeleteTemplate.IsEnabled = hasSelection;
//        }

//        private async void BtnAddTemplate_Click(object? sender, RoutedEventArgs e)
//        {
//            var dialog = new TemplateEditDialog();
//            var result = await dialog.ShowDialog<DisplayTemplate?>(this);

//            if (result != null)
//            {
//                _configService.InsertDisplayTemplate(result);
//                ReloadTemplates();
//            }
//        }

//        private async void BtnEditTemplate_Click(object? sender, RoutedEventArgs e)
//        {
//            if (dgTemplates.SelectedItem is not DisplayTemplate selected) return;

//            var dialog = new TemplateEditDialog(selected);
//            var result = await dialog.ShowDialog<DisplayTemplate?>(this);

//            if (result != null)
//            {
//                _configService.UpdateDisplayTemplate(result);
//                ReloadTemplates();
//            }
//        }

//        private async void BtnDeleteTemplate_Click(object? sender, RoutedEventArgs e)
//        {
//            if (dgTemplates.SelectedItem is not DisplayTemplate selected) return;

//            if (await ShowConfirmDialog($"Xóa kịch bản '{selected.Name}'?"))
//            {
//                _configService.DeleteDisplayTemplate(selected.Id);
//                ReloadTemplates();
//            }
//        }

//        // ==================== SAVE & CLOSE ====================
//        private void BtnSave_Click(object? sender, RoutedEventArgs e)
//        {
//            var config = _configService.GetConfig() ?? new AppConfig();
//            config.MongoConnectionString = txtMongoConnection.Text ?? "";
//            config.DatabaseName = txtDatabaseName.Text ?? "";
//            config.CollectionName = txtCollectionName.Text ?? "";

//            _configService.UpdateConfig(config);
//            Close();
//        }

//        private void BtnClose_Click(object? sender, RoutedEventArgs e)
//        {
//            Close();
//        }

//        // ==================== DIALOG HELPERS ====================
//        private async System.Threading.Tasks.Task<bool> ShowConfirmDialog(string message)
//        {
//            // Tạo dialog xác nhận đơn giản bằng code (không cần file axaml)
//            var dialog = new Window
//            {
//                Title = "Xác nhận",
//                Width = 350,
//                Height = 150,
//                WindowStartupLocation = WindowStartupLocation.CenterOwner,
//                CanResize = false,
//                Content = new StackPanel
//                {
//                    Margin = new Avalonia.Thickness(20),
//                    Spacing = 20,
//                    Children = {
//                        new TextBlock { Text = message, TextWrapping = Avalonia.Media.TextWrapping.Wrap },
//                        new StackPanel {
//                            Orientation = Avalonia.Layout.Orientation.Horizontal,
//                            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right, Spacing = 10,
//                            Children = {
//                                new Button { Content = "Có", Width = 80, Name = "btnYes", Background = Avalonia.Media.Brushes.IndianRed, Foreground = Avalonia.Media.Brushes.White },
//                                new Button { Content = "Không", Width = 80, Name = "btnNo" }
//                            }
//                        }
//                    }
//                }
//            };

//            bool result = false;
//            var btnYes = dialog.FindControl<Button>("btnYes");
//            var btnNo = dialog.FindControl<Button>("btnNo");

//            if (btnYes != null) btnYes.Click += (_, _) => { result = true; dialog.Close(); };
//            if (btnNo != null) btnNo.Click += (_, _) => { result = false; dialog.Close(); };

//            await dialog.ShowDialog(this);
//            return result;
//        }

//        private async System.Threading.Tasks.Task ShowMessageDialog(string title, string message)
//        {
//            var dialog = new Window
//            {
//                Title = title,
//                Width = 350,
//                Height = 150,
//                WindowStartupLocation = WindowStartupLocation.CenterOwner,
//                CanResize = false,
//                Content = new StackPanel
//                {
//                    Margin = new Avalonia.Thickness(20),
//                    Spacing = 20,
//                    Children = {
//                        new TextBlock { Text = message, TextWrapping = Avalonia.Media.TextWrapping.Wrap },
//                        new Button { Content = "OK", Width = 80, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right, Name="btnOk" }
//                    }
//                }
//            };
//            var btnOk = dialog.FindControl<Button>("btnOk");
//            if (btnOk != null) btnOk.Click += (_, _) => dialog.Close();
//            await dialog.ShowDialog(this);
//        }
//    }
//}

using App01.Models;
using App01.Services;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace App01.Views
{
    public partial class SettingsWindow : Window
    {
        private readonly ConfigService _configService;

        // ObservableCollection giúp UI tự động cập nhật khi list thay đổi
        private ObservableCollection<Area> _areas = new();
        private ObservableCollection<LedBoard> _ledBoards = new();
        private ObservableCollection<DisplayTemplate> _templates = new();

        public SettingsWindow()
        {
            InitializeComponent();
            _configService = new ConfigService();

            // Gán nguồn dữ liệu cho DataGrid
            dgAreas.ItemsSource = _areas;
            dgLedBoards.ItemsSource = _ledBoards;
            dgTemplates.ItemsSource = _templates;

            LoadData();
        }

        private void LoadData()
        {
            // 1. Load MongoDB Config
            var config = _configService.GetConfig();
            if (config != null)
            {
                txtMongoConnection.Text = config.MongoConnectionString;
                txtDatabaseName.Text = config.DatabaseName;
                txtCollectionName.Text = config.CollectionName;
            }

            // 2. Load Lists
            ReloadAreas();
            ReloadLedBoards();
            ReloadTemplates();
        }

        // --- Helpers Load Data ---
        private void ReloadAreas()
        {
            _areas.Clear();
            var list = _configService.GetAllAreas();
            foreach (var item in list) _areas.Add(item);
            dgAreas.ItemsSource = null;
            dgAreas.ItemsSource = _areas;
        }

        private void ReloadLedBoards()
        {
            _ledBoards.Clear();
            var list = _configService.GetAllLedBoards();
            foreach (var item in list) _ledBoards.Add(item);
            dgLedBoards.ItemsSource = null;
            dgLedBoards.ItemsSource = _ledBoards;
        }

        private void ReloadTemplates()
        {
            _templates.Clear();
            var list = _configService.GetAllDisplayTemplates();
            foreach (var item in list) _templates.Add(item);
            Debug.WriteLine($"[DEBUG] ReloadTemplates: Loaded {list.Count} items from DB");  // Log count
            if (list.Count > 0)
            {
                Debug.WriteLine($"[DEBUG] First item: Id={list[0].Id}, Name={list[0].Name}");
            }
            dgTemplates.ItemsSource = null;
            dgTemplates.ItemsSource = _templates;
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
                }
            }
        }

        private async void BtnDeleteArea_Click(object? sender, RoutedEventArgs e)
        {
            if (dgAreas.SelectedItem is Area item)
            {
                // Confirm dialog đơn giản
                _configService.DeleteArea(item.Id);
                ReloadAreas();
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
            var templates = _configService.GetAllDisplayTemplates();

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

        // ================= SAVE & CLOSE =================
        private void BtnSave_Click(object? sender, RoutedEventArgs e)
        {
            var config = _configService.GetConfig();
            config.MongoConnectionString = txtMongoConnection.Text;
            config.DatabaseName = txtDatabaseName.Text;
            config.CollectionName = txtCollectionName.Text;

            _configService.UpdateConfig(config);
            Close();
        }

        private void BtnClose_Click(object? sender, RoutedEventArgs e) => Close();
    }
}