using Research.Services.Caching.Models;
using System;
using System.Collections.Generic;

namespace Research.Services.Caching.Writer
{
    /// <summary>
    /// Được cài đặt để dùng static cache. Có thể chuyển qua NullCache/PerRequestCache để test trong cấu hình Autofact
    /// Tuy nhiên việc chuyển này sẽ ko linh động để chỉ mỗi ISettingService chuyển cache mà là chuyển toàn bộ hệ thống sang xài cache mới
    /// </summary>
    public interface ISettingCacheWriter
    {
        IDictionary<string, SettingForCache> GetAll(Func<IDictionary<string, SettingForCache>> acquire);

        /// <summary>
        /// Hàm cho phép cache trực tiếp setting kiểu type vào static cache, giúp lấy giá trị này ra từ cache
        /// thay vì phải đọc lại ở mỗi request
        /// </summary>
        object GetSetting(int storeId, Type type, Func<object> acquire);

        /// <summary>
        /// Hàm thử lấy ra giá trị cache cho setting kiểu type
        /// </summary>
        bool TryGetSetting(int storeId, Type type, out object result);
    }
}
