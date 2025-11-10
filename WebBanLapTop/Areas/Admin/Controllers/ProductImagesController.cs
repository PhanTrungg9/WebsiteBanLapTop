using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using WebBanLapTop.Models;

namespace WebBanLapTop.Areas.Admin.Controllers
{
    public class ProductImagesController : Controller
    {
        // GET: Admin/ProductImages
        DatabaseDataContext db = new DatabaseDataContext();
        public ActionResult Index(int product_id)
        {
            var item = db.tb_product_images.Where(x => x.product_id == product_id).ToList();
            var product = db.tb_products.FirstOrDefault(x => x.product_id == product_id);
            ViewBag.product_id = product_id;
            ViewBag.DefaultImage = product.image;
            return View(item);
        }
        [HttpPost]
        public JsonResult AddImages(int product_id, string url)
        {
            try
            {
                var image = new tb_product_image
                {
                    product_id = product_id, 
                    image = url
                };

                db.tb_product_images.InsertOnSubmit(image);
                db.SubmitChanges();

                return Json(new { success = true, message = "Thêm ảnh thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [HttpPost]
        public JsonResult Delete(int product_id)
        {
            try
            {
                var sp = db.tb_product_images.FirstOrDefault(x =>x.product_image_id == product_id);
                if (sp != null)
                {
                    db.tb_product_images.DeleteOnSubmit(sp);
                    db.SubmitChanges();
                    return Json(new { success = true, message = "Xóa hình ảnh  thành công!" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, message = "Không tìm thấy hình ảnh." }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public JsonResult DeleteAll(int product_id)
        {
            try
            {
                var images = db.tb_product_images.Where(x =>x.product_id == product_id).ToList();
                if (images.Any())
                {
                    db.tb_product_images.DeleteAllOnSubmit(images);
                    db.SubmitChanges();
                    return Json(new { success = true, message = "Đã xóa tất cả ảnh của sản phẩm!"});
                }
                else
                {
                    return Json(new { success = false, message = "Không có ảnh nào để xóa." });
                }
            }
            catch(Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }
        [HttpPost]
        public JsonResult SetDefault(int product_id, int product_image_id)
        {
            try
            {
                var selectedImage = db.tb_product_images.FirstOrDefault(x => x.product_image_id == product_image_id);
                if (selectedImage == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy ảnh!" });
                }
                var product = db.tb_products.FirstOrDefault(x => x.product_id == product_id);
                if (product == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy sản phẩm!" });
                }
                product.image = selectedImage.image;

                db.SubmitChanges();

                return Json(new { success = true, message = "Đã đặt ảnh mặc định thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

    }
}