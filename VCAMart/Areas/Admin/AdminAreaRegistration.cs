using System.Web.Mvc;
using System.Web.Routing;

namespace RICTotalAdmin.Areas.Admin
{
    public class AdminAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Admin";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            //context.Routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            /*
            
               context.MapRoute(
                "ContentArticles",
                "Admin/ContentArticles/{id}",
                new { controller = "ContentArticles", action = "Index", id = UrlParameter.Optional },
                new[] { "TMDT.Controllers" }
            );*/
            context.MapRoute("loginAdmin", "Admin/Login/{action}", new { controller = "Login", action = "Index", id = UrlParameter.Optional }, new[] { "RICTotalAdmin.Controllers" });
            context.MapRoute("LogoutAdmin", "Admin/Logout/{action}", new { controller = "Logout", action = "Index", id = UrlParameter.Optional }, new[] { "RICTotalAdmin.Controllers" });
       

            context.MapRoute(
             "DefaulAdmint123",
             "Admin/{controller}/{id}",
             new { controller = "Home", action = "Index", id = UrlParameter.Optional },
             new[] { "RICTotalAdmin.Controllers" }
             );

            context.MapRoute(
                "Admin_default",
                "Admin/{controller}/{action}/{id}",
                new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                new[] { "RICTotalAdmin.Controllers" }
            );

            context.MapRoute("DefaultAdmin", "Admin/{controller}/{action}/{id}", new { controller = "Home", action = "Index", id = UrlParameter.Optional }, new[] { "RICTotalAdmin.Controllers" });
            //context.MapRoute("ContentArticlesAdmin", "Admin/{controller}/{action}/{id}", new { controller = "ContentArticles", action = "GetContent", id = UrlParameter.Optional }, new[] { "RICTotalAdmin.Controllers" });

        }
    }
}