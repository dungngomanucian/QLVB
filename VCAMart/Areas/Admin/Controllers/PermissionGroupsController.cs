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
    public class PermissionGroupsController : Controller
    {
        string msg = "";
        public string tblname;
        jsonClientPost jc ;
        PermissionGroups mdl = new PermissionGroups();

        public PermissionGroupsController()
        {
            tblname = "tblKhuvuc";
            ViewBag.Function_Title = "Khu vực";
            ViewBag.Function_Title_Short = "Area";
            ViewBag.ControllerName =  this.GetType().Name.Replace("Controller", "");
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
            ViewBag.UserMenu = RICTotalAdmin.Models.common.GetMenuAdmin(lg.userId); //common.GetMenuAdmin(lg.userId);
            ViewBag.user_fromdate = lg.userCreated;

            ViewBag.hansudung = (lg.songaycondung < 0 ? "<b class='bg-red hansudung'>Phần mềm đã hết hạn sử dụng</b>&nbsp;&nbsp;  " : (lg.songaycondung + 1 <= 5 ? "<b class='bg-red hansudung'>Phần mềm sẽ hết hạn sử dụng trong " + (lg.songaycondung + 1).ToString() + " ngày tới (<i>" + DateTime.Now.AddDays(lg.songaycondung + 1).ToString("dd/MM/yyyy") + "</i>)</b>&nbsp;&nbsp;  " : ""));

            ViewBag.congty_ten = lg.congty_ten;
            ViewBag.congty_diachi = lg.congty_diachi;
            ViewBag.congty_tel = lg.congty_tel;



            mdl.pageSize = ViewBag.pagesize;
            mdl.User_ID = new Guid(lg.userId);
            mdl.User_UID = ViewBag.user_uid;
            mdl.RC_ID = new Guid(lg.rc_id);
            mdl.CH_ID_X = (System.Web.HttpContext.Current.Session["ch_id_x"] == null ? Guid.Empty : new Guid(System.Web.HttpContext.Current.Session["ch_id_x"].ToString()));

        }
        // GET: MenuAdmin
        public ActionResult Index(string id)
        {
            if (!common.checkQuyenSudung(mdl.User_ID.ToString(), this.GetType().Name.Replace("Controller", ""), ref msg))
            {
                //return View("~/Views/MSG/" + msg);
            }
            ViewBag.mn_tt = id;
            ViewBag.Function_Title = "NHÓM QUYỀN";
            ViewBag.Function_Title_Short = "Nhóm quyền";
            return View("~/Areas/Admin/Views/PermissionGroups/Index.cshtml");
        }

        public ActionResult GetList()
        {
            jc.data = mdl.GetList(0);
            jc.msg = mdl.msg;
            jc.code = (jc.msg == "100" ? 100 : 101);
            string skq = "{\"code\":" + jc.code.ToString() + ",\"msg\":\"" + jc.msg.ToString() + "\", \"data\":" + (jc.data.ToString().Trim() == "" ? "[]" : jc.data.ToString()) + "}";
            return Content(skq, "application/json");
        }

        public ActionResult GetPhanQuyen(string data)
        {
            jc.data = mdl.GetPhanQuyen(data);
            jc.msg = mdl.msg;
            jc.code = (jc.msg == "100" ? 100 : 101);
            string skq = "{\"code\":" + jc.code.ToString() + ",\"msg\":\"" + jc.msg.ToString() + "\", \"data\":" + (jc.data.ToString().Trim() == "" ? "[]" : jc.data.ToString()) + "}";
            return Content(skq, "application/json");
        }

        bool IsValid(ref dynamic data, bool bAdd = true)
        {
            Guid id1;
            msg = "";
            if (!bAdd)
            {
                if (!Guid.TryParse(data.id.ToString(), out id1))
                {
                    msg += "<br> - ID is not correnct.";
                }
            }

            if (data.name.ToString().Trim() == "") msg += "<br> - Nhập Tên nhóm quyền";

            data.name = common.ChuanHoaXau(data.name.ToString());

            if (msg != "") return false;

            return true;
        }
        // public string Insert(string data)
        public ActionResult Insert(string data)
        {
            dynamic row = JObject.Parse(data);
            row.id = Guid.NewGuid().ToString();

            string skeys = "name;active;";

            if (!common.CheckObjectEnoughProperies(row, skeys, ref msg))
            {
                jc.code = 121;
                jc.msg = "";
                return Json(jc, JsonRequestBehavior.AllowGet);
            }

            if (!IsValid(ref row))
            {
                jc.code = 101;
                jc.msg = "";
                return Json(jc, JsonRequestBehavior.AllowGet);
            }
            mdl.Insert(row);
            jc.code = (mdl.msg == "100" ? 100 : 101);
            jc.msg = mdl.msg;
            jc.data.key = row.id.ToString();
            return Json(jc, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Update(string data)
        {

            dynamic row = JObject.Parse(data);
            string skeys = "id;name;active;";

            if (!common.CheckObjectEnoughProperies(row, skeys, ref msg))
            {
                jc.code = 121;
                jc.msg = "";
                return Json(jc, JsonRequestBehavior.AllowGet);
            }

            if (!IsValid(ref row, false))
            {
                jc.code = 101;
                jc.msg = msg;
                return Json(jc, JsonRequestBehavior.AllowGet);
            }
            mdl.Update(row);
            jc.code = (mdl.msg == "100" ? 100 : 101);
            jc.msg = mdl.msg;
            jc.data = mdl.iRowEffected.ToString();
            return Json(jc, JsonRequestBehavior.AllowGet);
        }

        bool IsValidImport(dynamic row)
        {
          
            return true;
        }
        public ActionResult Import(string data, string cols)
        {
            // thuc hien import
            dynamic arr = JObject.Parse("{\"vl\":" + data + "}");
            dynamic arrcols = JObject.Parse("{\"vl\":" + cols + "}");

            if (!IsValidImport(arr))
            {
                jc.code = 101;
                jc.msg = msg;
                return Json(jc, JsonRequestBehavior.AllowGet);
            }
            mdl.Import(arr.vl, arrcols.vl);
            jc.code = (mdl.msg == "100" ? 100 : 101);
            jc.msg = mdl.msg;
            jc.data = mdl.iRowEffected.ToString();
            return Json(jc, JsonRequestBehavior.AllowGet);

        }


        public ActionResult Delete(string data)
        {
            data = data.Replace(" ", "");
            if (data.Trim() == "")
            {
                jc.code = 121;
                jc.msg = "No records selected to delete.";
                return Json(jc, JsonRequestBehavior.AllowGet);
            }
            mdl.Delete(data);
           
            jc.code = (mdl.msg == "100" ? 100 : 101);
            jc.msg = mdl.msg;
            jc.data = mdl.iRowEffected;
            return Json(jc, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SavePhanQuyen(string data)
        {
            dynamic row = JObject.Parse(data);
            mdl.SavePhanQuyen(row);
            jc.code = (mdl.msg == "100" ? 100 : 101);
            jc.msg = mdl.msg;
            jc.data = mdl.iRowEffected.ToString();
            return Json(jc, JsonRequestBehavior.AllowGet);
        }

        protected override void Dispose(bool disposing)
        {

        }
    }
}