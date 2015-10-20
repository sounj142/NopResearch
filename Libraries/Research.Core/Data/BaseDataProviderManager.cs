using System;

namespace Research.Core.Data
{
    public abstract class BaseDataProviderManager // BaseDataProviderManager sẽ đc đăng ký với DI ?
    {
        /// <summary>
        /// lưu trữ trong nó 1 database setting
        /// </summary>
        protected DataSettings Settings { get; private set; }

        /// <summary>
        /// Bắt buộc lớp kế thừa phải cung cấp IDataProvider
        /// </summary>
        public abstract IDataProvider LoadDataProvider();

        protected BaseDataProviderManager(DataSettings settings)
        {
            if (settings == null) throw new ArgumentNullException("settings");
            this.Settings = settings;
        }
    }
}
