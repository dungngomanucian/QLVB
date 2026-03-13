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
using System.Xml.Linq;


namespace RICTotalAdmin.Controllers
{
    public class DocumentController:Controller
    {
        string msg = "";
        jsonClientPost jc = new jsonClientPost();
        DocumentModel mdl = new DocumentModel();

        public string tblname;
        public string tbaoquyenxoa = "Bạn không có quyền xóa thông tin này";
        public string tbaoquyensua = "Bạn không có quyền sửa thông tin này";
        public string tbaoquyenthem = "Bạn không có quyền thêm thông tin này";
        public DocumentController()
        {
            msg = "";
            tblname = "tblDocument";
            ViewBag.Function_Title = "DANH MỤC TÀI LIỆU";
            ViewBag.Function_Title_Short = "Document";
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

            ViewBag.Function_Title = "DANH MỤC TÀI LIỆU";
            ViewBag.Function_Title_Short = "Document";
            return View("~/Areas/Admin/Views/Document/Index.cshtml");

        }

        public ActionResult GetList(int pageSize = 20, int page = 1)
        {
            int total = 0;
            try { pageSize = int.Parse(System.Web.HttpContext.Current.Session["userPageSize"].ToString()); } catch { }

            string data = mdl.GetList(pageSize.ToString(), page.ToString(), ref total);

            if (data == "") data = "[]";
            jc.data = data;
            jc.code = total;
            return Json(new { total = total, data = data }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetCategoryTree()
        {
            string data = mdl.GetCategoryTree();
            return Content(data, "application/json");
        }

        public ActionResult GetLawGroups(string prefix)
        {
            string strcnn = System.Configuration.ConfigurationManager.AppSettings["cnn"].ToString();
            using (var conn = new SqlConnection(strcnn))
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
SELECT group_code, group_title
FROM tblGroupDocument
WHERE ISNULL(is_active, 1) = 1
  AND (@prefix IS NULL OR group_code LIKE (@prefix + '%'))
ORDER BY group_title";

                cmd.Parameters.Add("@prefix", SqlDbType.NVarChar, 10).Value =
                    (object)(string.IsNullOrWhiteSpace(prefix) ? (object)DBNull.Value : prefix.Trim());

                var dt = new DataTable();
                var da = new SqlDataAdapter(cmd);
                da.Fill(dt);

                var list = dt.AsEnumerable()
                    .Select(r => new
                    {
                        group_code = r["group_code"].ToString(),
                        group_title = r["group_title"].ToString()
                    })
                    .ToList();

                return Json(list, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetLawCategories(string groupCode, string prefix)
        {
            string strcnn = System.Configuration.ConfigurationManager.AppSettings["cnn"].ToString();
            using (var conn = new SqlConnection(strcnn))
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
SELECT c.category_code, c.category_title
FROM tblCategoryDocument c
LEFT JOIN tblGroupDocument g ON c.group_id = g.group_code
WHERE ISNULL(c.is_active, 1) = 1
  AND (@groupCode IS NULL OR g.group_code = @groupCode)
  AND (@prefix IS NULL OR g.group_code LIKE (@prefix + '%'))
ORDER BY c.sort_order, c.category_title";

                cmd.Parameters.Add("@groupCode", SqlDbType.NVarChar, 100).Value =
                    (object)(groupCode ?? (object)DBNull.Value);
                cmd.Parameters.Add("@prefix", SqlDbType.NVarChar, 10).Value =
                    (object)(string.IsNullOrWhiteSpace(prefix) ? (object)DBNull.Value : prefix.Trim());

                var dt = new DataTable();
                var da = new SqlDataAdapter(cmd);
                da.Fill(dt);

                var list = dt.AsEnumerable()
                    .Select(r => new
                    {
                        category_code = r["category_code"].ToString(),
                        category_title = r["category_title"].ToString()
                    })
                    .ToList();

                return Json(list, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult ImportJsonToDatabase(string data, string uid, string id)
        {
            uid = ViewBag.login_nv_id.ToString();
            dynamic row = JObject.Parse(data);
            row.id = Guid.NewGuid().ToString();

            if (!IsValidUploadFile(ref row))
            {
                jc.code = 101;
                jc.msg = msg;
                return Json(jc, JsonRequestBehavior.AllowGet);
            }

            string fileJson = row.jsonFile.ToString();
            string jsonPath = Server.MapPath(fileJson);
            /*string file = Server.MapPath(fileJson);
            string directory = Path.GetDirectoryName(file);
            string fileName = System.IO.Path.GetFileName(file);
            string name = fileName.Split('.').First();
            string jsonPath = Path.Combine(directory, name + ".json");
            if (System.IO.File.Exists(file))
            {
                try
                {
                    System.IO.File.Move(file, jsonPath);
                }
                catch { }

            }*/
            jc.data = mdl.ImportJsonToDatabase(jsonPath, row);
            jc.msg = mdl.msg;
            jc.code = (jc.msg == "100" ? 100 : 101);
            // jc.data.key = row.id.ToString();
            return Json(jc, JsonRequestBehavior.AllowGet);
        }
        bool IsValidUploadFile(ref dynamic data, bool bAdd = true)
        {
            bool kt = true;
            string uid = ViewBag.login_nv_id.ToString();


            if (data.jsonFile == "") kt = false;

            return kt;
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
            if (data.doc_code == null || data.doc_code.ToString().Trim() == "")
            {
                msg = "Chưa nhập Mã tài liệu.";
                return false;
            }
            if (data.doc_title == null || data.doc_title.ToString().Trim() == "")
            {
                msg = "Chưa nhập Tên/Trích yếu tài liệu.";
                return false;
            }

            // 3. Check Loại tài liệu
            /*
            if (data.category_id == null || data.category_id.ToString().Trim() == "")
            {
                msg = "Chưa chọn Loại tài liệu.";
                return false;
            }
            */

            return kt;
        }

        public ActionResult Insert(string data, string uid, string id)
        {
            uid = ViewBag.login_nv_id.ToString();
            dynamic row = JObject.Parse(data);
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
            jc.data = new { key = row.id.ToString() };
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
            jc.data = new { key = row.id.ToString() };
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
                jc.msg = "Chưa chọn bản ghi nào để xóa.";
                return Json(jc, JsonRequestBehavior.AllowGet);
            }
            mdl.Delete(data);

            jc.code = (mdl.msg == "100" ? 100 : 101);
            jc.msg = mdl.msg;
            jc.data = mdl.iRowEffected;
            return Json(jc, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UploadPdf(string id)
        {
            // Nhận file PDF từ client (kéo thả / chọn file) và lưu vào ~/Uploads/Documents/
            try
            {
                if (Request.Files == null || Request.Files.Count == 0)
                {
                    return Json(new { code = 101, msg = "Chưa chọn tệp tin.", data = (object)null }, JsonRequestBehavior.AllowGet);
                }

                var file = Request.Files[0];
                if (file == null || file.ContentLength <= 0)
                {
                    return Json(new { code = 101, msg = "Tệp tin không hợp lệ.", data = (object)null }, JsonRequestBehavior.AllowGet);
                }

                var ext = Path.GetExtension(file.FileName) ?? "";
                if (!ext.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    return Json(new { code = 101, msg = "Chỉ hỗ trợ tệp PDF (*.pdf).", data = (object)null }, JsonRequestBehavior.AllowGet);
                }

                // 50MB mặc định (đồng bộ với Web.config nếu có set giới hạn)
                const int maxBytes = 50 * 1024 * 1024;
                if (file.ContentLength > maxBytes)
                {
                    return Json(new { code = 101, msg = "Tệp tin quá lớn (tối đa 50MB).", data = (object)null }, JsonRequestBehavior.AllowGet);
                }

                var groupCode = Request["groupCode"];
                var categoryCode = Request["categoryCode"];
                if (string.IsNullOrWhiteSpace(groupCode) || string.IsNullOrWhiteSpace(categoryCode))
                {
                    return Json(new { code = 101, msg = "Bạn phải chọn đầy đủ Luật/Bộ luật và Nhóm (Dân sự, Hình sự, ...).", data = (object)null }, JsonRequestBehavior.AllowGet);
                }

                var uploadDirVirtual = "~/Uploads/Documents/";
                var uploadDirPhysical = Server.MapPath(uploadDirVirtual);
                if (!Directory.Exists(uploadDirPhysical)) Directory.CreateDirectory(uploadDirPhysical);

                var safeFileName = "DOC_" + Guid.NewGuid().ToString("N") + ".pdf";
                var savePath = Path.Combine(uploadDirPhysical, safeFileName);
                file.SaveAs(savePath);

                var fileUrl = VirtualPathUtility.ToAbsolute(uploadDirVirtual + safeFileName);

                // Đọc & trích nội dung pháp lý
                var fullText = PdfLawExtractor.ReadAllText(savePath);
                var extract = PdfLawExtractor.ExtractLegalSegment(fullText);
                if (!extract.Success)
                {
                    return Json(new
                    {
                        code = 101,
                        msg = extract.Message,
                        data = (object)null
                    }, JsonRequestBehavior.AllowGet);
                }

                // Ghi bản ghi vào tblDocument
                var title = Path.GetFileNameWithoutExtension(file.FileName);
                var documentId = PdfLawExtractor.InsertLegalDocumentToDb(
                    extract.LawNumber,
                    categoryCode,
                    fileUrl,
                    title,
                    extract.Body);

                // Auto embedding liền mạch sau khi đã có tblRagUnit_Search
                var ragModel = new RagUnitModel();
                bool embedOk = ragModel.SyncRagUnitSearchEmbeddingByDocument(documentId, 3000);
                if (!embedOk)
                {
                    return Json(new
                    {
                        code = 101,
                        msg = "Lưu dữ liệu thành công nhưng embedding thất bại: " + ragModel.msg,
                        data = (object)null
                    }, JsonRequestBehavior.AllowGet);
                }

                return Json(new
                {
                    code = 100,
                    msg = "100",
                    data = new
                    {
                        fileUrl,
                        originalName = Path.GetFileName(file.FileName),
                        size = file.ContentLength,
                        embedded = ragModel.LastEmbeddedCount
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { code = 101, msg = ex.Message, data = (object)null }, JsonRequestBehavior.AllowGet);
            }
        }

    }
}