using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace MTModel.Category
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;

    public partial class tblLog
    {
       

       
        public System.Guid User_ID { get; set; }
        public string User_UID { get; set; }

       
        public Nullable<System.Guid> CH_ID_X { get; set; }
        public Nullable<System.Guid> RC_ID { get; set; }
         
        public int pageSize { get; set; }
        public int pageIndex { get; set; }
       
 
        public int iRowEffected;
         
        public string tableName;
        DataTable dt;

        string  strcnn;
        public string Idlist;
        public string userId;
        public string userUid;
        public Guid languageId;
        public string msg;
        
   
        string tblname;
        string rc_id;

        SqlCommand cmd;
       
        public tblLog()
        {
            pageSize = 10;
            pageIndex = 0;
        
            User_ID = Guid.Empty;
          
            User_UID = "";

          
            CH_ID_X = Guid.Empty;
            RC_ID = Guid.Empty;

        
          
            iRowEffected = 0;
            msg = "100";
            tableName = "tblLog";

            tblname = tableName;
            msg = "";
            strcnn = System.Configuration.ConfigurationManager.AppSettings["cnn"].ToString();
            Idlist = "";
            try
            {
                rc_id = System.Web.HttpContext.Current.Session["rc_id"].ToString();
                userId = System.Web.HttpContext.Current.Session["user_id"].ToString();
                userUid = System.Web.HttpContext.Current.Session["user_uid"].ToString();
                pageSize = 10;
                languageId = new Guid(System.Web.HttpContext.Current.Session["lang_id"].ToString());
            }
            catch (Exception )
            {
                pageSize = 10;
            }

        }


        public tblLog(string strcnn) : this()
        {
            this.strcnn = strcnn;
            cmd = new SqlCommand();
            cmd.Connection = new SqlConnection(strcnn);
            cmd.CommandType = CommandType.StoredProcedure;
        }

        
      
        public string GetList(string tungay, string denngay, ref string msg)
        {
            string sql = "exec sp_" + tableName + "_Get_List '"+ RC_ID.ToString() +"','" + this.User_ID.ToString() +"','" + tungay + "','" + denngay + "'";
            dt = RICDB.DB.RunSQL(sql, ref msg, strcnn);
            if (msg != "100") return "";
            if (dt.Rows.Count == 0) return "";
            return JsonConvert.SerializeObject(dt); //, Formatting.Indented);
        }

        public string GetHome(string chid, string maxid, ref string msg)
        {
            string sql = "exec [sp_tblLog_Get_List_Home]   '" + RC_ID + "','" + chid + "', " + maxid;
            return RICTotalAdmin.Models.common.RunSQLToJson(sql, ref msg);
        }

    }
}