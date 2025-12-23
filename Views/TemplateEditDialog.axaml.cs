using App01.Models;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.TextFormatting;
using System.Collections.Generic;
using System.Diagnostics;
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
            
            _editingTemplate = template;

            if (template != null)
            {
                // Edit mode
                Title = $"Sửa Kịch Bản: {template.Name}";
                txtName.Text = template.Name;
                txtLine1.Text = template.Line1Format;
                txtLine2.Text = template.Line2Format;
                txtNotes.Text = template.Notes;

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

        private void BtnSave_Click(object? sender, RoutedEventArgs e)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                // TODO: Show validation error
                ShowError("Tên kịch bản không được để trống!");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtLine1.Text) || string.IsNullOrWhiteSpace(txtLine2.Text))
            {
                // TODO: Show validation error
                ShowError("Dòng 1 và Dòng 2 không được để trống!");
                return;
            }

            var template = _editingTemplate ?? new DisplayTemplate();

            template.Name = txtName.Text;
            template.Line1Format = txtLine1.Text;
            template.Line2Format = txtLine2.Text;
            template.Notes = txtNotes.Text;

            // Font Size
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

            // Alignment
            template.Alignment = cboAlignment.SelectedIndex switch
            {
                0 => "Left",
                1 => "Center",
                2 => "Right",
                _ => "Center"
            };

            // Default Color
            template.DefaultColor = GetColorName(cboDefaultColor.SelectedIndex);

            // Color Rules
            var rules = new List<ColorRule>
            {
                new ColorRule
                {
                    MinPercent = (int)numMin1.Value,
                    MaxPercent = (int)numMax1.Value,
                    Color = GetColorName(cboColor1.SelectedIndex)
                },
                new ColorRule
                {
                    MinPercent = (int)numMin2.Value,
                    MaxPercent = (int)numMax2.Value,
                    Color = GetColorName(cboColor2.SelectedIndex)
                },
                new ColorRule
                {
                    MinPercent = (int)numMin3.Value,
                    MaxPercent = (int)numMax3.Value,
                    Color = GetColorName(cboColor3.SelectedIndex)
                }
            };

            template.ColorRules = rules;
            Debug.WriteLine($"[DEBUG] Saving template: Id={template.Id}, Name={template.Name}, FontSize={template.FontSize}");
            Close(template);
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