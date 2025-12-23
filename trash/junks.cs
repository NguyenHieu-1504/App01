////using App01.Models;
////using SQLite;
////using System;
////using System.Collections.Generic;
////using System.IO;
////using System.Linq;

////namespace App01.Services
////{
////    public class ConfigService
////    {
////        private readonly SQLiteConnection _db;

////        public ConfigService()
////        {
////            var dbPath = Path.Combine(Environment.CurrentDirectory, "parking_config.db");
////            _db = new SQLiteConnection(dbPath);

////            // Tạo tất cả bảng
////            _db.CreateTable<AppConfig>();
////            _db.CreateTable<Area>();
////            _db.CreateTable<LedBoard>();
////            _db.CreateTable<DisplayTemplate>();

////            // Khởi tạo dữ liệu mặc định nếu chưa có
////            InitializeDefaultData();
////        }

////        private void InitializeDefaultData()
////        {
////            // 1. AppConfig
////            if (_db.Table<AppConfig>().Count() == 0)
////            {
////                _db.Insert(new AppConfig());
////            }

////            // 2. Area mặc định
////            if (_db.Table<Area>().Count() == 0)
////            {
////                _db.Insert(new Area
////                {
////                    Name = "Khu vực chính",
////                    MaxCapacity = 100,
////                    RefreshIntervalSeconds = 5,
////                    IsActive = true
////                });
////            }

////            // 3. DisplayTemplate mặc định
////            if (_db.Table<DisplayTemplate>().Count() == 0)
////            {
////                var template = new DisplayTemplate
////                {
////                    Name = "Mẫu mặc định",
////                    Line1Format = "{ParkedCount} xe",
////                    Line2Format = "{AvailableCount} chỗ",
////                    DefaultColor = "Green",
////                    FontSize = 10,
////                    Alignment = "Center"
////                };

////                // Thiết lập ColorRules
////                template.ColorRules = new List<ColorRule>
////                {
////                    new ColorRule { MinPercent = 0, MaxPercent = 20, Color = "Red" },
////                    new ColorRule { MinPercent = 21, MaxPercent = 50, Color = "Yellow" },
////                    new ColorRule { MinPercent = 51, MaxPercent = 100, Color = "Green" }
////                };

////                _db.Insert(template);
////            }
////        }

////        // ============ AppConfig ============
////        public AppConfig GetConfig()
////        {
////            return _db.Table<AppConfig>().FirstOrDefault() ?? new AppConfig();
////        }

////        public void UpdateConfig(AppConfig config)
////        {
////            _db.Update(config);
////        }

////        // ============ Areas ============
////        public List<Area> GetAllAreas()
////        {
////            return _db.Table<Area>().ToList();
////        }

////        public List<Area> GetActiveAreas()
////        {
////            return _db.Table<Area>().Where(a => a.IsActive).ToList();
////        }

////        public Area? GetArea(int id)
////        {
////            return _db.Table<Area>().FirstOrDefault(a => a.Id == id);
////        }

////        public void InsertArea(Area area)
////        {
////            _db.Insert(area);
////        }

////        public void UpdateArea(Area area)
////        {
////            _db.Update(area);
////        }

////        public void DeleteArea(int id)
////        {
////            // Xóa tất cả LED boards liên quan
////            var boards = GetLedBoardsByArea(id);
////            foreach (var board in boards)
////            {
////                _db.Delete<LedBoard>(board.Id);
////            }

////            // Xóa area
////            _db.Delete<Area>(id);
////        }

////        // ============ LED Boards ============
////        public List<LedBoard> GetAllLedBoards()
////        {
////            return _db.Table<LedBoard>().ToList();
////        }

////        public List<LedBoard> GetLedBoardsByArea(int areaId)
////        {
////            return _db.Table<LedBoard>().Where(b => b.AreaId == areaId).ToList();
////        }

////        public List<LedBoard> GetActiveLedBoardsByArea(int areaId)
////        {
////            return _db.Table<LedBoard>()
////                .Where(b => b.AreaId == areaId && b.IsActive)
////                .ToList();
////        }

////        public LedBoard? GetLedBoard(int id)
////        {
////            return _db.Table<LedBoard>().FirstOrDefault(b => b.Id == id);
////        }

////        public void InsertLedBoard(LedBoard board)
////        {
////            _db.Insert(board);
////        }

////        public void UpdateLedBoard(LedBoard board)
////        {
////            _db.Update(board);
////        }

////        public void DeleteLedBoard(int id)
////        {
////            _db.Delete<LedBoard>(id);
////        }

////        // ============ Display Templates ============
////        public List<DisplayTemplate> GetAllDisplayTemplates()
////        {
////            return _db.Table<DisplayTemplate>().ToList();
////        }

////        public DisplayTemplate? GetDisplayTemplate(int id)
////        {
////            return _db.Table<DisplayTemplate>().FirstOrDefault(t => t.Id == id);
////        }

////        public void InsertDisplayTemplate(DisplayTemplate template)
////        {
////            _db.Insert(template);
////        }

////        public void UpdateDisplayTemplate(DisplayTemplate template)
////        {
////            _db.Update(template);
////        }

////        public void DeleteDisplayTemplate(int id)
////        {
////            _db.Delete<DisplayTemplate>(id);
////        }

////        // ============ Utility ============
////        public void CloseConnection()
////        {
////            _db?.Close();
////        }
////    }
////}




//using Avalonia.Controls;
//using Avalonia.Layout;
//using Avalonia.Media;
//using DnsClient;
//using System.Xml.Linq;
//using static System.Net.Mime.MediaTypeNames;




//using System.Collections.Generic;
//using System.Linq;
//using Avalonia.Controls;
//using Avalonia.Interactivity;
//using App01.Models;

//namespace App01.Views
//{
//    public partial class LedBoardEditDialog : Window
//    {
//        private readonly LedBoard? _editingBoard;
//        private readonly List<Area> _areas;
//        private readonly List<DisplayTemplate> _templates;

//        public LedBoardEditDialog(List<Area> areas, List<DisplayTemplate> templates, LedBoard? board = null)
//        {
//            InitializeComponent();

//            _editingBoard = board;
//            _areas = areas;
//            _templates = templates;

//            // Load ComboBox data
//            cboArea.ItemsSource = areas;
//            cboTemplate.ItemsSource = templates;

//            if (board != null)
//            {
//                // Edit mode
//                Title = $"Sửa Bảng LED: {board.Name}";
//                txtName.Text = board.Name;
//                txtIpAddress.Text = board.IpAddress;
//                numPort.Value = board.Port;
//                numTimeout.Value = board.TimeoutMs;
//                chkActive.IsChecked = board.IsActive;
//                txtNotes.Text = board.Notes;

//                // Select current area
//                var selectedArea = areas.FirstOrDefault(a => a.Id == board.AreaId);
//                if (selectedArea != null)
//                    cboArea.SelectedItem = selectedArea;

//                // Select current template
//                var selectedTemplate = templates.FirstOrDefault(t => t.Id == board.DisplayTemplateId);
//                if (selectedTemplate != null)
//                    cboTemplate.SelectedItem = selectedTemplate;
//            }
//            else
//            {
//                // Add mode
//                Title = "Thêm Bảng LED Mới";

//                // Select first items by default
//                if (areas.Count > 0)
//                    cboArea.SelectedIndex = 0;
//                if (templates.Count > 0)
//                    cboTemplate.SelectedIndex = 0;
//            }
//        }

//        private void BtnSave_Click(object? sender, RoutedEventArgs e)
//        {
//            // Validation
//            if (string.IsNullOrWhiteSpace(txtName.Text))
//            {
//                // TODO: Show validation error
//                return;
//            }

//            if (string.IsNullOrWhiteSpace(txtIpAddress.Text))
//            {
//                // TODO: Show validation error
//                return;
//            }

//            if (cboArea.SelectedItem is not Area selectedArea)
//            {
//                // TODO: Show validation error
//                return;
//            }

//            if (cboTemplate.SelectedItem is not DisplayTemplate selectedTemplate)
//            {
//                // TODO: Show validation error
//                return;
//            }

//            var board = _editingBoard ?? new LedBoard();

//            board.Name = txtName.Text;
//            board.IpAddress = txtIpAddress.Text;
//            board.Port = (int)numPort.Value;
//            board.AreaId = selectedArea.Id;
//            board.DisplayTemplateId = selectedTemplate.Id;
//            board.TimeoutMs = (int)numTimeout.Value;
//            board.IsActive = chkActive.IsChecked ?? true;
//            board.Notes = txtNotes.Text;

//            Close(board);
//        }

//        private void BtnCancel_Click(object? sender, RoutedEventArgs e)
//        {
//            Close(null);
//        }
//    }
//}


//using Avalonia.Controls;
//using Avalonia.Layout;
//using Avalonia.Media;
//using DnsClient;
//using System.Xml.Linq;
//using static System.Net.Mime.MediaTypeNames;



//using App01.Models;
//using Avalonia.Controls;
//using Avalonia.Interactivity;
//using System.Xml.Linq;

//namespace App01.Views
//{
//    public partial class AreaEditDialog : Window
//    {
//        private readonly Area? _editingArea;

//        public AreaEditDialog(Area? area = null)
//        {
//            InitializeComponent();
//            _editingArea = area;

//            if (area != null)
//            {
//                // Edit mode
//                Title = $"Sửa Khu Vực: {area.Name}";
//                txtName.Text = area.Name;
//                numMaxCapacity.Value = area.MaxCapacity;
//                numRefreshInterval.Value = area.RefreshIntervalSeconds;
//                chkActive.IsChecked = area.IsActive;
//                txtMongoConnection.Text = area.MongoConnectionString;
//                txtDatabaseName.Text = area.DatabaseName;
//                txtCollectionName.Text = area.CollectionName;
//            }
//            else
//            {
//                // Add mode
//                Title = "Thêm Khu Vực Mới";
//            }
//        }

//        private void BtnSave_Click(object? sender, RoutedEventArgs e)
//        {
//            if (string.IsNullOrWhiteSpace(txtName.Text))
//            {
//                // TODO: Show validation error
//                return;
//            }

//            var area = _editingArea ?? new Area();

//            area.Name = txtName.Text;
//            area.MaxCapacity = (int)numMaxCapacity.Value;
//            area.RefreshIntervalSeconds = (int)numRefreshInterval.Value;
//            area.IsActive = chkActive.IsChecked ?? true;
//            area.MongoConnectionString = string.IsNullOrWhiteSpace(txtMongoConnection.Text) ? null : txtMongoConnection.Text;
//            area.DatabaseName = string.IsNullOrWhiteSpace(txtDatabaseName.Text) ? null : txtDatabaseName.Text;
//            area.CollectionName = string.IsNullOrWhiteSpace(txtCollectionName.Text) ? null : txtCollectionName.Text;

//            Close(area);
//        }

//        private void BtnCancel_Click(object? sender, RoutedEventArgs e)
//        {
//            Close(null);
//        }
//    }
//}

//using Avalonia.Controls;
//using Avalonia.Layout;
//using Avalonia.Media;
//using Avalonia.Platform;
//using DnsClient;
//using System;
//using System.ComponentModel;
//using System.Xml;
//using System.Xml.Linq;
//using static System.Net.Mime.MediaTypeNames;



//using App01.Models;
//using Avalonia.Controls;
//using Avalonia.Interactivity;
//using Avalonia.Media.TextFormatting;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Xml.Linq;

//namespace App01.Views
//{
//    public partial class TemplateEditDialog : Window
//    {
//        private readonly DisplayTemplate? _editingTemplate;

//        public TemplateEditDialog(DisplayTemplate? template = null)
//        {
//            InitializeComponent();
//            _editingTemplate = template;

//            if (template != null)
//            {
//                // Edit mode
//                Title = $"Sửa Kịch Bản: {template.Name}";
//                txtName.Text = template.Name;
//                txtLine1.Text = template.Line1Format;
//                txtLine2.Text = template.Line2Format;
//                txtNotes.Text = template.Notes;

//                // Font Size
//                cboFontSize.SelectedIndex = template.FontSize switch
//                {
//                    7 => 0,
//                    10 => 1,
//                    12 => 2,
//                    13 => 3,
//                    14 => 4,
//                    16 => 5,
//                    _ => 1
//                };

//                // Alignment
//                cboAlignment.SelectedIndex = template.Alignment switch
//                {
//                    "Left" => 0,
//                    "Center" => 1,
//                    "Right" => 2,
//                    _ => 1
//                };

//                // Default Color
//                cboDefaultColor.SelectedIndex = GetColorIndex(template.DefaultColor);

//                // Color Rules
//                var rules = template.ColorRules;
//                if (rules.Count > 0)
//                {
//                    numMin1.Value = rules[0].MinPercent;
//                    numMax1.Value = rules[0].MaxPercent;
//                    cboColor1.SelectedIndex = GetColorIndex(rules[0].Color);
//                }
//                if (rules.Count > 1)
//                {
//                    numMin2.Value = rules[1].MinPercent;
//                    numMax2.Value = rules[1].MaxPercent;
//                    cboColor2.SelectedIndex = GetColorIndex(rules[1].Color);
//                }
//                if (rules.Count > 2)
//                {
//                    numMin3.Value = rules[2].MinPercent;
//                    numMax3.Value = rules[2].MaxPercent;
//                    cboColor3.SelectedIndex = GetColorIndex(rules[2].Color);
//                }
//            }
//            else
//            {
//                // Add mode
//                Title = "Thêm Kịch Bản Mới";
//            }
//        }

//        private void BtnSave_Click(object? sender, RoutedEventArgs e)
//        {
//            // Validation
//            if (string.IsNullOrWhiteSpace(txtName.Text))
//            {
//                // TODO: Show validation error
//                ShowError("Tên kịch bản không được để trống!");
//                return;
//            }

//            if (string.IsNullOrWhiteSpace(txtLine1.Text) || string.IsNullOrWhiteSpace(txtLine2.Text))
//            {
//                // TODO: Show validation error
//                ShowError("Dòng 1 và Dòng 2 không được để trống!");
//                return;
//            }

//            var template = _editingTemplate ?? new DisplayTemplate();

//            template.Name = txtName.Text;
//            template.Line1Format = txtLine1.Text;
//            template.Line2Format = txtLine2.Text;
//            template.Notes = txtNotes.Text;

//            // Font Size
//            template.FontSize = cboFontSize.SelectedIndex switch
//            {
//                0 => 7,
//                1 => 10,
//                2 => 12,
//                3 => 13,
//                4 => 14,
//                5 => 16,
//                _ => 10
//            };

//            // Alignment
//            template.Alignment = cboAlignment.SelectedIndex switch
//            {
//                0 => "Left",
//                1 => "Center",
//                2 => "Right",
//                _ => "Center"
//            };

//            // Default Color
//            template.DefaultColor = GetColorName(cboDefaultColor.SelectedIndex);

//            // Color Rules
//            var rules = new List<ColorRule>
//            {
//                new ColorRule
//                {
//                    MinPercent = (int)numMin1.Value,
//                    MaxPercent = (int)numMax1.Value,
//                    Color = GetColorName(cboColor1.SelectedIndex)
//                },
//                new ColorRule
//                {
//                    MinPercent = (int)numMin2.Value,
//                    MaxPercent = (int)numMax2.Value,
//                    Color = GetColorName(cboColor2.SelectedIndex)
//                },
//                new ColorRule
//                {
//                    MinPercent = (int)numMin3.Value,
//                    MaxPercent = (int)numMax3.Value,
//                    Color = GetColorName(cboColor3.SelectedIndex)
//                }
//            };

//            template.ColorRules = rules;
//            Debug.WriteLine($"[DEBUG] Saving template: Id={template.Id}, Name={template.Name}, FontSize={template.FontSize}");
//            Close(template);
//        }

//        private async void ShowError(string message)
//        {
//            var dialog = new Window
//            {
//                Title = "Lỗi",
//                Width = 300,
//                Height = 150,
//                Content = new StackPanel
//                {
//                    Margin = new Avalonia.Thickness(20),
//                    Children =
//            {
//                new TextBlock { Text = message },
//                new Button { Content = "OK", HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center }
//            }
//                }
//            };
//            var btn = (dialog.Content as StackPanel)?.Children[1] as Button;
//            btn.Click += (_, __) => dialog.Close();
//            await dialog.ShowDialog(this);
//        }


//        private void BtnCancel_Click(object? sender, RoutedEventArgs e)
//        {
//            Close(null);
//        }

//        private string GetColorName(int index)
//        {
//            return index switch
//            {
//                0 => "Red",
//                1 => "Green",
//                2 => "Yellow",
//                3 => "Blue",
//                4 => "Purple",
//                5 => "Cyan",
//                6 => "White",
//                _ => "Green"
//            };
//        }

//        private int GetColorIndex(string colorName)
//        {
//            return colorName.ToLower() switch
//            {
//                "red" => 0,
//                "green" => 1,
//                "yellow" => 2,
//                "blue" => 3,
//                "purple" => 4,
//                "cyan" => 5,
//                "white" => 6,
//                _ => 1
//            };
//        }
//    }
//}