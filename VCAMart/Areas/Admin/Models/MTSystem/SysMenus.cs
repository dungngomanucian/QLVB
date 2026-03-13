using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RICTotalAdmin.Models
{
    public class SysMenus
    {
        string sql, strcnn;
        public    Guid  userId;
        public Guid languageId;
        public string msg;
        public int iRowEffected;
        int pageSize;
       
        string tblname = "tblSysMenu";
        string rc_id;

        public SysMenus() {
            sql = ""; msg = "";
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
           string sql = "exec sp_" + tblname + "_Get_List '" + rc_id  + "'";
           return common.RunSQLToJsonManual(sql, ";qten;qid;", ref msg);
        }

        public bool Insert(dynamic row)
        {
            sql = "EXEC [sp_" + tblname + "_Insert] N'" + row.id.ToString() + "',N'" + row.parentid.ToString() + "',N'" + row.ten.ToString() + "',N'" + row.mota.ToString() + "', N'" + row.url.ToString() + "', " + row.thutu.ToString() + ", " + row.active.ToString() + ", N'" + row.icon.ToString() + "', N'" + row.target.ToString() + "', " + row.direct.ToString() + ", " + row.begingroup.ToString() + " ,N'" + userId + "'";
            RICDB.DB.RunSQLNoReturn(sql, ref msg, strcnn);
            return (msg == "100" ? true : false);
        }
        public bool Update(dynamic row)
        {
            sql = "EXEC [sp_" + tblname + "_Update] N'" + row.id.ToString() + "',N'" + row.parentid.ToString() + "',N'" + row.ten.ToString() + "',N'" + row.mota.ToString() + "', N'" + row.url.ToString() + "', " + row.thutu.ToString() + ", " + row.active.ToString() + ", N'" + row.icon.ToString() + "', N'" + row.target.ToString() + "', " + row.direct.ToString() + ", " + row.begingroup.ToString() + " ,N'" + userId + "'";
            RICDB.DB.RunSQLNoReturn(sql, ref msg, strcnn);
            return (msg == "100" ? true : false);
        }
        public bool Delete(string ids)
        {
            sql = "EXEC [sp_" + tblname + "_Delete] N'" + ids + "',N'" + userId + "'";
            RICDB.DB.RunSQLNoReturn(sql, ref msg, strcnn);
            return (msg == "100" ? true : false);
        }

      
    }

}