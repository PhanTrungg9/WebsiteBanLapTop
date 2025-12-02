using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using WebBanLapTop.Models;

namespace WebBanLapTop.Areas.Admin.Controllers
{
    public class QLDMSPController : Controller
    {
        // GET: Admin/QLDMSP

        public ActionResult DanhMucSanPham(int page = 1, int pageSize = 5)
        {
            DatabaseDataContext db = new DatabaseDataContext();
            var items = db.tb_categories
                .OrderBy(c => c.category_id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            int totalCategories = db.tb_categories.Count();
            int totalPages = (int)Math.Ceiling((double)totalCategories / pageSize);

            ViewBag.Page = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.PageSize = pageSize;

            return View(items);
        }

        public ActionResult Them()
        {
            return View();
        }

        [HttpPost]
        public string Insert()
        {
            string rs = "";

            // Lấy dữ liệu từ form
            string categoryid_str = Request["txt_categoryid"];
            string name_str = Request["txt_name"];
            string description_str = Request["txt_description"];

            // Kiểm tra dữ liệu rỗng
            if (string.IsNullOrEmpty(categoryid_str))
            {
                return "Vui lòng nhập mã danh mục (category_id)!";
            }

            if (string.IsNullOrEmpty(name_str))
            {
                return "Vui lòng nhập tên danh mục!";
            }

            // Chuyển brand_id sang int
            int categoryid;
            if (!int.TryParse(categoryid_str, out categoryid))
            {
                return "Giá trị mã danh mục (category_id) phải là số!";
            }

            // Kết nối database
            DatabaseDataContext db = new DatabaseDataContext();

            // Kiểm tra trùng ID
            var category_qr = db.tb_categories.FirstOrDefault(o => o.category_id == categoryid);
            if (category_qr != null)
            {
                rs = "Đã tồn tại danh mục có mã = " + categoryid;
            }
            else
            {
                // Tạo đối tượng mới
                tb_category dm_obj = new tb_category
                {
                    category_id = categoryid,
                    name = name_str,
                    description = description_str
                };

                // Thêm vào database
                db.tb_categories.InsertOnSubmit(dm_obj);
                db.SubmitChanges();

                rs = "Thêm mới danh mục thành công thành công!";
            }

            return rs;
        }

        public ActionResult Sua(int id)
        {
            DatabaseDataContext db = new DatabaseDataContext();

            var category = db.tb_categories.SingleOrDefault(b => b.category_id == id);
            if (category == null)
            {
                return HttpNotFound("Không tìm thấy thương hiệu có mã = " + id);
            }

            return View(category); // truyền đối tượng sang view
        }


        [HttpGet]
        public string LayTTDM(int id)
        {
            DatabaseDataContext db = new DatabaseDataContext();
            var category = db.tb_categories.SingleOrDefault(b => b.category_id == id);

            if (category == null)
                return "Không tìm thấy danh mục có mã = " + id;

            var json = new
            {
                category_id = category.category_id,
                name = category.name,
                description = category.description
            };
            return JsonConvert.SerializeObject(json);
        }

        [HttpPost]
        public JsonResult CapNhat(int category_id, string name, string description)
        {
            try
            {
                DatabaseDataContext db = new DatabaseDataContext();
                var category = db.tb_categories.SingleOrDefault(b => b.category_id == category_id);
                if (category == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy danh mục cần cập nhật!" });
                }

                category.name = name;
                category.description = description;
                db.SubmitChanges();

                return Json(new { success = true, message = "Cập nhật thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        [HttpPost]
        public ActionResult Toggle(int id)
        {
            DatabaseDataContext db = new DatabaseDataContext();
            var item = db.tb_categories.SingleOrDefault(c => c.category_id == id);
            if (item == null)
            {
                return Content("Không tìm thấy danh mục!");
            }

            // Đảo trạng thái
            item.is_active = !item.is_active;
            db.SubmitChanges();

            return Content(item.is_active ? "Đã hiển thị danh mục!" : "Đã ẩn danh mục!");
        }
    }
}