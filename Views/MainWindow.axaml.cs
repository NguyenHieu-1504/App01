using Avalonia.Controls;
using Avalonia.Interactivity;
using App01.ViewModels;

namespace App01.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Cleanup khi đóng window
            Closing += MainWindow_Closing;
        }

        private void BtnSettings_Click(object? sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow();
            settingsWindow.ShowDialog(this);

            // Sau khi đóng Settings, có thể reload app
            // TODO: Implement reload logic nếu cần
        }

        private void BtnDashboard_Click(object? sender, RoutedEventArgs e)
        {
            var dashboard = new DashboardWindow();
            dashboard.Show();
        }

        private void BtnTestLed_Click(object? sender, RoutedEventArgs e)
        {
            var testWindow = new LedTestWindow();
            testWindow.Show();
        }
        private async void BtnReload_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel vm)
            {
                await vm.ReloadConfigurationAsync();
            }
        }

        //private async void BtnStart_Click(object? sender, RoutedEventArgs e)
        //{
        //    if (DataContext is MainWindowViewModel vm)
        //    {
        //        await vm.StartMonitoringAsync();
        //    }
        //}

        //private void BtnStop_Click(object? sender, RoutedEventArgs e)
        //{
        //    if (DataContext is MainWindowViewModel vm)
        //    {
        //        vm.StopMonitoring();
        //    }
        //}

        private void MainWindow_Closing(object? sender, WindowClosingEventArgs e)
        {
            // Cleanup resources
            if (DataContext is MainWindowViewModel vm)
            {
                vm.Cleanup();
            }
        }
    }
}