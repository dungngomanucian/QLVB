using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RICTotalAdmin.Models
{
    public class UsingInfo
    {
        string sql, strcnn;
        public    Guid  userId;
        public Guid languageId;
        public string msg;
        public int iRowEffected;
        int pageSize;
     
        string tblname = "tblriccustomerlist";
        string rc_id;

        public UsingInfo() {
            sql = ""; msg = "";
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

        public string GetList()
        {
           string sql = "exec sp_" + tblname + "_Get_One '" + rc_id  + "'";
           return common.RunSQLToJsonManual(sql,";cn;", ref msg);
        }
 
        public bool Update(dynamic row)
        {
            sql = "EXEC [sp_" + tblname + "_Update] N'" + rc_id + "',N'" + row.name.ToString() + "',N'" + row.addr.ToString() + "',N'" + row.tel.ToString() + "', N'" + row.logo.ToString() + "', N'" + row.email.ToString() + "', N'" + row.web.ToString() + "', N'" + row.daidien.ToString() + "',   N'" + row.tinhid.ToString() + "', N'" + row.nganhid.ToString() + "', N'" + row.ngaysinh.ToString() + "' ,N'" + userId + "'";
            RICDB.DB.RunSQLNoReturn(sql, ref msg, strcnn);
            return (msg == "100" ? true : false);
        }
       
      
    }

}