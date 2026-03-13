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
    public class CategoryDocumentController:Controller
    {
        string msg = "";
        jsonClientPost jc = new jsonClientPost();
        CategoryDocumentModel mdl = new CategoryDocumentModel();

        public string tblname;
        public string tbaoquyenxoa = "Bạn không có quyền xóa thông tin này";
        public string tbaoquyensua = "Bạn không có quyền sửa thông tin này";
        public string tbaoquyenthem = "Bạn không có quyền thêm thông tin này";
        public CategoryDocumentController()
        {
            msg = "";
            tblname = "tblCategoryDocument";
            ViewBag.Function_Title = "DANH SÁCH LOẠI TÀI LIỆU";
            ViewBag.Function_Title_Short = "LoaiTaiLieu";
            ViewBag.ControllerName = this.GetType().Name.Replace("Controller", "");
            ViewBag.tblname = tblname;
            ViewBag.mn_tt = "";

            try
            {
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
            /*  if (!common.checkQuyenSudung(mdl.userId.ToString(), this.GetType().Name.Replace("Controller", ""), ref msg))
               {
                   return View("~/Views/MSG/" + msg);
               } */

            ViewBag.mn_tt = id;

            ViewBag.Function_Title = "DANH SÁCH LOẠI TÀI LIỆU";
            ViewBag.Function_Title_Short = "LoaiTaiLieu";
            return View("~/Areas/Admin/Views/CategoryDocument/Index.cshtml");

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
        public ActionResult GetParent()
        {
            jc.data = mdl.GetParent(0);
            jc.msg = mdl.msg;
            jc.code = (jc.msg == "100" ? 100 : 101);
            string skq = "{\"code\":" + jc.code.ToString() + ",\"msg\":\"" + jc.msg.ToString() + "\", \"data\":" + (jc.data.ToString().Trim() == "" ? "[]" : jc.data.ToString()) + "}";
            return Content(skq, "application/json");
        }
        bool CheckQuyen(int Type, string id)
        {
            /*Type=1: Them; 2: Sua; 3: Xoa*/

            string strcnn;
            strcnn = System.Configuration.ConfigurationManager.AppSettings["cnn"].ToString();
            string MN_Id = "";
            string uid = ViewBag.login_nv_id.ToString();
            string quyen = "";
            DataTable dt = new DataTable();
            DataSet ds = new DataSet();
            string strSQL = "Select * From tblMenu Where MN_TT=" + id;
            SqlConnection conn = new SqlConnection(strcnn);
            try
            {
                conn.Open();
                SqlDataAdapter da = new SqlDataAdapter(strSQL, conn);
                da.Fill(ds, "data");
                dt = ds.Tables[0];
                if (dt.Rows.Count > 0)
                    MN_Id = dt.Rows[0]["MN_Id"].ToString();
                dt.Dispose();
                conn.Close();
                conn.Dispose();
            }
            catch
            {
            }
            strSQL = "select * From tblNhomQuyen_Menu as a where NQMN_NQ_ID in ";
            strSQL += "(select UNQ_NQ_ID from tblUser_NhomQuyen Where UNQ_USER_ID ='" + uid + "') ";
            strSQL += " and a.NQMN_MN_ID ='" + MN_Id + "'";
            conn = new SqlConnection(strcnn);
            try
            {
                conn.Open();
                SqlDataAdapter da = new SqlDataAdapter(strSQL, conn);
                ds = new DataSet();
                da.Fill(ds, "data1");
                dt = ds.Tables[0];
                if (Type == 1)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        quyen += dt.Rows[i]["NQMN_THEM"].ToString();
                    }
                }
                else if (Type == 2)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        quyen += dt.Rows[i]["NQMN_SUA"].ToString();
                    }
                }
                else if (Type == 3)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        quyen += dt.Rows[i]["NQMN_XOA"].ToString();
                    }
                }

                dt.Dispose();
                conn.Close();
            }
            catch
            {
            }

            if (quyen.Contains("1"))
                return true;
            else return true;
        }

        bool IsValid(ref dynamic data, bool bAdd = true)
        {
            bool kt = true;
            string uid = ViewBag.login_nv_id.ToString();

            //if (data.CV_CQL_Id == "00000000-0000-0000-0000-000000000000")
            //    kt = false;
            if (data.category_code == "") kt = false;
            if (data.category_title == "") kt = false;
            return kt;
        }
        public ActionResult Insert(string data, string uid, string id)
        {
            uid = ViewBag.login_nv_id.ToString();


            dynamic row = JObject.Parse(data);
            //row.CV_Id = Guid.NewGuid().ToString();
            row.id = Guid.NewGuid().ToString();

            if (!CheckQuyen(1, id))
            {
                jc.code = 101;
                jc.msg = tbaoquyenthem;
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
            jc.data.key = row.id.ToString();
            return Json(jc, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Update(string data, string uid, string id)
        {

            uid = ViewBag.login_nv_id.ToString();

            dynamic row = JObject.Parse(data);


            if (!CheckQuyen(2, id))
            {
                jc.code = 101;
                jc.msg = tbaoquyensua;
                return Json(jc, JsonRequestBehavior.AllowGet);
            }

            if (!IsValid(ref row))
            {
                jc.code = 101;
                jc.msg = msg;
                return Json(jc, JsonRequestBehavior.AllowGet);
            }
            mdl.Update(row, uid);
            jc.code = (mdl.msg == "100" ? 100 : 101);
            jc.msg = mdl.msg;
            //   jc.data = row.id.ToString();
            jc.data.key = row.id.ToString();
            return Json(jc, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Delete(string data, string id)
        {
            if (!CheckQuyen(3, id))
            {
                jc.code = 101;
                jc.msg = tbaoquyenxoa;
                return Json(jc, JsonRequestBehavior.AllowGet);
            }

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