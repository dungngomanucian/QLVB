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
    public class UsersController : Controller
    {
        string msg = "";
        jsonClientPost jc = new jsonClientPost();
        Users mdl = new Users();

        public string tblname;


        public UsersController()
        {
            msg = "";
            tblname = "tblUser";
            ViewBag.Function_Title = "DANH SÁCH NHÂN VIÊN";
            ViewBag.Function_Title_Short = "Users";
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
            LIB.CLogin lg = new LIB.CLogin(RICTotalAdmin.Models.common.strcnn, RICTotalAdmin.Models.common.userCookieTokenName );

            if (!lg.checkTokenLogin())
            { lg.GotoLoginPage(); return;
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
            if (!common.checkQuyenSudung(mdl.userId.ToString(), this.GetType().Name.Replace("Controller", ""), ref msg))
            {
               
               // return View("~/Views/MSG/" + msg);
            }
            ViewBag.mn_tt = id;
            ViewBag.Function_Title = "DANH SÁCH USERS";
            ViewBag.Function_Title_Short = "Users";
            return View("~/Areas/Admin/Views/Users/Index.cshtml");
        }

        public ActionResult Get_ChucVu()
        {
            jc.data = mdl.Get_ChucVu();
            jc.msg = mdl.msg;
            jc.code = (jc.msg == "100" ? 100 : 101);
            string skq = "{\"code\":" + jc.code.ToString() + ",\"msg\":\"" + jc.msg.ToString() + "\", \"data\":" + (jc.data.ToString().Trim() == "" ? "[]" : jc.data.ToString()) + "}";
            return Content(skq, "application/json");
        }
        public ActionResult Get_PB()
        {
            jc.data = mdl.Get_PB();
            jc.msg = mdl.msg;
            jc.code = (jc.msg == "100" ? 100 : 101);
            string skq = "{\"code\":" + jc.code.ToString() + ",\"msg\":\"" + jc.msg.ToString() + "\", \"data\":" + (jc.data.ToString().Trim() == "" ? "[]" : jc.data.ToString()) + "}";
            return Content(skq, "application/json");
        }

        public ActionResult GetList()
        {
            jc.data = mdl.GetList(0);
            jc.msg = mdl.msg;
            jc.code = (jc.msg=="100"?100:101);
            string skq = "{\"code\":" + jc.code.ToString() + ",\"msg\":\"" + jc.msg.ToString() + "\", \"data\":" + (jc.data.ToString().Trim() ==""?"[]":jc.data.ToString()) + "}";
            return Content(skq, "application/json");
        }

         

        bool IsValid(ref dynamic data, bool bAdd = true)
        {
            Guid id1;
            msg = "";
            if (!bAdd) {
                if (!Guid.TryParse(data.id.ToString(), out id1))
                {
                    msg += "<br> - ID is not correnct.";
                }
            }

            if (data.uidname.ToString().Trim() == "") msg += "<br> - Nhập email đăng nhập.";
            if (data.name.ToString().Trim() == "") msg += "<br> - Nhập họ tên";
           
          
            data.uidname = common.ChuanHoaXau(data.uidname.ToString());
            data.name = common.ChuanHoaXau(data.name.ToString());
            data.avata = "";// common.ChuanHoaXau(data.avata.ToString());
            data.tel = common.ChuanHoaXau(data.tel.ToString());

            if (bAdd)
            {
                if (data.pwd.ToString().Trim() == "") msg += "<br> - Nhập mật khẩu.";
                if (data.pwd1.ToString().Trim() == "") msg += "<br> - Nhập mật khẩu xác nhận.";
                if (string.Compare(data.pwd1.ToString().Trim(), data.pwd1.ToString().Trim(), false) != 0 ) msg += "<br> - Nhập mật khẩu xác nhận = mật khẩu.";

                // check tên đăng nhập ok
                if (mdl.existsUser(data.uidname.ToString().Trim())) msg += "<br> - Email đăng nhập đã có.";
            }

            if (msg != "") return false;
 
            return true;
        }
        // public string Insert(string data)
        public ActionResult Insert(string data)
        {
            dynamic row = JObject.Parse(data);
            row.id = Guid.NewGuid().ToString();

            string skeys = "uidname;name;tel;qid;cnid;active;pwd;pwd1;";

            if (!common.CheckObjectEnoughProperies(row, skeys, ref msg))
            {
                jc.code = 121;
                jc.msg = "";
                return Json(jc, JsonRequestBehavior.AllowGet);
            }

            if (!IsValid(ref row))
            {
                jc.code = 101;
                jc.msg = msg;
                return    Json(jc, JsonRequestBehavior.AllowGet);
            }
            mdl.Insert(row);
            jc.code = (mdl.msg=="100"?100:101);
            jc.msg = mdl.msg;
         //   jc.data = row.id.ToString();
            jc.data.key = row.id.ToString();
            return Json(jc, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Update(string data)
        {

            dynamic row = JObject.Parse(data);
            string skeys = "id;uidname;name;tel;qid;cnid;active;pwd;pwd1;";

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

            if (!IsValidImport(  arr))
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
            if (data.Trim()=="")
            {
                jc.code = 121;
                jc.msg = "No records selected to delete.";
                return Json(jc, JsonRequestBehavior.AllowGet);
            }
            mdl.Delete(data);
         
            jc.code = (mdl.msg == "100" ? 100 : 101);
            jc.msg = mdl.msg;
            jc.data= mdl.iRowEffected;
            return Json(jc, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ActiveDeactive(string data)
        {
            data = data.Replace(" ", "");
            if (data.Trim() == "")
            {
                jc.code = 121;
                jc.msg = "No records selected to delete.";
                return Json(jc, JsonRequestBehavior.AllowGet);
            }
            mdl.Idlist = data;
            mdl.ActiveDeactive(ref jc.msg);
            jc.code = (jc.msg == "100" ? 100 : 101);
            jc.data = mdl.iRowEffected;
            return Json(jc, JsonRequestBehavior.AllowGet);
        }

        bool IsValidChangePassword(ref dynamic data)
        {

            data.op = data.op.ToString().Replace("'", "");
            data.np = data.np.ToString().Replace("'", "");
            data.np1 = data.np1.ToString().Replace("'", "");
            msg = "";

            if (data.op.ToString().Trim() == "") msg += "<br> - Vào mật khẩu cũ.";
            if (data.np.ToString().Trim() == "") msg += "<br> - Vào mật khẩu mới.";
            if (data.np1.ToString().Trim() == "") msg += "<br> - Vào mật khẩu xác nhận.";
            if (String.Compare(data.np1.ToString().Trim(), data.np1.ToString().Trim(), false) != 0) msg += "<br> - Mật khẩu mới và mật khẩu xác nhận phải bằng nhau";

            if (!mdl.checkCurrentPassword(data.op.ToString(), ref jc.msg))
            {
                msg += "<br>- " + jc.msg;
            }

            if (msg != "") return false;

            return true;
        }


        public ActionResult ChangePassword(string data)
        {
            dynamic row = JObject.Parse(data);
            string skeys = "op;np;np1;";

            if (!common.CheckObjectEnoughProperies(row, skeys, ref msg))
            {
                jc.code = 121;
                jc.msg = "";
                return Json(jc, JsonRequestBehavior.AllowGet);
            }

            if (!IsValidChangePassword(ref row))
            {
                jc.code = 101;
                jc.msg = msg;
                return Json(jc, JsonRequestBehavior.AllowGet);
            }
            mdl.ChangePassword(row.np.ToString(), ref jc.msg);
            jc.code = (jc.msg == "100" ? 100 : 101);
            jc.data = mdl.iRowEffected.ToString();
            return Json(jc, JsonRequestBehavior.AllowGet);
        }
        protected override void Dispose(bool disposing)
        {

        }
    }
}