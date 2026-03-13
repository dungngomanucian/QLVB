using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace VCAMart
{
    public class RouteConfig
    {
        /*public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }*/

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            // routes.MapRoute("Default456", "{controller}/{action}/{id}", new { controller = "Home", action = "Index", id = UrlParameter.Optional }, new[] { "RICTotal.Controllers" });



            routes.MapRoute("login", "Login/{action}", new { controller = "Login", action = "Index", id = UrlParameter.Optional }, new[] { "RICTotal.Controllers" });
            routes.MapRoute("Logout", "Logout/{action}", new { controller = "Logout", action = "Index", id = UrlParameter.Optional }, new[] { "RICTotal.Controllers" });

            routes.MapRoute("Chaomua1", "Chaomua/{id}", new { controller = "Chaomua", action = "Index", id = UrlParameter.Optional }, new[] { "RICTotal.Controllers" });
            routes.MapRoute("Chaomua", "Chaomua/{action}/{id}", new { controller = "Chaomua", action = "Index", id = UrlParameter.Optional }, new[] { "RICTotal.Controllers" });



            routes.MapRoute(
              "Default123",
              "{controller}/{id}",
              new { controller = "Home", action = "Index", id = UrlParameter.Optional },
              new[] { "RICTotal.Controllers" }
              );



            routes.MapRoute("Default", "{controller}/{action}/{id}", new { controller = "Home", action = "Index", id = UrlParameter.Optional }, new[] { "RICTotal.Controllers" });





            routes.MapRoute("Member", "Member/{controller}/{action}/{id}", new { action = "Index", id = UrlParameter.Optional });
            /*
            routes.MapRoute("QTDonThu", "QTDonThu/{controller}/{action}/{id}", new { action = "Index", id = UrlParameter.Optional });
            routes.MapRoute("Report", "Report/{controller}/{action}/{id}", new { action = "Index", id = UrlParameter.Optional });
            */


        }
    }
}
