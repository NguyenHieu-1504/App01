using App01.Models;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace App01.Views
{
    public partial class GateImportDialog : Window
    {
        private ObservableCollection<GateSelectionItem> _gateItems;

        public List<Gate> SelectedGates { get; private set; } = new();
        public Area? SelectedArea { get; private set; }

        public GateImportDialog()
        {
            InitializeComponent();
        }
        public GateImportDialog(List<GateMongo> gatesFromMongo, List<Area> areas)
        {
            InitializeComponent();

            // Bind Areas vào ComboBox
            CbAreaSelect.ItemsSource = areas;
            CbAreaSelect.DisplayMemberBinding = new Avalonia.Data.Binding("Name");
            if (areas.Count > 0)
                CbAreaSelect.SelectedIndex = 0;

            // Chuyển đổi GateMongo → GateSelectionItem
            _gateItems = new ObservableCollection<GateSelectionItem>(
                gatesFromMongo.Select(g => new GateSelectionItem
                {
                    Gate = g,
                    IsSelected = !g.Inactive // Tự động chọn nếu Active
                })
            );

            GatesList.ItemsSource = _gateItems;

            // Theo dõi thay đổi selection
            foreach (var item in _gateItems)
            {
                item.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(GateSelectionItem.IsSelected))
                        UpdateSelectedCount();
                };
            }

            UpdateSelectedCount();
        }

        private void UpdateSelectedCount()
        {
            int count = _gateItems.Count(g => g.IsSelected);
            TxtSelectedCount.Text = $"Đã chọn: {count} Gates";
            BtnImport.IsEnabled = count > 0 && CbAreaSelect.SelectedItem != null;
        }

        private void BtnCancel_Click(object? sender, RoutedEventArgs e)
        {
            Close(null);
        }

        private void BtnImport_Click(object? sender, RoutedEventArgs e)
        {
            SelectedArea = CbAreaSelect.SelectedItem as Area;

            if (SelectedArea == null)
            {
                // Hiển thị thông báo
                return;
            }

            // Chuyển đổi GateMongo → Gate
            SelectedGates = _gateItems
                .Where(item => item.IsSelected)
                .Select(item => new Gate
                {
                    GateIdMongo = item.Gate.GateID,
                    GateCode = item.Gate.GateCode,
                    GateName = item.Gate.GateName,
                    AreaId = SelectedArea.Id,
                    IsActive = true
                })
                .ToList();

            Close(this); // Trả về dialog result
        }
    }

    // Helper class để binding
    public class GateSelectionItem : ReactiveUI.ReactiveObject
    {
        private bool _isSelected;

        public GateMongo Gate { get; set; } = null!;

        public bool IsSelected
        {
            get => _isSelected;
            set => this.RaiseAndSetIfChanged(ref _isSelected, value);
        }
    }
}