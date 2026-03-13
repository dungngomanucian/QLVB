using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using RICTotalAdmin.Models;
using System.Data;
 

namespace RICTotalAdmin.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public static string langId;
 
        public ActionResult Index()
        {
            try
            {
                if (System.Web.HttpContext.Current.Session["lang_id"] != null)
                {
                    langId = System.Web.HttpContext.Current.Session["lang_id"].ToString();
                }
                else langId = "";
            }
            catch
            {
                langId = "1";
            }
            if (langId == "1")
            {
                ViewBag.lang_text = "Vietnamese";
                ViewBag.lang_link = "/Admin/Home/ChangeLanguage/1";
                ViewBag.lang_img = "/Areas/Admin/Images/VietNamFlag.png";
                System.Web.HttpContext.Current.Session["lang_id"] = 1;
            }
            else
            {
                ViewBag.lang_text = "English";
                ViewBag.lang_link = "/Admin/Home/ChangeLanguage/2";
                ViewBag.lang_img = "/Areas/Admin/Images/EnglishFlag.png";
                System.Web.HttpContext.Current.Session["lang_id"] = 2;
            }

           

            LIB.CLogin lg = new LIB.CLogin(common.strcnn, common.userCookieTokenName );
            if (!lg.checkTokenLogin())
            {
                //lg.GotoLoginPage();
                return View("NoLogin");
            }


            
            
            ViewBag.login_nv_id = lg.userId;
            ViewBag.user_uid = lg.userUid;
            ViewBag.user_fullname = lg.userFullName;
            ViewBag.user_avata = lg.userAvata; //
            ViewBag.UserMenu = RICTotalAdmin.Models.common.GetMenuAdmin(lg.userId); //common.GetMenuAdmin(lg.userId);
            

           // ViewBag.PB_Code  ;
          //  ViewBag.UserMenu += menu;
            ViewBag.user_fromdate = lg.userCreated;
            ViewBag.hansudung = (lg.songaycondung < 0 ? "<b class='bg-red hansudung'>Phần mềm đã hết hạn sử dụng</b>&nbsp;&nbsp;  " : (lg.songaycondung + 1 <= 5 ? "<b class='bg-red hansudung'>Phần mềm sẽ hết hạn sử dụng trong " + (lg.songaycondung + 1).ToString() + " ngày tới (<i>" + DateTime.Now.AddDays(lg.songaycondung + 1).ToString("dd/MM/yyyy") + "</i>)</b>&nbsp;&nbsp;  " : ""));

            ViewBag.congty_ten = lg.congty_ten;
            ViewBag.congty_diachi = lg.congty_diachi;
            ViewBag.congty_tel = lg.congty_tel;

            ViewBag.PB_Code = lg.PB_Code.ToString();

            //try {

            //    string ip = Request.ServerVariables["REMOTE_ADDR"];

            //    //common cm = new RICTotalAdmin.Models.common();
            //    //string ip = cm.GetLocalIPAddress();
            //    RICTotal.Models.ESB_MAIL esb_Mail = new RICTotal.Models.ESB_MAIL();
            //    //ESB_MAIL esb_Mail = new ESB_MAIL();
            //    bool _result = false;
            //    string _sendMailAddress = "atc.support@samsung.com";
            //    string _reciveMailAddress = "";
            //    _reciveMailAddress = "dovan.tuan@samsung.com;tronghien.le@samsung.com;toanpt.sdv@samsung.com;";

            //    //Mail Content
            //    string _title = string.Empty;
            //    string _content = string.Empty;

            //    string dtnow = DateTime.Now.ToString("dd/MM/yyyy");
            //    string currentDate = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss");
            //    _title = "[New_Warning]_" + DateTime.Now.ToString("yyMMdd") + "_Logged in AI Platform system";

            //    _content += "<p>Đã có người đăng nhập vào hệ thống AI Platform System</p>";
            //    _content += "<p>From IP: " + ip + "</p>";
            //    _content += "<p>Người đăng nhập: " + ViewBag.user_fullname + "</p>";
            //    _content += "<p>Thời gian đăng nhập: " + currentDate + "</p>";

            //    _result = esb_Mail.SendMISMail(_sendMailAddress, _reciveMailAddress, _title, _content, true, "t");

            //}
            //catch(Exception e)
            //{

            //}

            //ViewBag.VB_Sumary = (new Home()).GetSumary();

          
            //Xu ly thong tin ve Trang thai don thu

            return View("index");
        }

        public void Logout()
        {
            
         
           // (new LIB.Logout()).RunLogout();
            
        }

        jsonClientPost jc= new jsonClientPost();
        public ActionResult LoadDashboard(string data)
        {
            dynamic row = JObject.Parse(data);
          
            if (row.chid==null)
            {
                
                return Content("[]", "application/json");
            }
            jc.data = "[]";// common.LoadDashboard(Session["rc_id"].ToString(), row.chid.ToString(), ref jc.msg);
            jc.code = (jc.msg == "100" ? 100 : 101);
              string skq = "{\"code\":" + jc.code.ToString() + ",\"msg\":\"" + jc.msg.ToString() + "\", \"data\":" + (jc.data.ToString().Trim() == "" ? "{}" : jc.data.ToString()) + "}";

            return Content(skq, "application/json");
             
        }

       public void ChangeLanguage()
       {
            langId = System.Web.HttpContext.Current.Session["lang_id"].ToString();
            if (langId == "2")
            {
                ViewBag.lang_text = "Vietnamese";
                ViewBag.lang_link = "/Admin/Home/ChangeLanguage/1";
                ViewBag.lang_img = "/Areas/Admin/Images/VietNamFlag.png";
                System.Web.HttpContext.Current.Session["lang_id"] = 1;
            }
            else
            {
                ViewBag.lang_text = "English";
                ViewBag.lang_link = "/Admin/Home/ChangeLanguage/2";
                ViewBag.lang_img = "/Areas/Admin/Images/EnglishFlag.png";
                System.Web.HttpContext.Current.Session["lang_id"] = 2;
            }

            Response.Redirect("/Admin/Home");
        }
     
        
    }
}