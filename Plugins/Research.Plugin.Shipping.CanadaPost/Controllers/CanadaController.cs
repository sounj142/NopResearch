using System;
using System.Web.Mvc;

namespace Research.Plugin.Shipping.CanadaPost.Controllers
{
    public class CanadaController: Controller
    {
        public ActionResult Index()
        {
            return View("~/Plugins/Shipping.CanadaPost/Views/Canada/Index.cshtml");
        }
    }
}
