using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RICTotalAdmin.Models
{
    public class AdminMenu
    {
        string  strcnn;
        public Guid  userId;
        public Guid languageId;
        public string msg;
        public int iRowEffected;
        int pageSize;
        SqlCommand cmd;
        string tblname = "tblMenu_Admin";

        public AdminMenu() {
            msg = "";
            strcnn = System.Configuration.ConfigurationManager.AppSettings["cnn"].ToString();
         
            try
            {
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
           string sql = "exec sp_" + tblname + "_Get_List " + pageCurrent.ToString() +"," + pageSize.ToString() +",'" + userId + "'";
           return common.RunSQLToJson(sql, ref msg);
        }
        public string GetListParent()
        {
            string sql = "exec sp_tblMenu_Admin_Get_List_Parent '" + userId + "'";
            return common.RunSQLToJson(sql, ref msg);
        }

        public bool Insert(dynamic row)
        {
            iRowEffected = 0;
            cmd = new SqlCommand("sp_" +  tblname + "_Insert", new SqlConnection(strcnn));
            cmd.CommandType = CommandType.StoredProcedure;

            //data: { "MN_Id":"","MN_Parent_Id":"00000000-0000-0000-0000-000000000000","MN_Ten":"111","MN_Mota":"","MN_Url":"222","MN_Icon":"","MN_Thutu":"0","MN_Status":"0"}
          
            cmd.Parameters.Add(new SqlParameter("@MN_Id", Guid.NewGuid()));
            cmd.Parameters.Add(new SqlParameter("@MN_Parent_Id", new Guid( row.MN_Parent_Id.ToString())));
            cmd.Parameters.Add(new SqlParameter("@MN_Ten", row.MN_Ten.ToString()));
            cmd.Parameters.Add(new SqlParameter("@MN_Mota", row.MN_Ten.ToString()));
            cmd.Parameters.Add(new SqlParameter("@MN_Url", row.MN_Url.ToString()));
            cmd.Parameters.Add(new SqlParameter("@MN_Status", int.Parse(row.MN_Status.ToString())));
            cmd.Parameters.Add(new SqlParameter("@MN_Icon", row.MN_Icon.ToString()));
            cmd.Parameters.Add(new SqlParameter("@MN_Thutu", int.Parse(row.MN_Thutu.ToString())));
            cmd.Parameters.Add(new SqlParameter("@userId", userId));
            try
            {
                cmd.Connection.Open();
                iRowEffected =  cmd.ExecuteNonQuery();
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
        public bool Update(dynamic row)
        {
            iRowEffected = 0;
            cmd = new SqlCommand("sp_" + tblname + "_Update", new SqlConnection(strcnn));
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@MN_Id", new Guid(row.MN_Id.ToString())));
            cmd.Parameters.Add(new SqlParameter("@MN_Parent_Id", new Guid(row.MN_Parent_Id.ToString())));
            cmd.Parameters.Add(new SqlParameter("@MN_Ten", row.MN_Ten.ToString()));
            cmd.Parameters.Add(new SqlParameter("@MN_Mota", row.MN_Ten.ToString()));
            cmd.Parameters.Add(new SqlParameter("@MN_Url", row.MN_Url.ToString()));
            cmd.Parameters.Add(new SqlParameter("@MN_Status", int.Parse(row.MN_Status.ToString())));
            cmd.Parameters.Add(new SqlParameter("@MN_Icon", row.MN_Icon.ToString()));
            cmd.Parameters.Add(new SqlParameter("@MN_Thutu", int.Parse(row.MN_Thutu.ToString())));
            cmd.Parameters.Add(new SqlParameter("@userId", userId));
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
        public bool Delete(string ids)
        {
            iRowEffected = 0;
            cmd = new SqlCommand("sp" + tblname + "_Delete", new SqlConnection(strcnn));
            cmd.CommandType = CommandType.StoredProcedure;
           
            cmd.Parameters.Add(new SqlParameter("@id",  ids.Trim()));
            cmd.Parameters.Add(new SqlParameter("@userId", userId));
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