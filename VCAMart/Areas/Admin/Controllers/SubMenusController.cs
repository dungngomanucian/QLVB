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
 
namespace RICTotalAdmin.Controllers
{
    [ValidateInput(false)]
    public class SubMenusController : Controller
    {
        
        public string tblname;
        jsonClientPost jc;
         
        int lnxid = 20;

        //tblNhapxuat mdl = new tblNhapxuat(common.strcnn);
        //string view_folder = "Views/Bill/ChuyenShipper";

        public SubMenusController()
        {
             
            ViewBag.view_folder = "";// view_folder;
            ViewBag.lnx = lnxid;

            tblname = "tblMenu";
            
            ViewBag.Function_Title = "".ToUpper();
            ViewBag.Function_Title_Short = "Chức năng";
            ViewBag.ControllerName = "SubMenus";// + this.GetType().Name.Replace("Controller", "");

            ViewBag.tblname = "";// mdl.tableName;
            
            jc = new jsonClientPost();

            RICTotalAdmin.Models.common.InitDefault();
            LIB.CLogin lg = new LIB.CLogin(RICTotalAdmin.Models.common.strcnn, RICTotalAdmin.Models.common.userCookieTokenName);

            if (!lg.checkTokenLogin())
            { lg.GotoLoginPage(); return; }
            //  ViewBag.login_nv_id = lg.nhanvienId;

            ViewBag.login_nv_id = lg.userId;
            ViewBag.user_uid = lg.userUid;
            ViewBag.user_fullname = lg.userFullName;
            ViewBag.user_avata = lg.userAvata; //
            ViewBag.UserMenu = RICTotalAdmin.Models.common.GetMenuAdmin(lg.userId); //common.GetMenuAdmin(lg.userId);
            ViewBag.user_fromdate = lg.userCreated;

            ViewBag.hansudung = (lg.songaycondung < 0 ? "<b class='bg-red hansudung'>Phần mềm đã hết hạn sử dụng</b>&nbsp;&nbsp;  " : (lg.songaycondung + 1 <= 5 ? "<b class='bg-red hansudung'>Phần mềm sẽ hết hạn sử dụng trong " + (lg.songaycondung + 1).ToString() + " ngày tới (<i>" + DateTime.Now.AddDays(lg.songaycondung + 1).ToString("dd/MM/yyyy") + "</i>)</b>&nbsp;&nbsp;  " : ""));

         
              
             
          
             
        } // GET: SubMenus
        public ActionResult List(string id)
        {

            ViewBag.SubMenus = RICTotalAdmin.Models.common.GetSubMenus(ViewBag.login_nv_id, id); //common.GetMenuAdmin(lg.userId);
            return View();
        }


        public ActionResult Index()
        {
            return View();
        }
    }
}