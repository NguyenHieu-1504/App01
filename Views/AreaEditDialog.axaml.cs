using App01.Models;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Xml.Linq;

namespace App01.Views
{
    public partial class AreaEditDialog : Window
    {
        private readonly Area? _editingArea;

        public AreaEditDialog()
        {
            InitializeComponent();
        }
        public AreaEditDialog(Area? area = null)
        {
            InitializeComponent();

            _editingArea = area;

            if (area != null)
            {
                // Edit mode
                Title = $"Sửa Khu Vực: {area.Name}";
                txtName.Text = area.Name;
                numMaxCapacity.Value = area.MaxCapacity;
                numRefreshInterval.Value = area.RefreshIntervalSeconds;
                chkActive.IsChecked = area.IsActive;
                txtMongoConnection.Text = area.MongoConnectionString;
                txtDatabaseName.Text = area.DatabaseName;
                txtCollectionName.Text = area.CollectionName;
            }
            else
            {
                // Add mode
                Title = "Thêm Khu Vực Mới";
            }
        }

        private void BtnSave_Click(object? sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                // TODO: Show validation error
                return;
            }

            var area = _editingArea ?? new Area();

            area.Name = txtName.Text;
            area.MaxCapacity = (int)numMaxCapacity.Value;
            area.RefreshIntervalSeconds = (int)numRefreshInterval.Value;
            area.IsActive = chkActive.IsChecked ?? true;
            area.MongoConnectionString = string.IsNullOrWhiteSpace(txtMongoConnection.Text) ? null : txtMongoConnection.Text;
            area.DatabaseName = string.IsNullOrWhiteSpace(txtDatabaseName.Text) ? null : txtDatabaseName.Text;
            area.CollectionName = string.IsNullOrWhiteSpace(txtCollectionName.Text) ? null : txtCollectionName.Text;

            Close(area);
        }

        private void BtnCancel_Click(object? sender, RoutedEventArgs e)
        {
            Close(null);
        }
    }
}