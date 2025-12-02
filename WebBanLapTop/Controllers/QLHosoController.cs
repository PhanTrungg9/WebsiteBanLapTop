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
            var detailList = db.vw_OrderDetailList
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





    }



}