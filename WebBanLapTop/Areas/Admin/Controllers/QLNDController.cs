using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using WebBanLapTop.Models;

namespace WebBanLapTop.Areas.Admin.Controllers
{
    public class QLNDController : Controller
    {
        // GET: Admin/User

        public ActionResult DanhSachNguoiDung()
        {
            DatabaseDataContext db = new DatabaseDataContext();
            var users = db.tb_users;
            return View(users);
        }

        [HttpPost]
        public JsonResult Delete(int id)
        {
            DatabaseDataContext db = new DatabaseDataContext();
            try
            {
                var user = db.tb_users.FirstOrDefault(x => x.user_id == id);
                if (user == null)
                    return Json(new { success = false, message = "Không tìm thấy người dùng!" });

                db.tb_users.DeleteOnSubmit(user);
                db.SubmitChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult DeleteMulti(int[] ids)
        {
            DatabaseDataContext db = new DatabaseDataContext();
            try
            {
                var users = db.tb_users.Where(x => ids.Contains(x.user_id)).ToList();
                if (!users.Any())
                    return Json(new { success = false, message = "Không tìm thấy bản ghi nào!" });

                db.tb_users.DeleteAllOnSubmit(users);
                db.SubmitChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public ActionResult SearchUser(string keyword)
        {
            DatabaseDataContext db = new DatabaseDataContext();

            var users = db.tb_users
                .Where(u =>
                    string.IsNullOrEmpty(keyword) ||
                    u.full_name.Contains(keyword))
                //u.email.Contains(keyword) ||
                //u.phone.Contains(keyword))
                .ToList();

            // Nếu không có kết quả
            if (users == null || !users.Any())
            {
                return Content("<tr><td colspan='10' class='text-center text-danger'>Không tìm thấy người dùng nào</td></tr>");
            }

            int stt = 1;
            string html = "";

            foreach (var user in users)
            {
                html += $@"
        <tr id='trow_{user.user_id}'>
            <td><input type='checkbox' class='checkuser' value='{user.user_id}' /></td>
            <td>{stt}</td>
            <td>{user.full_name}</td>
            <td>{user.user_name}</td>
            <td>{user.password}</td>
            <td>{user.email}</td>
            <td>{user.phone}</td>
            <td>{user.address}</td>

            <!-- Toggle tròn: khóa / mở khóa -->
            <td class='text-center'>
                <label class='toggle-round'>
                    <input type='checkbox' class='toggle-active' data-id='{user.user_id}' {(user.is_active == true ? "checked" : "")} />
                    <span class='slider'></span>
                </label>
            </td>

            <!-- Toggle vuông: phân quyền -->
            <td class='text-center'>
                <label class='toggle-square'>
                    <input type='checkbox' class='toggle-role' data-id='{user.user_id}' {(user.usertype == true ? "checked" : "")} />
                    <span class='slider'></span>
                </label>
            </td>
        </tr>";
                stt++;
            }

            return Content(html);
        }

        [HttpPost]
        public JsonResult ToggleActive(int id, bool isActive)
        {
            DatabaseDataContext db = new DatabaseDataContext();
            try
            {
                var user = db.tb_users.FirstOrDefault(u => u.user_id == id);
                if (user == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy người dùng!" });
                }

                user.is_active = isActive;
                db.SubmitChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi cập nhật trạng thái: " + ex.Message });
            }
        }


        [HttpPost]
        public JsonResult ToggleRole(int id, bool isAdmin)
        {
            DatabaseDataContext db = new DatabaseDataContext();
            try
            {
                var user = db.tb_users.FirstOrDefault(u => u.user_id == id);
                if (user == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy người dùng!" });
                }

                // usertype = true → Admin, false → User
                user.usertype = isAdmin;
                db.SubmitChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi cập nhật quyền: " + ex.Message });
            }
        }
    }
}