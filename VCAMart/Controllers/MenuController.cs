using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using Newtonsoft.Json.Linq;
using RICTotal.Models;

 

namespace RICTotal.Controllers
{
    public class MenuController:Controller
    {
        string msg = "";
        jsonClientPost jc = new jsonClientPost();
        Menu mdl = new Menu();

        public string tblname;
        public MenuController()
        {
            msg = "";
            tblname = "tblMenu";
            ViewBag.Function_Title = "DANH SÁCH CHỨC NĂNG";
            ViewBag.Function_Title_Short = "Menu";
            ViewBag.ControllerName = this.GetType().Name.Replace("Controller", "");
            ViewBag.mn_tt = "";
            ViewBag.tblname = tblname;
            try
            {
                ViewBag.pagesize = int.Parse(System.Web.HttpContext.Current.Session["userPageSize"].ToString());
            }
            catch (Exception e1)
            {
                ViewBag.pagesize = 10;
            }

            jc = new jsonClientPost();
           
               
            RICTotal.Models.common.InitDefault();
            LIB.CLogin lg = new LIB.CLogin(RICTotal.Models.common.strcnn, RICTotal.Models.common.userCookieTokenName);

            if (!lg.checkTokenLogin())
            {
                lg.GotoLoginPage(); return;
            }

            ViewBag.login_nv_id = lg.userId;
            ViewBag.user_uid = lg.userUid;
            ViewBag.user_fullname = lg.userFullName;
            ViewBag.user_avata = lg.userAvata; //
            ViewBag.UserMenu = RICTotal.Models.common.GetMenuAdmin(lg.userId); //common.GetMenuAdmin(lg.userId);
            ViewBag.user_fromdate = lg.userCreated;

            ViewBag.hansudung = (lg.songaycondung < 0 ? "<b class='bg-red hansudung'>Phần mềm đã hết hạn sử dụng</b>&nbsp;&nbsp;  " : (lg.songaycondung + 1 <= 5 ? "<b class='bg-red hansudung'>Phần mềm sẽ hết hạn sử dụng trong " + (lg.songaycondung + 1).ToString() + " ngày tới (<i>" + DateTime.Now.AddDays(lg.songaycondung + 1).ToString("dd/MM/yyyy") + "</i>)</b>&nbsp;&nbsp;  " : ""));

            ViewBag.congty_ten = lg.congty_ten;
            ViewBag.congty_diachi = lg.congty_diachi;
            ViewBag.congty_tel = lg.congty_tel;

            mdl.rc_id = lg.rc_id;
        }
        public ActionResult Index(string id)
        {
         /*  if (!common.checkQuyenSudung(mdl.userId.ToString(), this.GetType().Name.Replace("Controller", ""), ref msg))
            {
                return View("~/Views/MSG/" + msg);
            } */
            ViewBag.mn_tt = id;
            ViewBag.Function_Title = "DANH MỤC CHỨC NĂNG";
            ViewBag.Function_Title_Short = "Menu";
            return View("~/Views/Menu/Index.cshtml");
        }

        public ActionResult GetList()
        {
            jc.data = mdl.GetList(0);
            jc.msg = mdl.msg;
            jc.code = (jc.msg == "100" ? 100 : 101);
            string skq = "{\"code\":" + jc.code.ToString() + ",\"msg\":\"" + jc.msg.ToString() + "\", \"data\":" + (jc.data.ToString().Trim() == "" ? "[]" : jc.data.ToString()) + "}";
            return Content(skq, "application/json");
        }


        public ActionResult GetParent()
        {
            jc.data = mdl.GetParent(0);
            jc.msg = mdl.msg;
            jc.code = (jc.msg == "100" ? 100 : 101);
            string skq = "{\"code\":" + jc.code.ToString() + ",\"msg\":\"" + jc.msg.ToString() + "\", \"data\":" + (jc.data.ToString().Trim() == "" ? "[]" : jc.data.ToString()) + "}";
            return Content(skq, "application/json");
        }

        bool IsValid(ref dynamic data, bool bAdd = true)
        {
            /*Guid id1;
            msg = "";
            if (!bAdd)
            {
                if (!Guid.TryParse(data.id.ToString(), out id1))
                {
                    msg += "<br> - ID is not correnct.";
                }
            }

            if (data.uidname.ToString().Trim() == "") msg += "<br> - Nhập email đăng nhập.";
            if (data.name.ToString().Trim() == "") msg += "<br> - Nhập họ tên";


            data.uidname = common.ChuanHoaXau(data.uidname.ToString());
            data.name = common.ChuanHoaXau(data.name.ToString());
            data.avata = ""; 
            data.tel = common.ChuanHoaXau(data.tel.ToString());

            if (bAdd)
            {
                if (data.pwd.ToString().Trim() == "") msg += "<br> - Nhập mật khẩu.";
                if (data.pwd1.ToString().Trim() == "") msg += "<br> - Nhập mật khẩu xác nhận.";
                if (string.Compare(data.pwd1.ToString().Trim(), data.pwd1.ToString().Trim(), false) != 0) msg += "<br> - Nhập mật khẩu xác nhận = mật khẩu.";

                // check tên đăng nhập ok
                if (mdl.existsUser(data.uidname.ToString().Trim())) msg += "<br> - Email đăng nhập đã có.";
            }

            if (msg != "") return false;*/

            return true;
        }
        public ActionResult Update(string data)
        {

            dynamic row = JObject.Parse(data);
            string skeys = "MN_Id;MN_Parent_Id;MN_Ten;MN_Url;MN_Icon;MN_Thutu;active;";

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

        // public string Insert(string data)
        public ActionResult Insert(string data, string uid)
        {
            uid = ViewBag.login_nv_id.ToString();
            dynamic row = JObject.Parse(data);
            row.MN_Id = Guid.NewGuid().ToString();

            string skeys = "MN_Id;MN_Parent_Id;MN_Ten;MN_Url;MN_Icon;MN_Thutu;active;";

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
                return Json(jc, JsonRequestBehavior.AllowGet);
            }
            mdl.Insert(row, uid);
            jc.code = (mdl.msg == "100" ? 100 : 101);
            jc.msg = mdl.msg;
            //   jc.data = row.id.ToString();
            jc.data.key = row.MN_Id.ToString();
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
       
    }
}