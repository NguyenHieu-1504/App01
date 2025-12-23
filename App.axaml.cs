using App01.Services;
using App01.ViewModels;
using App01.Views;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace App01;

public partial class App : Application
{
    public override void Initialize()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Khởi tạo cửa sổ chính (giữ nguyên code cũ của bạn)
            desktop.MainWindow = new Views.MainWindow()

            {
                DataContext = new MainWindowViewModel()
            };


           // ---BẮT ĐẦU ĐOẠN CODE TEST ---

           // 1.Cấu hình chuỗi kết nối(Thay thế username/ password của bạn vào đây)
           //  Lưu ý: directConnection = true rất quan trọng với server Standalone cũ
           // string connString = "mongodb://kztek:Kztek123456@14.160.26.45:27701/event?authSource=admin&directConnection=true";

           // Nếu có user / pass thì dùng chuỗi này:
           //  string connString = "mongodb://username:password@14.160.26.45:27701/?authSource=admin&directConnection=true";

           // string dbName = "event";

           // 2.Gọi Service
           //var service = new ParkingDataService();
           // Debug.WriteLine("--> Đang kết nối MongoDB...");

           // bool isConnected = service.Connect(connString, dbName);

           // if (isConnected)
           // {
           //     Debug.WriteLine("--> Kết nối thành công! Đang đếm xe...");

           //     Gọi hàm đếm(chạy bất đồng bộ)
           //     System.Threading.Tasks.Task.Run(async () =>
           //     {
           //         long count = await service.CountParkedCarsAsync();

           //         IN KẾT QUẢ RA MÀN HÌNH OUTPUT
           //         Debug.WriteLine("=========================================");
           //         Debug.WriteLine($"SỐ XE ĐANG GỬI THỰC TẾ: {count}");

           //     });
           // }
           // else
           // {
           //     Debug.WriteLine("--> Kết nối thất bại. Kiểm tra lại IP/Pass.");
           // }
           // ---KẾT THÚC ĐOẠN CODE TEST ---

           //var ledService = new LedService();
           // bool connected = ledService.Connect("192.168.1.100", 100);
           // if (connected)
           // {
           //     await ledService.DisplayTwoLinesAsync("TEST", "OK", 1, 2);
           // }

        }

        base.OnFrameworkInitializationCompleted();
    }
}