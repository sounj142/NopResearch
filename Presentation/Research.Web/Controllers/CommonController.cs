using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Research.Web.Controllers
{
    public class CommonController : BasePublicController
    {
        /// <summary>
        /// Trang báo lỗi này hoạt động theo phương thức sẽ vẫn trả về 1 trang nội dung html đầy đủ, 
        /// nhưng mã trả về là 404 thay vì mã 200 thông thường
        /// </summary>
        public ActionResult PageNotFound()
        {
            this.Response.StatusCode = 404;
            this.Response.TrySkipIisCustomErrors = true;
            return View();
        }
	}
}