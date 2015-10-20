using System.Collections.Generic;

namespace Research.Services.Events
{
    /// <summary>
    /// Giao diện đóng vai trò Consumer - người tiêu dùng - 1 mẫu design parterm ?
    /// 
    /// 1 đối tượng cài giao diện này sẽ đc dùng để xử lý sự kiện ?
    /// T là loại thông điệp được xử lý ?
    /// 
    /// Những nơi cần nhận sự kiện sẽ cài đặt giao diện này
    /// 
    /// IConsumer sẽ chịu trách nhiệm clear cả static và per request cache
    /// 
    /// </summary>
    /// <typeparam name="T">T ở đây sẽ có dạng EntityInserted[Product], EntityDeleted[Product],.... đại loại thế</typeparam>
    public interface ICacheConsumer<T>
    {
        /// <summary>
        /// Hàm xử lý sự kiện do Publish cung cấp
        /// </summary>
        void HandleEvent(T eventMessage, IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes);

        /// <summary>
        /// Thứ tự thực hiện của consumer nếu nhà sản xuất tìm được nhiều hơn 1 consumer đăng ký xử lý sự kiện
        /// Mặc định thì ta dùng 1 cài đặt Order chung, nhưng có thể viết Order riêng cho những Interface có nhu cầu
        /// thay đổi cấu hình Order
        /// </summary>
        int Order { get; }
    }
}
