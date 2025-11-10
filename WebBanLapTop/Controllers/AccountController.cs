using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
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
                return rs = "Chúc mừng bạn đã đăng kí tài khoản thành công ! Hãy dăng nhập ngay nào . ";
            }
            catch (Exception ex)
            {
                rs = "Lỗi" + ex.Message;
            }
            return rs;
        }
    }
}