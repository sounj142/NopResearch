using System;
using System.Collections.ObjectModel;
using Research.Core.Domain.Customers;

namespace Research.Core.Interface.Service
{
    /// <summary>
    /// service cung cấp 1 số hàm tiện ích về thời gian, dùng cho việc chuyển đổi múi giờ.
    /// Hệ thống Nop cho phép mỗi store tự qui định 1 time zone của riêng mình, mỗi customer cũng có thể tự qui định time zone của
    /// mình . Hệ quả là hệ thống sẽ phải chuyển đổi phần thời gian trong nội dung hiển thị về đúng múi giờ của customer/store tương ứng
    /// . Đương nhiên, ở toàn cục thì hệ thống lưu thời gian ở dạng chuẩn UTC để đảm bảo tính chính xác và nhất quán, chỉ chuyển đổi
    /// nó thành timezone tương ứng và chuyển ngược từ time zone tương ứng về UTC time khi cần hiển thị hay lấy thời gian từ người dùng
    /// mà thôi
    /// </summary>
    public partial interface IDateTimeHelper
    {
        /// <summary>
        /// Retrieves a System.TimeZoneInfo object from the registry based on its identifier.
        /// Lấy về TimeZoneInfo object từ registry tương ứng với chuỗi id
        /// </summary>
        /// <param name="id">The time zone identifier, which corresponds to the System.TimeZoneInfo.Id property.</param>
        /// <returns>A System.TimeZoneInfo object whose identifier is the value of the id parameter.</returns>
        TimeZoneInfo FindTimeZoneById(string id);

        /// <summary>
        /// Returns a sorted collection of all the time zones.
        /// Lấy về tập hợp tất cả các time zone của hệ thống, tập này đã đc sắp xếp và chỉ đọc
        /// </summary>
        /// <returns>A read-only collection of System.TimeZoneInfo objects.</returns>
        ReadOnlyCollection<TimeZoneInfo> GetSystemTimeZones();

        /// <summary>
        /// Converts the date and time to current user date and time.
        /// Convert ngày giờ về ngày giờ theo múi giờ của người dùng hiện hành
        /// </summary>
        /// <param name="dt">Ngày giờ cần convert, dạng system local time hoặc là UTC time</param>
        /// <returns>1 ngày giờ theo time zone của người dùng hiện hành</returns>
        DateTime ConvertToUserTime(DateTime dt);

        /// <summary>
        /// Converts the date and time to current user date and time.
        /// Convert ngày giờ về ngày giờ theo múi giờ của người dùng hiện hành
        /// </summary>
        /// <param name="dt">The date and time (respesents local system time or UTC time) to convert.</param>
        /// <param name="sourceDateTimeKind">The source datetimekind</param>
        /// <returns>A DateTime value that represents time that corresponds to the dateTime parameter in customer time zone.</returns>
        DateTime ConvertToUserTime(DateTime dt, DateTimeKind sourceDateTimeKind);

        /// <summary>
        /// Converts the date and time to current user date and time
        /// </summary>
        /// <param name="dt">The date and time to convert.</param>
        /// <param name="sourceTimeZone">The time zone of dateTime.</param>
        /// <returns>A DateTime value that represents time that corresponds to the dateTime parameter in customer time zone.</returns>
        DateTime ConvertToUserTime(DateTime dt, TimeZoneInfo sourceTimeZone);

        /// <summary>
        /// Converts the date and time to current user date and time
        /// </summary>
        /// <param name="dt">The date and time to convert.</param>
        /// <param name="sourceTimeZone">The time zone of dateTime.</param>
        /// <param name="destinationTimeZone">The time zone to convert dateTime to.</param>
        /// <returns>A DateTime value that represents time that corresponds to the dateTime parameter in customer time zone.</returns>
        DateTime ConvertToUserTime(DateTime dt, TimeZoneInfo sourceTimeZone, TimeZoneInfo destinationTimeZone);

        /// <summary>
        /// Converts the date and time to Coordinated Universal Time (UTC)
        /// </summary>
        /// <param name="dt">The date and time (respesents local system time or UTC time) to convert.</param>
        /// <returns>A DateTime value that represents the Coordinated Universal Time (UTC) that corresponds to the dateTime parameter. The DateTime value's Kind property is always set to DateTimeKind.Utc.</returns>
        DateTime ConvertToUtcTime(DateTime dt);

        /// <summary>
        /// Converts the date and time to Coordinated Universal Time (UTC)
        /// </summary>
        /// <param name="dt">The date and time (respesents local system time or UTC time) to convert.</param>
        /// <param name="sourceDateTimeKind">The source datetimekind</param>
        /// <returns>A DateTime value that represents the Coordinated Universal Time (UTC) that corresponds to the dateTime parameter. The DateTime value's Kind property is always set to DateTimeKind.Utc.</returns>
        DateTime ConvertToUtcTime(DateTime dt, DateTimeKind sourceDateTimeKind);

        /// <summary>
        /// Converts the date and time to Coordinated Universal Time (UTC)
        /// </summary>
        /// <param name="dt">The date and time to convert.</param>
        /// <param name="sourceTimeZone">The time zone of dateTime.</param>
        /// <returns>A DateTime value that represents the Coordinated Universal Time (UTC) that corresponds to the dateTime parameter. The DateTime value's Kind property is always set to DateTimeKind.Utc.</returns>
        DateTime ConvertToUtcTime(DateTime dt, TimeZoneInfo sourceTimeZone);

        /// <summary>
        /// Gets a customer time zone.
        /// Lấy về time zone của người dùng customer nếu hệ thống cho phép customer cấu hình time zone riêng, hoặc time zone của store
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <returns>Customer time zone; if customer is null, then default store time zone</returns>
        TimeZoneInfo GetCustomerTimeZone(Customer customer);

        /// <summary>
        /// Time zone của store hiện hành. Nếu không có cấu hình riêng thì mặc định sẽ lấy time zone chung ( store id = 0)
        /// </summary>
        TimeZoneInfo DefaultStoreTimeZone { get; set; }

        /// <summary>
        /// Time zone của người dùng hiện hành. Nó được quyết đinh tùy thuộc vào cấu hình, có thể là time zone của customer trong trường
        /// hợp hệ thống cho phép customer tự chọn time zone, hoặc là time zone của store hiện hành
        /// </summary>
        TimeZoneInfo CurrentTimeZone { get; set; }
    }
}
