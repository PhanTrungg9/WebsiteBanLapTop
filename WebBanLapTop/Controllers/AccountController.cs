using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.Services.Description;
using WebBanLapTop.Models;

namespace WebBanLapTop.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Register()
        {
            return View();
        }
        public ActionResult Login()
        {
            return View();
        }

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
            var acc = db.tb_users.FirstOrDefault(u => u.user_name == user && u.password == pass);

            if (acc != null)
            {
                // Lưu thông tin vào Session
                Session["UserID"] = acc.user_id;
                Session["UserName"] = acc.user_name;
                Session["UserType"] = acc.usertype;

                if (acc.is_active == true)
                {
                    // Tạo cookie xác thực
                    FormsAuthentication.SetAuthCookie(acc.user_name, false);
                    // Redirect theo role
                        return Json(new
                        {
                            success = true,
                            message = "Đăng nhập thành công!",
                            redirectUrl = Url.Content("~/Home/Index")
                        });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "Tài khoản của bạn đã bị khóa , vui lòng kiểm tra lại!"
                    });
                }
            }
            else
            {
                return Json(new { success = false, message = "Tài khoản hoặc mật khẩu không đúng!" });
            }
        }
        public string In_Register() {
            string rs = "";
            DatabaseDataContext db = new DatabaseDataContext();
            String full_name = Request["full_name"];
            String email = Request["email"];
            String phone = Request["phone"];
            String password = Request["password"];
            String address = Request["address"];
            String cfpass = Request["confirmPassword"];
            string user = Request["user_name"];
            if(string.IsNullOrEmpty(user) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(cfpass) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(phone) || string.IsNullOrEmpty(address))
            {
                rs = "Vui lòng nhập đầy đủ thông tin";
                return rs;
            }
            if(password != cfpass)
            {
                rs = "Mật khẩu xác nhận không khớp";
                return rs;
            }
            var checkUser = db.tb_users.Where(u => u.user_name == user);
            var checkEmail = db.tb_users.Where(u => u.email == email);
            var checkPhone = db.tb_users.Where(u => u.phone == phone);
            if (checkUser.Any()) { 
                return rs = "Tên đăng nhập đã tồn tại";
            }  
            if (checkEmail.Any())
            {
                return rs = "Email đã được sử dụng";
            }
            if (checkPhone.Any())
            {
                return rs = "SĐT này đã được sử dụng";
            }
            tb_user newUser = new tb_user();
            newUser.full_name = full_name;
            newUser.email = email;
            newUser.phone = phone;
            newUser.password = password;
            newUser.address = address;
            newUser.user_name = user;
            newUser.is_active = true;
            newUser.phone = phone;
            newUser.usertype = false;
            newUser.created_at = DateTime.Now;
            try
            {
                db.tb_users.InsertOnSubmit(newUser);
                db.SubmitChanges();
                // Đăng nhập luôn sau khi đăng ký thành công
                //FormsAuthentication.SetAuthCookie(user, false);
                return rs = "Chúc mừng bạn đã đăng kí tài khoản thành công ! Hãy dăng nhập ngay nào . ";
            }
            catch (Exception ex)
            {
                rs = "Lỗi" + ex.Message;
            }
            return rs;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();
            //AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }
        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }
    }

}