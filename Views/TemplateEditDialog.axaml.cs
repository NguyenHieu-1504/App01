using App01.Models;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.TextFormatting;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace App01.Views
{
    public partial class TemplateEditDialog : Window
    {
        private readonly DisplayTemplate? _editingTemplate;

        public TemplateEditDialog()
        {
            InitializeComponent();
        }

        public TemplateEditDialog(DisplayTemplate? template = null)
        {
            InitializeComponent();

            _editingTemplate = template;

            if (template != null)
            {
                // Edit mode
                Title = $"Sửa Kịch Bản: {template.Name}";
                txtName.Text = template.Name;
                txtLine1.Text = template.Line1Format;
                txtLine2.Text = template.Line2Format;
                txtNotes.Text = template.Notes;
                chkActive.IsChecked = template.IsActive;

                // Font Size
                cboFontSize.SelectedIndex = template.FontSize switch
                {
                    7 => 0,
                    10 => 1,
                    12 => 2,
                    13 => 3,
                    14 => 4,
                    16 => 5,
                    _ => 1
                };

                // Alignment
                cboAlignment.SelectedIndex = template.Alignment switch
                {
                    "Left" => 0,
                    "Center" => 1,
                    "Right" => 2,
                    _ => 1
                };

                // Default Color
                cboDefaultColor.SelectedIndex = GetColorIndex(template.DefaultColor);

                // Color Rules
                var rules = template.ColorRules;
                if (rules.Count > 0)
                {
                    numMin1.Value = rules[0].MinPercent;
                    numMax1.Value = rules[0].MaxPercent;
                    cboColor1.SelectedIndex = GetColorIndex(rules[0].Color);
                }
                if (rules.Count > 1)
                {
                    numMin2.Value = rules[1].MinPercent;
                    numMax2.Value = rules[1].MaxPercent;
                    cboColor2.SelectedIndex = GetColorIndex(rules[1].Color);
                }
                if (rules.Count > 2)
                {
                    numMin3.Value = rules[2].MinPercent;
                    numMax3.Value = rules[2].MaxPercent;
                    cboColor3.SelectedIndex = GetColorIndex(rules[2].Color);
                }
            }
            else
            {
                // Add mode
                Title = "Thêm Kịch Bản Mới";
            }
        }

        private async void BtnSave_Click(object? sender, RoutedEventArgs e)
        {
            // Validation cơ bản
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                ShowError("Tên kịch bản không được để trống!");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtLine1.Text) || string.IsNullOrWhiteSpace(txtLine2.Text))
            {
                ShowError("Dòng 1 và Dòng 2 không được để trống!");
                return;
            }

            // ===== VALIDATION COLOR RULES =====
            int min1 = (int)numMin1.Value;
            int max1 = (int)numMax1.Value;
            int min2 = (int)numMin2.Value;
            int max2 = (int)numMax2.Value;
            int min3 = (int)numMin3.Value;
            int max3 = (int)numMax3.Value;

            // Kiểm tra min <= max
            if (min1 > max1)
            {
                ShowError("Quy tắc 1: Giá trị Min phải <= Max!");
                return;
            }
            if (min2 > max2)
            {
                ShowError("Quy tắc 2: Giá trị Min phải <= Max!");
                return;
            }
            if (min3 > max3)
            {
                ShowError("Quy tắc 3: Giá trị Min phải <= Max!");
                return;
            }

            // Lọc ra các rules có giá trị
            var rules = new List<(int min, int max, int index)>();

            if (!(min1 == 0 && max1 == 0))
                rules.Add((min1, max1, 1));

            if (!(min2 == 0 && max2 == 0))
                rules.Add((min2, max2, 2));

            if (!(min3 == 0 && max3 == 0))
                rules.Add((min3, max3, 3));

            // Kiểm tra overlap
            for (int i = 0; i < rules.Count; i++)
            {
                for (int j = i + 1; j < rules.Count; j++)
                {
                    var r1 = rules[i];
                    var r2 = rules[j];

                    if (!(r1.max < r2.min || r2.max < r1.min))
                    {
                        ShowError($"Quy tắc {r1.index} và {r2.index} bị chồng lấn!\n" +
                                 $"Rule {r1.index}: {r1.min}-{r1.max}%\n" +
                                 $"Rule {r2.index}: {r2.min}-{r2.max}%\n\n" +
                                 $"Ví dụ đúng: Rule1(0-30), Rule2(31-100)");
                        return;
                    }
                }
            }

            //  KIỂM TRA GAP (KHOẢNG RỖNG)
            string gapWarning = CheckForGaps(rules);
            if (!string.IsNullOrEmpty(gapWarning))
            {
                Debug.WriteLine($"[VALIDATION] Phát hiện gap: {gapWarning}");

                // ⬇ AWAIT và kiểm tra kết quả
                bool shouldContinue = await ShowWarningAsync(
                    $"⚠️ Cảnh báo:\n{gapWarning}\n\n" +
                    $"Các khoảng rỗng này sẽ dùng màu mặc định: {GetColorName(cboDefaultColor.SelectedIndex)}\n\n" +
                    $"Bạn có muốn tiếp tục lưu?"
                );

                if (!shouldContinue)
                {
                    Debug.WriteLine("[VALIDATION] User chọn sửa lại");
                    return; // ⬅️ DỪNG LẠI, KHÔNG LƯU
                }

                Debug.WriteLine("[VALIDATION] User chọn tiếp tục");
            }

            // ===== LƯU DỮ LIỆU =====
            var template = _editingTemplate ?? new DisplayTemplate();

            template.Name = txtName.Text;
            template.Line1Format = txtLine1.Text;
            template.Line2Format = txtLine2.Text;
            template.Notes = txtNotes.Text;
            template.IsActive = chkActive.IsChecked ?? true;

            template.FontSize = cboFontSize.SelectedIndex switch
            {
                0 => 7,
                1 => 10,
                2 => 12,
                3 => 13,
                4 => 14,
                5 => 16,
                _ => 10
            };

            template.Alignment = cboAlignment.SelectedIndex switch
            {
                0 => "Left",
                1 => "Center",
                2 => "Right",
                _ => "Center"
            };

            template.DefaultColor = GetColorName(cboDefaultColor.SelectedIndex);

            // Color Rules - CHỈ LƯU RULES CÓ GIÁ TRỊ
            var colorRules = new List<ColorRule>();

            if (!(min1 == 0 && max1 == 0))
            {
                colorRules.Add(new ColorRule
                {
                    MinPercent = min1,
                    MaxPercent = max1,
                    Color = GetColorName(cboColor1.SelectedIndex)
                });
            }

            if (!(min2 == 0 && max2 == 0))
            {
                colorRules.Add(new ColorRule
                {
                    MinPercent = min2,
                    MaxPercent = max2,
                    Color = GetColorName(cboColor2.SelectedIndex)
                });
            }

            if (!(min3 == 0 && max3 == 0))
            {
                colorRules.Add(new ColorRule
                {
                    MinPercent = min3,
                    MaxPercent = max3,
                    Color = GetColorName(cboColor3.SelectedIndex)
                });
            }

            template.ColorRules = colorRules.OrderBy(r => r.MinPercent).ToList();

            Debug.WriteLine($"[DEBUG] Saving template: Id={template.Id}, Name={template.Name}, Rules={template.ColorRules.Count}");
            Close(template);
        }

        //  HÀM KIỂM TRA GAP
        private string CheckForGaps(List<(int min, int max, int index)> rules)
        {
            if (rules.Count == 0) return string.Empty;

            // Sắp xếp theo min
            var sorted = rules.OrderBy(r => r.min).ToList();

            var gaps = new List<string>();

            // Kiểm tra từ 0 đến rule đầu tiên
            if (sorted[0].min > 0)
            {
                gaps.Add($"0-{sorted[0].min - 1}%");
            }

            // Kiểm tra gap giữa các rules
            for (int i = 0; i < sorted.Count - 1; i++)
            {
                int currentMax = sorted[i].max;
                int nextMin = sorted[i + 1].min;

                if (nextMin > currentMax + 1)
                {
                    gaps.Add($"{currentMax + 1}-{nextMin - 1}%");
                }
            }

            // Kiểm tra từ rule cuối đến 100
            if (sorted[sorted.Count - 1].max < 100)
            {
                gaps.Add($"{sorted[sorted.Count - 1].max + 1}-100%");
            }

            if (gaps.Count > 0)
            {
                return "Có khoảng rỗng: " + string.Join(", ", gaps);
            }

            return string.Empty;
        }

        //  SHOW WARNING (CONFIRMATION DIALOG)
        private async Task<bool> ShowWarningAsync(string message)
        {
            var btnYes = new Button
            {
                Content = "Tiếp tục",
                Width = 100,
                Background = Avalonia.Media.Brushes.Orange,
                Foreground = Avalonia.Media.Brushes.White
            };

            var btnNo = new Button
            {
                Content = "Sửa lại",
                Width = 100
            };

            var dialog = new Window
            {
                Title = "Cảnh báo",
                Width = 450,
                Height = 220,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false
            };

            btnYes.Click += (_, _) => dialog.Close(true);
            btnNo.Click += (_, _) => dialog.Close(false);

            dialog.Content = new StackPanel
            {
                Margin = new Avalonia.Thickness(20),
                Spacing = 15,
                Children =
        {
            new TextBlock
            {
                Text = message,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                FontSize = 14
            },
            new StackPanel
            {
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Spacing = 10,
                Children = { btnYes, btnNo }
            }
        }
            };

            // QUAN TRỌNG
            var result = await dialog.ShowDialog<bool>(this);

            return result;
        }


        private async void ShowError(string message)
        {
            var dialog = new Window
            {
                Title = "Lỗi",
                Width = 300,
                Height = 150,
                Content = new StackPanel
                {
                    Margin = new Avalonia.Thickness(20),
                    Children =
            {
                new TextBlock { Text = message },
                new Button { Content = "OK", HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center }
            }
                }
            };
            var btn = (dialog.Content as StackPanel)?.Children[1] as Button;
            btn.Click += (_, __) => dialog.Close();
            await dialog.ShowDialog(this);
        }


        private void BtnCancel_Click(object? sender, RoutedEventArgs e)
        {
            Close(null);
        }

        private string GetColorName(int index)
        {
            return index switch
            {
                0 => "Red",
                1 => "Green",
                2 => "Yellow",
                3 => "Blue",
                4 => "Purple",
                5 => "Cyan",
                6 => "White",
                _ => "Green"
            };
        }

        private int GetColorIndex(string colorName)
        {
            return colorName.ToLower() switch
            {
                "red" => 0,
                "green" => 1,
                "yellow" => 2,
                "blue" => 3,
                "purple" => 4,
                "cyan" => 5,
                "white" => 6,
                _ => 1
            };
        }
    }
}