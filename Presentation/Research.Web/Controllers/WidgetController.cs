using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Research.Web.Controllers
{
    public class WidgetController : BasePublicController
    {
        [ChildActionOnly]
        public ActionResult WidgetsByZone()
        {
            return View();
        }
	}
}