using System.Collections.Generic;

namespace Research.Services.Events
{
    /// <summary>
    /// Cho phép lấy về danh sách những Consumer đăng ký nhận sự kiện T
    /// 
    /// Hệ thống sẽ chỉ có  đối tượng ISubscriptionService duy nhất, singleton ?
    /// </summary>
    public interface ISubscriptionService
    {
        /// <summary>
        /// Hàm trả về 1 list các IConsumer có đăng ký xử lý sự kiện T
        /// dựa vào đó, khi có sự kiện T phát sinh, ta sẽ lặp qua danh sách này và gọi đến IConsumer.HandleEvent() để gọi đến hàm
        /// mà các IConsumer đăng ký dùng để xử lý sự kiện
        /// </summary>
        IList<ICacheConsumer<T>> GetSubscriptions<T>();
    }
}
