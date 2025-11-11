using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanLapTop.Models;

namespace WebBanLapTop.Areas.Admin.Controllers
{
    public class ThongkeController : Controller
    {
        // GET: Admin/Thongke

        public ActionResult Index()
        {
            return View();
        }
        public JsonResult Getdoanhthu()
        {               
            {
                // Lấy năm hiện tại
                int currentYear = DateTime.Now.Year;
                DatabaseDataContext db = new DatabaseDataContext();
                var doanhThus = db.tb_orders
                    .Where(o => o.status == "Hoàn tất"
                           && o.order_date.HasValue
                           && o.order_date.Value.Year == currentYear)  // Thêm filter năm
                    .GroupBy(o => o.order_date.Value.Month)
                    .Select(g => new
                    {
                        Thang = g.Key,
                        SoTien = g.Sum(o => o.total_amount ?? 0)
                    })
                    .OrderBy(x => x.Thang)
                    .ToList();

                // Đảm bảo có đủ 12 tháng
                var allMonths = Enumerable.Range(1, 12)
                    .Select(month => new
                    {
                        Thang = month,
                        SoTien = doanhThus.FirstOrDefault(d => d.Thang == month)?.SoTien ?? 0
                    })
                    .ToList();

                return Json(allMonths, JsonRequestBehavior.AllowGet);
            }
        }
        //public JsonResult Getdoanhthu()
        //{
        //    int currentYear = DateTime.Now.Year;
        //    int currentMonth = DateTime.Now.Month;

        //    DatabaseDataContext db = new DatabaseDataContext();

        //    var doanhThus = db.tb_orders
        //        .Where(o => o.status == "Hoàn tất"
        //               && o.order_date.HasValue
        //               && o.order_date.Value.Year == currentYear
        //               && o.order_date.Value.Month <= currentMonth) // Chỉ lấy tới tháng hiện tại
        //        .GroupBy(o => o.order_date.Value.Month)
        //        .Select(g => new
        //        {
        //            Thang = g.Key,
        //            SoTien = g.Sum(o => o.total_amount ?? 0)
        //        })
        //        .OrderBy(x => x.Thang)
        //        .ToList();

        //    return Json(doanhThus, JsonRequestBehavior.AllowGet);
        //}
        public JsonResult GetDoanhThuTheoHang()
        {
            DatabaseDataContext db = new DatabaseDataContext();
            try
            {
                var data = db.tb_brand_statistics
                    .Where(bs => bs.total_revenue > 0)
                    .Select(bs => new
                    {
                        TenHang = bs.brand_name,
                        DoanhThu = bs.total_revenue
                    })
                    .OrderByDescending(x => x.DoanhThu)
                    .ToList();

                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetSoLuongBanRa()
        {
            DatabaseDataContext db = new DatabaseDataContext();
            try
            {
                var data = db.tb_brand_statistics
                    .Where(bs => bs.quantity_sold > 0)
                    .Select(bs => new
                    {
                        TenHang = bs.brand_name,
                        SoLuong = bs.quantity_sold
                    })
                    .OrderByDescending(x => x.SoLuong)
                    .ToList();

                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}