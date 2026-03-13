using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RICTotalAdmin.Models
{
    public class DoiTacModel
    {
        string  strcnn;
        public Guid userId;
        public Guid languageId;
        public string msg;
        public int iRowEffected;
        int pageSize;
        SqlCommand cmd;
        string tblname = "tblDoiTac";
        public string rc_id;

        public string Idlist { get; set; }
        public DoiTacModel()
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
            string sql = "exec sp_" + tblname + "_Get_List " + System.Web.HttpContext.Current.Session["lang_id"].ToString();
            return common.RunSQLToJsonManual(sql, "", ref msg);
        }

        public bool Insert(dynamic row, string uid)
        {
            string userId = uid;

            iRowEffected = 0;
            cmd = new SqlCommand("sp_" + tblname + "_Insert", new SqlConnection(strcnn));
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@DT_Id", row.id.ToString()));
            cmd.Parameters.Add(new SqlParameter("@DT_URL", row.DT_URL.ToString()));

            cmd.Parameters.Add(new SqlParameter("@DT_LG_Id", System.Web.HttpContext.Current.Session["lang_id"].ToString()));

            cmd.Parameters.Add(new SqlParameter("@DT_Name", row.DT_Name.ToString()));
            cmd.Parameters.Add(new SqlParameter("@DT_Image", row.DT_Image.ToString()));

            cmd.Parameters.Add(new SqlParameter("@DT_Status", row.active.ToString()));
            cmd.Parameters.Add(new SqlParameter("@DT_CreatedById", userId));

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
            cmd = new SqlCommand("sp_" + tblname + "_Update", new SqlConnection(strcnn));
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@DT_Id", row.id.ToString()));
            cmd.Parameters.Add(new SqlParameter("@DT_URL", row.DT_URL.ToString()));

            cmd.Parameters.Add(new SqlParameter("@DT_LG_Id", System.Web.HttpContext.Current.Session["lang_id"].ToString()));

            cmd.Parameters.Add(new SqlParameter("@DT_Name", row.DT_Name.ToString()));
            cmd.Parameters.Add(new SqlParameter("@DT_Image", row.DT_Image.ToString()));

            cmd.Parameters.Add(new SqlParameter("@DT_Status", row.active.ToString()));
            cmd.Parameters.Add(new SqlParameter("@DT_UpdatedById", userId));


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
            cmd = new SqlCommand("sp_" + tblname + "_Delete", new SqlConnection(strcnn));
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