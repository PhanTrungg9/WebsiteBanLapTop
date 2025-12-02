using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace WebBanLapTop
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "vnpay_return",
                url: "vnpay_return",
                defaults: new { controller = "ShoppingCart", action = "VnpayReturn", id = UrlParameter.Optional },
                namespaces: new[] { "WebBanLapTop.Controllers" }
            );
            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "WebBanLapTop.Controllers" }
            );
        }
    }
}
