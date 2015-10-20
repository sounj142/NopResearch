using Research.Core.Interface.Service;
using Research.Core.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Research.Web.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly IOfficialFeedManager _feedManager;
        private readonly ICurrencyService _currencyService;

        public ProductController(IProductService productService, 
            IOfficialFeedManager feedManager,
            ICurrencyService currencyService)
        {
            _productService = productService;
            _feedManager = feedManager;
            _currencyService = currencyService;
        }

        public ActionResult ProductDetails(int productid, string seName)
        {
            var product = _productService.GetById(productid);
            ViewBag.seName = seName;

            product.Name = product.GetLocalized(p => p.Name);

            return View(product);
        }

        public ActionResult DoIt()
        {
            return View();
        }

        [HttpPost]
        public ActionResult DoIt(int id)
        {
            //var currency = _currencyService.GetById(id);
            //_currencyService.Delete(currency);

            return View();
        }

        public async Task<ActionResult> Test()
        {
            var data = await _feedManager.GetAllPlugins();

            return View(data);
        }
	}
}