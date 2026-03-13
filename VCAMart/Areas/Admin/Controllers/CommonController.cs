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
    public class CommonController : Controller
    {
        string rc_id;
        string user_id;//= System.Web.HttpContext.Current.Session["user_id"].ToString();
        string ch_id;//= System.Web.HttpContext.Current.Session["ch_id_x"].ToString();
        string msg = "";
        jsonClientPost jc = new jsonClientPost();
        // GET: Common
        public CommonController()
        {
            try
            {
                 rc_id = System.Web.HttpContext.Current.Session["rc_id"].ToString();
                 user_id = System.Web.HttpContext.Current.Session["user_id"].ToString();
                 ch_id = System.Web.HttpContext.Current.Session["ch_id_x"].ToString();
            } catch  { }
        }
        public ActionResult Index()
        {
            return View();
        }

        public  void InitViewBag(LIB.CLogin lg)
        {
            
            // khởi tạo các viewbag dùng chung
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

        public ActionResult GetTinh()
        {
            string data = common.GetTinh();
            data = (data == "" ? "{\"msg\":\"101\"}" : "{\"msg\":\"100\", \"data\":" + data + "}");
            return Content(data, "application/json");
        }
        public ActionResult GetParentMenus()
        {
            string data = common.GetParentMenus();
            data = (data == "" ? "{\"msg\":\"101\"}" : "{\"msg\":\"100\", \"data\":" + data + "}");
            return Content(data, "application/json");
        }

        public ActionResult GetPermissionGroups()
        {
            string data = common.GetPermisionGroups();
            data = (data == "" ? "{\"msg\":\"101\"}" : "{\"msg\":\"100\", \"data\":" + data + "}");
            return Content(data, "application/json");
        }

        public ActionResult GetAllUser()
        {
            string data = common.GetAllUser();
            data = (data == "" ? "{\"msg\":\"101\"}" : "{\"msg\":\"100\", \"data\":" + data + "}");
            return Content(data, "application/json");
        }
      

         

        public ActionResult Upload()
        {
            //   dynamic row = JObject.Parse(data1);
            string data = "";
            string type = Request["type"].ToString();
            string refid = Request["refid"].ToString();
            string urlupload = common.ImageSavePath(type);
            int maxwidth = 500, maxheight = 500;

            try
            {
                for (int i = 0; i < Request.Files.Count; i++)
                {
                    var file = Request.Files[i];
                    var fileName = System.IO.Path.GetFileName(file.FileName).Replace(" ", "").Replace(",", "").Replace("-", "");
                    var path = Server.MapPath("~" + urlupload);

                    if (!System.IO.Directory.Exists(path)) System.IO.Directory.CreateDirectory(path);

                    var fname = common.ImageFileName(type, rc_id);

                    data += (data == "" ? "" : ",") + "\"" + urlupload + fname + "\"";
                    fname = System.IO.Path.Combine(path, fname);

                    System.Drawing.Bitmap bitmap_file = common.ImageResize(System.Drawing.Image.FromStream(file.InputStream), maxwidth, maxheight);
                    bitmap_file.Save(fname, System.Drawing.Imaging.ImageFormat.Png);
                    bitmap_file.Dispose();

                    common.ImageInsert(fname, type, refid, "", rc_id, ref msg, file.InputStream.Length, maxwidth, maxheight);
                    if (msg != "100")
                    {
                        jc.msg = msg;
                        jc.code = 101;
                        jc.data = "null";
                        return Json(jc, JsonRequestBehavior.AllowGet);
                    }
                }
                jc.data = "[" + data + "]";
            }
            catch
            {
                jc.data = "[]";
            }
            string skq = "{\"code\":" + jc.code.ToString() + ",\"msg\":\"" + jc.msg.ToString() + "\", \"data\":" + (jc.data.ToString().Trim() == "" ? "[]" : jc.data.ToString()) + "}";
            return Content(skq, "application/json");

        }

      

        
    } 

    }