using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RICTotalAdmin.Models
{
    public class tblConfig
    {
        string sql, strcnn;
        public string  userId;
        public string userUid;
        public Guid languageId;
        public string msg;
        public int iRowEffected;
        int pageSize;
        string tblname;
        string rc_id;

        public tblConfig() {
            tblname = "tblConfig";
            sql = ""; msg = "";
            strcnn = System.Configuration.ConfigurationManager.AppSettings["cnn"].ToString();
         
            try
            {
                rc_id =  System.Web.HttpContext.Current.Session["rc_id"].ToString() ;
                userId = System.Web.HttpContext.Current.Session["user_id"].ToString() ;
                userUid = System.Web.HttpContext.Current.Session["user_uid"].ToString();
                pageSize = int.Parse(System.Web.HttpContext.Current.Session["userPageSize"].ToString());
                languageId = new Guid(System.Web.HttpContext.Current.Session["lang_id"].ToString());
            }
            catch (Exception )
            {
                pageSize = 10;
            }
           
        }

        public string GetList(int pageid)
        {
           string sql = "exec sp_" + tblname + "_Get_List '" + rc_id  + "'," + pageid + ',' + pageSize;
           return common.RunSQLToJson(sql, ref msg);
        }
        public string GetOne(string cfid, ref string msg)
        { // lay thong tin value cua 1 config 
            string sql = "exec sp_tblConfig_Get_One '" + rc_id + "'," + cfid;
            return RICDB.DB.RunSQLReturn1Value(sql, ref msg, strcnn);  
        }
        public bool UpdateOne(string cfid, string cfvl, ref string msg)
        {
            string sql = "exec sp_tblConfig_Update '" + rc_id + "'," + cfid + ",N'"  + cfvl + "'";
            RICDB.DB.RunSQLNoReturn(sql, ref msg, strcnn);
            return (msg == "100" ? true : false);
        }

        public string GetListParent()
        {
            string sql = "exec sp_" + tblname + "_Get_List_Parent '" + rc_id + "'";
            return common.RunSQLToJson(sql, ref msg);
        }


        public bool Insert(dynamic row)
        {
            sql = "EXEC [sp_" + tblname + "_Insert] N'" + row.id.ToString() + "',N'" + row.pid.ToString() + "',N'" + row.ma.ToString() + "',N'" + row.ten.ToString() + "',0,0,N'" + row.mota.ToString() + "',N'" + userId + "',N'" + userUid + "', " + row.active.ToString() + ",null,N'" + rc_id + "', 0";
            RICDB.DB.RunSQLNoReturn(sql, ref msg, strcnn);
            return (msg == "100" ? true : false);
        }
        public bool Update(dynamic row)
        {
            sql = "EXEC [sp_" + tblname + "_Update] N'" + row.id.ToString() + "',N'" + row.pid.ToString() + "',N'" + row.ma.ToString() + "',N'" + row.ten.ToString() + "',0,0,N'" + row.mota.ToString() + "',N'" + userId + "',N'" + userUid + "', " + row.active.ToString() + ",null,N'" + rc_id + "', 0";
            RICDB.DB.RunSQLNoReturn(sql, ref msg, strcnn);
            return (msg == "100" ? true : false);
        }
        public bool Delete(string ids)
        {
            sql = "EXEC [sp_" + tblname + "_Delete] N'" + ids + "', N'" + rc_id + "',N'" + userId + "'";
            RICDB.DB.RunSQLNoReturn(sql, ref msg, strcnn);

            return (msg == "100" ? true : false);
        }

        public bool Import(dynamic arr, dynamic cols)
        {

            // dua vao 1 bang tam
            // thuc hien import
            int i = 0;
            string sql1, ma, ten;
            bool b1 = true; // row data dau tien de ghi header a.b.c...
   
            sql = "delete from tblImport_Temp where rc_id='" + rc_id + "'";
            RICDB.DB.RunSQLNoReturn(sql, ref msg, strcnn);
         

            sql = " INSERT INTO tblImport_Temp(tablename, RCID,s1, s2 ) VALUES ";
            try
            {
                sql1 = sql;
                foreach (var row in arr)
                {
                    if (b1) { b1 = false; continue; }
                    ma = row[cols[1].ToString()].ToString();
                    ten = row[cols[2].ToString()].ToString();

                    ma = common.ChuanHoaXau(ma);
                    ten = common.ChuanHoaXau(ten);
                  
                    sql1 += (i == 0 ? "" : ",");
                    sql1 += "('" + tblname + "','" + rc_id + "',N'" + ma + "',N'" + ten + "' )";
                    i += 1;
                    if (i >= 100)
                    {
                        common.RunSQLNoReturn(sql1, ref msg, strcnn, ref iRowEffected);
                        if (msg != "100")
                        {
                            return false;
                        }
                        sql1 = sql;
                        i = 0;
                    }
                }
            }
            catch (Exception e)
            {
                msg = e.Message;
                return false;
            }

            common.RunSQLNoReturn(sql1, ref msg, strcnn, ref iRowEffected);

            // xu ly import trong sql server
            sql = "EXEC [sp_" + tblname + "_Import] N'" + tblname + "',N'" + rc_id + "',N'" + userId + "'";
            common.RunSQLNoReturn(sql, ref msg, strcnn, ref iRowEffected);
            return (msg == "100" ? true : false);
        }


    }

}