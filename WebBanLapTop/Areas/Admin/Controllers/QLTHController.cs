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
    public class QLTHController : Controller
    {
        // GET: Admin/QLTH

        public ActionResult DanhSachThuongHieu(int page = 1, int pageSize = 5)
        {
            DatabaseDataContext db = new DatabaseDataContext();
            var items = db.tb_brands
                .OrderBy(c => c.brand_id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            int totalBrands = db.tb_brands.Count();
            int totalPages = (int)Math.Ceiling((double)totalBrands / pageSize);

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
            string brandid_str = Request["txt_brandid"];
            string name_str = Request["txt_name"];
            string description_str = Request["txt_description"];

            // Kiểm tra dữ liệu rỗng
            if (string.IsNullOrEmpty(brandid_str))
            {
                return "Vui lòng nhập mã thương hiệu (brand_id)!";
            }

            if (string.IsNullOrEmpty(name_str))
            {
                return "Vui lòng nhập tên thương hiệu!";
            }

            // Chuyển brand_id sang int
            int brandid;
            if (!int.TryParse(brandid_str, out brandid))
            {
                return "Giá trị mã thương hiệu (brand_id) phải là số!";
            }

            // Kết nối database
            DatabaseDataContext db = new DatabaseDataContext();

            // Kiểm tra trùng ID
            var brand_qr = db.tb_brands.FirstOrDefault(o => o.brand_id == brandid);
            if (brand_qr != null)
            {
                rs = "Đã tồn tại thương hiệu có mã = " + brandid;
            }
            else
            {
                // Tạo đối tượng mới
                tb_brand th_obj = new tb_brand
                {
                    brand_id = brandid,
                    name = name_str,
                    description = description_str
                };

                // Thêm vào database
                db.tb_brands.InsertOnSubmit(th_obj);
                db.SubmitChanges();

                rs = "Thêm mới thương hiệu thành công!";
            }

            return rs;
        }

        public ActionResult Sua(int id)
        {
            DatabaseDataContext db = new DatabaseDataContext();

            var brand = db.tb_brands.SingleOrDefault(b => b.brand_id == id);
            if (brand == null)
            {
                return HttpNotFound("Không tìm thấy thương hiệu có mã = " + id);
            }

            return View(brand); // truyền đối tượng sang view
        }


        [HttpGet]
        public string LayTTTH(int id)
        {
            DatabaseDataContext db = new DatabaseDataContext();
            var brand = db.tb_brands.SingleOrDefault(b => b.brand_id == id);

            if (brand == null)
                return "Không tìm thấy thương hiệu có mã = " + id;

            var json = new
            {
                brand_id = brand.brand_id,
                name = brand.name,
                description = brand.description
            };
            return JsonConvert.SerializeObject(json);
        }

        [HttpPost]
        public JsonResult CapNhat(int brand_id, string name, string description)
        {
            try
            {
                DatabaseDataContext db = new DatabaseDataContext();
                var brand = db.tb_brands.SingleOrDefault(b => b.brand_id == brand_id);
                if (brand == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thương hiệu cần cập nhật!" });
                }

                brand.name = name;
                brand.description = description;
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
            string brandIdStr = Request["id"];

            DatabaseDataContext db = new DatabaseDataContext();

            // kiểm tra dữ liệu đầu vào (chỉ kiểm tra, không return)
            if (!string.IsNullOrEmpty(brandIdStr))
            {
                int brandId;
                if (int.TryParse(brandIdStr, out brandId))
                {
                    // tìm thương hiệu cần xóa
                    var brand_obj = db.tb_brands.SingleOrDefault(o => o.brand_id == brandId);

                    if (brand_obj != null)
                    {
                        // XÓA CỨNG bản ghi khỏi database
                        db.tb_brands.DeleteOnSubmit(brand_obj);

                        try
                        {
                            db.SubmitChanges();
                            rs = "Xóa thương hiệu thành công!";
                        }
                        catch (Exception ex)
                        {
                            rs = "Lỗi khi xóa thương hiệu: " + ex.Message;
                        }
                    }
                    else
                    {
                        rs = "Không tìm thấy thương hiệu cần xóa!";
                    }
                }
                else
                {
                    rs = "mã thương hiệu không hợp lệ!";
                }
            }
            else
            {
                rs = "Thiếu mã thương hiệu!";
            }

            return rs;
        }

    }
}

