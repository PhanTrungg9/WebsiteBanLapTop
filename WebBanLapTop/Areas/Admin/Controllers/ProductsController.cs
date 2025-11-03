using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using WebBanLapTop.Models;

namespace WebBanLapTop.Areas.Admin.Controllers
{
    public class ProductsController : Controller
    {
        // GET: Admin/Products
        DatabaseDataContext db = new DatabaseDataContext();

        public ActionResult Index(int? page)
        {
            int pageSize = 5;
            int pageNumber = page ?? 1;


            var paged = db.vw_ProductWithDiscountCounts
                          .OrderByDescending(x => x.product_id)
                          .ToPagedList(pageNumber, pageSize);


            ViewBag.PageSize = pageSize;
            ViewBag.Page = pageNumber;

            return View(paged);
        }
        public JsonResult getProductName()
        {
            var data = db.tb_products.Select(x => new
            {
                x.product_id,
                x.name
            }).ToList();
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCategories()
        {
            var data = db.tb_categories.Select(x => new
            {
                x.category_id,
                x.name
            }).ToList();
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetBrands()
        {
            var data = db.tb_brands.Select(x => new
            {
                x.brand_id,
                x.name
            }).ToList();
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetDiscountsByProduct(int productId)
        {
            var discounts = db.tb_product_discounts
             .Where(d => d.product_id == productId)
             .Join(db.tb_products,
                 pd => pd.product_id,
                 p => p.product_id,
                 (pd, p) => new
                 {
                     pd.product_discount_id,
                     pd.name,
                     pd.amount,
                     pd.is_fixed,
                     pd.start_date,
                     pd.end_date,
                     original_price = p.price,
                     product_id = p.product_id
                 })
             .OrderByDescending(d => d.start_date)
             .ToList();
            return Json(new { success = true, data = discounts }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Add()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Insert(tb_product model, List<string> images, int? rdefault)
        {
            try
            {
                // Kiểm tra ModelState
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return Json(new { success = false, message = "Dữ liệu không hợp lệ: " + string.Join("; ", errors) });
                }

                if (images != null && images.Count > 0)
                {
                    if (rdefault.HasValue && rdefault.Value > 0 && rdefault.Value <= images.Count)
                    {
                        model.image = images[rdefault.Value - 1];
                    }
                    else
                    {
                
                        model.image = images[0];
                    }
                }

                model.create_at = DateTime.Now;
                db.tb_products.InsertOnSubmit(model);
                db.SubmitChanges(); 
                if (images != null && images.Count > 0)
                {
                    foreach (var img in images)
                    {
                        var productImage = new tb_product_image
                        {
                            product_id = model.product_id,
                            image = img
                        };
                        db.tb_product_images.InsertOnSubmit(productImage);
                    }
                    db.SubmitChanges();
                }

                return Json(new { success = true, message = "Thêm sản phẩm thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message + (ex.InnerException != null ? " | " + ex.InnerException.Message : "") });
            }
        }
    }
}