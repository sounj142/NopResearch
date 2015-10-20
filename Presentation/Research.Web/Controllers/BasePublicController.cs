using Research.Core.Infrastructure;
using Research.Web.Framework.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Research.Web.Controllers
{
    public abstract partial class BasePublicController : BaseController
    {
        /// <summary>
        /// Hàm trả về mã lỗi 404, bằng cách tạo ra đối tượng CommonController và yêu cầu render action PageNotFound của nó
        /// </summary>
        /// <returns></returns>
        protected virtual ActionResult InvokeHttp404()
        {
            var controller = EngineContext.Current.Resolve<CommonController>();
            var routeData = new RouteData();
            routeData.Values.Add("controller", "Common");
            routeData.Values.Add("action", "PageNotFound");

            controller.Execute(new RequestContext(HttpContext, routeData));
            return new EmptyResult();
        }
	}
}