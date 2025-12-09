using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanLapTop.Models; 

namespace WebBanLapTop.Controllers
{
    public class QLHosoController : Controller
    {
        // GET: QLHoso
        private DatabaseDataContext db = new DatabaseDataContext();

        public ActionResult Index()
        {
            if (Session["UserID"] == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int userId = (int)Session["UserID"];   // ← lấy đúng key

            var user = db.tb_users.FirstOrDefault(u => u.user_id == userId);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View(user);
        }

        [HttpPost]
        public JsonResult Update(int user_id, string full_name, string email, string phone, string password, string address)
        {
            var user = db.tb_users.FirstOrDefault(u => u.user_id == user_id);
            if (user == null)
            {
                return Json(new { success = false, message = "Không tìm thấy tài khoản!" });
            }

            // cập nhật
            user.full_name = full_name;
            user.email = email;
            user.phone = phone;
            user.password = password;
            user.address = address;
            db.SubmitChanges();

            return Json(new
            {
                success = true,
                message = "Cập nhật thành công!",
                data = new
                {
                    full_name,
                    email,
                    phone,
                    password,
                    address
                }
            });
        }
        public ActionResult GetOrderHistory() 
        {
            int userId = (int)Session["UserID"];
            var orders = db.tb_orders
                .Where(o => o.user_id == userId)
                .OrderByDescending(o => o.order_id)
                .AsEnumerable()
                .Select((o) => new
                {
                    o.order_id,
                    order_date_str = o.order_date.HasValue
                          ?o.order_date.Value.ToString("yyyy-MM-dd")
                          : " ",
                    o.status,
                })
                .ToList();
            return Json(new { success = true, data = orders }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Xemchitiet(int id)
        {
            // Lấy tất cả chi tiết sản phẩm của order_id
            //var detailList = db.vw_OrderDetailList
            //                   .Where(x => x.order_id == id)
            //                   .ToList();
            var detailList = db.vw_OrderDetailLists
                               .Where(x => x.order_id == id)
                               .ToList();

            if (!detailList.Any())
            {
                return HttpNotFound("Không tìm thấy đơn hàng");
            }

            // Lấy tổng tiền từ dòng đầu (vì view của bạn đã trả về total_amount trùng nhau)
            var totalAmount = detailList.First().total_amount;

            ViewBag.TotalAmount = totalAmount;

            return View(detailList);
        }
       

        /// <summary>
        /// Sửa review
        /// </summary>
        //[HttpPost]
        //public JsonResult EditReview(int reviewId, string comment)
        //{
        //    try
        //    {
        //        // Kiểm tra đăng nhập
        //        if (Session["UserID"] == null)
        //        {
        //            return Json(new { success = false, message = "Vui lòng đăng nhập!" });
        //        }

        //        int userId = (int)Session["UserID"];

        //        // Validate comment
        //        if (string.IsNullOrWhiteSpace(comment))
        //        {
        //            return Json(new { success = false, message = "Nội dung đánh giá không được để trống!" });
        //        }

        //        // Tìm review
        //        var review = db.tb_reviews.FirstOrDefault(r => r.review_id == reviewId);

        //        if (review == null)
        //        {
        //            return Json(new { success = false, message = "Không tìm thấy đánh giá!" });
        //        }

        //        // Kiểm tra quyền sở hữu
        //        if (review.user_id != userId)
        //        {
        //            return Json(new { success = false, message = "Bạn không có quyền chỉnh sửa đánh giá này!" });
        //        }

        //        // Cập nhật
        //        review.comment = comment.Trim();
        //        review.created_at = DateTime.Now; // Có thể thêm trường updated_at nếu muốn

        //        db.SubmitChanges();

        //        return Json(new
        //        {
        //            success = true,
        //            message = "Cập nhật đánh giá thành công!",
        //            data = new
        //            {
        //                review_id = review.review_id,
        //                comment = review.comment,
        //                created_at = review.created_at.ToString("dd/MM/yyyy HH:mm")
        //            }
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
        //    }
        //}
        //[HttpPost]
        //public JsonResult DeleteReview(int reviewId)
        //{
        //    try
        //    {
        //        // Kiểm tra đăng nhập
        //        if (Session["UserID"] == null)
        //        {
        //            return Json(new { success = false, message = "Vui lòng đăng nhập!" });
        //        }

        //        int userId = (int)Session["UserID"];

        //        // Tìm review
        //        var review = db.tb_reviews.FirstOrDefault(r => r.review_id == reviewId);

        //        if (review == null)
        //        {
        //            return Json(new { success = false, message = "Không tìm thấy đánh giá!" });
        //        }

        //        // Kiểm tra quyền sở hữu
        //        if (review.user_id != userId)
        //        {
        //            return Json(new { success = false, message = "Bạn không có quyền xóa đánh giá này!" });
        //        }

        //        // Xóa
        //        db.tb_reviews.DeleteOnSubmit(review);
        //        db.SubmitChanges();

        //        return Json(new
        //        {
        //            success = true,
        //            message = "Xóa đánh giá thành công!"
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = "Có lỗi xảy ra: " + ex.Message });
        //    }
    }
}