using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Research.Services.Events
{
    /// <summary>
    /// Giao diện đóng vai trò nhà sản xuất. Mẫu publisher - consumer : nhà sản xuất - người tiêu dùng ?
    /// 
    /// Sự kiện sẽ do 1 / 1 số nhà sản xuất tạo ra, và do 1 / 1 số người tiêu dùng xử lý. Nhà sản xuất và người tiêu dùng sẽ ko cần biết
    /// về nhau, mà chỉ tham gia vào 2 đầu băng chuyền, một bên đẩy vào và 1 bên lấy ra. Nhà sản xuất sẽ lấy ra 1 danh sách các
    /// consumer có đăng ký và triệu gọi mỗi khi sản xuất ra 1 sự kiện ?
    /// 
    /// Có thể sẽ có nhiều người tiêu dùng đăng ký vào tiến trình xử lý sự kiện
    /// 
    ///Hệ thống sẽ chỉ có  đối tượng IEventPublisher duy nhất, singleton ?
    /// </summary>
    public interface IEventPublisher
    {
        /// <summary>
        /// Sản xuất ra 1 sự kiện, đưa vào băng chuyền để cho consumer lấy ra xử lý theo ý mình
        ///
        /// Những nơi sau khi sửa đổi dữ liệu xong muốn phát ra thông báo clear cache cần cài đặt giao diện này
        /// 
        /// Ghi chú : Sẽ bỏ qua nếu eventMessage == null
        /// </summary>
        /// <typeparam name="T"> T ở đây sẽ có dạng EntityInserted[Product], EntityDeleted[Product],.... đại loại thế</typeparam>
        /// <param name="eventMessage">Sự kiện được đưa vào băng chuyền</param>
        void Publish<T>(T eventMessage);

        /// <summary>
        /// Cho phép phát ra 2 sự kiện cùng 1 lúc ( ở đây ta sẽ gộp 2 thao tác clear cache lại với nhau để thực hiện chung 1 lần )
        /// </summary>
        void Publish<T1, T2>(T1 eventMessage1, T2 eventMessage2);

        void Publish<T1, T2, T3>(T1 eventMessage1, T2 eventMessage2, T3 eventMessage3);
    }



    // => Trong cơ chế này, ta có thể dùng bất cứ thứ "T" gì làm sự kiện và phát nó ra bằng cách gọi Publish()
    // Để đăng ký nhận và xử lý sự kiện đó ta chỉ cần viết 1 lớp cài đặt giao diện IConsumer<T> , và vứt nó ở bất cứ xó xình nào.
    // Hệ thống sẽ tự động find type vào lúc app start để đưa lớp của chúng ta vào danh sách xử lý sự kiện cho sự kiện "T"
}
