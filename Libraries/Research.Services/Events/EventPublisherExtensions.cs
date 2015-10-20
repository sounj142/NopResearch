using Research.Core;
using Research.Core.Events;

namespace Research.Services.Events
{
    /// <summary>
    /// Đăng ký các extention method thay vì khai báo nó trực tiếp trong định nghĩa của IEventPublisher là để tận dụng được lợi thế của
    /// extension method. Bằng cách khái báo như thế này, mọi lớp có cài đặt IEventPublisher sẽ mặc nhiên có sẵn các hàm này, trong khi
    /// nếu khai báo trực tiếp trong IEventPublisher thì mọi lớp sẽ phải tự mình cài đặt lại các hàm này. Nếu chỉ có 1 lớp EventPublisher
    /// thì ko sao, nhưng nếu ta cài đặt custom lớp khác thì sẽ phải đối mặt với vấn đề cài đặt lại
    /// Và cũng ko hay lắm nếu cài đặt 1 lớp base chứa các phương thức trong extension method làm lớp cơ sở
    /// </summary>
    public static class EventPublisherExtensions
    {
        /// <summary>
        /// Cú pháp sẽ là eventPublisher.EntityInserted(product), hiểu là phát ra sự kiện insert xảy ra trên đối tượng product
        /// 
        /// Xử lý bên trong: Tạo ra 1 đối tượng tùng chứa EntityInserted[Product] bao lấy product, rồi gọi đến 
        /// eventPublisher.Publish() để thông báo sự kiện này đến các consumer
        /// </summary>
        public static void EntityInserted<T>(this IEventPublisher eventPublisher, T entity) where T:BaseEntity
        {
            eventPublisher.Publish(new EntityInserted<T>(entity));
        }

        public static void EntityUpdated<T>(this IEventPublisher eventPublisher, T entity) where T : BaseEntity
        {
            eventPublisher.Publish(new EntityUpdated<T>(entity));
        }

        public static void EntityDeleted<T>(this IEventPublisher eventPublisher, T entity) where T : BaseEntity
        {
            eventPublisher.Publish(new EntityDeleted<T>(entity));
        }

        public static void EntityAllChange<T>(this IEventPublisher eventPublisher, T entity) where T : BaseEntity
        {
            eventPublisher.Publish(new EntityAllChange<T>(entity));
        }
    }
}
