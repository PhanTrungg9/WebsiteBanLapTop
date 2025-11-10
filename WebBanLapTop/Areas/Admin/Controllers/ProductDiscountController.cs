using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanLapTop.Models;

namespace WebBanLapTop.Areas.Admin.Controllers
{
    public class ProductDiscountController : Controller
    {
        DatabaseDataContext db = new DatabaseDataContext();
        // GET: Admin/ProductDiscount
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public JsonResult SaveDiscount(int product_id, bool is_fixed, decimal amount, DateTime start_date, DateTime end_date, string name)
        {
            try
            {
               
                if (end_date.Date < DateTime.Now.Date)
                {
                    return Json(new { success = false, message = "Ngày kết thúc không được trong quá khứ" });
                }
                var product = db.tb_products.FirstOrDefault(p => p.product_id == product_id);
                if (product == null)
                {
                    return Json(new { success = false, message = "Sản phẩm không tồn tại" });
                }

                var existingDiscount = db.tb_product_discounts
                    .Where(d => d.product_id == product_id)
                    .Where(d =>
                        (start_date >= d.start_date && start_date <= d.end_date) ||
                        (end_date >= d.start_date && end_date <= d.end_date) ||
                        (start_date <= d.start_date && end_date >= d.end_date)
                    )
                    .FirstOrDefault();

                if (existingDiscount != null)
                {
                    return Json(new { success = false, message = "Đã có đợt giảm giá trong khoảng thời gian này" });
                }

                var discount = new tb_product_discount
                {
                    product_id = product_id,
                    is_fixed = is_fixed,
                    amount = amount,
                    start_date = start_date,
                    end_date = end_date,
                    name = string.IsNullOrWhiteSpace(name) ? $"Giảm giá {(is_fixed ? amount.ToString("N0") + "₫" : amount + "%")}" : name,
                };

                db.tb_product_discounts.InsertOnSubmit(discount);
                db.SubmitChanges();

                return Json(new { success = true, message = "Thêm giảm giá thành công", data = new { id = discount.product_discount_id } });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }
        [HttpPost]
        public JsonResult LayTTDiscount(int id)
        {
            try
            {
               
                var discount = db.tb_product_discounts.Where(d => d.product_discount_id == id)
                    .Select(d => new {
                        product_discount_id = d.product_discount_id,
                        product_id = d.product_id,
                        product_name = d.tb_product != null ? d.tb_product.name : "",
                        is_fixed = d.is_fixed,
                        amount = d.amount,
                        start_date = d.start_date,
                        end_date = d.end_date,
                        name = d.name
                    })
                    .FirstOrDefault();

                if (discount != null)
                {
                    return Json(new
                    {
                        success = true,
                        data = discount
                    }, JsonRequestBehavior.AllowGet);
                }

                return Json(new
                {
                    success = false,
                    message = "Không tìm thấy giảm giá với ID: " + id
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Lỗi: " + ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public JsonResult UpdateDiscount(int product_discount_id, int product_id, bool is_fixed, decimal amount, DateTime start_date, DateTime end_date, string name)
        {
            try
            {
                // Tìm discount cần update
                var discount = db.tb_product_discounts
                    .Where(d => d.product_discount_id == product_discount_id)
                    .FirstOrDefault();

                if (discount == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy giảm giá cần cập nhật" });
                }

                // Kiểm tra ngày kết thúc
                if (end_date.Date < DateTime.Now.Date)
                {
                    return Json(new { success = false, message = "Ngày kết thúc không được trong quá khứ" });
                }

                // Kiểm tra sản phẩm tồn tại
                var product = db.tb_products.FirstOrDefault(p => p.product_id == product_id);
                if (product == null)
                {
                    return Json(new { success = false, message = "Sản phẩm không tồn tại" });
                }

                // Kiểm tra trùng lặp thời gian (trừ chính nó)
                var existingDiscount = db.tb_product_discounts
                    .Where(d => d.product_id == product_id)
                    .Where(d => d.product_discount_id != product_discount_id) // Loại trừ chính nó
                    .Where(d =>
                        (start_date >= d.start_date && start_date <= d.end_date) ||
                        (end_date >= d.start_date && end_date <= d.end_date) ||
                        (start_date <= d.start_date && end_date >= d.end_date)
                    )
                    .FirstOrDefault();

                if (existingDiscount != null)
                {
                    return Json(new { success = false, message = "Đã có đợt giảm giá khác trong khoảng thời gian này" });
                }

                // Update thông tin
                discount.product_id = product_id;
                discount.is_fixed = is_fixed;
                discount.amount = amount;
                discount.start_date = start_date;
                discount.end_date = end_date;
                discount.name = string.IsNullOrWhiteSpace(name)
                    ? $"Giảm giá {(is_fixed ? amount.ToString("N0") + "₫" : amount + "%")}"
                    : name;

                db.SubmitChanges();

                return Json(new { success = true, message = "Cập nhật giảm giá thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
            }
        }

    }

}