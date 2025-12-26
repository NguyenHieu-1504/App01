
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
            Debug.WriteLine($"[DEBUG] After LoadData - Areas: {_areas.Count}");
            Debug.WriteLine($"[DEBUG] After LoadData - LedBoards: {_ledBoards.Count}");
            Debug.WriteLine($"[DEBUG] After LoadData - Templates: {_templates.Count}");
        }

        private void LoadData()
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
        }

        // --- Helpers Load Data ---
        private void ReloadAreas()
        {
            _areas.Clear();
            var list = _configService.GetAllAreas();
            foreach (var item in list) _areas.Add(item);
            //dgAreas.ItemsSource = null;
            //dgAreas.ItemsSource = _areas;
        }

        private void ReloadLedBoards()
        {
            _ledBoards.Clear();
            var list = _configService.GetAllLedBoards();
            foreach (var item in list) _ledBoards.Add(item);
            //dgLedBoards.ItemsSource = null;
            //dgLedBoards.ItemsSource = _ledBoards;
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
            //dgTemplates.ItemsSource = null;
            //dgTemplates.ItemsSource = _templates;
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

        // ================= SAVE & CLOSE =================
        private async void BtnSave_Click(object? sender, RoutedEventArgs e)
        {
            var config = _configService.GetConfig();
            config.MongoConnectionString = txtMongoConnection.Text;
            config.DatabaseName = txtDatabaseName.Text;
            config.CollectionName = "tblCardEventDay";

            _configService.UpdateConfig(config);

            Debug.WriteLine("[SETTINGS] Đã lưu cấu hình!");

            // Hiện thông báo 
            //await ShowMessageDialog("✅ Thành công", "Cấu hình đã được lưu!\nĐang reload ứng dụng...");

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

        //  HELPER 
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
                    Children = {
                new TextBlock { Text = message, TextWrapping = Avalonia.Media.TextWrapping.Wrap },
                new Button { Content = "OK", Width = 80, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right, Name="btnOk" }
            }
                }
            };
            var btnOk = dialog.FindControl<Button>("btnOk");
            if (btnOk != null) btnOk.Click += (_, _) => dialog.Close();
            await dialog.ShowDialog(this);
        }

        private void BtnClose_Click(object? sender, RoutedEventArgs e) => Close();
    }
}