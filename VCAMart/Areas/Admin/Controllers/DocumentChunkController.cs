using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using RICTotalAdmin.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace RICTotalAdmin.Controllers
{
    public class DocumentChunkController:Controller
    {
        string msg = "";
        jsonClientPost jc = new jsonClientPost();
        DocumentChunkModel mdl = new DocumentChunkModel();

        public string tblname;
        public string tbaoquyenxoa = "Bạn không có quyền xóa thông tin này";
        public string tbaoquyensua = "Bạn không có quyền sửa thông tin này";
        public string tbaoquyenthem = "Bạn không có quyền thêm thông tin này";
        public DocumentChunkController()
        {
            msg = "";
            tblname = "tblDocumentChunk";
            ViewBag.Function_Title = "DANH MỤC CÁC ĐOẠN TRONG TÀI LIỆU";
            ViewBag.Function_Title_Short = "Document-Chunk";
            ViewBag.ControllerName = this.GetType().Name.Replace("Controller", "");
            ViewBag.tblname = tblname;
            ViewBag.mn_tt = "";

            try
            {
                ViewBag.TotalNotChunk = mdl.GetDocumentNoChunk(); 
                ViewBag.pagesize = int.Parse(System.Web.HttpContext.Current.Session["userPageSize"].ToString());
            }
            catch (Exception)
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


            ViewBag.congty_ten = lg.congty_ten;
            ViewBag.congty_diachi = lg.congty_diachi;
            ViewBag.congty_tel = lg.congty_tel;


        }
        public ActionResult Index(string id)
        {
           

            ViewBag.mn_tt = id;

           
            ViewBag.Function_Title = "DANH MỤC CÁC ĐOẠN TRONG TÀI LIỆU";
            ViewBag.Function_Title_Short = "Document-Chunk";
            return View("~/Areas/Admin/Views/DocumentChunk/Index.cshtml");

        }

        public ActionResult GetList(int pageSize = 20, int page = 1)
        {
            
            int total = 0;
            pageSize = ViewBag.pagesize;
            string data = mdl.GetList(pageSize.ToString(), page.ToString(), ref total);
            if (data == "") data = "[]";
            jc.data = data;
            jc.code = total;
            return Json(new { total = total, data = data }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult RunChunkBatch(string data,int batchSize = 20)
        {

            dynamic row = JObject.Parse(data);
            int numDocument = batchSize;
            try {
                numDocument = Convert.ToInt32(row.numDocument);
            }
            catch { }
            bool ok = mdl.RunChunkBatch(numDocument);
            int totalNotChunk = mdl.GetDocumentNoChunk();
            return Json(new
            {
                code = ok ? 100 : 101,
                msg = mdl.msg,
                processed = mdl.iRowEffected,
                total_not_chunked = totalNotChunk
            }, JsonRequestBehavior.AllowGet);

            /*jc.data = mdl.RunChunkBatch(batchSize);
            jc.msg = mdl.msg;
            jc.code = (jc.msg == "100" ? 100 : 101);
            
            return Json(jc, JsonRequestBehavior.AllowGet);*/

        }
    }
}