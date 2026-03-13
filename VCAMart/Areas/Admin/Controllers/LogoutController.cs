using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RICTotalAdmin.Models;
using System.Collections.Specialized;
namespace RICTotalAdmin.Controllers
{
    public class LogoutController:Controller
    {
        public ActionResult Index()
        {
            (new LIB.Logout(common.strcnn)).RunLogout();
            return View("~/Areas/Admin/Views/Shared/Login.cshtml");
            //return View("Login");
        }
    }
}