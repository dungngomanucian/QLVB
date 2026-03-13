using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using Newtonsoft.Json;
using System.Text;
using System.IO;

namespace RICTotalAdmin.Models
{
    public class ContentCategoryModel
    {
        string sql, strcnn;
        public Guid userId;
        public Guid languageId;
        public string msg;
        public int iRowEffected;
        int pageSize;
        SqlCommand cmd;
        string tblname = "tblContentCategory";
        public string rc_id;

        public string Idlist { get; set; }
        public ContentCategoryModel()
        {
            sql = ""; msg = "";
            strcnn = System.Configuration.ConfigurationManager.AppSettings["cnn"].ToString();

            try
            {
                //rc_id = System.Web.HttpContext.Current.Session["rc_id"].ToString();
                userId = new Guid(System.Web.HttpContext.Current.Session["user_id"].ToString());
                pageSize = int.Parse(System.Web.HttpContext.Current.Session["userPageSize"].ToString());
                languageId = new Guid(System.Web.HttpContext.Current.Session["lang_id"].ToString());
            }
            catch  
            {
                pageSize = 10;
            }

        }

        public string GetList()
        {
            string sql = "exec Admin.sp_tblContentCategory_Get_List " + System.Web.HttpContext.Current.Session["lang_id"].ToString();
            return common.RunSQLToJson(sql, ref msg);
        }

        public string ListParent()
        {
            string sql = "exec Admin.sp_tblContentCategory_Get_List_Parent " + System.Web.HttpContext.Current.Session["lang_id"].ToString(); ;
            return common.RunSQLToJsonManual(sql, "", ref msg);
        }

        public bool Insert(dynamic row, string uid)
        {
            string userId = uid;
            //string MN_Id = Guid.NewGuid().ToString();
            //  common objCommon = new common();
            // if (objCommon.check_code("tblContentCategory", "LCT_Code", row.LCT_Code.ToString()))
            //  {
            // msg = "Thông tin trường mã đã có trong cơ sở dữ liệu";
            //  return false;
            //  }

            iRowEffected = 0;
            cmd = new SqlCommand("Admin.sp_" + tblname + "_Insert", new SqlConnection(strcnn));
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@CC_Id", row.id.ToString()));
            cmd.Parameters.Add(new SqlParameter("@CC_ParentId", row.CC_ParentId.ToString()));
            cmd.Parameters.Add(new SqlParameter("@CC_CapHoc", row.CC_CapHoc.ToString()));
            cmd.Parameters.Add(new SqlParameter("@CC_LG_Id", System.Web.HttpContext.Current.Session["lang_id"].ToString()));

            cmd.Parameters.Add(new SqlParameter("@CC_Name", row.CC_Name.ToString()));
            cmd.Parameters.Add(new SqlParameter("@CC_FriendlyUrl", row.CC_FriendlyUrl.ToString()));
            cmd.Parameters.Add(new SqlParameter("@CC_LinkWebsite", row.CC_LinkWebsite.ToString()));

            cmd.Parameters.Add(new SqlParameter("@CC_Image", row.CC_Image.ToString()));
            cmd.Parameters.Add(new SqlParameter("@CC_Description", row.CC_Description.ToString()));
            cmd.Parameters.Add(new SqlParameter("@CC_SortOrder", row.CC_SortOrder.ToString()));

            cmd.Parameters.Add(new SqlParameter("@CC_Position", row.CC_Position.ToString()));
            cmd.Parameters.Add(new SqlParameter("@CC_PageTitle", row.CC_PageTitle.ToString()));
            cmd.Parameters.Add(new SqlParameter("@CC_MetaKeyword", row.CC_MetaKeyword.ToString()));

            cmd.Parameters.Add(new SqlParameter("@CC_MetaDescription", row.CC_MetaDescription.ToString()));
            cmd.Parameters.Add(new SqlParameter("@CC_Published", row.CC_Published.ToString()));
            cmd.Parameters.Add(new SqlParameter("@CC_Notes", row.CC_Notes.ToString()));





            cmd.Parameters.Add(new SqlParameter("@CC_Status", row.active.ToString()));
            cmd.Parameters.Add(new SqlParameter("@CC_CreatedById", userId));

            //cmd.Parameters.Add(new SqlParameter("@userId", userId));
            try
            {
                cmd.Connection.Open();
                iRowEffected = cmd.ExecuteNonQuery();
                cmd.Connection.Close();
                cmd.Connection.Dispose();
                cmd.Dispose();
                //Insert vao bang NhomQuyen_Menu
                DataTable dt = new DataTable();
                DataSet ds = new DataSet();
                string strSQL = "select * from tblNhomQuyen"; // Where UNQ_USER_ID='" + userId + "'";
                SqlConnection conn = new SqlConnection(strcnn);
                try
                {
                    conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter(strSQL, conn);
                    da.Fill(ds, "data");
                    dt = ds.Tables[0];
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        string groupid = dt.Rows[i]["NQ_ID"].ToString();
                        sql = "EXEC Admin.sp_tblNhomQuyen_ContentCategory_Insert_Value N'" + groupid + "',N'" + row.id.ToString() + "'";
                        RICDB.DB.RunSQLNoReturn(sql, ref msg, strcnn);
                    }
                }
                catch
                {
                }
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
            //  string Odl_Code = row.Odl_Code.ToString().Trim();
            //  string New_Code = row.LCT_Code.ToString().Trim();
            /* if (Odl_Code.ToUpper() != New_Code.ToUpper())
             {
                 common objCommon = new common();
                 if (objCommon.check_code("tblLoaiCT", "LCT_Code", row.LCT_Code.ToString()))
                 {
                     msg = "Thông tin trường mã đã có trong cơ sở dữ liệu";
                     return false;
                 }
             }*/


            iRowEffected = 0;
            cmd = new SqlCommand("Admin.sp_" + tblname + "_Update", new SqlConnection(strcnn));
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@CC_Id", row.id.ToString()));
            cmd.Parameters.Add(new SqlParameter("@CC_ParentId", row.CC_ParentId.ToString()));
            cmd.Parameters.Add(new SqlParameter("@CC_CapHoc", row.CC_CapHoc.ToString()));
            cmd.Parameters.Add(new SqlParameter("@CC_LG_Id", System.Web.HttpContext.Current.Session["lang_id"].ToString()));

            cmd.Parameters.Add(new SqlParameter("@CC_Name", row.CC_Name.ToString()));
            cmd.Parameters.Add(new SqlParameter("@CC_FriendlyUrl", row.CC_FriendlyUrl.ToString()));
            cmd.Parameters.Add(new SqlParameter("@CC_LinkWebsite", row.CC_LinkWebsite.ToString()));

            cmd.Parameters.Add(new SqlParameter("@CC_Image", row.CC_Image.ToString()));
            cmd.Parameters.Add(new SqlParameter("@CC_Description", row.CC_Description.ToString()));
            cmd.Parameters.Add(new SqlParameter("@CC_SortOrder", row.CC_SortOrder.ToString()));

            cmd.Parameters.Add(new SqlParameter("@CC_Position", row.CC_Position.ToString()));
            cmd.Parameters.Add(new SqlParameter("@CC_PageTitle", row.CC_PageTitle.ToString()));
            cmd.Parameters.Add(new SqlParameter("@CC_MetaKeyword", row.CC_MetaKeyword.ToString()));

            cmd.Parameters.Add(new SqlParameter("@CC_MetaDescription", row.CC_MetaDescription.ToString()));
            cmd.Parameters.Add(new SqlParameter("@CC_Published", row.CC_Published.ToString()));
            cmd.Parameters.Add(new SqlParameter("@CC_Notes", row.CC_Notes.ToString()));





            cmd.Parameters.Add(new SqlParameter("@CC_Status", row.active.ToString()));
            cmd.Parameters.Add(new SqlParameter("@CC_UpdatedById", userId));

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

    }
}