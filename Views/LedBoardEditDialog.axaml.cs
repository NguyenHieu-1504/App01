using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using App01.Models;

namespace App01.Views
{
    public partial class LedBoardEditDialog : Window
    {
        private readonly LedBoard? _editingBoard;
        private readonly List<Area> _areas;
        private readonly List<DisplayTemplate> _templates;

        
        public LedBoardEditDialog()
        {
            InitializeComponent();

            // Khởi tạo rỗng để tránh null reference
            _areas = new List<Area>();
            _templates = new List<DisplayTemplate>();
        }

        
        public LedBoardEditDialog(
            List<Area> areas,
            List<DisplayTemplate> templates,
            LedBoard? board = null) : this()
        {
            _editingBoard = board;
            _areas = areas;
            _templates = templates;

            // Bind dữ liệu cho ComboBox
            cboArea.ItemsSource = areas;
            cboTemplate.ItemsSource = templates;

            if (board != null)
            {
                // ===== EDIT MODE =====
                Title = $"Sửa Bảng LED: {board.Name}";
                txtName.Text = board.Name;
                txtIpAddress.Text = board.IpAddress;
                numPort.Value = board.Port;
                numTimeout.Value = board.TimeoutMs;
                chkActive.IsChecked = board.IsActive;
                txtNotes.Text = board.Notes;

                // Select Area
                var selectedArea = areas.FirstOrDefault(a => a.Id == board.AreaId);
                if (selectedArea != null)
                    cboArea.SelectedItem = selectedArea;

                // Select Template
                var selectedTemplate = templates.FirstOrDefault(t => t.Id == board.DisplayTemplateId);
                if (selectedTemplate != null)
                    cboTemplate.SelectedItem = selectedTemplate;
            }
            else
            {
                // ===== ADD MODE =====
                Title = "Thêm Bảng LED Mới";

                if (areas.Count > 0)
                    cboArea.SelectedIndex = 0;

                if (templates.Count > 0)
                    cboTemplate.SelectedIndex = 0;
            }
        }

        
        private void BtnSave_Click(object? sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
                return;

            if (string.IsNullOrWhiteSpace(txtIpAddress.Text))
                return;

            if (cboArea.SelectedItem is not Area selectedArea)
                return;

            if (cboTemplate.SelectedItem is not DisplayTemplate selectedTemplate)
                return;

            var board = _editingBoard ?? new LedBoard();

            board.Name = txtName.Text;
            board.IpAddress = txtIpAddress.Text;
            board.Port = (int)numPort.Value;
            board.TimeoutMs = (int)numTimeout.Value;
            board.AreaId = selectedArea.Id;
            board.DisplayTemplateId = selectedTemplate.Id;
            board.IsActive = chkActive.IsChecked ?? true;
            board.Notes = txtNotes.Text;

            Close(board);
        }
        // cancel
        
        private void BtnCancel_Click(object? sender, RoutedEventArgs e)
        {
            Close(null);
        }
    }
}
