using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web;

namespace Research.Core.Caching
{
    /// <summary>
    /// Perrequest thường ko có nhu cầu bất đồng bộ nên nhìn chung ko cần đặt khóa
    /// </summary>
    public partial class PerRequestCacheManager : IPerRequestCacheManager
    {
        #region thread safe

        public virtual T Get<T>(string key, Func<T> acquire)
        {
            return Get(key, 60, acquire);
        }

        public virtual T Get<T>(string key, int cacheTime, Func<T> acquire)
        {
            if (IsSet(key)) return Get<T>(key);

            var result = acquire();
            if (cacheTime > 0) Set(key, result, cacheTime);
            return result;
        }

        #endregion

        private readonly HttpContextBase _context;

        public PerRequestCacheManager(HttpContextBase context)
        {
            this._context = context;
        }

        /// <summary>
        /// GetItems - Trả về danh sách Item của context.Items
        /// </summary>
        protected virtual IDictionary Items
        {
            get { return _context != null ? _context.Items : null; }
        }


        public virtual T Get<T>(string key)
        {
            var items = Items;
            if (items == null) return default(T);
            return (T)items[key];
        }

        public virtual bool TryGet<T>(string key, out T value)
        {
            var items = Items;
            if (items != null && items.Contains(key))
            {
                value = (T)items[key];
                return true;
            }
            else
            {
                value = default(T);
                return false;
            }
        }
        /// <summary>
        /// Lưu ý: Cho phép add null vào cache ( nguyên bản ko cho phép ) ////NULL
        /// </summary>
        public virtual void Set<T>(string key, T data, int cacheTime)
        {
            if (data == null) return;

            var items = Items;
            if (items == null) return;
            items[key] = data;
        }

        public virtual bool IsSet(string key)
        {
            var items = Items;
            if (items == null) return false;
            return items.Contains(key);
        }

        public virtual void Remove(string key)
        {
            var items = Items;
            if (items == null) return;
            items.Remove(key);
        }

        public virtual void RemoveByPattern(string pattern)
        {
            var items = Items;
            if (items == null) return;

            var enumerator = items.GetEnumerator();
            var regex = new Regex(pattern, RegexOptions.Singleline | RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var keyToRemove = new List<string>();
            while (enumerator.MoveNext())
            {
                var key = enumerator.Key.ToString();
                if (regex.IsMatch(key))
                    keyToRemove.Add(key);
            }
            foreach (var key in keyToRemove) items.Remove(key);
        }

        public virtual void Clear()
        {
            var items = Items;
            if (items == null) return;

            var enumerator = items.GetEnumerator();
            var keyToRemove = new List<string>();
            while (enumerator.MoveNext())
            {
                keyToRemove.Add(enumerator.Key.ToString());
            }
            foreach (var key in keyToRemove) items.Remove(key);

            // Ý tưởng: Nếu về sau hệ thống cần dùng 1 vài khóa trong per request cache cho những mục đích đặc biệt mà ko nên bị clear.
            // Khi đó cần định nghĩa 1 danh sách các khóa đó ở đâu đó, và thao tác clear sẽ quét qua để loại bỏ các key chỉ
            // khi nó nằm ngoài danh sách khóa này
        }


        public void Set<T>(string key, T data)
        {
            Set(key, data, 60);
        }


        public void RemoveByKeysStartsWith(string prefix)
        {
            if (prefix == null) return;
            var items = Items;
            if (items == null) return;

            var enumerator = items.GetEnumerator();
            var keyToRemove = new List<string>();
            while (enumerator.MoveNext())
            {
                var key = enumerator.Key.ToString();
                if (key.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase))
                    keyToRemove.Add(key);
            }
            foreach (var key in keyToRemove) items.Remove(key);
        }

        public void RemoveByKeysStartsWith(IList<string> prefixes)
        {
            if (prefixes == null || prefixes.Count == 0) return;
            var items = Items;
            if (items == null) return;

            var enumerator = items.GetEnumerator();
            var keyToRemove = new List<string>();
            while (enumerator.MoveNext())
            {
                var key = enumerator.Key.ToString();
                foreach (var prefix in prefixes)
                    if (key.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase))
                    {
                        keyToRemove.Add(key);
                        break;
                    }
            }
            foreach (var key in keyToRemove) items.Remove(key);
        }
    }
}
