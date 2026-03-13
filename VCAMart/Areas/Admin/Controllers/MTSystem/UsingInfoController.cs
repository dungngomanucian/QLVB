using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using RICTotalAdmin.Models;

namespace RICTotalAdmin.Controllers.MTSystem
{
    public class UsingInfoController : Controller
    {
        string msg = "", tblname  = "UsingInfo";
        jsonClientPost jc = new jsonClientPost();
        UsingInfo mdl = new UsingInfo();

        public UsingInfoController()
        {
            ViewBag.tblname = tblname;
            ViewBag.Function_Title = "THÔNG TIN SỬ DỤNG";
            ViewBag.Function_Title_Short = "Thông tin khách hàng";
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

        }
        // GET: MenuAdmin
        public ActionResult Index()
        {
            if (!common.checkQuyenSudung(mdl.userId.ToString(), this.GetType().Name.Replace("Controller", ""), ref msg))
            {
                return View("~/Views/MSG/" + msg);
            }

         
            return View("~/Views/MTSystem/UsingInfo.cshtml");
        }

        public ActionResult GetList()
        {
            jc.data = mdl.GetList();
            jc.msg = mdl.msg;
            jc.code = (jc.msg == "100" ? 100 : 101);

            // load nganh nghe
            string nganh = "[]";
            // load tinh
            string tinh = common.GetTinh();

            string skq = "{\"code\":" + jc.code.ToString() + ",\"msg\":\"" + jc.msg.ToString() + "\", \"data\":" + (jc.data.ToString().Trim() == "" ? "[]" : jc.data.ToString()) + ", \"tinh\":" + (tinh == "" ? "[]" : tinh) + ", \"nganh\":" + (nganh == "" ? "[]" : nganh) + "}";

            return Content(skq, "application/json");
        }



        bool IsValid(ref dynamic data, bool bAdd = true)
        {
            
            msg = "";
  
            data.name= common.ChuanHoaXau(data.name.ToString());
            data.addr = common.ChuanHoaXau(data.addr.ToString());
            data.tel= common.ChuanHoaXau(data.tel.ToString());
            data.email= common.ChuanHoaXau(data.email.ToString());
            data.web = common.ChuanHoaXau(data.web.ToString());
            data.logo = common.ChuanHoaXau(data.logo.ToString(),"/");
            data.daidien = common.ChuanHoaXau(data.daidien.ToString());
            data.ngaysinh = common.ChuanHoaXau(data.ngaysinh.ToString(), "/");
            data.tinhid = common.ChuanHoaXau(data.tinhid.ToString());
            data.nganhid = common.ChuanHoaXau(data.nganhid.ToString());

            if (data.name.ToString().Trim() == "")
            {
                msg += "<br> - Cần vào Tên";
            }
            if (data.addr.ToString().Trim() == "")
            {
                msg += "<br> - Cần vào Địa chỉ";
            }
            if (data.tel.ToString().Trim() == "")
            {
                msg += "<br> - Cần vào Điện thoại";
            }

            if (msg != "") return false;

            return true;
        }
        // public string Insert(string data)
      
        public ActionResult Update(string data)
        {

            dynamic row = JObject.Parse(data);
            string skeys = "ngaysinh;id;name;addr;tel;daidien;tinhid;nganhid;email;web;logo;";

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
       

        protected override void Dispose(bool disposing)
        {

        }
    }
}