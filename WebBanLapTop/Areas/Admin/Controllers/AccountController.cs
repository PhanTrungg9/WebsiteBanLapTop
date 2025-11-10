using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Security;
using WebBanLapTop.Models;

namespace WebBanLapTop.Areas.Admin.Controllers
{
    public class AccountController : Controller
    {
        // GET: Admin/Account
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public JsonResult Sign_In()
        {
            string user = Request["txt_user"];
            string pass = Request["txt_pass"];

            if (string.IsNullOrEmpty(user))
            {
                return Json(new { success = false, message = "Vui lòng nhập tài khoản của bạn" });
            }

            if (string.IsNullOrEmpty(pass))
            {
                return Json(new { success = false, message = "Vui lòng nhập mật khẩu của bạn" });
            }

            DatabaseDataContext db = new DatabaseDataContext();
            var acc = db.tb_users.FirstOrDefault(u => u.user_name == user && u.password == pass && u.is_active == true);

            if (acc != null)
            {
                // Lưu thông tin vào Session
                Session["UserID"] = acc.user_id;
                Session["UserName"] = acc.user_name;
                Session["UserType"] = acc.usertype;

                // Redirect theo role
                if (acc.usertype == true)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Đăng nhập thành công!",
                        redirectUrl = Url.Action("Index", "Home")
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = true,
                        message = "Đăng nhập thành công!",
                        redirectUrl = Url.Content("~/Home/Index")
                });
                }
            }
            else
            {
                return Json(new { success = false, message = "Tài khoản hoặc mật khẩu không đúng!" });
            }
        }
    }
}
