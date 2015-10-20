

namespace Research.Core.Domain.Customers
{
    public static partial class SystemCustomerNames
    {
        /// <summary>
        /// SystemName dành cho tài khoản thộc loại SearchEngine, chuyên dùng cho các máy tìm kiếm vốn đòi hỏi cách cư xử hơi đặc biệt.
        /// Sẽ chỉ có duy nhất 1 tài khoản thuộc loại này trong hệ thống
        /// </summary>
        public const string SearchEngine = "SearchEngine";

        /// <summary>
        /// SystemName dành cho tài khoản thộc loại BackgroundTask, chuyên dùng cho các tác vụ chạy nền dùng FakeHttpRequest.
        /// Sẽ chỉ có duy nhất 1 tài khoản thuộc loại này trong hệ thống
        /// </summary>
        public const string BackgroundTask = "BackgroundTask";
    }
}
