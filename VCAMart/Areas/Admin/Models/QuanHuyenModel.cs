using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace RICTotalAdmin.Models
{
    public class QuanHuyenModel
    {
          string  strcnn;
        public    Guid  userId;
        public Guid languageId;
        public string msg;
        public int iRowEffected;
        int pageSize;
        SqlCommand cmd;
        string tblname = "tblDistricts";
        public string rc_id;
      
        public string Idlist { get; set; }
        public QuanHuyenModel()
        {
            msg = "";
            strcnn = System.Configuration.ConfigurationManager.AppSettings["cnn"].ToString();
         
            try
            {
                rc_id =  System.Web.HttpContext.Current.Session["rc_id"].ToString() ;
                userId = new Guid(System.Web.HttpContext.Current.Session["user_id"].ToString());
                pageSize = int.Parse(System.Web.HttpContext.Current.Session["userPageSize"].ToString());
                languageId = new Guid(System.Web.HttpContext.Current.Session["lang_id"].ToString());
            }
            catch (Exception )
            {
                pageSize = 10;
            }
           
        }

        public string GetList(int pageCurrent)
        {
            string sql = "exec sp_" + tblname + "_Get_List ";
            return common.RunSQLToJson(sql, ref msg);
            //return common.RunSQLToJsonManual(sql, ";MN_Parent_Name;MN_Id;MN_Ten;MN_Url;MN_Icon;MN_Thutu, active;", ref msg);
        }

        public string GetParent(int pageCurrent)
        {
            string sql = "exec [sp_" + tblname + "_Get_List_Parent] ";
            return common.RunSQLToJson(sql, ref msg);
            //return common.RunSQLToJsonManual(sql, ";MN_Parent_Name;MN_Id;MN_Ten;MN_Url;MN_Icon;MN_Thutu, active;", ref msg);
        }

        public bool Insert(dynamic row, string uid)
        {
            string userId = uid;
            common objCommon = new common();
            if (objCommon.check_code("tblDistricts", "DTR_Code", row.DTR_Code.ToString()))
            {
                msg = "Thông tin trường mã đã có trong cơ sở dữ liệu";
                return false;
            }


            iRowEffected = 0;
            cmd = new SqlCommand("sp_" + tblname + "_Insert", new SqlConnection(strcnn));
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@DTR_Id", row.id.ToString()));
            cmd.Parameters.Add(new SqlParameter("@DTR_Code", row.DTR_Code.ToString()));

            cmd.Parameters.Add(new SqlParameter("@DTR_Name", row.DTR_Name.ToString()));
            
            cmd.Parameters.Add(new SqlParameter("@DTR_PR_Id", row.DTR_PR_Id.ToString()));

            cmd.Parameters.Add(new SqlParameter("@DTR_Status", row.active.ToString()));
            cmd.Parameters.Add(new SqlParameter("@DTR_CreatedById", userId));

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
            string Odl_Code = row.Odl_Code.ToString().Trim();
            string New_Code = row.DTR_Code.ToString().Trim();
            if (Odl_Code.ToUpper() != New_Code.ToUpper())
            {
                common objCommon = new common();
                if (objCommon.check_code("tblDistricts", "DTR_Code", row.DTR_Code.ToString()))
                {
                    msg = "Thông tin trường mã đã có trong cơ sở dữ liệu";
                    return false;
                }
            }

            iRowEffected = 0;
            cmd = new SqlCommand("sp_" + tblname + "_Update", new SqlConnection(strcnn));
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@DTR_Id", row.id.ToString()));
            cmd.Parameters.Add(new SqlParameter("@DTR_Code", row.DTR_Code.ToString()));

            cmd.Parameters.Add(new SqlParameter("@DTR_Name", row.DTR_Name.ToString()));
            
            cmd.Parameters.Add(new SqlParameter("@DTR_PR_Id", row.DTR_PR_Id.ToString()));

            cmd.Parameters.Add(new SqlParameter("@DTR_Status", row.active.ToString()));
            cmd.Parameters.Add(new SqlParameter("@DTR_UpdatedById", userId));

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