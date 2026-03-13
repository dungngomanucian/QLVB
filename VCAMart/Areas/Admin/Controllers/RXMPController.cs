using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using RICTotalAdmin.Models;


namespace RICTotalAdmin.Controllers
{
    public class RXMPController : Controller
    {
        // RXMP: RIC Extra Module Packages
        string rc_id;
        string user_id;//= System.Web.HttpContext.Current.Session["user_id"].ToString();
        string ch_id;//= System.Web.HttpContext.Current.Session["ch_id_x"].ToString();
         
        jsonClientPost jc = new jsonClientPost();
        string view_folder = "Views/RXMP";
         
        // GET: Common
        public RXMPController()
        {
           
            ViewBag.view_folder = view_folder;
            
            ViewBag.ControllerName = "RXMP"; 
            jc = new jsonClientPost();

            RICTotalAdmin.Models.common.InitDefault();
            LIB.CLogin lg = new LIB.CLogin(RICTotalAdmin.Models.common.strcnn, RICTotalAdmin.Models.common.userCookieTokenName  );

            if (!lg.checkTokenLogin())
            { lg.GotoLoginPage(); return; }
            //  ViewBag.login_nv_id = lg.nhanvienId;

            ViewBag.login_nv_id = lg.userId;
            ViewBag.user_uid = lg.userUid;
            ViewBag.user_fullname = lg.userFullName;
            ViewBag.user_avata = lg.userAvata; //
            ViewBag.UserMenu = RICTotalAdmin.Models.common.GetMenuAdmin(lg.userId); //common.GetMenuAdmin(lg.userId);
            ViewBag.user_fromdate = lg.userCreated;
            ViewBag.pagesize = 1000;

            ViewBag.hansudung = (lg.songaycondung < 0 ? "<b class='bg-red hansudung'>Phần mềm đã hết hạn sử dụng</b>&nbsp;&nbsp;  " : (lg.songaycondung + 1 <= 5 ? "<b class='bg-red hansudung'>Phần mềm sẽ hết hạn sử dụng trong " + (lg.songaycondung + 1).ToString() + " ngày tới (<i>" + DateTime.Now.AddDays(lg.songaycondung + 1).ToString("dd/MM/yyyy") + "</i>)</b>&nbsp;&nbsp;  " : ""));

            ViewBag.congty_ten = lg.congty_ten;
            ViewBag.congty_diachi = lg.congty_diachi;
            ViewBag.congty_tel = lg.congty_tel;

            try
            {
                rc_id = System.Web.HttpContext.Current.Session["rc_id"].ToString();
                user_id = System.Web.HttpContext.Current.Session["user_id"].ToString();
                ch_id = System.Web.HttpContext.Current.Session["ch_id_x"].ToString();
            }
            catch   { }

        }
        public ActionResult Show(string id)
        {
            //if (!common.checkQuyenSudung(mdl.User_ID_Tao.ToString(), ViewBag.ControllerName, ref msg))
            //{
            //    return View("~/Views/MSG/" + msg);
            //}
            return View("~/" + view_folder + "/" + id + ".cshtml");
        }

        public ActionResult Run(string data)
        {// bao cao doanh thu
            if (data.Trim() == "")
            {
                jc.msg = "NODATA.";
                jc.code = 101;
                jc.data = "[]";
                return Json(jc, JsonRequestBehavior.AllowGet);
            }

            dynamic row = JObject.Parse(data);
            string[] ngay; string[] ngay1; string  proc;
            try
            {
                ngay = row.from.ToString().Split('/');
                row.from = ngay[1] + "/" + ngay[0] + "/" + ngay[2];
                ngay1 = row.to.ToString().Split('/');
                row.to = ngay1[1] + "/" + ngay1[0] + "/" + ngay1[2];
                proc = "sp_RXMP_" + row.name;
            }
            catch (Exception e)
            {
                jc.msg = e.Message;
                jc.code = 101;
                jc.data = "[]";
                return Json(jc, JsonRequestBehavior.AllowGet);
            }
            string[] p = { "", "", "", "", "", "", "", "", "", "", "" };
            try { p[0] = row.p1.ToString(); } catch  { }
            try { p[1] = row.p2.ToString(); } catch  { }
            try { p[2] = row.p3.ToString(); } catch  { }
            try { p[3] = row.p4.ToString(); } catch  { }
            try { p[4] = row.p5.ToString(); } catch  { }
            try { p[5] = row.p6.ToString(); } catch  { }
            try { p[6] = row.p7.ToString(); } catch  { }
            try { p[7] = row.p8.ToString(); } catch  { }
            try { p[8] = row.p9.ToString(); } catch  { }
            try { p[9] = row.p10.ToString(); } catch  { }


            string sql = "exec " + proc + " '" + rc_id.ToString() + "','" + ch_id + "','" + user_id + "', '" + row.from.ToString() + "','" + row.to.ToString() + "'";

            // bo sung tham so mac dinh
            for (int i=0; i<10; i++)
            {
                sql += ",N'" + p[i] + "'";
            }
            jc.data =  common.RunSQLToJson(sql, ref jc.msg);
            jc.code = (jc.msg == "100" ? 100 : 101);
            jc.data = jc.code == 100 ? jc.data : "[]";
            jc.data = jc.data != "" ? jc.data : "[]";
            return Json(jc, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Detail(string data)
        {// bao cao doanh thu
            if (data.Trim() == "")
            {
                jc.msg = "NODATA.";
                jc.code = 101;
                jc.data = "[]";
                return Json(jc, JsonRequestBehavior.AllowGet);
            }

            dynamic row = JObject.Parse(data);
            string[] ngay; string[] ngay1; string proc;
            try
            {
                ngay = row.from.ToString().Split('/');
                row.from = ngay[1] + "/" + ngay[0] + "/" + ngay[2];
                ngay1 = row.to.ToString().Split('/');
                row.to = ngay1[1] + "/" + ngay1[0] + "/" + ngay1[2];
                proc = "sp_RXMP_" + row.name + "_Detail";
            }
            catch (Exception e)
            {
                jc.msg = e.Message;
                jc.code = 101;
                jc.data = "[]";
                return Json(jc, JsonRequestBehavior.AllowGet);
            }
            string [] p = { "", "", "", "", "", "", "", "", "", "", "" };
            try { p[0] = row.p1.ToString(); } catch  { }
            try { p[1] = row.p2.ToString(); } catch  { }
            try { p[2] = row.p3.ToString(); } catch { }
            try { p[3] = row.p4.ToString(); } catch  { }
            try { p[4] = row.p5.ToString(); } catch  { }
            try { p[5] = row.p6.ToString(); } catch  { }
            try { p[6] = row.p7.ToString(); } catch  { }
            try { p[7] = row.p8.ToString(); } catch  { }
            try { p[8] = row.p9.ToString(); } catch  { }
            try { p[9] = row.p10.ToString(); } catch  { }

            string sql = "exec " + proc + " '" + rc_id.ToString() + "','" + ch_id + "','" + user_id + "', '" + row.from.ToString() + "','" + row.to.ToString() + "'";
            // bo sung tham so mac dinh
            for (int i = 0; i < 10; i++)
            {
                sql += ",N'" + p[i] + "'";
            }
            jc.data = common.RunSQLToJson(sql, ref jc.msg);
            jc.code = (jc.msg == "100" ? 100 : 101);
            jc.data = jc.code == 100 ? jc.data : "[]";
            jc.data = jc.data != "" ? jc.data : "[]";
            return Json(jc, JsonRequestBehavior.AllowGet);
        }
    } 

    }