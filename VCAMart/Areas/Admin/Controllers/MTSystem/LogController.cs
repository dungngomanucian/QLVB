using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using RICTotalAdmin.Models;
using MTModel.Category;

namespace RICTotalAdmin.Controllers.MTSystem
{
    public class LogController : Controller
    {

        string msg;
        public string tblname;
        jsonClientPost jc;


        tblLog mdl = new tblLog(common.strcnn);
        string view_folder = "Views/MTSystem";

        int ltc = 1;
        public LogController()
        {
            ViewBag.view_folder = view_folder;
            ViewBag.ltc = ltc;

            msg = "";
            tblname = "";
            ViewBag.Function_Title = "LỊCH SỬ THAO TÁC".ToUpper();
            ViewBag.Function_Title_Short = "History";
            ViewBag.ControllerName = "Log";// + this.GetType().Name.Replace("Controller", "");
            ViewBag.mn_tt = "";
            ViewBag.tblname = mdl.tableName;
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
            LIB.CLogin lg = new LIB.CLogin(RICTotalAdmin.Models.common.strcnn, RICTotalAdmin.Models.common.userCookieTokenName  );

            if (!lg.checkTokenLogin())
            { lg.GotoLoginPage(); return; }

            ViewBag.login_nv_id = lg.userId;
            ViewBag.user_uid = lg.userUid;
            ViewBag.user_fullname = lg.userFullName;
            ViewBag.user_avata = lg.userAvata; //
            ViewBag.UserMenu = ""; //common.GetMenuAdmin(lg.userId);
            ViewBag.user_fromdate = lg.userCreated;

            ViewBag.hansudung = (lg.songaycondung < 0 ? "<b class='bg-red hansudung'>Phần mềm đã hết hạn sử dụng</b>&nbsp;&nbsp;  " : (lg.songaycondung + 1 <= 5 ? "<b class='bg-red hansudung'>Phần mềm sẽ hết hạn sử dụng trong " + (lg.songaycondung + 1).ToString() + " ngày tới (<i>" + DateTime.Now.AddDays(lg.songaycondung + 1).ToString("dd/MM/yyyy") + "</i>)</b>&nbsp;&nbsp;  " : ""));

            ViewBag.congty_ten = lg.congty_ten;
            ViewBag.congty_diachi = lg.congty_diachi;
            ViewBag.congty_tel = lg.congty_tel;

          //  mdl.User_ID_Tao = new Guid(lg.userId);
            mdl.RC_ID = new Guid(lg.rc_id);
            mdl.pageSize = ViewBag.pagesize;
            mdl.User_ID = new Guid(lg.userId);
            mdl.User_UID = ViewBag.user_uid;
            mdl.RC_ID = new Guid(lg.rc_id);
            mdl.CH_ID_X = (System.Web.HttpContext.Current.Session["ch_id_x"] == null ? Guid.Empty : new Guid(System.Web.HttpContext.Current.Session["ch_id_x"].ToString()));

        }

        public ActionResult Index(string id)
        {
            if (!common.checkQuyenSudung(mdl.User_ID.ToString(), ViewBag.ControllerName, ref msg))
            {
                return View("~/Views/MSG/" + msg);
            }
            ViewBag.mn_tt = id;
            return View("~/" + view_folder  + "/Log.cshtml");
        }
        public ActionResult GetList(string data)
        {
            dynamic row = JObject.Parse(data);
            
            string[] ngay = row.tungay.ToString().Split('/');
            row.tungay = ngay[1] + "/" + ngay[0] + "/" + ngay[2];
            string[] ngay1 = row.denngay.ToString().Split('/');
            row.denngay = ngay1[1] + "/" + ngay1[0] + "/" + ngay1[2];

            jc.data = mdl.GetList (row.tungay.ToString(), row.denngay.ToString(),   ref jc.msg);
            jc.code = (jc.msg == "100" ? 100 : 101);
            jc.data = jc.code == 100 ? jc.data : "[]";
            jc.data = jc.data != "" ? jc.data : "[]";
            return Json(jc, JsonRequestBehavior.AllowGet);
        }
        public ActionResult LoadHome(string data)
        {
            dynamic row = JObject.Parse(data);
            if (row.chid == null) row.chid = Guid.Empty.ToString();
            if (row.id == null) row.id = 0;

            jc.data = mdl.GetHome(row.chid.ToString(), row.id.ToString(), ref jc.msg);
            jc.code = (jc.msg == "100" ? 100 : 101);
            string skq = "{\"code\":" + jc.code.ToString() + ",\"msg\":\"" + jc.msg.ToString() + "\", \"data\":" + (jc.data.ToString().Trim() == "" ? "[]" : jc.data.ToString()) + "}";
            return Content(skq, "application/json");
        }
    }
}