using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace RICTotal.Models
{
    public class Menu
    {
         string sql, strcnn;
        public    Guid  userId;
        public Guid languageId;
        public string msg;
        public int iRowEffected;
        int pageSize;
        SqlCommand cmd;
        string tblname = "tblMenu";
        public string rc_id;
      
        public string Idlist { get; set; }
        public Menu()
        {
            sql = ""; msg = "";
            strcnn = System.Configuration.ConfigurationManager.AppSettings["cnn"].ToString();
         
            try
            {
                rc_id =  System.Web.HttpContext.Current.Session["rc_id"].ToString() ;
                userId = new Guid(System.Web.HttpContext.Current.Session["user_id"].ToString());
                pageSize = int.Parse(System.Web.HttpContext.Current.Session["userPageSize"].ToString());
                languageId = new Guid(System.Web.HttpContext.Current.Session["lang_id"].ToString());
            }
            catch (Exception e1)
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
            string sql = "exec [sp_tblMenu_Get_List_Parent] ";
            return common.RunSQLToJson(sql, ref msg);
            //return common.RunSQLToJsonManual(sql, ";MN_Parent_Name;MN_Id;MN_Ten;MN_Url;MN_Icon;MN_Thutu, active;", ref msg);
        }

        public bool Insert(dynamic row, string uid)
        {
            string userId = uid;
            string MN_Id = Guid.NewGuid().ToString();
          
            
            //string quyen = row.qid.ToString();
           // quyen = quyen.Replace("\"", "").Replace("\n", "").Replace("\r", "").Replace("[", "").Replace("]", "").Trim().Replace(" ", "");
           

           // sql = "EXEC [sp_" + tblname + "_Insert] N'" + row.id.ToString() + "',N'" + row.uidname.ToString() + "',N'" + row.pwd.ToString() + "',N'" + row.name.ToString() + "',1, N'" + row.tel.ToString() + "',100,N'" + rc_id + "',N'" + row.active.ToString() + "',N'" + quyen + "',N'" + chinhanh + "',1";
         //   RICDB.DB.RunSQLNoReturn(sql, ref msg, strcnn);

            
            iRowEffected = 0;
            cmd = new SqlCommand("sp_" + tblname + "_Insert", new SqlConnection(strcnn));
            cmd.CommandType = CommandType.StoredProcedure;
           
           // string NQMN_Id = 
            //data: { "MN_Id":"","MN_Parent_Id":"00000000-0000-0000-0000-000000000000","MN_Ten":"111","MN_Mota":"","MN_Url":"222","MN_Icon":"","MN_Thutu":"0","MN_Status":"0"}
            //  string a = HttpContext.Current.Session["lang_id"].ToString();
            cmd.Parameters.Add(new SqlParameter("@MN_Id", MN_Id));
            cmd.Parameters.Add(new SqlParameter("@MN_Parent_Id", row.MN_Parent_Id.ToString()));
          
            cmd.Parameters.Add(new SqlParameter("@MN_Ten", row.MN_Ten.ToString()));
            cmd.Parameters.Add(new SqlParameter("@MN_Url", row.MN_Url.ToString()));
            cmd.Parameters.Add(new SqlParameter("@MN_Icon", row.MN_Icon.ToString()));
            cmd.Parameters.Add(new SqlParameter("@MN_Thutu", row.MN_Thutu.ToString()));

            cmd.Parameters.Add(new SqlParameter("@Active", row.active.ToString()));
      
            //cmd.Parameters.Add(new SqlParameter("@userId", userId));
            try
            {
                cmd.Connection.Open();
                iRowEffected = cmd.ExecuteNonQuery();
                cmd.Connection.Close();
                cmd.Connection.Dispose();
                cmd.Dispose();
                msg = "100";
                //Insert vao bang NhomQuyen_Menu
                DataTable dt = new DataTable();
                DataSet ds = new DataSet();
                string strSQL = "select * from tblNhomQuyen"; // Where UNQ_USER_ID='" + userId + "'";
                SqlConnection conn = new SqlConnection(strcnn);
                try
                {
                    conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter(strSQL, conn);
                    da.Fill(ds, "data");
                    dt = ds.Tables[0];
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        string groupid = dt.Rows[i]["NQ_ID"].ToString();
                        sql = "EXEC [sp_tblNhomQuyen_Menu_Insert_Value] N'" + groupid + "',N'" + MN_Id + "'";
                        RICDB.DB.RunSQLNoReturn(sql, ref msg, strcnn);
                    }
                }
                catch
                {
                }

            }
            catch (Exception e1)
            {
                msg = e1.Message;
                return false;
            }
            
     
            return (msg == "100" ? true : false);
        }
        //---------------Update
        public bool Update(dynamic row)
        {
            //string quyen = row.qid.ToString();
            // quyen = quyen.Replace("\"", "").Replace("\n", "").Replace("\r", "").Replace("[", "").Replace("]", "").Trim().Replace(" ", "");


            // sql = "EXEC [sp_" + tblname + "_Insert] N'" + row.id.ToString() + "',N'" + row.uidname.ToString() + "',N'" + row.pwd.ToString() + "',N'" + row.name.ToString() + "',1, N'" + row.tel.ToString() + "',100,N'" + rc_id + "',N'" + row.active.ToString() + "',N'" + quyen + "',N'" + chinhanh + "',1";
            //   RICDB.DB.RunSQLNoReturn(sql, ref msg, strcnn);


            iRowEffected = 0;
            cmd = new SqlCommand("sp_" + tblname + "_Update", new SqlConnection(strcnn));
            cmd.CommandType = CommandType.StoredProcedure;

            //data: { "MN_Id":"","MN_Parent_Id":"00000000-0000-0000-0000-000000000000","MN_Ten":"111","MN_Mota":"","MN_Url":"222","MN_Icon":"","MN_Thutu":"0","MN_Status":"0"}
            //  string a = HttpContext.Current.Session["lang_id"].ToString();
            cmd.Parameters.Add(new SqlParameter("@MN_Id", row.MN_Id.ToString()));
            cmd.Parameters.Add(new SqlParameter("@MN_Parent_Id", row.MN_Parent_Id.ToString()));

            cmd.Parameters.Add(new SqlParameter("@MN_Ten", row.MN_Ten.ToString()));
            cmd.Parameters.Add(new SqlParameter("@MN_Url", row.MN_Url.ToString()));
            cmd.Parameters.Add(new SqlParameter("@MN_Icon", row.MN_Icon.ToString()));
            cmd.Parameters.Add(new SqlParameter("@MN_Thutu", row.MN_Thutu.ToString()));

            cmd.Parameters.Add(new SqlParameter("@Active", row.active.ToString()));

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