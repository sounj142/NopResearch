using Research.Core;
using Research.Core.Domain.Stores;
using Research.Core.Interface.Service;
using System;
using System.Linq;
using System.Web;

namespace Research.Web.Framework
{
    /// <summary>
    /// Lớp cài đặt ngữ cảnh Store hiện hành cho ứng dụng
    /// 
    /// 
    /// Cẩn trọng với các task, khi mà dùng FakeHttpContext, khi đó CurrentStore trả về sẽ là 1 đối tượng giả với Id = 0. Chú ý điều này !
    /// </summary>
    public partial class WebStoreContext: IStoreContext
    {
        private readonly IStoreService _storeService;
        private readonly IWebHelper _webHelper;

        /// <summary>
        /// Biến dùng để lưu giữ Store hiện hành tìm được, đùng để lấy nhanh giá trị cho lần sau mà ko cần tính lại
        /// </summary>
        private Store _cacheStore;

        public WebStoreContext(IStoreService storeService, IWebHelper webHelper)
        {
            _storeService = storeService;
            _webHelper = webHelper;
        }

        /// <summary>
        /// Lấy về Store hiện hành, được cài đặt lazyloading
        /// Store được lấy ra thông qua host của url
        /// 
        /// 
        /// Có 1 vấn đề đối với CurrentStore khi chạy trong 1 Task riêng ( dùng FakeHttpContext ): đó là sẽ luôn lấy ra Store đầu tiên
        /// làm CurrentStore, và toàn bộ các code chạy trên Task riêng sẽ mặc nhiên làm việc với Store đầu tiên này. Liệu có đủ
        /// tổng quát khi mà những tác vụ chạy nền lại đi làm việc trên 1 Store cụ thể, thay vì làm việc với 1 Store tổng quát ?
        /// 
        /// Điển hình nhất là thao tác lấy ra các đối tượng Settings. Các đối tượng Setting luôn luôn có cấu hình chung ( StoreId = 0)
        /// và cấu hình riêng cho từng Store (StoreId != 0). Trong 1 tác vụ chạy nền, vì lấy ra Store đầu tiên làm CurrentStore, nên
        /// tất cả các đối tượng Setting mà tác vụ nền sử dụng đều được ưu tiên lấy theo Store đầu tiên này. Do đó, tác vụ nền lại chạy
        /// bằng dữ liệu cấu hình của Store đầu tiên, liệu điều đó có đúng ko khi mà theo logic, tác vụ chạy nền chỉ nên dùng
        /// dữ liệu cấu hình chung ( với StoreId = 0 ) ?
        /// </summary>
        public Store CurrentStore
        {
            get
            {
                if (_cacheStore != null) return _cacheStore;

                // store hiện hành được nhận diện từ chuỗi host hiện hành ( chuỗi domain[:port] )
                string host = _webHelper.ServerVariables("HTTP_HOST");
                var allStores = _storeService.GetAllStores();
                Store store = null;
                if (!string.IsNullOrEmpty(host))
                    store = allStores.FirstOrDefault(s => s.ContainsHostValue(host));
                /* test 1 chút để khi HttpContext.Current == null sẽ trả về 1 đối tượng Store ảo với Id=0. Nếu về sau có lỗi, 
                 * chỉ cần ẩn đoạn code này đi là ok */
                else if (HttpContext.Current == null) // trường hợp dùng FakeHttpContext và chuỗi host rỗng
                {
                    _cacheStore = new Store
                    {
                        Id = 0,
                        Url = "NonHttpContext/HiddenTask"
                    };
                    return _cacheStore;
                }
                /* end test */

                // nếu ko thấy 1 store nào phù hợp với host thì lấy store đầu tiên làm store hiện hành
                if (store == null) store = allStores.FirstOrDefault();
                if (store == null) throw new Exception("Dữ liệu sai! Hệ thống ko có store nào cả");

                _cacheStore = store;
                return _cacheStore;
            }
        }
    }
}
