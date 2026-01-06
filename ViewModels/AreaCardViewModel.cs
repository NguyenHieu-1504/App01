using ReactiveUI;
using Avalonia.Media;

namespace App01.ViewModels
{
    /// <summary>
    /// ViewModel cho mỗi Area Card trong Dashboard
    /// </summary>
    public class AreaCardViewModel : ViewModelBase
    {
        private string _name = "";
        private int _parkedCount;
        private int _availableCount;
        private int _maxCapacity;

        public string Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        public int ParkedCount
        {
            get => _parkedCount;
            set
            {
                this.RaiseAndSetIfChanged(ref _parkedCount, value);
                this.RaisePropertyChanged(nameof(PercentFilled));
                this.RaisePropertyChanged(nameof(PercentAvailable));
                this.RaisePropertyChanged(nameof(StatusColor));
                this.RaisePropertyChanged(nameof(StatusLightColor));
                this.RaisePropertyChanged(nameof(StatusText));
            }
        }

        public int AvailableCount
        {
            get => _availableCount;
            set => this.RaiseAndSetIfChanged(ref _availableCount, value);
        }

        public int MaxCapacity
        {
            get => _maxCapacity;
            set
            {
                this.RaiseAndSetIfChanged(ref _maxCapacity, value);
                this.RaisePropertyChanged(nameof(PercentFilled));
                this.RaisePropertyChanged(nameof(PercentAvailable));
            }
        }

        // Computed properties

        /// <summary>
        /// % đã lấp đầy (0-100)
        /// </summary>
        public int PercentFilled
        {
            get
            {
                if (MaxCapacity == 0) return 0;
                return (ParkedCount * 100) / MaxCapacity;
            }
        }

        /// <summary>
        /// % chỗ trống (0-100)
        /// </summary>
        public int PercentAvailable
        {
            get
            {
                if (MaxCapacity == 0) return 0;
                return (AvailableCount * 100) / MaxCapacity;
            }
        }

        /// <summary>
        /// Màu status theo % chỗ trống
        /// </summary>
        public IBrush StatusColor
        {
            get
            {
                int percent = PercentAvailable;

                if (percent <= 5)
                    return Brushes.Red;        // 🔴 Nguy hiểm
                else if (percent <= 15)
                    return Brushes.OrangeRed;  // 🟠 Cảnh báo cao
                else if (percent <= 30)
                    return Brushes.Orange;     // 🟡 Cảnh báo
                else if (percent <= 50)
                    return Brushes.Gold;       // 🟡 Bình thường
                else
                    return Brushes.MediumSeaGreen; // 🟢 Tốt
            }
        }

        /// <summary>
        /// Màu nền nhạt cho footer
        /// </summary>
        public IBrush StatusLightColor
        {
            get
            {
                int percent = PercentAvailable;

                if (percent <= 5)
                    return new SolidColorBrush(Color.FromRgb(255, 235, 238));  // Đỏ nhạt
                else if (percent <= 15)
                    return new SolidColorBrush(Color.FromRgb(255, 243, 224));  // Cam nhạt
                else if (percent <= 30)
                    return new SolidColorBrush(Color.FromRgb(255, 248, 225));  // Vàng nhạt
                else if (percent <= 50)
                    return new SolidColorBrush(Color.FromRgb(255, 253, 231));  // Vàng rất nhạt
                else
                    return new SolidColorBrush(Color.FromRgb(232, 245, 233));  // Xanh nhạt
            }
        }

        /// <summary>
        /// Text trạng thái
        /// </summary>
        public string StatusText
        {
            get
            {
                int percent = PercentAvailable;

                if (percent <= 5)
                    return "🔴  Gần đầy!";
                else if (percent <= 15)
                    return "⚠️  Sắp đầy";
                else if (percent <= 30)
                    return "🟡 Cảnh báo nhẹ";
                else if (percent <= 50)
                    return "🟢 Bình thường";
                else
                    return "🟢 Rất tốt";
            }
        }
    }
}