using Microsoft.Owin;
using Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.AspNet.Identity;

[assembly: OwinStartupAttribute(typeof(WebBanHangOnline.Startup))]
namespace WebBanHangOnline
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Cấu hình trực tiếp luôn, không cần gọi ConfigureAuth
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Admin/Account/Login")
            });
        }
    }
}