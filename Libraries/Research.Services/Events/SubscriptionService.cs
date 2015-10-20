using System.Collections.Generic;
using System.Linq;
using Research.Core.Infrastructure;

namespace Research.Services.Events
{
    /// <summary>
    /// Lớp chịu trách nhiệm lấy về danh sách các consumer đăng ký xử lý sự kiện T,IConsumer[T], 
    /// với T là các EntityInserted[X], EntityUpdate[X]....
    /// </summary>
    public class SubscriptionService : ISubscriptionService
    {
        public IList<ICacheConsumer<T>> GetSubscriptions<T>()
        {
            // lấy về 1 list tất cả các kiểu IConsumer<T> có đăng ký với autofac
            // chú ý: Bằng việc bổ sung khả năng sắp thứ tự Order, ta sẽ qui định 1 thứ tự mặc định, đồng thời sẽ cho phép từng
            // interface có thể override lại thư tự thực hiện, bằng cách đó ta có thể sắp xếp các hàm clear cache thực hiện
            // theo thứ tự mong muốn nếu có nhu cầu đặc biệt
            
            
            //return EngineContext.Current.ResolveAll<ICacheConsumer<T>>().OrderBy(p => p.Order).ToList();
            // vì đã chuyển mục đích ICacheConsumer sang đăng ký các mẫu key cần clear nên ko cần quan tâm đến thứ tự nữa
            return EngineContext.Current.ResolveAll<ICacheConsumer<T>>().ToList();
        }
    }
}
