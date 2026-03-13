using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
namespace RICTotalAdmin.Models
{
    public class GroupDocumentModel
    {
        string strcnn;
        public Guid userId;
        public Guid languageId;
        public string msg;
        public int iRowEffected;
        int pageSize;
        SqlCommand cmd;
        string tblname = "tblGroupDocument";
        public string rc_id;

        public string Idlist { get; set; }
        public GroupDocumentModel()
        {
            msg = "";
            strcnn = System.Configuration.ConfigurationManager.AppSettings["cnn"].ToString();

            try
            {
                rc_id = System.Web.HttpContext.Current.Session["rc_id"].ToString();
                userId = new Guid(System.Web.HttpContext.Current.Session["user_id"].ToString());
                pageSize = int.Parse(System.Web.HttpContext.Current.Session["userPageSize"].ToString());
                languageId = new Guid(System.Web.HttpContext.Current.Session["lang_id"].ToString());
            }
            catch (Exception)
            {
                pageSize = 10;
            }

        }

        public string GetList(int pageCurrent)
        {
            string sql = "exec Admin.sp_" + tblname + "_Get_List ";
            return common.RunSQLToJson(sql, ref msg);
            
        }
        //CÁC THỦ TỤC INSERT, DELETE, UPDATE
        public bool Insert(dynamic row, string uid)
        {
            string userId = uid;
         
            common objCommon = new common();
            if (objCommon.check_code("tblGroupDocument", "group_code", row.group_code.ToString()))
            {
                msg = "Thông tin trường mã đã có trong cơ sở dữ liệu";
                return false;
            }

            iRowEffected = 0;
            cmd = new SqlCommand("Admin.sp_" + tblname + "_Insert", new SqlConnection(strcnn));
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@group_id", row.id.ToString()));
           
            cmd.Parameters.Add(new SqlParameter("@group_code", row.group_code.ToString()));
            cmd.Parameters.Add(new SqlParameter("@group_title", row.group_title.ToString()));
            cmd.Parameters.Add(new SqlParameter("@source", row.source.ToString()));
            cmd.Parameters.Add(new SqlParameter("@group_type", row.group_type.ToString()));
            cmd.Parameters.Add(new SqlParameter("@is_active", row.active.ToString()));
            cmd.Parameters.Add(new SqlParameter("@CreatedById", userId));

            //cmd.Parameters.Add(new SqlParameter("@userId", userId));
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

        public bool Update(dynamic row, string uid)
        {
            string userId = uid;
            string group_id = Guid.NewGuid().ToString();
            string Odl_Code = row.Odl_Code.ToString().Trim();
            string New_Code = row.group_code.ToString().Trim();
            if (Odl_Code.ToUpper() != New_Code.ToUpper())
            {
                common objCommon = new common();
                if (objCommon.check_code("tblGroupDocument", "group_code", row.group_code.ToString()))
                {
                    msg = "Thông tin trường mã đã có trong cơ sở dữ liệu";
                    return false;
                }
            }

            iRowEffected = 0;
            cmd = new SqlCommand("Admin.sp_" + tblname + "_Update", new SqlConnection(strcnn));
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@group_id", row.id.ToString()));

            cmd.Parameters.Add(new SqlParameter("@group_code", row.group_code.ToString()));
            cmd.Parameters.Add(new SqlParameter("@group_title", row.group_title.ToString()));
            cmd.Parameters.Add(new SqlParameter("@source", row.source.ToString()));
            cmd.Parameters.Add(new SqlParameter("@group_type", row.group_type.ToString()));
            cmd.Parameters.Add(new SqlParameter("@is_active", row.active.ToString()));
            cmd.Parameters.Add(new SqlParameter("@UpdatedById", userId));

            //cmd.Parameters.Add(new SqlParameter("@userId", userId));
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

        //---------------Delete record
        public bool Delete(string ids)
        {
            iRowEffected = 0;
            cmd = new SqlCommand("Admin.sp_" + tblname + "_Delete", new SqlConnection(strcnn));
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@id", ids.Trim()));
            // cmd.Parameters.Add(new SqlParameter("@userId", userId));
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
        //===================================================
        public bool ImportJsonToDatabase(string physicalJsonPath, dynamic row)
        {
            msg = "";
            iRowEffected = 0;
            string type = row.LoaiHinh.ToString();
            // 1. Đọc file JSON
            string jsonText = File.ReadAllText(physicalJsonPath);
            RootJson root = JsonConvert.DeserializeObject<RootJson>(jsonText);

            if (root == null || root.data == null)
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
                    string source = root.meta != null ? root.meta.source : "";

                    // 2. Duyệt từng GROUP
                    foreach (var g in root.data)
                    {
                        InsertGroupIfNotExists(conn, tran,
                            g.group_id,
                            g.group_title,
                            source,
                            type
                            );

                        // 3. CATEGORY
                        if (g.events != null)
                        {
                            int sort = 1;
                            foreach (var c in g.events)
                            {
                                InsertCategoryIfNotExists(conn, tran,
                                    g.group_id,
                                    c.event_id,
                                    c.category,
                                    sort++);

                                // 4. DOCUMENT
                                if (c.children != null)
                                {
                                    foreach (var d in c.children)
                                    {
                                        string docCode = ExtractMaThuTuc(d.url);

                                        InsertDocumentIfNotExists(conn, tran,
                                            docCode,
                                            c.event_id,
                                            d.title,
                                            d.url);
                                    }
                                }
                            }
                        }
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

        private string ExtractMaThuTuc(string url)
        {
            if (string.IsNullOrEmpty(url)) return "";

            try
            {
                Uri uri = new Uri(url);
                return HttpUtility.ParseQueryString(uri.Query)
                    .Get("ma_thu_tuc") ?? "";
            }
            catch
            {
                return "";
            }
        }

        private void InsertGroupIfNotExists(SqlConnection conn, SqlTransaction tran, 
            string groupCode, string groupTitle, string source, string group_type)
        {
            SqlCommand check = new SqlCommand(
                "SELECT COUNT(*) FROM tblGroupDocument WHERE group_code = @code",
                conn, tran);
            check.Parameters.AddWithValue("@code", groupCode);

            int exists = (int)check.ExecuteScalar();
            if (exists > 0) return;

            SqlCommand cmd = new SqlCommand(@"
            INSERT INTO tblGroupDocument
            (group_id, group_code, group_title, source, is_active, group_type, CreatedDate, CreatedById)
            VALUES
            (NEWID(), @code, @title, @source, 1,@group_type, GETDATE(), @by)",
                    conn, tran);

            cmd.Parameters.AddWithValue("@code", groupCode);
            cmd.Parameters.AddWithValue("@title", groupTitle ?? "");
            cmd.Parameters.AddWithValue("@source", source ?? "");
            cmd.Parameters.AddWithValue("@group_type", group_type ?? "");
            cmd.Parameters.AddWithValue("@by", userId);

            cmd.ExecuteNonQuery();
            iRowEffected++;
        }

        private void InsertCategoryIfNotExists(SqlConnection conn, SqlTransaction tran, string groupCode, string categoryCode, string categoryTitle, int sortOrder)
        {
            SqlCommand check = new SqlCommand(
                "SELECT COUNT(*) FROM tblCategoryDocument WHERE category_code = @code",
                conn, tran);
            check.Parameters.AddWithValue("@code", categoryCode);

            int exists = (int)check.ExecuteScalar();
            if (exists > 0) return;

            SqlCommand cmd = new SqlCommand(@"
            INSERT INTO tblCategoryDocument
            (category_id, group_id, category_code, category_title,
             sort_order, is_active, CreatedDate, CreatedById)
            VALUES
            (NEWID(), @group, @code, @title, @sort, 1, GETDATE(), @by)",
                    conn, tran);
            cmd.Parameters.AddWithValue("@group", groupCode);
            cmd.Parameters.AddWithValue("@code", categoryCode);
            cmd.Parameters.AddWithValue("@title", categoryTitle ?? "");
            cmd.Parameters.AddWithValue("@sort", sortOrder);
            cmd.Parameters.AddWithValue("@by", userId);

            cmd.ExecuteNonQuery();
            iRowEffected++;
        }

        private void InsertDocumentIfNotExists(
        SqlConnection conn,
        SqlTransaction tran,
        string docCode,
        string categoryCode,
        string title,
        string url)
        {
           
            if (string.IsNullOrWhiteSpace(docCode)) return;

            // 1️⃣ Kiểm tra tồn tại
            using (SqlCommand check = new SqlCommand(
                "SELECT COUNT(1) FROM tblDocument WHERE doc_code = @code",
                conn, tran))
            {
                check.Parameters.Add("@code", SqlDbType.NVarChar, 100).Value = docCode;

                int exists = (int)check.ExecuteScalar();
                if (exists > 0) return;
            }

            // 2️⃣ Insert mới với document_type = 'PROCEDURE'
            using (SqlCommand cmd = new SqlCommand(@"
            INSERT INTO tblDocument
            (
                doc_Id,
                doc_code,
                category_id,
                doc_title,
                source_url,
                is_active,
                CreatedDate,
                updated_at,
                doc_type
            )
            VALUES
            (
                NEWID(),
                @code,
                @cate,
                @title,
                @url,
                1,
                GETDATE(),
                GETDATE(),
                @doc_type
            )", conn, tran))
            {
                cmd.Parameters.Add("@code", SqlDbType.NVarChar, 50).Value = docCode;
                cmd.Parameters.Add("@cate", SqlDbType.NVarChar, 50).Value = categoryCode;
                cmd.Parameters.Add("@title", SqlDbType.NVarChar, 1000).Value = title ?? "";
                cmd.Parameters.Add("@url", SqlDbType.NVarChar, 1000).Value = url ?? "";
                cmd.Parameters.Add("@doc_type", SqlDbType.NVarChar, 250).Value = "PROCEDURE";
                cmd.ExecuteNonQuery();
            }

            iRowEffected++;
        }


        //private void InsertDocumentIfNotExists(SqlConnection conn, SqlTransaction tran, string docCode, string categoryCode, string title, string url)
        //{
        //    if (string.IsNullOrEmpty(docCode)) return;

        //    SqlCommand check = new SqlCommand(
        //        "SELECT COUNT(*) FROM tblDocument WHERE doc_code = @code",
        //        conn, tran);
        //    check.Parameters.AddWithValue("@code", docCode);

        //    int exists = (int)check.ExecuteScalar();
        //    if (exists > 0) return;

        //    SqlCommand cmd = new SqlCommand(@"
        //    INSERT INTO tblDocument
        //    (doc_Id, doc_code, category_id, doc_title,
        //     source_url, is_active, CreatedDate, updated_at,document_type)
        //    VALUES
        //    (NEWID(), @code, @cate, @title, @url, 1, GETDATE(), GETDATE())",
        //            conn, tran);

        //    cmd.Parameters.AddWithValue("@code", docCode);
        //    cmd.Parameters.AddWithValue("@cate", categoryCode);
        //    cmd.Parameters.AddWithValue("@title", title ?? "");
        //    cmd.Parameters.AddWithValue("@url", url ?? "");

        //    cmd.ExecuteNonQuery();
        //    iRowEffected++;
        //}


    }
}