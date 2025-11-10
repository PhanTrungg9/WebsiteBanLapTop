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

         public ActionResult Index(int? page, string search)
         {
            int pageSize = 5;
            int pageNumber = page ?? 1;
            var query = db.vw_ProductWithDiscountCounts
                          .Where(x => x.is_delete == false || x.is_delete == null);
            //tim kiem
            if (!string.IsNullOrEmpty(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(x => x.name.ToLower().Contains(search));
            }
            var disProducts = query.GroupBy(x => x.product_id).Select(g => g.FirstOrDefault());
            var paged = disProducts.OrderByDescending(x => x.product_id)
                            .ToPagedList(pageNumber, pageSize);
            ViewBag.PageSize = pageSize;
            ViewBag.Page = pageNumber;
            ViewBag.Search = search;

            if (Request.IsAjaxRequest())
            {
                return PartialView("_ProductListPartial", paged);
            }
            return View(paged);
        }
        public JsonResult getProductName()
        {
            var data = db.tb_products.Where(x => x.is_delete == false || x.is_delete == null).Select(x => new
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
                model.is_delete = false;
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
        public ActionResult Edit() { 

            return View();
        }
        [HttpPost]
        public JsonResult LayTTSP(int id)
        {
            try
            {
                var sp = db.tb_products.SingleOrDefault(x => x.product_id == id);
                if (sp != null)
                {
                    var spdata = new
                    {
                        product_id = sp.product_id,
                        name = sp.name,
                        category_id = sp.category_id,
                        brand_id = sp.brand_id,
                        price = sp.price,
                        description = sp.description,
                        quantity = sp.quantity,
                        image = sp.image,
                        create_at = sp.create_at,
                        is_delete = sp.is_delete
                    };
                    return Json(new { success = true, data = spdata }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, message = "Không tìm thấy sản phẩm." }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public JsonResult Update(tb_product model)
        {
            try
            {
                int id = int.Parse(Request["product_id"]);
                var sp = db.tb_products.FirstOrDefault(x => x.product_id == id);
                if (sp != null)
                {
                    sp.name = model.name;
                    sp.category_id = model.category_id;
                    sp.brand_id = model.brand_id;
                    sp.price = model.price;
                    sp.description = model.description;
                    sp.quantity = model.quantity;
                    db.SubmitChanges();
                    return Json(new { success = true, message = "Cập nhật sản phẩm thành công!" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, message = "Không tìm thấy sản phẩm." }, JsonRequestBehavior.AllowGet);
                }
               
            }
            catch(Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                var sp = db.tb_products.FirstOrDefault(x => x.product_id == id);
                var discount = db.tb_product_discounts.Where(x => x.product_id == id);
                var images = db.tb_product_images.Where(x => x.product_id == id).ToList();
                if (sp != null)
                {
                    sp.is_delete = true;
                    db.tb_product_discounts.DeleteAllOnSubmit(discount);
                    db.tb_product_images.DeleteAllOnSubmit(images);
                    db.SubmitChanges();
                    return Json(new { success = true, message = "Xóa sản phẩm thành công!" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, message = "Không tìm thấy sản phẩm." }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

    }
}