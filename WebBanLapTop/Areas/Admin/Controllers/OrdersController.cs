using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Utilities;
using PagedList;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
            int pageSize = 5;
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
        public ActionResult DetailOrder(int id)
        {
            var data = db.vw_OrderDetailLists
                 .Where(x => x.order_id == id)
                 .ToList();
            return View(data);
        }
        [HttpPost]
        public ActionResult UpdateStatus(int id)
        {
            try
            {
                var order = db.tb_orders.FirstOrDefault(x => x.order_id == id);
                if (order == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy đơn hàng!" });
                }
                order.status = "Hoàn tất";
                db.SubmitChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi cập nhật đơn hàng!",
                    error = ex.Message
                });
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
                        x.TotalProduct,
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
                    to = to.AddDays(1); 
                    data = data.Where(x => x.order_date < to);
                }
            }
            return data;
        }
        //public ActionResult ExportOrderToPDF(int orderId)
        //{
        //    try
        //    {
        //        var data = db.vw_OrderDetailLists.Where(o => o.order_id == orderId);
        //        if (data == null || !data.Any())
        //        {
        //            return HttpNotFound("Không tìm thấy đơn hàng");
        //        }
        //        MemoryStream memoryStream = new MemoryStream();
        //        Document document = new Document();
        //        PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);

        //        document.Open();
        //        string fontPath = Server.MapPath("~/fonts/arial.ttf");
        //        BaseFont baseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
        //        Font headerFont = new Font(baseFont, 14, Font.BOLD);
        //        Font normalFont = new Font(baseFont, 11);
        //        Font boldFont = new Font(baseFont, 11, Font.BOLD);
        //        Font smallFont = new Font(baseFont, 9);

        //        Paragraph company = new Paragraph();
        //        Chunk colo = new Chunk("COLO", new Font(baseFont, 18, Font.BOLD, BaseColor.BLACK));
        //        Chunk shop = new Chunk("SHOP", new Font(baseFont, 18, Font.BOLD, BaseColor.RED));
        //        company.Alignment = Element.ALIGN_CENTER;
        //        company.Add(colo);
        //        company.Add(shop);
        //        document.Add(company);

        //        Paragraph companyInfo = new Paragraph("Địa chỉ: 55 Giải Phóng,Phường Hai Bà Trưng, Hà Nội", smallFont);
        //        companyInfo.Alignment = Element.ALIGN_CENTER;
        //        document.Add(companyInfo);

        //        Paragraph companyContact = new Paragraph("Điện thoại: 0123456789 | Email: info@laptop.com", smallFont);
        //        companyContact.Alignment = Element.ALIGN_CENTER;
        //        document.Add(companyContact);

        //        document.Add(new Paragraph(" "));
        //        Paragraph title = new Paragraph("HÓA ĐƠN BÁN HÀNG", headerFont);
        //        title.Alignment = Element.ALIGN_CENTER;
        //        document.Add(title);

        //        Paragraph orderCode = new Paragraph($"Mã đơn hàng: {data.order_id}", boldFont);
        //        orderCode.Alignment = Element.ALIGN_CENTER;
        //        document.Add(orderCode);

        //        document.Add(new Paragraph(" "));
        //        document.Add(new Chunk(new LineSeparator(1f, 100f, BaseColor.BLACK, Element.ALIGN_CENTER, -1)));
        //        document.Add(new Paragraph(" "));

        //        PdfPTable infoTable = new PdfPTable(2);
        //        infoTable.WidthPercentage = 100;
        //        infoTable.SetWidths(new float[] { 1f, 1f });

        //        PdfPCell leftCell = new PdfPCell();
        //        leftCell.Border = Rectangle.NO_BORDER;
        //        leftCell.AddElement(new Paragraph("THÔNG TIN ĐƠN HÀNG", boldFont));
        //        leftCell.AddElement(new Paragraph($"Ngày đặt: {Convert.ToDateTime(data.order_date):dd/MM/yyyy HH:mm}", normalFont));
        //        string status = data.status.Trim() == "Hoàn tất" ? "Đã Thanh Toán" : "Chưa Thanh Toán";
        //        leftCell.AddElement(new Paragraph($"Trạng thái: {status}", normalFont));
        //        infoTable.AddCell(leftCell);

        //        PdfPCell rightCell = new PdfPCell();
        //        rightCell.Border = Rectangle.NO_BORDER;
        //        rightCell.AddElement(new Paragraph("THÔNG TIN KHÁCH HÀNG", boldFont));
        //        rightCell.AddElement(new Paragraph($"Tên: {data.UserName}", normalFont));
        //        rightCell.AddElement(new Paragraph($"SĐT: {data.Phone ?? "Chưa cập nhật"}", normalFont));
        //        rightCell.AddElement(new Paragraph($"Địa chỉ: {data.address ?? "Chưa cập nhât"}", normalFont));
        //        infoTable.AddCell(rightCell);

        //        document.Add(infoTable);
        //        document.Add(new Paragraph(" "));

        //        PdfPTable productTable = new PdfPTable(6);
        //        productTable.WidthPercentage = 100;
        //        productTable.SetWidths(new float[] { 8f, 30f, 15f, 12f, 15f, 20f });
        //        string[] headers = { "STT", "Tên sản phẩm", "Đơn giá", "Số lượng", "Giảm giá", "Thành tiền" };
        //        foreach (string header in headers)
        //        {
        //            PdfPCell cell = new PdfPCell(new Phrase(header, boldFont));
        //            cell.BackgroundColor = new BaseColor(200, 200, 200);
        //            cell.HorizontalAlignment = Element.ALIGN_CENTER;
        //            cell.Padding = 8;
        //            productTable.AddCell(cell);
        //        }
        //        int index = 1;
        //        decimal subtotal = 0;
        //        decimal totalBeforeDiscount = 0;

        //        foreach (var item in data)
        //        {
        //            productTable.AddCell(new PdfPCell(new Phrase(index.ToString(), normalFont))
        //            { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 5 });

        //            productTable.AddCell(new PdfPCell(new Phrase(item.ProductName, normalFont))
        //            { Padding = 5 });

        //            decimal originalPrice = item.ProductPrice ?? 0;
        //            decimal salePrice = item.OrderPrice ?? 0;
        //            int quantity = item.quantity ?? 0;
        //            decimal discount = originalPrice - salePrice;

        //            productTable.AddCell(new PdfPCell(new Phrase(originalPrice.ToString("N0") + " đ", normalFont))
        //            { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 5 });

        //            productTable.AddCell(new PdfPCell(new Phrase(quantity.ToString(), normalFont))
        //            { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 5 });

        //            productTable.AddCell(new PdfPCell(new Phrase(discount.ToString("N0") + " đ", normalFont))
        //            { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 5 });

        //            decimal total = salePrice * quantity;
        //            productTable.AddCell(new PdfPCell(new Phrase(total.ToString("N0") + " đ", normalFont))
        //            { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 5 });

        //            subtotal += total;
        //            totalBeforeDiscount += originalPrice * quantity;
        //            index++;
        //        }

        //        document.Add(productTable);
        //        document.Add(new Paragraph(" "));
        //        document.Close();
        //        writer.Close();
        //        byte[] bytes = memoryStream.ToArray();
        //        memoryStream.Close();
        //        return File(bytes, "application/pdf", $"HoaDon_{data.order_id}_{DateTime.Now:yyyyMMddHHmmss}.pdf");
        //    }
        //    catch (Exception ex)
        //    {
        //        return Content("Lỗi khi xuất PDF: " + ex.Message);
        //    }
        //}
        public ActionResult ExportOrderToPDF(int orderId)
        {
            try
            {
                // BUG FIX 1: Lấy từ vw_OrderDetailLists thay vì vw_OrderLists
                var orderDetails = db.vw_OrderDetailLists
                    .Where(x => x.order_id == orderId)
                    .ToList();

                if (orderDetails == null || !orderDetails.Any())
                {
                    return HttpNotFound("Không tìm thấy đơn hàng");
                }

                var order = orderDetails.FirstOrDefault();

                // Tạo PDF
                MemoryStream memoryStream = new MemoryStream();
                Document document = new Document(PageSize.A4, 50, 50, 50, 50);
                PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);

                document.Open();

                // Load font
                string fontPath = Server.MapPath("~/fonts/arial.ttf");
                BaseFont baseFont = BaseFont.CreateFont(fontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
                Font titleFont = new Font(baseFont, 18, Font.BOLD);
                Font headerFont = new Font(baseFont, 14, Font.BOLD);
                Font normalFont = new Font(baseFont, 11);
                Font boldFont = new Font(baseFont, 11, Font.BOLD);
                Font smallFont = new Font(baseFont, 9);

                // Header
                Paragraph company = new Paragraph();
                Chunk colo = new Chunk("COLO", new Font(baseFont, 18, Font.BOLD, BaseColor.BLACK));
                Chunk shop = new Chunk("SHOP", new Font(baseFont, 18, Font.BOLD, BaseColor.RED));
                company.Alignment = Element.ALIGN_CENTER;
                company.Add(colo);
                company.Add(shop);
                document.Add(company);

                Paragraph companyInfo = new Paragraph("Địa chỉ: 55 Giải Phóng, Phường Hai Bà Trưng, Hà Nội", smallFont);
                companyInfo.Alignment = Element.ALIGN_CENTER;
                document.Add(companyInfo);

                Paragraph companyContact = new Paragraph("Điện thoại: 0123456789 | Email: info@laptop.com", smallFont);
                companyContact.Alignment = Element.ALIGN_CENTER;
                document.Add(companyContact);

                document.Add(new Paragraph(" "));

                // Title
                Paragraph title = new Paragraph("HÓA ĐƠN BÁN HÀNG", headerFont);
                title.Alignment = Element.ALIGN_CENTER;
                document.Add(title);

                Paragraph orderCode = new Paragraph($"Mã đơn hàng: {order.order_id}", boldFont);
                orderCode.Alignment = Element.ALIGN_CENTER;
                document.Add(orderCode);

                document.Add(new Paragraph(" "));
                document.Add(new Chunk(new LineSeparator(1f, 100f, BaseColor.BLACK, Element.ALIGN_CENTER, -1)));
                document.Add(new Paragraph(" "));

                // Thông tin đơn hàng và khách hàng
                PdfPTable infoTable = new PdfPTable(2);
                infoTable.WidthPercentage = 100;
                infoTable.SetWidths(new float[] { 1f, 1f });

                PdfPCell leftCell = new PdfPCell();
                leftCell.Border = Rectangle.NO_BORDER;
                leftCell.AddElement(new Paragraph("THÔNG TIN ĐƠN HÀNG", boldFont));
                leftCell.AddElement(new Paragraph($"Ngày đặt: {Convert.ToDateTime(order.order_date):dd/MM/yyyy HH:mm}", normalFont));

                // BUG FIX 2: Xử lý null-safe cho status
                string status = order.status?.Trim() == "Hoàn tất" ? "Đã Thanh Toán" : "Chưa Thanh Toán";
                leftCell.AddElement(new Paragraph($"Trạng thái: {status}", normalFont));
                infoTable.AddCell(leftCell);

                PdfPCell rightCell = new PdfPCell();
                rightCell.Border = Rectangle.NO_BORDER;
                rightCell.AddElement(new Paragraph("THÔNG TIN KHÁCH HÀNG", boldFont));
                rightCell.AddElement(new Paragraph($"Tên: {order.UserName}", normalFont));
                rightCell.AddElement(new Paragraph($"SĐT: {order.Phone ?? "Chưa cập nhật"}", normalFont));
                rightCell.AddElement(new Paragraph($"Địa chỉ: {order.address ?? "Chưa cập nhật"}", normalFont));
                infoTable.AddCell(rightCell);

                document.Add(infoTable);
                document.Add(new Paragraph(" "));

                // Bảng sản phẩm
                PdfPTable productTable = new PdfPTable(6);
                productTable.WidthPercentage = 100;
                productTable.SetWidths(new float[] { 8f, 30f, 15f, 12f, 15f, 20f });

                string[] headers = { "STT", "Tên sản phẩm", "Đơn giá", "Số lượng", "Giảm giá", "Thành tiền" };
                foreach (string header in headers)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(header, boldFont));
                    cell.BackgroundColor = new BaseColor(200, 200, 200);
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell.Padding = 8;
                    productTable.AddCell(cell);
                }
                int index = 1;
                decimal subtotal = 0;
                decimal totalBeforeDiscount = 0;

                foreach (var item in orderDetails)
                {
                    productTable.AddCell(new PdfPCell(new Phrase(index.ToString(), normalFont))
                    { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 5 });

                    productTable.AddCell(new PdfPCell(new Phrase(item.ProductName, normalFont))
                    { Padding = 5 });

                    decimal originalPrice = item.ProductPrice ?? 0;
                    decimal salePrice = item.OrderPrice ?? 0;
                    int quantity = item.quantity ?? 0;
                    decimal discount = originalPrice - salePrice;

                    productTable.AddCell(new PdfPCell(new Phrase(originalPrice.ToString("N0") + " đ", normalFont))
                    { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 5 });

                    productTable.AddCell(new PdfPCell(new Phrase(quantity.ToString(), normalFont))
                    { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 5 });

                    productTable.AddCell(new PdfPCell(new Phrase(discount.ToString("N0") + " đ", normalFont))
                    { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 5 });

                    decimal total = salePrice * quantity;
                    productTable.AddCell(new PdfPCell(new Phrase(total.ToString("N0") + " đ", normalFont))
                    { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 5 });

                    subtotal += total;
                    totalBeforeDiscount += originalPrice * quantity;
                    index++;
                }

                document.Add(productTable);
                document.Add(new Paragraph(" "));

                PdfPTable summaryTable = new PdfPTable(2);
                summaryTable.WidthPercentage = 50;
                summaryTable.HorizontalAlignment = Element.ALIGN_RIGHT;
                summaryTable.SetWidths(new float[] { 2f, 1f });

 
                summaryTable.AddCell(new PdfPCell(new Phrase($"Tạm tính ({order.TotalProduct} sản phẩm):", normalFont))
                { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_LEFT, Padding = 5 });
                summaryTable.AddCell(new PdfPCell(new Phrase(totalBeforeDiscount.ToString("N0") + " đ", normalFont))
                { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 5 });

    
                decimal totalDiscount = totalBeforeDiscount - subtotal;
                summaryTable.AddCell(new PdfPCell(new Phrase("Giảm giá:", normalFont))
                { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_LEFT, Padding = 5 });
                summaryTable.AddCell(new PdfPCell(new Phrase("-" + totalDiscount.ToString("N0") + " đ", normalFont))
                { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 5 });

           
                summaryTable.AddCell(new PdfPCell(new Phrase("Thuế VAT (0%):", normalFont))
                { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_LEFT, Padding = 5 });
                summaryTable.AddCell(new PdfPCell(new Phrase("0 đ", normalFont))
                { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 5 });

            
                summaryTable.AddCell(new PdfPCell(new Phrase("Phí vận chuyển:", normalFont))
                { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_LEFT, Padding = 5 });
                summaryTable.AddCell(new PdfPCell(new Phrase("0 đ", normalFont))
                { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 5 });

          
                PdfPCell totalLabelCell = new PdfPCell(new Phrase("TỔNG CỘNG:", boldFont));
                totalLabelCell.Border = Rectangle.TOP_BORDER;
                totalLabelCell.BorderWidth = 2;
                totalLabelCell.HorizontalAlignment = Element.ALIGN_LEFT;
                totalLabelCell.Padding = 8;
                totalLabelCell.BackgroundColor = new BaseColor(230, 230, 230);
                summaryTable.AddCell(totalLabelCell);

                PdfPCell totalValueCell = new PdfPCell(new Phrase(subtotal.ToString("N0") + " đ", boldFont));
                totalValueCell.Border = Rectangle.TOP_BORDER;
                totalValueCell.BorderWidth = 2;
                totalValueCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                totalValueCell.Padding = 8;
                totalValueCell.BackgroundColor = new BaseColor(230, 230, 230);
                summaryTable.AddCell(totalValueCell);

                document.Add(summaryTable);

               
                document.Add(new Paragraph(" "));
                document.Add(new Paragraph(" "));
                Paragraph footer = new Paragraph("Cảm ơn quý khách đã mua hàng!", normalFont);
                footer.Alignment = Element.ALIGN_CENTER;
                document.Add(footer);

                document.Close();
                writer.Close();

                byte[] bytes = memoryStream.ToArray();
                memoryStream.Close();

                return File(bytes, "application/pdf", $"HoaDon_{order.order_id}_{DateTime.Now:yyyyMMddHHmmss}.pdf");
            }
            catch (Exception ex)
            {
  
                return Content("Lỗi khi xuất PDF: " + ex.Message);
            }
        }
    }
}