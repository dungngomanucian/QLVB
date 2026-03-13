using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RICTotalAdmin.Models
{
    public class Branch
    {
        string sql, strcnn;
        public Guid userId;
        public string userUid;
        public Guid languageId;
        public string msg;
        public int iRowEffected;
        int pageSize;
      
        string tblname = "tblDanhSachCuaHang";
        string rc_id;

        public Branch() {
            sql = ""; msg = "";
            strcnn = System.Configuration.ConfigurationManager.AppSettings["cnn"].ToString();
         
            try
            {
                rc_id =  System.Web.HttpContext.Current.Session["rc_id"].ToString() ;
                userId = new Guid(System.Web.HttpContext.Current.Session["user_id"].ToString());
                userUid = System.Web.HttpContext.Current.Session["user_uid"].ToString();
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
           string sql = "exec sp_" + tblname + "_Get_List '" + rc_id  + "'";
           return common.RunSQLToJson(sql, ref msg);
        }
        

        public bool Insert(dynamic row)
        {
            sql = "EXEC [sp_" + tblname + "_Insert] N'" + row.id.ToString() + "', N'" + row.ma.ToString() + "',N'" + row.name.ToString() + "',N'" + row.addr.ToString() + "',N'" + row.tel.ToString() + "',N'" + row.note.ToString() + "', " + row.lakho.ToString() + ", " + row.active.ToString() + ",N'" + rc_id + "',N'" + userId + "',N'" + userUid + "'";
            RICDB.DB.RunSQLNoReturn(sql, ref msg, strcnn);
            return (msg == "100" ? true : false);
        }
        public bool Update(dynamic row)
        {
            sql = "EXEC [sp_" + tblname + "_Update] N'" + row.id.ToString() + "',N'" + row.ma.ToString() + "',N'" + row.name.ToString() + "',N'" + row.addr.ToString() + "',N'" + row.tel.ToString() + "',N'" + row.note.ToString() + "', " + row.lakho.ToString() + ", " + row.active.ToString() + ",N'" + rc_id + "',N'" + userId + "',N'" + userUid + "'";
            RICDB.DB.RunSQLNoReturn(sql, ref msg, strcnn);
            return (msg == "100" ? true : false);
        }
        public bool Delete(string ids)
        {
            sql = "EXEC [sp_" + tblname + "_Delete] N'" + ids + "', N'" + rc_id + "',N'" + userId + "'";
            RICDB.DB.RunSQLNoReturn(sql, ref msg, strcnn);

            return (msg == "100" ? true : false);
        }

        public bool CheckConChinhanh(ref int so, ref int dadung, ref int conlai)
        {
            so = 1; dadung = 1; conlai = 0;

            sql = "EXEC [sp_" + tblname + "_CheckSoChinhanh]   N'" + rc_id + "' ";
            DataTable dt =   RICDB.DB.RunSQL(sql, ref msg, strcnn);
            if (msg != "100") return false;
            if (dt.Rows.Count == 0) { msg = "Không lấy được dữ liệu"; return false; }
            so = int.Parse(dt.Rows[0]["so"].ToString());
            dadung = int.Parse(dt.Rows[0]["dadung"].ToString());
            conlai = int.Parse(dt.Rows[0]["conlai"].ToString());
            return true;
        }
    }

}