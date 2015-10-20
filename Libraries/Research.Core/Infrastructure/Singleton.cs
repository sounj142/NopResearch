using System;
using System.Collections.Generic;

namespace Research.Core.Infrastructure
{
    /// <summary>
    /// Lớp đóng vai trò như cửa ngõ để lấy về danh sách các Singelton thuộc kiểu Singelton[T].
    /// Sở hữu 1 static Dictionary AllSingletons cho phép lưu trữ danh sách các Singleton[T] hiện hành
    /// </summary>
    public class Singleton
    { 
        private static readonly IDictionary<Type, object> allSingletons = new Dictionary<Type, object>();

        public static IDictionary<Type, object> AllSingletons
        {
            get { return allSingletons; }
        }
    }

    /// <summary>
    /// Dùng để lưu trữ 1 đối tượng chỉ tồn tại duy nhất "singleton" trong vòng đời ứng dụng. Ko giống với mẫu singleton thông thường,
    /// ở đây cho phép code bên ngoài tự ý thiết lập đối tượng singleton mới theo ý mình
    /// </summary>
    /// <typeparam name="T">Kiểu đối tượng được lưu trữ trong lớp singleton</typeparam>
    public class Singleton<T> : Singleton
    {
        private static T instance;

        /// <summary>
        /// Đối tương singleton kiểu T. Do có hàm set cho phép thiết lập từ bên ngoài nên có khả năng Singleton<T>.Instance ở những thời
        /// điểm khác nhau trả về kết quả khác nhau, có thể là default(T), có thể là các đối tượng T khác nhau. Nhưng sẽ chỉ có
        /// 1 đối tượng
        /// </summary>
        public static T Instance
        {
            get { return instance; }
            set
            {
                var allSingletons = AllSingletons;
                lock (allSingletons) //// nghiêm trọng
                {
                    instance = value;
                    allSingletons[typeof(T)] = value;
                }
            }
        }
    }

    /// <summary>
    /// lớp Singleton<> mà kiểu chứa trong nó là 1 IList />
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SingletonList<T>:Singleton<IList<T>>
    {
        static SingletonList()
        {
            Singleton<IList<T>>.Instance = new List<T>();
        }
        /// <summary>
        /// Định nghĩa mới để che dấu method set trong lớp cơ sở
        /// </summary>
        public static new IList<T> Instance
        {
            get { return Singleton<IList<T>>.Instance; }
        }
    }

    /// <summary>
    /// lớp Singleton<> mà kiểu chứa trong nó là 1 IDictionary />
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SingletonDictionary<TKey, TValue> : Singleton<IDictionary<TKey, TValue>>
    {
        static SingletonDictionary()
        {
            Singleton<IDictionary<TKey, TValue>>.Instance = new Dictionary<TKey, TValue>();
        }
        /// <summary>
        /// Định nghĩa mới để che dấu method set trong lớp cơ sở
        /// </summary>
        public static new IDictionary<TKey, TValue> Instance
        {
            get { return Singleton<IDictionary<TKey, TValue>>.Instance; }
        }
    }
}
