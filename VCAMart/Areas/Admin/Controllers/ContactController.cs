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
using System.Data.SqlClient;

namespace RICTotalAdmin.Controllers
{
    public class ContactController:Controller
    {
        string msg = "";
        jsonClientPost jc = new jsonClientPost();
        ContactModel mdl = new ContactModel();
        public string tbaoquyenxoa = "Bạn không có quyền xóa thông tin này";
        public string tbaoquyensua = "Bạn không có quyền sửa thông tin này";
        public string tbaoquyenthem = "Bạn không có quyền thêm thông tin này";
        public string tblname;
        public static string langId;
        public static string CategoryID = "";
        public static string cat = "";
        public ContactController()
        {
            msg = "";
            tblname = "tblContact";
            ViewBag.Function_Title = "THÔNG TIN LIÊN HỆ";
            ViewBag.Function_Title_Short = "Contact";
            ViewBag.ControllerName = this.GetType().Name.Replace("Controller", "");
            ViewBag.mn_tt = "";
            ViewBag.tblname = tblname;
            try
            {
                ViewBag.pagesize = int.Parse(System.Web.HttpContext.Current.Session["userPageSize"].ToString());
            }
            catch  
            {
                ViewBag.pagesize = 10;
            }
            jc = new jsonClientPost();

            RICTotalAdmin.Models.common.InitDefault();
            LIB.CLogin lg = new LIB.CLogin(RICTotalAdmin.Models.common.strcnn, RICTotalAdmin.Models.common.userCookieTokenName);

            if (!lg.checkTokenLogin())
            {
                lg.GotoLoginPage(); return;
            }

            ViewBag.login_nv_id = lg.userId;
            ViewBag.user_uid = lg.userUid;
            ViewBag.user_fullname = lg.userFullName;
            ViewBag.user_avata = lg.userAvata; //
            ViewBag.UserMenu = RICTotalAdmin.Models.common.GetMenuAdmin(lg.userId); //common.GetMenuAdmin(lg.userId);
            ViewBag.user_fromdate = lg.userCreated;

            ViewBag.hansudung = (lg.songaycondung < 0 ? "<b class='bg-red hansudung'>Phần mềm đã hết hạn sử dụng</b>&nbsp;&nbsp;  " : (lg.songaycondung + 1 <= 5 ? "<b class='bg-red hansudung'>Phần mềm sẽ hết hạn sử dụng trong " + (lg.songaycondung + 1).ToString() + " ngày tới (<i>" + DateTime.Now.AddDays(lg.songaycondung + 1).ToString("dd/MM/yyyy") + "</i>)</b>&nbsp;&nbsp;  " : ""));

            ViewBag.congty_ten = lg.congty_ten;
            ViewBag.congty_diachi = lg.congty_diachi;
            ViewBag.congty_tel = lg.congty_tel;

            mdl.rc_id = lg.rc_id;
        }

        // GET: MenuAdmin
        public ActionResult Index(string id)
        {
            try
            {
                langId = System.Web.HttpContext.Current.Session["lang_id"].ToString();
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

            if (!common.checkQuyenSudung(mdl.userId.ToString(), this.GetType().Name.Replace("Controller", ""), ref msg))
            {

                // return View("~/Views/MSG/" + msg);
            }

            CategoryID = id;
            ViewBag.NewID = CategoryID;




            ViewBag.mn_tt = id;
            ViewBag.Function_Title = "THÔNG TIN LIÊN HỆ";
            ViewBag.Function_Title_Short = "Contact";
            return View("~/Areas/Admin/Views/Contact/Index.cshtml");
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
        public ActionResult GetList()
        {



            jc.data = mdl.GetList();
            jc.msg = mdl.msg;
            jc.code = (jc.msg == "100" ? 100 : 101);
            string skq = "{\"code\":" + jc.code.ToString() + ",\"msg\":\"" + jc.msg.ToString() + "\", \"data\":" + (jc.data.ToString().Trim() == "" ? "[]" : jc.data.ToString()) + "}";
            return Content(skq, "application/json");
        }
    }
}