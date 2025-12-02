using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebBanLapTop.Models;

namespace WebBanLapTop.Controllers
{
    public class ProductController : Controller
    {
        DatabaseDataContext db = new DatabaseDataContext();

        // GET: Product/Category
        public ActionResult ProductCategory(int? categoryId, string priceRange, string brand, string sort, int page = 1)
        {
            int pageSize = 16;

            var query = db.vw_CategoryProductLists.AsQueryable();

            // CATEGORY FILTER
            ViewBag.CategoryId = categoryId;
            ViewBag.CategoryName = "Tất cả sản phẩm";

            if (categoryId.HasValue && categoryId > 0)
            {
                query = query.Where(p => p.category_id == categoryId.Value);
                var cat = db.tb_categories
                    .FirstOrDefault(c => c.category_id == categoryId.Value && c.is_active == true);

                if (cat != null)
                    ViewBag.CategoryName = cat.name;
                else
                    return RedirectToAction("ProductCategory", new { categoryId = (int?)null });
            }

            // PRICE FILTER
            if (!string.IsNullOrEmpty(priceRange))
            {
                if (priceRange == "40+")
                {
                    query = query.Where(p => p.final_price > 40000000);
                }
                else
                {
                    var prices = priceRange.Split('-');
                    if (prices.Length == 2)
                    {
                        if (decimal.TryParse(prices[0], out decimal min))
                            query = query.Where(p => p.final_price >= min * 1000000);
                        if (decimal.TryParse(prices[1], out decimal max))
                            query = query.Where(p => p.final_price <= max * 1000000);
                    }
                }
            }

            // BRAND FILTER
            if (!string.IsNullOrEmpty(brand))
            {
                var brandIds = brand.Split(',').Select(int.Parse).ToList();
                var activeBrandIds = db.tb_brands
                    .Where(b => brandIds.Contains(b.brand_id) && b.is_active == true)
                    .Select(b => b.brand_id)
                    .ToList();

                if (activeBrandIds.Any())
                {
                    query = query.Where(p => p.brand_id != null && activeBrandIds.Contains(p.brand_id.Value));
                }
            }

            // SORT
            switch (sort)
            {
                case "price-asc":
                    query = query.OrderBy(p => p.final_price);
                    break;
                case "price-desc":
                    query = query.OrderByDescending(p => p.final_price);
                    break;
                case "name":
                    query = query.OrderBy(p => p.product_name);
                    break;
                case "newest":
                    query = query.OrderByDescending(p => p.create_at);
                    break;
                case "discount":
                    query = query.OrderByDescending(p => p.has_active_discount)
                                 .ThenByDescending(p => p.save_amount);
                    break;
                default:
                    query = query.OrderByDescending(p => p.create_at);
                    break;
            }

            // TOTAL ITEMS
            int totalItems = query.Count();

            // PAGING
            var products = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            int totalPage = (int)Math.Ceiling((double)totalItems / pageSize);
            if (totalPage < 1) totalPage = 1;

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPage = totalPage;

            // BRANDS - chỉ lấy brand active
            ViewBag.Brands = db.tb_brands
                .Where(b => b.is_active == true)
                .OrderBy(b => b.name)
                .ToList();

            // CATEGORIES - chỉ lấy category active (KHÔNG cần có sản phẩm)
            ViewBag.Categories = db.tb_categories
                .Where(c => c.is_active == true)
                .OrderBy(c => c.name)
                .ToList();

            return View(products);
        }
        // Thêm vào ProductController.cs

        public ActionResult Detail(int? id)
        {
            // Kiểm tra id có null không
            if (id == null)
            {
                return HttpNotFound();
            }

            // Product Detail
            var product = db.vw_ProductDetails.FirstOrDefault(p => p.product_id == id.Value);
            if (product == null)
            {
                return HttpNotFound();
            }

            // Product Images
            ViewBag.ProductImages = db.vw_ProductImages
                .Where(pi => pi.product_id == id.Value)
                .ToList();

            // Reviews
            ViewBag.Reviews = db.vw_ProductReviews
                .Where(r => r.product_id == id.Value)
                .OrderByDescending(r => r.created_at)
                .Take(10)
                .ToList();

            // Related Products (cùng category, khác product_id)
            ViewBag.RelatedProducts = db.vw_CategoryProductLists
                .Where(p => p.product_id != id.Value && (p.brand_id == product.brand_id))
                .OrderByDescending(p => p.create_at)
                .Take(4)
                .ToList();

            return View(product);
        }
    }
}