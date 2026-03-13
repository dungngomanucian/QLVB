using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RICTotalAdmin.Models;
using System.Collections.Specialized;

namespace RICTotalAdmin.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
     
        public ActionResult Index()
        {
            
            string loginvl = "";
            try { loginvl = System.Web.HttpContext.Current.Session["user_logined"].ToString(); } catch { loginvl = ""; }
            if (loginvl == LIB.ConfigInfo.userLoginedConfirm )
            {
               // Response.Redirect("~/Category/Customer"); 
            }
            
            return View("Login");
        }

        public string LoginProcess()
        {
            //RICTotal.Models.common.userCookieTokenName = "RICTotal_Token";
            string msg = "Tên đăng nhập hoặc mật khẩu không đúng";
            string uid, pwd;
            uid = Request.Form["u"].ToString().Trim();
            pwd = Request.Form["p"].ToString().Trim();

            LIB.CLogin lg = new LIB.CLogin(uid, pwd, RICTotalAdmin.Models.common.strcnn, RICTotalAdmin.Models.common.userCookieTokenName );
            msg =  lg.runLogin();
            if (lg.code == 100)
            {
                // kiem tra thu muc image da co chua, neu chua co thi tao
                string thumuc = "PF" + (Session["user_id"] == null ? "NoLogin" : Session["user_id"].ToString().Replace("-", ""));
                var sfl = Server.MapPath("~/") + "Images\\" + thumuc;
                if (!System.IO.Directory.Exists(sfl))
                {
                    try
                    {
                        System.IO.Directory.CreateDirectory(sfl);
                    }
                    catch   { }
                }
            }
            return msg=="100"?"100":"Đăng nhập không thành công.Kiểm tra lại kết nội mạng INTERNET";
        }
    }
}