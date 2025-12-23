//using App01.Models;
//using ReactiveUI;
//using System;
//using System.Collections.ObjectModel;
//using System.Linq;
//using System.Reactive;

//namespace App01.ViewModels
//{
//    public class LedBoardEditDialogViewModel : ViewModelBase
//    {
//        public ObservableCollection<Area> Areas { get; }
//        public ObservableCollection<DisplayTemplate> Templates { get; }

//        private string _name = "";
//        private string _ipAddress = "";
//        private int _port = 100;
//        private int _timeout = 3000;
//        private bool _isActive = true;
//        private string _notes = "";
//        private Area? _selectedArea;
//        private DisplayTemplate? _selectedTemplate;

//        public string Name
//        {
//            get => _name;
//            set => this.RaiseAndSetIfChanged(ref _name, value);
//        }

//        public string IpAddress
//        {
//            get => _ipAddress;
//            set => this.RaiseAndSetIfChanged(ref _ipAddress, value);
//        }

//        public int Port
//        {
//            get => _port;
//            set => this.RaiseAndSetIfChanged(ref _port, value);
//        }

//        public int Timeout
//        {
//            get => _timeout;
//            set => this.RaiseAndSetIfChanged(ref _timeout, value);
//        }

//        public bool IsActive
//        {
//            get => _isActive;
//            set => this.RaiseAndSetIfChanged(ref _isActive, value);
//        }

//        public string Notes
//        {
//            get => _notes;
//            set => this.RaiseAndSetIfChanged(ref _notes, value);
//        }

//        public Area? SelectedArea
//        {
//            get => _selectedArea;
//            set => this.RaiseAndSetIfChanged(ref _selectedArea, value);
//        }

//        public DisplayTemplate? SelectedTemplate
//        {
//            get => _selectedTemplate;
//            set => this.RaiseAndSetIfChanged(ref _selectedTemplate, value);
//        }

//        // Sự kiện để view đóng dialog và nhận kết quả
//        public event Action<LedBoard?>? RequestClose;

//        public ReactiveCommand<Unit, Unit> SaveCommand { get; }
//        public ReactiveCommand<Unit, Unit> CancelCommand { get; }

//        public LedBoardEditDialogViewModel(
//            System.Collections.Generic.IEnumerable<Area>? areas = null,
//            System.Collections.Generic.IEnumerable<DisplayTemplate>? templates = null,
//            LedBoard? editing = null)
//        {
//            Areas = new ObservableCollection<Area>(areas ?? Array.Empty<Area>());
//            Templates = new ObservableCollection<DisplayTemplate>(templates ?? Array.Empty<DisplayTemplate>());

//            if (editing != null)
//            {
//                Name = editing.Name;
//                IpAddress = editing.IpAddress;
//                Port = editing.Port;
//                Timeout = editing.TimeoutMs;
//                IsActive = editing.IsActive;
//                Notes = editing.Notes;
//                SelectedArea = Areas.FirstOrDefault(a => a.Id == editing.AreaId);
//                SelectedTemplate = Templates.FirstOrDefault(t => t.Id == editing.DisplayTemplateId);
//            }
//            else
//            {
//                // chọn mặc định nếu có
//                if (Areas.Count > 0) SelectedArea = Areas[0];
//                if (Templates.Count > 0) SelectedTemplate = Templates[0];
//            }

//            SaveCommand = ReactiveCommand.Create(OnSave);
//            CancelCommand = ReactiveCommand.Create(OnCancel);
//        }

//        private void OnSave()
//        {
//            // Validate tối thiểu
//            if (string.IsNullOrWhiteSpace(Name) ||
//                string.IsNullOrWhiteSpace(IpAddress) ||
//                SelectedArea == null ||
//                SelectedTemplate == null)
//            {
//                // Không đóng, bạn có thể mở dialog lỗi bằng event khác hoặc dùng message bus
//                RequestClose?.Invoke(null);
//                return;
//            }

//            var board = new LedBoard
//            {
//                Name = Name,
//                IpAddress = IpAddress,
//                Port = Port,
//                TimeoutMs = Timeout,
//                IsActive = IsActive,
//                Notes = Notes,
//                AreaId = SelectedArea.Id,
//                DisplayTemplateId = SelectedTemplate.Id
//            };

//            RequestClose?.Invoke(board);
//        }

//        private void OnCancel()
//        {
//            RequestClose?.Invoke(null);
//        }
//    }
//}