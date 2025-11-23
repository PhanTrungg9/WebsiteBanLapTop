using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanLapTop.Models;

namespace WebBanLapTop.Controllers
{
    public class ShoppingCartController : Controller
    {
        // GET: ShoppingCart
        public ActionResult Index(int? page)
        {
            Merge_Cart_Item();
            if (Session["UserID"] == null)
            {
                Session["ReturnUrl"] = "/ShoppingCart/Index";
                TempData["Message"] = "Vui lòng đăng nhập để xem giỏ hàng!";
                return RedirectToAction("Login", "Account");
            }
            int pageSize = 10;
            int pageNumber = (page ?? 1);
            int userId = (int)Session["UserID"];
            // Test không filter theo user_id - LẤY TẤT CẢ
            DatabaseDataContext db = new DatabaseDataContext();
            //var cartItems = db.vw_ShoppingCarts
            //                  .OrderBy(x => x.cart_id)
            //                  .ToPagedList(pageNumber, pageSize);

            //return View(cartItems);
            var cartItems = db.vw_ShoppingCarts
                      .Where(x => x.user_id == userId)
                      .OrderBy(x => x.cart_id)
                      .ToPagedList(pageNumber, pageSize); 
            return View(cartItems);
        }

        [AllowAnonymous]
        public ActionResult ShowCount()
        {
            DatabaseDataContext db = new DatabaseDataContext();

            int userId = (int)Session["UserID"];
            int cartId = db.tb_carts
                   .Where(u => u.user_id == userId)
                   .Select(u => u.cart_id)
                   .FirstOrDefault();
            var items = db.tb_cart_items
            .Where(x => x.cart_id == cartId)
            .ToList();
            var count = 0;
            if (items != null)
            {
                foreach (var item in items)
                {
                    count++;
                }

                return Json(new { Count = count , JsonRequestBehavior.AllowGet });
            }
            return Json(new { Count = 0 }, JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult Delete(int id)
        {
            var code = new { Success = false, msg = "", code = -1};
            DatabaseDataContext db = new DatabaseDataContext();
            var cart = db.tb_cart_items.FirstOrDefault(u => u.cartitem_id == id);
            if (cart != null)
            {
                code = new { Success = true, msg = "", code = 1 };
                db.tb_cart_items.DeleteOnSubmit(cart);
                db.SubmitChanges();
            }
            return Json(code);
        }

        public ActionResult DeleteAll()
        {
            DatabaseDataContext db = new DatabaseDataContext();
            int userId = (int)Session["UserID"];
            int cartId = db.tb_carts
                   .Where(u => u.user_id == userId)
                   .Select(u => u.cart_id)
                   .FirstOrDefault();
            var items = db.tb_cart_items
            .Where(x => x.cart_id == cartId)
            .ToList();
            if( items != null)
            {
                foreach (var item in items)
                {
                    db.tb_cart_items.DeleteOnSubmit(item);
                }

                db.SubmitChanges();
                return Json(new { Success = true });
            }
            return Json(new { Success = false });
        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult Update(int id, int quantity)
        {
            DatabaseDataContext db = new DatabaseDataContext();
            var cart = db.tb_cart_items.FirstOrDefault(u => u.cartitem_id == id);
            if (cart != null)
            {
                if(cart.quantity == quantity)
                {
                    return Json(new { Success = false });
                }
                if(quantity <= 0)
                {
                    Delete(id);
                    return Json(new { Success = true });
                }
                cart.quantity = quantity;
                db.SubmitChanges();
                return Json(new { Success = true });
            }
            return Json(new { Success = false });
        }

        public void Merge_Cart_Item()
        {
            DatabaseDataContext db = new DatabaseDataContext();

            var groups = db.tb_cart_items
                .GroupBy(x => new { x.cart_id, x.product_id })
                .Where(g => g.Count() > 1)
                .ToList();

            foreach (var group in groups)
            {
                // Tổng quantity
                int totalQty = group.Sum(x => x.quantity ?? 0);

                // Lấy dòng đầu tiên
                var first = group.First();
                first.quantity = totalQty;

                // Xóa các dòng còn lại
                foreach (var item in group.Skip(1))
                {
                    db.tb_cart_items.DeleteOnSubmit(item);
                }
            }

            db.SubmitChanges();
        }

    }
}