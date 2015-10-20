using Research.Core;
using Research.Core.Data;
using Research.Core.Infrastructure;

namespace Research.Data
{
    // Các hàm Execute sẽ được gọi tuần tự, ko phải song song

    /// <summary>
    /// Đây chính là nơi xảy ra ma thuật khởi tạo database trong ứng dụng Nop. Chúng ta sẽ tạo ra 1 đối tượng IStartupTask, đặt vào nó
    /// những code cần thiết để khởi tạo database, và sau đó sẽ .... ko làm gì cả. Việc triệu gọi code sẽ được thực hiện bới thao tác load
    /// động tác vụ của IEngine, IEngine sẽ bảo đảm tạo và triệu gọi EfStartUpTask tại thời điểm app start
    /// </summary>
    public class EfStartUpTask : IStartupTask
    {
        public void Execute()
        {
            var engine = EngineContext.Current;
            var settings = engine.Resolve<DataSettings>(); // lấy DataSettings thông qua DI với method thuân tiện Resolve, dùng 
            // engine.ContainerManager để có các method DI năng cao cấp
            if(settings != null && settings.IsValid())
            {
                // Có 2 data provider là SqlServerDataProvider và SqlCeDataProvider. Việc Autofac chọn cái nào là do cấu hình mà
                // chúng ta chọn trong lần chạy code đầu tiên khi chưa có cấu hình database. Sau khi đã qua bước này, thông tin cấu
                // hình sẽ được lưu trữ xuống file ( dạng DataSettings ) => Mỗi lần chạy đều chỉ cần load từ DataSetting
                var provider = engine.Resolve<IDataProvider>();
                // 1 điểm nữa : Xem lại Resolve<T>() !. Resolve luôn mặc định chọn ngữ cảnh phạm vi là vòng đời request, có ổn ko khi lấy ra
                // 1 đối tượng vòng đời ứng dụng từ Resolve<T>() 

                if (provider == null) throw new ResearchException("No IDataProvider found");
                // thử nghiệm chuyển việc thực thi dataProvider.InitConnectionFactory vào cho EfStartUpTask thực hiện
                provider.InitDatabase();
                ////provider.SetDatabaseInitializer(); // tại sao ko gọi InitDatabase ? Vì InitDatabase có 2 thao tác thì InitConnectionFactory(); đã được triệu gọi
                ////// trước đó, ko cần gọi lại, chỉ cần gọi đến SetDatabaseInitializer
            }
        }

        public int Order
        {
            // đảm bảo đc chạy đầu tiên
            get { return -1000; }
        }
    }
}
