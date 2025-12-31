using App01.Models;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace App01.Views
{
    public partial class LaneImportDialog : Window
    {
        private ObservableCollection<LaneSelectionItem> _laneItems;

        public List<Lane> SelectedLanes { get; private set; } = new();
        public Gate? SelectedGate { get; private set; }

        public LaneImportDialog()
        {
            InitializeComponent();
        }

        public LaneImportDialog(List<LaneMongo> lanesFromMongo, List<Gate> gates)
        {
            InitializeComponent();

            // Bind Gates vào ComboBox
            CbGateSelect.ItemsSource = gates;
            CbGateSelect.DisplayMemberBinding = new Avalonia.Data.Binding("GateName");
            if (gates.Count > 0)
                CbGateSelect.SelectedIndex = 0;

            // Chuyển đổi LaneMongo → LaneSelectionItem
            _laneItems = new ObservableCollection<LaneSelectionItem>(
                lanesFromMongo.Select(l => new LaneSelectionItem
                {
                    Lane = l,
                    IsSelected = !l.Inactive // Tự động chọn nếu Active
                })
            );

            LanesList.ItemsSource = _laneItems;

            // Theo dõi thay đổi selection
            foreach (var item in _laneItems)
            {
                item.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(LaneSelectionItem.IsSelected))
                        UpdateSelectedCount();
                };
            }

            UpdateSelectedCount();
        }

        private void UpdateSelectedCount()
        {
            int count = _laneItems.Count(l => l.IsSelected);
            TxtSelectedCount.Text = $"Đã chọn: {count} Lanes";
            BtnImport.IsEnabled = count > 0 && CbGateSelect.SelectedItem != null;
        }

        private void BtnCancel_Click(object? sender, RoutedEventArgs e)
        {
            Close(null);
        }

        private void BtnImport_Click(object? sender, RoutedEventArgs e)
        {
            SelectedGate = CbGateSelect.SelectedItem as Gate;

            if (SelectedGate == null)
            {
                return;
            }

            // Chuyển đổi LaneMongo → Lane
            SelectedLanes = _laneItems
                .Where(item => item.IsSelected)
                .Select(item => new Lane
                {
                    LaneIdMongo = item.Lane.LaneID,
                    LaneCode = item.Lane.LaneCode,
                    LaneName = item.Lane.LaneName,
                    GateId = SelectedGate.Id,
                    IsActive = true
                })
                .ToList();

            Close(this);
        }
    }

    // Helper class để binding
    public class LaneSelectionItem : ReactiveUI.ReactiveObject
    {
        private bool _isSelected;

        public LaneMongo Lane { get; set; } = null!;

        public bool IsSelected
        {
            get => _isSelected;
            set => this.RaiseAndSetIfChanged(ref _isSelected, value);
        }

        // Hiển thị LaneType dạng text
        public string LaneTypeText
        {
            get
            {
                return Lane.LaneType switch
                {
                    0 => "RA",
                    1 => "VÀO",
                    6 => "KIOSK",
                    _ => $"Type {Lane.LaneType}"
                };
            }
        }

        // Màu badge theo LaneType
        public IBrush LaneTypeBadgeColor
        {
            get
            {
                return Lane.LaneType switch
                {
                    0 => Brushes.Tomato,      // RA = Đỏ
                    1 => Brushes.MediumSeaGreen, // VÀO = Xanh
                    6 => Brushes.DodgerBlue,  // KIOSK = Xanh dương
                    _ => Brushes.Gray
                };
            }
        }
    }
}