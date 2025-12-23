//using System;
//using System.Collections.ObjectModel;
//using System.Linq;
//using ReactiveUI;
//using System.Reactive;
//using App01.Models;

//namespace App01.ViewModels
//{
//    public class TemplateEditDialogViewModel : ViewModelBase
//    {
//        private string _name = "";
//        private string _line1 = "";
//        private string _line2 = "";
//        private int _fontSizeIndex = 1; // default index matching XAML
//        private int _alignmentIndex = 1; // Center
//        private int _defaultColorIndex = 1; // Green
//        private string? _notes;

//        // Color rule 1..3
//        private int _min1 = 0;
//        private int _max1 = 10;
//        private int _colorIndex1 = 0;

//        private int _min2 = 11;
//        private int _max2 = 30;
//        private int _colorIndex2 = 2;

//        private int _min3 = 31;
//        private int _max3 = 100;
//        private int _colorIndex3 = 1;

//        public string Name { get => _name; set => this.RaiseAndSetIfChanged(ref _name, value); }
//        public string Line1 { get => _line1; set => this.RaiseAndSetIfChanged(ref _line1, value); }
//        public string Line2 { get => _line2; set => this.RaiseAndSetIfChanged(ref _line2, value); }
//        public int FontSizeIndex { get => _fontSizeIndex; set => this.RaiseAndSetIfChanged(ref _fontSizeIndex, value); }
//        public int AlignmentIndex { get => _alignmentIndex; set => this.RaiseAndSetIfChanged(ref _alignmentIndex, value); }
//        public int DefaultColorIndex { get => _defaultColorIndex; set => this.RaiseAndSetIfChanged(ref _defaultColorIndex, value); }
//        public string? Notes { get => _notes; set => this.RaiseAndSetIfChanged(ref _notes, value); }

//        public int Min1 { get => _min1; set => this.RaiseAndSetIfChanged(ref _min1, value); }
//        public int Max1 { get => _max1; set => this.RaiseAndSetIfChanged(ref _max1, value); }
//        public int ColorIndex1 { get => _colorIndex1; set => this.RaiseAndSetIfChanged(ref _colorIndex1, value); }

//        public int Min2 { get => _min2; set => this.RaiseAndSetIfChanged(ref _min2, value); }
//        public int Max2 { get => _max2; set => this.RaiseAndSetIfChanged(ref _max2, value); }
//        public int ColorIndex2 { get => _colorIndex2; set => this.RaiseAndSetIfChanged(ref _colorIndex2, value); }

//        public int Min3 { get => _min3; set => this.RaiseAndSetIfChanged(ref _min3, value); }
//        public int Max3 { get => _max3; set => this.RaiseAndSetIfChanged(ref _max3, value); }
//        public int ColorIndex3 { get => _colorIndex3; set => this.RaiseAndSetIfChanged(ref _colorIndex3, value); }

//        public event Action<DisplayTemplate?>? RequestClose;

//        public ReactiveCommand<Unit, Unit> SaveCommand { get; }
//        public ReactiveCommand<Unit, Unit> CancelCommand { get; }

//        private readonly DisplayTemplate? _editing;

//        public TemplateEditDialogViewModel(DisplayTemplate? editing = null)
//        {
//            _editing = editing;

//            if (editing != null)
//            {
//                Name = editing.Name;
//                Line1 = editing.Line1Format;
//                Line2 = editing.Line2Format;
//                FontSizeIndex = editing.FontSize switch
//                {
//                    7 => 0,
//                    10 => 1,
//                    12 => 2,
//                    13 => 3,
//                    14 => 4,
//                    16 => 5,
//                    _ => 1
//                };
//                AlignmentIndex = editing.Alignment switch
//                {
//                    "Left" => 0,
//                    "Center" => 1,
//                    "Right" => 2,
//                    _ => 1
//                };
//                DefaultColorIndex = GetColorIndex(editing.DefaultColor);
//                Notes = editing.Notes;

//                var rules = editing.ColorRules ?? new System.Collections.Generic.List<ColorRule>();
//                if (rules.Count > 0) { Min1 = rules[0].MinPercent; Max1 = rules[0].MaxPercent; ColorIndex1 = GetColorIndex(rules[0].Color); }
//                if (rules.Count > 1) { Min2 = rules[1].MinPercent; Max2 = rules[1].MaxPercent; ColorIndex2 = GetColorIndex(rules[1].Color); }
//                if (rules.Count > 2) { Min3 = rules[2].MinPercent; Max3 = rules[2].MaxPercent; ColorIndex3 = GetColorIndex(rules[2].Color); }
//            }

//            SaveCommand = ReactiveCommand.Create(OnSave);
//            CancelCommand = ReactiveCommand.Create(OnCancel);
//        }

//        private void OnSave()
//        {
//            if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(Line1) || string.IsNullOrWhiteSpace(Line2))
//            {
//                RequestClose?.Invoke(null);
//                return;
//            }

//            var tpl = _editing ?? new DisplayTemplate();

//            tpl.Name = Name;
//            tpl.Line1Format = Line1;
//            tpl.Line2Format = Line2;
//            tpl.FontSize = FontSizeIndex switch
//            {
//                0 => 7,
//                1 => 10,
//                2 => 12,
//                3 => 13,
//                4 => 14,
//                5 => 16,
//                _ => 10
//            };
//            tpl.Alignment = AlignmentIndex switch
//            {
//                0 => "Left",
//                1 => "Center",
//                2 => "Right",
//                _ => "Center"
//            };
//            tpl.DefaultColor = GetColorName(DefaultColorIndex);

//            tpl.ColorRules = new System.Collections.Generic.List<ColorRule>
//            {
//                new ColorRule { MinPercent = Min1, MaxPercent = Max1, Color = GetColorName(ColorIndex1) },
//                new ColorRule { MinPercent = Min2, MaxPercent = Max2, Color = GetColorName(ColorIndex2) },
//                new ColorRule { MinPercent = Min3, MaxPercent = Max3, Color = GetColorName(ColorIndex3) },
//            };

//            tpl.Notes = Notes;

//            RequestClose?.Invoke(tpl);
//        }

//        private void OnCancel() => RequestClose?.Invoke(null);

//        private static int GetColorIndex(string colorName) =>
//            colorName.ToLower() switch
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

//        private static string GetColorName(int index) =>
//            index switch
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
//    }
//}