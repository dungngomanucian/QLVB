using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace RICTotalAdmin.Models
{
    public class PhuongXaModel
    {
          string  strcnn;
        public    Guid  userId;
        public Guid languageId;
        public string msg;
        public int iRowEffected;
        int pageSize;
        SqlCommand cmd;
        string tblname = "tblWards";
        public string rc_id;
      
        public string Idlist { get; set; }
        public PhuongXaModel()
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
            catch (Exception)
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
            if (objCommon.check_code("tblWards", "WA_Code", row.WA_Code.ToString()))
            {
                msg = "Thông tin trường mã đã có trong cơ sở dữ liệu";
                return false;
            }

            iRowEffected = 0;
            cmd = new SqlCommand("sp_" + tblname + "_Insert", new SqlConnection(strcnn));
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@WA_Id", row.id.ToString()));
            cmd.Parameters.Add(new SqlParameter("@WA_Code", row.WA_Code.ToString()));

            cmd.Parameters.Add(new SqlParameter("@WA_Name", row.WA_Name.ToString()));

            cmd.Parameters.Add(new SqlParameter("@WA_DTR_Id", row.WA_DTR_Id.ToString()));

            cmd.Parameters.Add(new SqlParameter("@WA_Status", row.active.ToString()));
            cmd.Parameters.Add(new SqlParameter("@WA_CreatedById", userId));

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
            string New_Code = row.WA_Code.ToString().Trim();
            if (Odl_Code.ToUpper() != New_Code.ToUpper())
            {
                common objCommon = new common();
                if (objCommon.check_code("tblWards", "WA_Code", row.WA_Code.ToString()))
                {
                    msg = "Thông tin trường mã đã có trong cơ sở dữ liệu";
                    return false;
                }
            }

            iRowEffected = 0;
            cmd = new SqlCommand("sp_" + tblname + "_Update", new SqlConnection(strcnn));
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@WA_Id", row.id.ToString()));
            cmd.Parameters.Add(new SqlParameter("@WA_Code", row.WA_Code.ToString()));

            cmd.Parameters.Add(new SqlParameter("@WA_Name", row.WA_Name.ToString()));

            cmd.Parameters.Add(new SqlParameter("@WA_DTR_Id", row.WA_DTR_Id.ToString()));

            cmd.Parameters.Add(new SqlParameter("@WA_Status", row.active.ToString()));
            cmd.Parameters.Add(new SqlParameter("@WA_UpdatedById", userId));

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