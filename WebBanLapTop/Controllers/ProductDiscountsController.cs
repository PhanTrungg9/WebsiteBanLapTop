using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanLapTop.Models;

namespace WebBanLapTop.Controllers
{
    public class ProductDiscountsController : Controller
    {
        // GET: ProductDiscounts
        DatabaseDataContext db = new DatabaseDataContext();
        public ActionResult Index()
        {
            var data = db.vv_ProductLists.Where(v => db.tb_products
                            .Any(p => p.product_id == v.product_id && (p.is_delete == false || p.is_delete == null)))
                            .ToList();
            return View(data);
        }
    }
}