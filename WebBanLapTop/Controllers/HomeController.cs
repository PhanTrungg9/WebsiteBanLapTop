using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanLapTop.Models;


namespace WebBanLapTop.Controllers
{
    public class HomeController : Controller
    {
        DatabaseDataContext db = new DatabaseDataContext();
        public ActionResult Index(string search)
        {  
            var data = db.vv_ProductLists.Where(v => db.tb_products
                            .Any(p => p.product_id == v.product_id && (p.is_delete == false || p.is_delete == null)))
                            .ToList();
            if (!string.IsNullOrEmpty(search))
            {
                search = search.Trim().ToLower();
                data = data.Where(p => p.product_name.ToLower().Contains(search)
                                     || p.category_name.ToLower().Contains(search)).ToList();
            }
            ViewBag.search = search;
            return View(data);
        }
        [HttpPost]
        public JsonResult Add(int id)
        {
            try
            {
                if (Session["UserID"] == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Vui lòng đăng nhập để thêm sản phẩm vào giỏ hàng.",
                        requireLogin = true
                    }, JsonRequestBehavior.AllowGet);
                }

                int userId = Convert.ToInt32(Session["UserID"]);

                var cart = db.tb_carts.FirstOrDefault(c => c.user_id == userId);
                if (cart == null)
                {
                    cart = new tb_cart
                    {
                        user_id = userId,
                        created_at = DateTime.Now
                    };
                    db.tb_carts.InsertOnSubmit(cart);
                    db.SubmitChanges();
                }

                var cartItem = db.tb_cart_items
                                 .FirstOrDefault(ci => ci.cart_id == cart.cart_id && ci.product_id == id);

                if (cartItem != null)
                {
                    // an toàn với giá trị null
                    cartItem.quantity = (cartItem.quantity ?? 0) + 1;
                }
                else
                {
                    cartItem = new tb_cart_item
                    {
                        cart_id = cart.cart_id,
                        product_id = id,
                        quantity = 1
                    };
                    db.tb_cart_items.InsertOnSubmit(cartItem);
                }

                db.SubmitChanges();

                int totalItems = db.tb_cart_items
                                   .Where(ci => ci.cart_id == cart.cart_id)
                                   .Sum(ci => (int?)ci.quantity) ?? 0;

                return Json(new
                {
                    success = true,
                    message = "Sản phẩm đã được thêm vào giỏ hàng!",
                    totalItems = totalItems,
                    quantityproduct = cartItem.quantity ?? 0
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


    }
}
