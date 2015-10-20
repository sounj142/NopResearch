using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web;
using System.Web.Hosting;
using Research.Core.Configuration;

namespace Research.Core.Infrastructure
{
    /// <summary>
    /// Cung cấp khả năng tìm kiếm những kiểu ở trong ứng dụng web, tùy chọn cho phép tìm kiếm ở trong tất cả các .dll ở trong thư mục bin.
    /// Sẽ đc dùng thay cho AppDomainTypeFinder ?
    /// Tồn tại singleton mức ứng dụng ?
    /// </summary>
    public class WebAppTypeFinder: AppDomainTypeFinder
    {
        #region Fields and Property and Ctor

        /// <summary>
        /// Đánh dấu đã load các dll từ thư mục bin
        /// </summary>
        private bool _binFolderAssembliesLoaded;

        /// <summary>
        /// kiểm tra và load dll từ thư mục bin ?
        /// </summary>
        public bool EnsureBinFolderAssembliesLoaded { get; set; }

        /// <summary>
        /// Khởi tạo đối tượng với thông tin cấu hình cho bởi config
        /// </summary>
        /// <param name="config"></param>
        public WebAppTypeFinder(NopConfig config)
        {
            this.EnsureBinFolderAssembliesLoaded = config.DynamicDiscovery;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Lấy về đường dẫn vật lý thư mục bin
        /// </summary>
        public virtual string GetBinDirectory()
        {
            if (HostingEnvironment.IsHosted) return HttpRuntime.BinDirectory;
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        /// <summary>
        /// Với cài đặt overide này và cách dùng biến _binFolderAssembliesLoaded, có thể khẳng định WebAppTypeFinder nên tồn tại như
        /// 1 singleton mức ứng dụng, nếu không thì logic gọi đến GetAssemblies sẽ gọi đến LoadMatchingAssemblies nhiều lần
        /// , ko có nhiều vấn đề nhưng có vẻ ko hợp logic
        /// </summary>
        public override IList<Assembly> GetAssemblies()
        {
            if (this.EnsureBinFolderAssembliesLoaded && !this._binFolderAssembliesLoaded)
            {
                _binFolderAssembliesLoaded = true; // nếu đặt xuống dưới LoadMatchingAssemblies sẽ gây ra đệ qui vô hạn
                // lời gọi này sẽ đi theo đường vòng như sau 
                // GetAssemblies[this](nơi gọi) -> LoadMatchingAssemblies -> GetAssemblies[this] -> GetAssemblies[base]
                // hơi quái dị phải ko ?
                LoadMatchingAssemblies(GetBinDirectory());
            }
            // triệu gọi đến hàm cơ sở
            return base.GetAssemblies();
        }

        #endregion
    }
}
