using PagedList;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing.Printing;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using WebBanHangOnline.Models.Payments;
using WebBanLapTop.Models;

namespace WebBanLapTop.Controllers
{
    public class ShoppingCartController : Controller
    {
        // GET: ShoppingCart
        public ActionResult CheckOut()
        {
            return View();
        }
        public ActionResult CheckOutSuccess()
        {
            return View();
        }
        public ActionResult VnPayReturn()
        {
            if (Request.QueryString.Count > 0)
            {
                string vnp_HashSecret = ConfigurationManager.AppSettings["vnp_HashSecret"]; //Chuoi bi mat
                var vnpayData = Request.QueryString;
                VnPayLibrary vnpay = new VnPayLibrary();

                foreach (string s in vnpayData)
                {
                    //get all querystring data
                    if (!string.IsNullOrEmpty(s) && s.StartsWith("vnp_"))
                    {
                        vnpay.AddResponseData(s, vnpayData[s]);
                    }
                }
                long orderCode = Convert.ToInt64(vnpay.GetResponseData("vnp_TxnRef"));
                long vnpayTranId = Convert.ToInt64(vnpay.GetResponseData("vnp_TransactionNo"));
                string vnp_ResponseCode = vnpay.GetResponseData("vnp_ResponseCode");
                string vnp_TransactionStatus = vnpay.GetResponseData("vnp_TransactionStatus");
                String vnp_SecureHash = Request.QueryString["vnp_SecureHash"];
                String TerminalID = Request.QueryString["vnp_TmnCode"];
                long vnp_Amount = Convert.ToInt64(vnpay.GetResponseData("vnp_Amount")) / 100;
                String bankCode = Request.QueryString["vnp_BankCode"];

                bool checkSignature = vnpay.ValidateSignature(vnp_SecureHash, vnp_HashSecret);
                if (checkSignature)
                {
                    if (vnp_ResponseCode == "00" && vnp_TransactionStatus == "00")
                    {
                        DatabaseDataContext db = new DatabaseDataContext();
                        var itemOrder = db.tb_orders.FirstOrDefault(x => x.order_id == orderCode);
                        if (itemOrder != null)
                        {
                            itemOrder.status = "Hoàn tất";//đã thanh toán
                            db.SubmitChanges();
                            ViewBag.ThanhToanThanhCong = "Số tiền thanh toán (VND):" + vnp_Amount.ToString();
                        }
                        //Thanh toan thanh cong
                        ViewBag.InnerText = "Giao dịch được thực hiện thành công. Cảm ơn quý khách đã sử dụng dịch vụ";
                        //log.InfoFormat("Thanh toan thanh cong, OrderId={0}, VNPAY TranId={1}", orderId, vnpayTranId);
                    }
                    else
                    {
                        //Thanh toan khong thanh cong. Ma loi: vnp_ResponseCode
                        ViewBag.InnerText = "Có lỗi xảy ra trong quá trình xử lý.Mã lỗi: " + vnp_ResponseCode;
                        //log.InfoFormat("Thanh toan loi, OrderId={0}, VNPAY TranId={1},ResponseCode={2}", orderId, vnpayTranId, vnp_ResponseCode);
                    }
                    //displayTmnCode.InnerText = "Mã Website (Terminal ID):" + TerminalID;
                    //displayTxnRef.InnerText = "Mã giao dịch thanh toán:" + orderId.ToString();
                    //displayVnpayTranNo.InnerText = "Mã giao dịch tại VNPAY:" + vnpayTranId.ToString();

                    //displayBankCode.InnerText = "Ngân hàng thanh toán:" + bankCode;
                }
            }
            //var a = UrlPayment(0, "DH3574");
            return View();
        }
        public ActionResult Partial_Checkout()
        {
            using (var db = new DatabaseDataContext())
            {
                if (!Request.IsAuthenticated)
                {
                    return PartialView(null);   // model = null
                }

                var username = User.Identity.Name;

                // LẤY USER
                var user = db.tb_users.FirstOrDefault(x => x.user_name == username);

                // NẾU KHÔNG TÌM THẤY USER → TRẢ VỀ NULL (để View xử lý)
                return PartialView(user);
            }
        }
        public ActionResult Partial_Item_ThanhToan()
        {
            using (var db = new DatabaseDataContext())
            {
                if (!Request.IsAuthenticated)
                {
                    return PartialView(null);   // model = null
                }
                int userId = (int)Session["UserID"];
                int cartId = db.tb_carts
                       .Where(u => u.user_id == userId)
                       .Select(u => u.cart_id)
                       .FirstOrDefault();
                var items = db.vw_ShoppingCarts
                .Where(x => x.cart_id == cartId)
                .ToList();
                return PartialView(items);
            }
        }
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
            var code = new { Success = false, msg = "", code = -1 };
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
                var product = db.tb_products.FirstOrDefault(p => p.product_id == cart.product_id);

                if (product == null)
                {
                    return Json(new { Success = false, Message = "Sản phẩm không tồn tại" });
                }

                if (quantity > product.quantity)
                {
                    return Json(new
                    {
                        Success = false,
                        Message = $"Thật thứ lỗi. Chúng tôi chỉ còn {product.quantity} sản phẩm",
                    });
                }
                else
                {
                    if (cart.quantity == quantity)
                    {
                        return Json(new { Success = false });
                    }
                    if (quantity <= 0)
                    {
                        Delete(id);
                        return Json(new { Success = true });
                    }
                }
                cart.quantity = quantity;
                db.SubmitChanges();
                return Json(new { Success = true });
            }
            return Json(new { Success = false , Message = "Lỗi"});
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
        [HttpPost]
        public ActionResult Payment()
        {
            var code = new { Success = false, Code = -1, Url = "" };
            DatabaseDataContext db = new DatabaseDataContext();
            int userId = (int)Session["UserID"];
            int cartId = db.tb_carts
                   .Where(u => u.user_id == userId)
                   .Select(u => u.cart_id)
                   .FirstOrDefault();
            var items = db.vw_ShoppingCarts
            .Where(x => x.cart_id == cartId)
            .ToList();
            String name = Request["CustomerName"];
            String address = Request["Address"];
            String phone = Request["Phone"];
            String email = Request["Email"];
            String method = Request["TypePayment"];
            int payment = int.Parse(Request["TypePaymentVN"]);
            String status = "";
            var totalPrice = decimal.Zero; ;
            switch (method)
            {
                case "1":
                    method = "Tiền mặt";
                    status = "Đang giao";
                    break;
                case "2":
                    method = "Chuyển khoản";
                    status = "Đang giao";
                    break;
            }
            if (items != null)
            {
                var order = new tb_order();
                order.user_id = userId;
                order.order_date = DateTime.Now;
                order.address = address;
                order.method = method;
                order.status = status;
                foreach (var item in items)
                {
                    var oderDetails = new tb_order_detail();
                    oderDetails.order_id = order.order_id;
                    oderDetails.product_id = item.product_id;
                    oderDetails.quantity = item.Quantity;   
                    oderDetails.price = item.DiscountedPrice;
                    order.tb_order_details.Add(oderDetails);
                    // giảm sl sản phẩm trong kho
                    var product = db.tb_products.FirstOrDefault(p => p.product_id == item.product_id);
                    product.quantity -= item.Quantity;
                    totalPrice += item.TotalAmount ?? decimal.Zero;

                }
                order.total_amount = totalPrice;
                db.tb_orders.InsertOnSubmit(order);
                var cartItemsToDelete = db.tb_cart_items
                .Where(x => x.cart_id == cartId)
                .ToList();
                db.SubmitChanges();
                //send mail cho khachs hang
                var strSanPham = "";
                var thanhtien = decimal.Zero;
                var TongTien = decimal.Zero;
                foreach (var sp in items)
                {
                    strSanPham += "<tr>";
                    strSanPham += "<td>" + sp.ProductName + "</td>";
                    strSanPham += "<td>" + sp.Quantity + "</td>";
                    strSanPham += "<td>" + WebBanLapTop.Common.Common.FormatNumber(sp.DiscountedPrice, 0) + "</td>";
                    strSanPham += "</tr>";
                    thanhtien += sp.TotalAmount ?? decimal.Zero;
                }
                TongTien = thanhtien;
                string contentCustomer = System.IO.File.ReadAllText(Server.MapPath("~/Content/templates/send2.html"));
                contentCustomer = contentCustomer.Replace("{{MaDon}}", order.order_id.ToString());
                contentCustomer = contentCustomer.Replace("{{SanPham}}", strSanPham);
                contentCustomer = contentCustomer.Replace("{{NgayDat}}", DateTime.Now.ToString("dd/MM/yyyy"));
                contentCustomer = contentCustomer.Replace("{{TenKhachHang}}",name);
                contentCustomer = contentCustomer.Replace("{{Phone}}", phone);
                contentCustomer = contentCustomer.Replace("{{Email}}", name);
                contentCustomer = contentCustomer.Replace("{{DiaChiNhanHang}}", address);
                contentCustomer = contentCustomer.Replace("{{PhuongThucThanhToan}}", method);
                //contentCustomer = contentCustomer.Replace("{{TrangThaiGiaoHang}}", status);
                contentCustomer = contentCustomer.Replace("{{ThanhTien}}", WebBanLapTop.Common.Common.FormatNumber(thanhtien, 0));
                contentCustomer = contentCustomer.Replace("{{TongTien}}", WebBanLapTop.Common.Common.FormatNumber(TongTien, 0));
                WebBanLapTop.Common.Common.SendMail("ShopOnline", "Đơn hàng #" + order.order_id.ToString(), contentCustomer.ToString(),email);

                string contentAdmin = System.IO.File.ReadAllText(Server.MapPath("~/Content/templates/send1.html"));
                contentAdmin = contentAdmin.Replace("{{MaDon}}", order.order_id.ToString());
                contentAdmin = contentAdmin.Replace("{{SanPham}}", strSanPham);
                contentAdmin = contentAdmin.Replace("{{NgayDat}}", DateTime.Now.ToString("dd/MM/yyyy"));
                contentAdmin = contentAdmin.Replace("{{TenKhachHang}}", name);
                contentAdmin = contentAdmin.Replace("{{Phone}}", phone);
                contentAdmin = contentAdmin.Replace("{{Email}}", email);
                contentAdmin = contentAdmin.Replace("{{DiaChiNhanHang}}", address);
                contentAdmin = contentAdmin.Replace("{{PhuongThucThanhToan}}", method);
                //contentAdmin = contentAdmin.Replace("{{TrangThaiGiaoHang}}", status);
                contentAdmin = contentAdmin.Replace("{{ThanhTien}}", WebBanLapTop.Common.Common.FormatNumber(thanhtien, 0));
                contentAdmin = contentAdmin.Replace("{{TongTien}}", WebBanLapTop.Common.Common.FormatNumber(TongTien, 0));
                WebBanLapTop.Common.Common.SendMail("ShopOnline", "Đơn hàng mới #" + order.order_id.ToString(), contentAdmin.ToString(), ConfigurationManager.AppSettings["EmailAdmin"]);
                db.tb_cart_items.DeleteAllOnSubmit(cartItemsToDelete);
                db.SubmitChanges();
                code = new { Success = true, Code = 1, Url = "" };
                if (method == "Chuyển khoản")
                {
                    var url = UrlPayment(payment, order.order_id);
                    code = new { Success = true, Code = payment, Url = url };
                }
            }
            return Json(code);
        }
        #region Thanh toán vnpay
        public string UrlPayment(int TypePaymentVN, int orderCode)
        {
            var urlPayment = "";
            DatabaseDataContext db = new DatabaseDataContext();
            var order = db.tb_orders.FirstOrDefault(x => x.order_id == orderCode);
            //Get Config Info
            string vnp_Returnurl = ConfigurationManager.AppSettings["vnp_Returnurl"]; //URL nhan ket qua tra ve 
            string vnp_Url = ConfigurationManager.AppSettings["vnp_Url"]; //URL thanh toan cua VNPAY 
            string vnp_TmnCode = ConfigurationManager.AppSettings["vnp_TmnCode"]; //Ma định danh merchant kết nối (Terminal Id)
            string vnp_HashSecret = ConfigurationManager.AppSettings["vnp_HashSecret"]; //Secret Key

            //Build URL for VNPAY
            VnPayLibrary vnpay = new VnPayLibrary();
            var Price = (long)order.total_amount * 100;
            vnpay.AddRequestData("vnp_Version", VnPayLibrary.VERSION);
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", vnp_TmnCode);
            vnpay.AddRequestData("vnp_Amount", Price.ToString()); //Số tiền thanh toán. Số tiền không mang các ký tự phân tách thập phân, phần nghìn, ký tự tiền tệ. Để gửi số tiền thanh toán là 100,000 VND (một trăm nghìn VNĐ) thì merchant cần nhân thêm 100 lần (khử phần thập phân), sau đó gửi sang VNPAY là: 10000000
            if (TypePaymentVN == 1)
            {
                vnpay.AddRequestData("vnp_BankCode", "VNPAYQR");
            }
            else if (TypePaymentVN == 2)
            {
                vnpay.AddRequestData("vnp_BankCode", "VNBANK");
            }
            else if (TypePaymentVN == 3)
            {
                vnpay.AddRequestData("vnp_BankCode", "INTCARD");
            }

            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", Utils.GetIpAddress());
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", "Thanh toán đơn hàng :" + order.order_id.ToString());
            vnpay.AddRequestData("vnp_OrderType", "other"); //default value: other

            vnpay.AddRequestData("vnp_ReturnUrl", vnp_Returnurl);
            vnpay.AddRequestData("vnp_TxnRef", order.order_id.ToString()); // Mã tham chiếu của giao dịch tại hệ thống của merchant. Mã này là duy nhất dùng để phân biệt các đơn hàng gửi sang VNPAY. Không được trùng lặp trong ngày

            //Add Params of 2.1.0 Version
            //Billing

            urlPayment = vnpay.CreateRequestUrl(vnp_Url, vnp_HashSecret);
            //log.InfoFormat("VNPAY URL: {0}", paymentUrl);
            return urlPayment;
        }
        #endregion
    }

}