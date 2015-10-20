using Research.Core.Configuration;

namespace Research.Services.Helpers
{
    /// <summary>
    /// Chứa các thông tin cấu hình cho ngày giờ
    /// </summary>
    public class DateTimeSettings : ISettings
    {
        /// <summary>
        /// default time zone của store. Mỗi store có thể tự qui định 1 múi giờ local của riêng mình 
        /// ( nhưng toàn hệ thống vẫn dùng UTC time để đảm bảo tính chính xác và nhất quán )
        /// </summary>
        public string DefaultStoreTimeZoneId { get; set; }

        /// <summary>
        /// Cho biết customer có đc phép tự chọn múi giờ của riêng họ hay ko ? Tức là sẽ có phần chọn múi giờ ở đâu đó, và sau khi
        /// họ chọn múi giờ, toàn bộ giờ giấc hiển thị trên trình duyệt của họ sẽ đc chuyển đổi sang múi giờ của customer.
        /// Như vậy hệ thống sẽ render giờ giấc cho mỗi customer khác nhau, nhưng tất cả sẽ được chuyển đổi thành UTC time khi
        /// lưu trữ ở server
        /// </summary>
        public bool AllowCustomersToSetTimeZone { get; set; }
    }
}
