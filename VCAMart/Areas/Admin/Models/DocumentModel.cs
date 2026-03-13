using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace RICTotalAdmin.Models
{
    public class DocumentModel
    {
        string strcnn;
        public Guid userId;
        public Guid languageId;
        public string msg;
        public int iRowEffected;
        int pageSize;
        SqlCommand cmd;
        string tblname = "tblDocument";
        public string rc_id;

        public string Idlist { get; set; }
        public DocumentModel()
        {
            msg = "";
            strcnn = System.Configuration.ConfigurationManager.AppSettings["cnn"].ToString();

            try
            {
                //rc_id = System.Web.HttpContext.Current.Session["rc_id"].ToString();
                userId = new Guid(System.Web.HttpContext.Current.Session["user_id"].ToString());
                pageSize = int.Parse(System.Web.HttpContext.Current.Session["userPageSize"].ToString());
                languageId = new Guid(System.Web.HttpContext.Current.Session["lang_id"].ToString());
            }
            catch (Exception)
            {
                pageSize = 10;
            }

        }

        public string GetList(string pageSize, string pageIndex, ref int total)
        {
            string msg = "";
            total = 0;
            string sql = "exec Admin.sp_" + tblname + "_Get_List " + pageSize + "," + pageIndex;
            DataTable dt;
            string strcnn = common.GetAppSetting("cnn");
            dt = RICDB.DB.RunSQL(sql, ref msg, strcnn);
            if (msg != "100") return "";
            if (dt.Rows.Count == 0) return "";
            total = int.Parse(dt.Rows[0]["total"].ToString());
            return JsonConvert.SerializeObject(dt);
        }
        public string GetCategoryTree()
        {
            string sql = "exec Admin.sp_tblCategoryDocument_Get_List_Tree";
            DataTable dt;
            string strcnn = common.GetAppSetting("cnn");
            string msg = "";
            dt = RICDB.DB.RunSQL(sql, ref msg, strcnn);
            if (msg != "100") return "[]";

            return JsonConvert.SerializeObject(dt);
        }

        public bool ImportJsonToDatabase(string physicalJsonPath, dynamic row)
        {
            msg = "";
            iRowEffected = 0;
           
            // 1. Đọc file JSON
            string jsonText = File.ReadAllText(physicalJsonPath);
            DetailRootJson root = JsonConvert.DeserializeObject<DetailRootJson>(jsonText);

            if (root == null || root.documents == null)
            {
                msg = "101";
                return false;
            }
            using (SqlConnection conn = new SqlConnection(strcnn))
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();

                try
                {
                    // 2. Duyệt từng document chi tiết
                    foreach (var d in root.documents)
                    {
                        if (string.IsNullOrWhiteSpace(d.procedure_code))
                            continue;

                        string docContent = BuildDocContent(d.sections);
                        if (string.IsNullOrWhiteSpace(docContent))
                            continue;

                        UpdateDocumentContentByCode(
                            conn,
                            tran,
                            d.procedure_code,
                            docContent
                        );

                        iRowEffected++;
                    }

                    tran.Commit();
                    msg = "100";
                    return true;
                }
                catch (Exception ex)
                {
                    try { tran.Rollback(); } catch { }
                    msg = ex.Message;
                    return false;
                }
            }
        }

        private void UpdateDocumentContentByCode(
        SqlConnection conn,
        SqlTransaction tran,
        string docCode,
        string docContent
        )
        {
            string sql = @"
            UPDATE tblDocument
            SET
                doc_content = @doc_content,
                updated_at = GETDATE()
            WHERE doc_code = @doc_code
        ";

            using (SqlCommand cmd = new SqlCommand(sql, conn, tran))
            {
                cmd.Parameters.Add("@doc_content", SqlDbType.NText).Value = docContent;
                cmd.Parameters.Add("@doc_code", SqlDbType.NVarChar, 50).Value = docCode;
                cmd.ExecuteNonQuery();
            }
        }

        private string BuildDocContent(List<DetailSectionNode> sections)
        {
            if (sections == null || sections.Count == 0)
                return null;

            StringBuilder sb = new StringBuilder();

            foreach (var s in sections)
            {
                if (string.IsNullOrWhiteSpace(s.html))
                    continue;
                sb.AppendLine($"<h2>{s.heading}</h2>");
                sb.AppendLine(s.html);
                sb.AppendLine();
            }

            return sb.ToString();
        }


        // --- 2. THÊM MỚI ---
        public bool Insert(dynamic row, string uid)
        {
            string userId = uid;
            common objCommon = new common();

            // Check trùng Mã tài liệu
            if (objCommon.check_code(tblname, "doc_code", row.doc_code.ToString()))
            {
                msg = "Mã tài liệu đã tồn tại trong hệ thống.";
                return false;
            }

            iRowEffected = 0;
            cmd = new SqlCommand("Admin.sp_" + tblname + "_Insert", new SqlConnection(strcnn));
            cmd.CommandType = CommandType.StoredProcedure;

            // Mapping tham số với Stored Procedure
            cmd.Parameters.Add(new SqlParameter("@doc_Id", row.id.ToString()));
            cmd.Parameters.Add(new SqlParameter("@doc_code", row.doc_code.ToString()));
            cmd.Parameters.Add(new SqlParameter("@category_id", row.category_id.ToString()));
            cmd.Parameters.Add(new SqlParameter("@doc_title", row.doc_title.ToString()));
            cmd.Parameters.Add(new SqlParameter("@source_url", row.source_url != null ? row.source_url.ToString() : ""));
            cmd.Parameters.Add(new SqlParameter("@is_active", row.active.ToString()));

            if (row.effective_date != null && row.effective_date.ToString() != "")
                cmd.Parameters.Add(new SqlParameter("@effective_date", row.effective_date.ToString()));
            else
                cmd.Parameters.Add(new SqlParameter("@effective_date", DBNull.Value));

            cmd.Parameters.Add(new SqlParameter("@issuing_authority", row.issuing_authority != null ? row.issuing_authority.ToString() : ""));
            cmd.Parameters.Add(new SqlParameter("@administrative_level", row.administrative_level != null ? row.administrative_level.ToString() : ""));


            try
            {
                cmd.Connection.Open();
                iRowEffected = cmd.ExecuteNonQuery();
                cmd.Connection.Close();
                cmd.Connection.Dispose();
                cmd.Dispose();
                msg = "100";
            }
            catch (Exception e1)
            {
                msg = e1.Message;
                return false;
            }

            return (msg == "100" ? true : false);
        }

        // --- 3. CẬP NHẬT ---
        public bool Update(dynamic row, string uid)
        {
            string userId = uid;
            string Odl_Code = row.Odl_Code.ToString().Trim();
            string New_Code = row.doc_code.ToString().Trim();

            // Check trùng mã nếu có thay đổi
            if (Odl_Code.ToUpper() != New_Code.ToUpper())
            {
                common objCommon = new common();
                if (objCommon.check_code(tblname, "doc_code", New_Code))
                {
                    msg = "Mã tài liệu đã tồn tại.";
                    return false;
                }
            }

            iRowEffected = 0;
            cmd = new SqlCommand("Admin.sp_" + tblname + "_Update", new SqlConnection(strcnn));
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@doc_Id", row.id.ToString()));
            cmd.Parameters.Add(new SqlParameter("@doc_code", row.doc_code.ToString()));
            cmd.Parameters.Add(new SqlParameter("@category_id", row.category_id.ToString()));
            cmd.Parameters.Add(new SqlParameter("@doc_title", row.doc_title.ToString()));
            cmd.Parameters.Add(new SqlParameter("@source_url", row.source_url != null ? row.source_url.ToString() : ""));
            cmd.Parameters.Add(new SqlParameter("@is_active", row.active.ToString()));

            if (row.effective_date != null && row.effective_date.ToString() != "")
                cmd.Parameters.Add(new SqlParameter("@effective_date", row.effective_date.ToString()));
            else
                cmd.Parameters.Add(new SqlParameter("@effective_date", DBNull.Value));

            cmd.Parameters.Add(new SqlParameter("@issuing_authority", row.issuing_authority != null ? row.issuing_authority.ToString() : ""));
            cmd.Parameters.Add(new SqlParameter("@administrative_level", row.administrative_level != null ? row.administrative_level.ToString() : ""));

            cmd.Parameters.Add(new SqlParameter("@UpdatedById", userId));

            try
            {
                cmd.Connection.Open();
                iRowEffected = cmd.ExecuteNonQuery();
                cmd.Connection.Close();
                cmd.Connection.Dispose();
                cmd.Dispose();
                msg = "100";
            }
            catch (Exception e1)
            {
                msg = e1.Message;
                return false;
            }

            return (msg == "100" ? true : false);
        }

        // --- 4. XÓA ---
        public bool Delete(string ids)
        {
            iRowEffected = 0;
            cmd = new SqlCommand("Admin.sp_" + tblname + "_Delete", new SqlConnection(strcnn));
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@id", ids.Trim()));
            try
            {
                cmd.Connection.Open();
                iRowEffected = cmd.ExecuteNonQuery();
                cmd.Connection.Close();
                cmd.Connection.Dispose();
                cmd.Dispose();
                msg = "100";
            }
            catch (Exception e1)
            {
                msg = e1.Message;
                return false;
            }

            return true;
        }
    }

    public class GroupCategoryNode
    {
        public string group_code { get; set; }
        public string group_title { get; set; }
        public string category_code { get; set; }
        public string category_title { get; set; }
    }
}