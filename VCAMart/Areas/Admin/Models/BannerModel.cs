using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace RICTotalAdmin.Models
{
    public class BannerModel
    {
        string  strcnn;
        public Guid userId;
        public Guid languageId;
        public string msg;
        public int iRowEffected;
        int pageSize;
        SqlCommand cmd;
        string tblname = "tblBanner";
        public string rc_id;

        public string Idlist { get; set; }
        public BannerModel()
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
            catch  
            {
                pageSize = 10;
            }

        }

        public string GetList(int pageCurrent)
        {
            string sql = "exec Admin.sp_" + tblname + "_Get_List " + System.Web.HttpContext.Current.Session["lang_id"].ToString() + ",0";
            return common.RunSQLToJsonManual(sql, "", ref msg);
        }

        public bool Insert(dynamic row, string uid)
        {
            string userId = uid;
            //  common objCommon = new common();
            // if (objCommon.check_code("tblContentCategory", "LCT_Code", row.LCT_Code.ToString()))
            //  {
            // msg = "Thông tin trường mã đã có trong cơ sở dữ liệu";
            //  return false;
            //  }

            iRowEffected = 0;
            cmd = new SqlCommand("Admin.sp_" + tblname + "_Insert", new SqlConnection(strcnn));
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@BN_Id", row.id.ToString()));
            cmd.Parameters.Add(new SqlParameter("@BN_Name", row.BN_Name.ToString()));
            cmd.Parameters.Add(new SqlParameter("@BN_LG_Id", System.Web.HttpContext.Current.Session["lang_id"].ToString()));
          //  cmd.Parameters.Add(new SqlParameter("@BN_LG_Id", "1"));

            cmd.Parameters.Add(new SqlParameter("@BN_Image", row.BN_Image.ToString()));
            cmd.Parameters.Add(new SqlParameter("@BN_Type", row.BN_Type.ToString()));
            cmd.Parameters.Add(new SqlParameter("@BN_URL", row.BN_URL.ToString()));

            cmd.Parameters.Add(new SqlParameter("@BN_Status", row.active.ToString()));
         

         

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
            

            iRowEffected = 0;
            cmd = new SqlCommand("Admin.sp_" + tblname + "_Update", new SqlConnection(strcnn));
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@BN_Id", row.id.ToString()));
            cmd.Parameters.Add(new SqlParameter("@BN_Name", row.BN_Name.ToString()));
            cmd.Parameters.Add(new SqlParameter("@BN_LG_Id", System.Web.HttpContext.Current.Session["lang_id"].ToString()));
            //  cmd.Parameters.Add(new SqlParameter("@BN_LG_Id", "1"));

            cmd.Parameters.Add(new SqlParameter("@BN_Image", row.BN_Image.ToString()));
            cmd.Parameters.Add(new SqlParameter("@BN_Type", row.BN_Type.ToString()));
            cmd.Parameters.Add(new SqlParameter("@BN_URL", row.BN_URL.ToString()));

            cmd.Parameters.Add(new SqlParameter("@BN_Status", row.active.ToString()));

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