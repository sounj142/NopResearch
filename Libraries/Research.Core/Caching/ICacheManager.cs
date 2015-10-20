using System;
using System.Collections.Generic;

namespace Research.Core.Caching
{
    public interface ICacheManager
    {
        T Get<T>(string key);

        bool TryGet<T>(string key, out T value);

        void Set<T>(string key, T data, int cacheTime);

        void Set<T>(string key, T data);

        /// <summary>
        /// Kiểm tra cache có chứa key
        /// </summary>
        bool IsSet(string key);

        void Remove(string key);

        /// <summary>
        /// Loại bỏ các key thỏa mãn 1 mẫu regex cho trước
        /// </summary>
        void RemoveByPattern(string pattern);

        /// <summary>
        /// Loại bỏ các key bắt đầu bằng 1 chuỗi prefix cho trước. Cách này luôn tỏ ra hiệu quả hơn cách so theo regex,
        /// hơn nữa key cache của ứng dụng cũng theo dạng prefix, nên ta sẽ tập trung làm theo hướng này
        /// </summary>
        void RemoveByKeysStartsWith(string prefix);

        void RemoveByKeysStartsWith(IList<string> prefixes);

        void Clear();

        T Get<T>(string key, Func<T> acquire);

        T Get<T>(string key, int cacheTime, Func<T> acquire);
    }
}
