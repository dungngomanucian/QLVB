using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RICTotalAdmin.Models
{
    public class ProductGroupModel
    {
        string strcnn;
        public Guid userId;
        public Guid languageId;
        public string msg;
        public int iRowEffected;
        int pageSize;
        SqlCommand cmd;
        string tblname = "tblProductGroup";
        public string rc_id;

        public string Idlist { get; set; }
        public ProductGroupModel()
        {
            msg = "";
            strcnn = System.Configuration.ConfigurationManager.AppSettings["cnn"].ToString();

            try
            {
               
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
            //return common.RunSQLToJsonManual(sql, ";MN_Parent_Name;MN_Id;MN_Ten;MN_Url;MN_Icon;MN_Thutu, active;", ref msg);
        }

        public bool Insert(dynamic row, string uid)
        {
            string userId = uid;
            // string CV_Id = Guid.NewGuid().ToString();
            common objCommon = new common();
            if (objCommon.check_code("tblProductGroup", "PUG_Code", row.PUG_Code.ToString()))
            {
                msg = "Thông tin trường mã đã có trong cơ sở dữ liệu";
                return false;
            }

            iRowEffected = 0;
            cmd = new SqlCommand("Admin.sp_" + tblname + "_Insert", new SqlConnection(strcnn));
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@PUG_Id", row.id.ToString()));
           
            cmd.Parameters.Add(new SqlParameter("@PUG_Code", row.PUG_Code.ToString()));
            cmd.Parameters.Add(new SqlParameter("@PUG_Name", row.PUG_Name.ToString()));
            cmd.Parameters.Add(new SqlParameter("@PUG_Icon", row.PUG_Icon.ToString()));
            cmd.Parameters.Add(new SqlParameter("@PUG_Notes", row.PUG_Notes.ToString()));
            //cmd.Parameters.Add(new SqlParameter("@PUG_Order", row.PUG_Order.ToString()));
            cmd.Parameters.Add(new SqlParameter("@PUG_Status", row.active.ToString()));
            cmd.Parameters.Add(new SqlParameter("@PUG_CreatedById", userId));
            cmd.Parameters.Add(new SqlParameter("@PUG_LG_Id", HttpContext.Current.Session["lang_id"].ToString()));
            
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
            string PUG_Id = Guid.NewGuid().ToString();
            string Odl_Code = row.Odl_Code.ToString().Trim();
            string New_Code = row.PUG_Code.ToString().Trim();
            if (Odl_Code.ToUpper() != New_Code.ToUpper())
            {
                common objCommon = new common();
                if (objCommon.check_code("tblProductGroup", "PUG_Code", row.PUG_Code.ToString()))
                {
                    msg = "Thông tin trường mã đã có trong cơ sở dữ liệu";
                    return false;
                }
            }

            iRowEffected = 0;
            cmd = new SqlCommand("Admin.sp_" + tblname + "_Update", new SqlConnection(strcnn));
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@PUG_Id", row.id.ToString()));

            cmd.Parameters.Add(new SqlParameter("@PUG_Code", row.PUG_Code.ToString()));
            cmd.Parameters.Add(new SqlParameter("@PUG_Name", row.PUG_Name.ToString()));
            cmd.Parameters.Add(new SqlParameter("@PUG_Icon", row.PUG_Icon.ToString()));
            cmd.Parameters.Add(new SqlParameter("@PUG_Notes", row.PUG_Notes.ToString()));
            //cmd.Parameters.Add(new SqlParameter("@PUG_Order", row.PUG_Order.ToString()));
            cmd.Parameters.Add(new SqlParameter("@PUG_Status", row.active.ToString()));
            cmd.Parameters.Add(new SqlParameter("@PUG_UpdatedById", userId));
            cmd.Parameters.Add(new SqlParameter("@PUG_LG_Id", HttpContext.Current.Session["lang_id"].ToString()));

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