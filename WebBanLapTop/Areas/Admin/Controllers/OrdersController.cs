using PagedList;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanLapTop.Models;

namespace WebBanLapTop.Areas.Admin.Controllers
{
    public class OrdersController : Controller
    {
        // GET: Admin/Orders
        DatabaseDataContext db = new DatabaseDataContext();
        public ActionResult Index(int? page, string search)
        {
            var data = db.vw_OrderLists.AsQueryable();
            int pageSize = 7;
            int pageNumber = page ?? 1;
            if (!string.IsNullOrEmpty(search))
            {
                search = search.Trim().ToLower();
                data = data.Where(x => x.ProductName.ToLower().Contains(search) || x.UserName.ToLower().Contains(search));
            }
            var paged = data.OrderByDescending(x => x.order_id)
                            .ToPagedList(pageNumber, pageSize);
            ViewBag.PageSize = pageSize;
            ViewBag.Page = pageNumber;
            ViewBag.Search = search;

            return View(paged);
        }
        public ActionResult Image(int id)
        {
            var data = db.vw_OrderLists.FirstOrDefault(o => o.order_id == id);
            return View(data);
        }
        [HttpPost]
        public JsonResult Delete(int id)
        {
            DatabaseDataContext db = new DatabaseDataContext();
            try
            {
                var order = db.tb_orders.FirstOrDefault(x => x.order_id == id);
                if (order == null)
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng!" });
                var orderDetail = db.tb_order_details.Where(x => x.order_id == id);
                if (orderDetail.Any())
                {
                    db.tb_order_details.DeleteAllOnSubmit(orderDetail);
                }
                db.tb_orders.DeleteOnSubmit(order);
                db.SubmitChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        public JsonResult FilterByDate(string fromDate, string toDate)
        {
            try
            {
                var data = GetFiltereOrder(fromDate, toDate)
                            .OrderByDescending(x => x.order_id)
                            .ToList();
                return Json(new
                {
                    success = true,
                    data = data.Select(x => new
                    {
                        x.order_id,
                        x.UserName,
                        x.ProductName,
                        x.ProductImage,
                        total_amount = x.total_amount.HasValue ? x.total_amount.Value.ToString("N0") : "0",
                        order_date = x.order_date.HasValue ? x.order_date.Value.ToString("dd/MM/yyyy") : "",
                        x.status
                    }),
                    totalRecords = data.Count
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        private IQueryable<vw_OrderList> GetFiltereOrder(string fromDate, string toDate)
        {
            var data = db.vw_OrderLists.AsQueryable();
            if (!string.IsNullOrEmpty(fromDate))
            {
                if (DateTime.TryParseExact(fromDate, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out DateTime from))
                {
                    data = data.Where(x => x.order_date >= from);
                }
            }
            if (!string.IsNullOrEmpty(toDate))
            {
                if (DateTime.TryParseExact(toDate, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out DateTime to))
                {
                    to = to.AddDays(1); // Bao gồm cả ngày kết thúc
                    data = data.Where(x => x.order_date < to);
                }
            }
            return data;
        }
    }
}