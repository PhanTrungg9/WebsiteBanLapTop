using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebBanLapTop.Models
{
    public static class CartHelper
    {
        // GET: CartHelper

        public static int GetCartItemCount()
        {
            using (DatabaseDataContext db = new DatabaseDataContext())
            {
                var session = HttpContext.Current.Session;
                if (session["UserID"] == null)
                {
                    return 0;
                }
                int userId = (int)session["UserID"];
                var cart = db.tb_carts.FirstOrDefault(c => c.user_id == userId);
                if (cart == null)
                {
                    return 0;
                }
                return db.tb_cart_items
                .Where(ci => ci.cart_id == cart.cart_id)
                .Sum(ci => (int?)ci.quantity).GetValueOrDefault();
            }
        }
    }
}