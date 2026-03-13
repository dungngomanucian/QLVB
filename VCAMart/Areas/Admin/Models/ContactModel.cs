using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RICTotalAdmin.Models
{
    public class ContactModel
    {
        string  strcnn;
        public Guid userId;
        public Guid languageId;
        public string msg;
        public int iRowEffected;
        int pageSize;
         
        //string tblname = "tblContact";
        public string rc_id;

        public string Idlist { get; set; }
        public ContactModel()
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
            string sql = "exec sp_tblContact_Get_List " + System.Web.HttpContext.Current.Session["lg_id"].ToString();
            return common.RunSQLToJson(sql, ref msg);
        }
    }
}