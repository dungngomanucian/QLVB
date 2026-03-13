using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RICTotalAdmin.Models
{
    public class DuyetThanhVienModel
    {
        string sql, strcnn;
        public Guid userId;
        public Guid languageId;
        public string msg;
        public int iRowEffected;
        int pageSize;
        SqlCommand cmd;
        string tblname = "tblMembers";
        public string rc_id;

        public string Idlist { get; set; }
        public DuyetThanhVienModel()
        {
            sql = ""; msg = "";
            strcnn = System.Configuration.ConfigurationManager.AppSettings["cnn"].ToString();

            try
            {
                rc_id = System.Web.HttpContext.Current.Session["rc_id"].ToString();
                userId = new Guid(System.Web.HttpContext.Current.Session["user_id"].ToString());
                pageSize = int.Parse(System.Web.HttpContext.Current.Session["userPageSize"].ToString());
                languageId = new Guid(System.Web.HttpContext.Current.Session["lang_id"].ToString());
            }
            catch (Exception)
            {
                pageSize = 10;
            }

        }

        public string GetList(string uid)
        {
            string sql = "exec sp_tblMembers_Get_List_group ";
            // return common.RunSQLToJsonManual(sql, "", ref msg);
            return common.RunSQLToJsonManual(sql, ";qid;qten;cnid;cnten;", ref msg);
        }

        public  string GetBookGroup()
        { // get nhom quyen de dua vao user chon

            string msg = "";
            string sql = "exec sp_tblNhomQuyen_Get_List_2Select  '" +  System.Web.HttpContext.Current.Session["lang_id"].ToString() + "'";
            return common.RunSQLToJson(sql, ref msg);

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

            string quyen = row.qid.ToString();
            quyen = quyen.Replace("\"", "").Replace("\n", "").Replace("\r", "").Replace("[", "").Replace("]", "").Trim().Replace(" ", "");


            sql = "EXEC [sp_tblMembers_Update_Status] N'" + row.id.ToString() + "','" + row.active.ToString() + "','" + row.MB_isAdmin + "',N'" + row.name + "',N'" + row.MB_Phone + "',N'" + row.MB_Address +  "',N'" + quyen + "'";
            RICDB.DB.RunSQLNoReturn(sql, ref msg, strcnn);

            return (msg == "100" ? true : false);
            /*

            iRowEffected = 0;
            cmd = new SqlCommand("sp_" + tblname + "_Update", new SqlConnection(strcnn));
            cmd.CommandType = CommandType.StoredProcedure;





            cmd.Parameters.Add(new SqlParameter("@MB_Id", row.id.ToString()));
            cmd.Parameters.Add(new SqlParameter("@MB_Active", row.active.ToString()));
         
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



            return (msg == "100" ? true : false);*/
        }

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