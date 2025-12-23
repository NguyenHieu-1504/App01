//using System;
//using ReactiveUI;
//using System.Reactive;
//using App01.Models;

//namespace App01.ViewModels
//{
//    public class AreaEditDialogViewModel : ViewModelBase
//    {
//        private string _name = "";
//        private int _maxCapacity = 100;
//        private int _refreshIntervalSeconds = 5;
//        private bool _isActive = true;
//        private string? _mongoConnectionString;
//        private string? _databaseName;
//        private string? _collectionName;
//        private string? _notes;

//        public string Name
//        {
//            get => _name;
//            set => this.RaiseAndSetIfChanged(ref _name, value);
//        }

//        public int MaxCapacity
//        {
//            get => _maxCapacity;
//            set => this.RaiseAndSetIfChanged(ref _maxCapacity, value);
//        }

//        public int RefreshIntervalSeconds
//        {
//            get => _refreshIntervalSeconds;
//            set => this.RaiseAndSetIfChanged(ref _refreshIntervalSeconds, value);
//        }

//        public bool IsActive
//        {
//            get => _isActive;
//            set => this.RaiseAndSetIfChanged(ref _isActive, value);
//        }

//        public string? MongoConnectionString
//        {
//            get => _mongoConnectionString;
//            set => this.RaiseAndSetIfChanged(ref _mongoConnectionString, value);
//        }

//        public string? DatabaseName
//        {
//            get => _databaseName;
//            set => this.RaiseAndSetIfChanged(ref _databaseName, value);
//        }

//        public string? CollectionName
//        {
//            get => _collectionName;
//            set => this.RaiseAndSetIfChanged(ref _collectionName, value);
//        }

//        public string? Notes
//        {
//            get => _notes;
//            set => this.RaiseAndSetIfChanged(ref _notes, value);
//        }

//        public event Action<Area?>? RequestClose;

//        public ReactiveCommand<Unit, Unit> SaveCommand { get; }
//        public ReactiveCommand<Unit, Unit> CancelCommand { get; }

//        private readonly Area? _editing;

//        public AreaEditDialogViewModel(Area? editing = null)
//        {
//            _editing = editing;

//            if (editing != null)
//            {
//                Name = editing.Name;
//                MaxCapacity = editing.MaxCapacity;
//                RefreshIntervalSeconds = editing.RefreshIntervalSeconds;
//                IsActive = editing.IsActive;
//                MongoConnectionString = editing.MongoConnectionString;
//                DatabaseName = editing.DatabaseName;
//                CollectionName = editing.CollectionName;
//                Notes = editing.Notes;
//            }

//            SaveCommand = ReactiveCommand.Create(OnSave);
//            CancelCommand = ReactiveCommand.Create(OnCancel);
//        }

//        private void OnSave()
//        {
//            if (string.IsNullOrWhiteSpace(Name)) { RequestClose?.Invoke(null); return; }

//            var area = _editing ?? new Area();
//            area.Name = Name;
//            area.MaxCapacity = MaxCapacity;
//            area.RefreshIntervalSeconds = RefreshIntervalSeconds;
//            area.IsActive = IsActive;
//            area.MongoConnectionString = string.IsNullOrWhiteSpace(MongoConnectionString) ? null : MongoConnectionString;
//            area.DatabaseName = string.IsNullOrWhiteSpace(DatabaseName) ? null : DatabaseName;
//            area.CollectionName = string.IsNullOrWhiteSpace(CollectionName) ? null : CollectionName;
//            area.Notes = Notes;

//            RequestClose?.Invoke(area);
//        }

//        private void OnCancel()
//        {
//            RequestClose?.Invoke(null);
//        }
//    }
//}