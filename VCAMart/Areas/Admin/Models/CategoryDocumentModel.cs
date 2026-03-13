using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace RICTotalAdmin.Models
{
    public class CategoryDocumentModel
    {
        string strcnn;
        public Guid userId;
        public Guid languageId;
        public string msg;
        public int iRowEffected;
        int pageSize;
        SqlCommand cmd;
        string tblname = "tblCategoryDocument";
        public string rc_id;

        public string Idlist { get; set; }
        public CategoryDocumentModel()
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

        public string GetParent(int pageCurrent)
        {
            string sql = "exec Admin.sp_tblCategoryDocument_Get_List_Parent ";
            return common.RunSQLToJson(sql, ref msg);
            //return common.RunSQLToJsonManual(sql, ";MN_Parent_Name;MN_Id;MN_Ten;MN_Url;MN_Icon;MN_Thutu, active;", ref msg);
        }


        public bool Insert(dynamic row, string uid)
        {
            string userId = uid;
            // string CV_Id = Guid.NewGuid().ToString();
            common objCommon = new common();
            if (objCommon.check_code("tblCategoryDocument", "category_code", row.category_code.ToString()))
            {
                msg = "Thông tin trường mã đã có trong cơ sở dữ liệu";
                return false;
            }

            iRowEffected = 0;
            cmd = new SqlCommand("Admin.sp_" + tblname + "_Insert", new SqlConnection(strcnn));
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@category_id", row.id.ToString()));
            //cmd.Parameters.Add(new SqlParameter("@CV_CQL_Id", row.CV_CQL_Id.ToString()));

            cmd.Parameters.Add(new SqlParameter("@group_id", row.group_id.ToString()));
            cmd.Parameters.Add(new SqlParameter("@category_code", row.category_code.ToString()));
            cmd.Parameters.Add(new SqlParameter("@category_title", row.category_title.ToString()));
            cmd.Parameters.Add(new SqlParameter("@sort_order", row.sort_order.ToString()));
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
            //string CV_Id = Guid.NewGuid().ToString();
            string Odl_Code = row.Odl_Code.ToString().Trim();
            string New_Code = row.category_code.ToString().Trim();
            if (Odl_Code.ToUpper() != New_Code.ToUpper())
            {
                common objCommon = new common();
                if (objCommon.check_code(tblname, "category_code", row.category_code.ToString()))
                {
                    msg = "Mã loại tài liệu này đã tồn tại.";
                    return false;
                }
            }

            iRowEffected = 0;
            cmd = new SqlCommand("Admin.sp_" + tblname + "_Update", new SqlConnection(strcnn));
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@category_id", row.id.ToString()));
            cmd.Parameters.Add(new SqlParameter("@group_id", row.group_id.ToString()));
            cmd.Parameters.Add(new SqlParameter("@category_code", row.category_code.ToString()));
            cmd.Parameters.Add(new SqlParameter("@category_title", row.category_title.ToString()));
            cmd.Parameters.Add(new SqlParameter("@sort_order", row.sort_order.ToString()));
            cmd.Parameters.Add(new SqlParameter("@is_active", row.active.ToString()));

            cmd.Parameters.Add(new SqlParameter("@CreatedById", userId));

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
    }
}