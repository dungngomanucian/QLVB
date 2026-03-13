using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace RICTotalAdmin.Models
{
    public class PriorityModel
    {
        string  strcnn;
        public Guid userId;
        public Guid languageId;
        public string msg;
        public int iRowEffected;
        int pageSize;
        SqlCommand cmd;
        string tblname = "tblPriority";
        public string rc_id;

        public string Idlist { get; set; }
        public PriorityModel()
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
            catch (Exception )
            {
                pageSize = 10;
            }

        }

        public string GetList()
        {
            string sql = "exec sp_" + tblname + "_Get_List ";// + System.Web.HttpContext.Current.Session["lg_id"].ToString();
            return common.RunSQLToJson(sql, ref msg);
        }

        public bool Insert(dynamic row, string uid)
        {
            string userId = uid;

            common objCommon = new common();
            if (objCommon.check_code("tblPriority", "PRI_Code", row.PRI_Code.ToString()))
            {
                msg = "Thông tin trường mã đã có trong cơ sở dữ liệu";
                return false;
            }

            iRowEffected = 0;
            cmd = new SqlCommand("sp_" + tblname + "_Insert", new SqlConnection(strcnn));
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@PRI_Id", row.id.ToString()));
            cmd.Parameters.Add(new SqlParameter("@PRI_Code", row.PRI_Code.ToString()));
            cmd.Parameters.Add(new SqlParameter("@PRI_Name", row.PRI_Name.ToString()));


            cmd.Parameters.Add(new SqlParameter("@PRI_Status", row.active.ToString()));
            cmd.Parameters.Add(new SqlParameter("@PRI_CreatedById", userId));
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
            string PRI_Id = Guid.NewGuid().ToString();
            string Odl_Code = row.Odl_Code.ToString().Trim();
            string New_Code = row.PRI_Code.ToString().Trim();
            if (Odl_Code.ToUpper() != New_Code.ToUpper())
            {
                common objCommon = new common();
                if (objCommon.check_code("tblPriority", "PRI_Code", row.PRI_Code.ToString()))
                {
                    msg = "Thông tin trường mã đã có trong cơ sở dữ liệu";
                    return false;
                }
            }

            iRowEffected = 0;
            cmd = new SqlCommand("sp_" + tblname + "_Update", new SqlConnection(strcnn));
            cmd.CommandType = CommandType.StoredProcedure;

            cmd = new SqlCommand("sp_" + tblname + "_Update", new SqlConnection(strcnn));
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@PRI_Id", row.id.ToString()));
            cmd.Parameters.Add(new SqlParameter("@PRI_Code", row.PRI_Code.ToString()));
            cmd.Parameters.Add(new SqlParameter("@PRI_Name", row.PRI_Name.ToString()));


            cmd.Parameters.Add(new SqlParameter("@PRI_Status", row.active.ToString()));
            cmd.Parameters.Add(new SqlParameter("@PRI_UpdatedById", userId));
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