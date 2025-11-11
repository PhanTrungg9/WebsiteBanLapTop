using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanLapTop.Models;
using Newtonsoft.Json;

namespace WebBanLapTop.Areas.Admin.Controllers
{
    public class QLDMSPController : Controller
    {
        // GET: Admin/QLDMSP
        public ActionResult DanhMucSanPham()
        {
            DatabaseDataContext db = new DatabaseDataContext();
            var items = db.tb_categories;
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
        public string Delete()
        {
            string rs = "";
            string categoryIdStr = Request["id"];

            DatabaseDataContext db = new DatabaseDataContext();

            // kiểm tra dữ liệu đầu vào (chỉ kiểm tra, không return)
            if (!string.IsNullOrEmpty(categoryIdStr))
            {
                int categoryId;
                if (int.TryParse(categoryIdStr, out categoryId))
                {
                    // tìm thương hiệu cần xóa
                    var category_obj = db.tb_categories.SingleOrDefault(o => o.category_id == categoryId);

                    if (category_obj != null)
                    {
                        // XÓA CỨNG bản ghi khỏi database
                        db.tb_categories.DeleteOnSubmit(category_obj);

                        try
                        {
                            db.SubmitChanges();
                            rs = "Xóa danh mục thành công!";
                        }
                        catch (Exception ex)
                        {
                            rs = "Lỗi khi xóa danh mục: " + ex.Message;
                        }
                    }
                    else
                    {
                        rs = "Không tìm thấy danh mục cần xóa!";
                    }
                }
                else
                {
                    rs = "Mã danh mục không hợp lệ!";
                }
            }
            else
            {
                rs = "Thiếu mã danh mục!";
            }

            return rs;
        }
    }
}