using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebBanLapTop.Controllers
{
    public class ProductController : Controller
    {
        // GET: Product
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Partial_ProductSales()
        {
            // Lấy dữ liệu cần thiết
            return PartialView("Partial_ProductSales");
        }
    }
}