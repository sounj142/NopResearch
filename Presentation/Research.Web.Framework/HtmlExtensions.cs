using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace Research.Web.Framework
{
    public static class HtmlExtensions
    {
        /// <summary>
        /// render 1 vùng widget với tên là widgetZone, nội dung bên trong widget sẽ được tập hợp lại từ khắp mọi nơi, bao gồm cả các
        /// plugin ( dùng cơ chế load động ). Hàm sẽ gọi đến 1 action method, và chính action method này sẽ chịu trách nhiệm tìm
        /// tất cả các nội dung liên quan đến tên Wiget và render ra kết quả
        /// </summary>
        public static MvcHtmlString Widget(this HtmlHelper helper, string widgetZone, object addtionalData = null)
        {
            return helper.Action("WidgetsByZone", "Widget", new { widgetZone, addtionalData });
        }
    }
}
