using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace RICTotalAdmin.Models
{
    public class AddressModel
    {
        string  strcnn;
        public Guid userId;
        public Guid languageId;
        public string msg;
        public int iRowEffected;
        int pageSize;
        SqlCommand cmd;
        string tblname = "tblAddress";
        public string rc_id;

        public string Idlist { get; set; }
        public AddressModel()
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

        public string GetList()
        {
            string sql = "exec sp_" + tblname + "_Get_List " + System.Web.HttpContext.Current.Session["lang_id"].ToString();
            return common.RunSQLToJson(sql, ref msg);
        }

        public bool Insert(dynamic row, string uid)
        {
            string userId = uid;
            iRowEffected = 0;
            cmd = new SqlCommand("sp_" + tblname + "_Insert", new SqlConnection(strcnn));
            cmd.CommandType = CommandType.StoredProcedure;

            //data: { "MN_Id":"","MN_Parent_Id":"00000000-0000-0000-0000-000000000000","MN_Ten":"111","MN_Mota":"","MN_Url":"222","MN_Icon":"","MN_Thutu":"0","MN_Status":"0"}
         
            cmd.Parameters.Add(new SqlParameter("@Add_Id", row.id.ToString()));
            cmd.Parameters.Add(new SqlParameter("@STT", row.STT.ToString()));
            cmd.Parameters.Add(new SqlParameter("@Add_LG_Id", HttpContext.Current.Session["lang_id"].ToString()));

           
            cmd.Parameters.Add(new SqlParameter("@Add_Title", row.Add_Title.ToString()));

         
            cmd.Parameters.Add(new SqlParameter("@Add_ContentBody", row.Add_ContentBody.ToString()));
            



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

            return true;
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
            cmd = new SqlCommand("sp_" + tblname + "_Update", new SqlConnection(strcnn));
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@Add_Id", row.id.ToString()));
            cmd.Parameters.Add(new SqlParameter("@STT", row.STT.ToString()));
            cmd.Parameters.Add(new SqlParameter("@Add_LG_Id", HttpContext.Current.Session["lang_id"].ToString()));


            cmd.Parameters.Add(new SqlParameter("@Add_Title", row.Add_Title.ToString()));


            cmd.Parameters.Add(new SqlParameter("@Add_ContentBody", row.Add_ContentBody.ToString()));


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