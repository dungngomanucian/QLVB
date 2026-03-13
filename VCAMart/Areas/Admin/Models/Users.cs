using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RICTotalAdmin.Models
{
    public class Users
    {
        string sql, strcnn;
        public Guid userId;
        public Guid languageId;
        public string msg;
        public int iRowEffected;
        int pageSize;
        SqlCommand cmd;
        string tblname = "tblUser";
        public string rc_id;

        public string Idlist { get; set; }
        public Users()
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
            catch (Exception )
            {
                pageSize = 10;
            }

        }

        public string GetList(int pageCurrent)
        {
            string sql = "exec Admin.sp_" + tblname + "_Get_List ";
            return common.RunSQLToJsonManual(sql, ";qid;qten;cnid;cnten;", ref msg);
        }
        public string GetListParent(int pageCurrent)
        {
            string sql = "exec Admin.sp_" + tblname + "_Get_List_Parent '" + rc_id + "'";
            return common.RunSQLToJsonManual(sql, "", ref msg);
        }

        public string Get_ChucVu()
        {
            string sql = "exec sp_tblUser_Get_ChucVu";
            return common.RunSQLToJsonManual(sql, "", ref msg);
        }
        public string Get_PB()
        {
            string sql = "exec sp_tblUser_Get_PhongBan";
            return common.RunSQLToJsonManual(sql, "", ref msg);
        }
        public bool existsUser(string uid)
        {
            string sql = "exec Admin.sp_" + tblname + "_CheckExists N'" + uid + "'";
            sql = RICDB.DB.RunSQLReturn1Value(sql, ref msg, strcnn);
            if (msg != "100") return false;
            return (sql.Trim() == "0" ? false : true);
        }


        public bool existsEmail(string email)
        {
            string sql = "exec Admin.sp_check_email_exists N'" + email + "'";
            sql = RICDB.DB.RunSQLReturn1Value(sql, ref msg, strcnn);
            if (msg != "100") return false;
            return (sql.Trim() == "0" ? false : true);
        }

        /// <summary>
        /// thuc hien dang ky 1 nguoi dung moi
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public bool DangKy(dynamic row)
        {
            iRowEffected = 0;
            sql = "EXEC [sp_dangky_nhanh_email] N'" + row.id.ToString() + "',N'" + row.email.ToString() + "'";
            RICDB.DB.RunSQLNoReturn(sql, ref msg, strcnn);
            return (msg == "100" ? true : false);
        }


        /// <summary>
        /// Cap nhap thong tin cho user 
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public bool CapNhatThongtin(dynamic row)
        {
            iRowEffected = 0;
            sql = "EXEC [sp_dangky_nhanh_updatethongtin] N'" + row.id.ToString() + "',N'" + row.tencongty.ToString() + "',N'" + row.ten.ToString() + "',N'" + row.diachi.ToString() + "',N'" + row.tel.ToString() + "',N'" + row.pwd.ToString() + "',N'" + row.tinh.ToString() + "',N'" + row.nganh.ToString() + "'";
            RICDB.DB.RunSQLNoReturn(sql, ref msg, strcnn);
            return (msg == "100" ? true : false);
        }


        public bool Insert(dynamic row)
        {
            string quyen = row.qid.ToString();
            quyen = quyen.Replace("\"", "").Replace("\n", "").Replace("\r", "").Replace("[", "").Replace("]", "").Trim().Replace(" ", "");
            string chinhanh = row.cnid.ToString();
            chinhanh = chinhanh.Replace("\"", "").Replace("\n", "").Replace("\r", "").Replace("[", "").Replace("]", "").Trim().Replace(" ", "");
            
            
            sql = "EXEC Admin.sp_" + tblname + "_Insert N'" + row.User_CV_Id + "',N'" + row.User_PB_Id + "',N'" + row.id.ToString() + "',N'" + row.User_Avatar + "',N'" + row.uidname.ToString() + "',N'" + row.pwd.ToString() + "',N'" + row.name.ToString() + "',1, N'" + row.tel.ToString() + "',100,N'" + rc_id + "',N'" + row.active.ToString() + "',N'" + row.User_LaTP + "',N'" + quyen + "',N'" + chinhanh + "',1";
            RICDB.DB.RunSQLNoReturn(sql, ref msg, strcnn);

            return (msg == "100" ? true : false);
        }
        public bool Update(dynamic row)
        {

            string quyen = row.qid.ToString();
            quyen = quyen.Replace("\"", "").Replace("\n", "").Replace("\r", "").Replace("[", "").Replace("]", "").Trim().Replace(" ", "");
            string chinhanh = row.cnid.ToString();
            chinhanh = chinhanh.Replace("\"", "").Replace("\n", "").Replace("\r", "").Replace("[", "").Replace("]", "").Trim().Replace(" ", "");

            sql = "EXEC Admin.sp_" + tblname + "_Update '" + row.User_CV_Id.ToString() + "', N'" + row.User_PB_Id.ToString() + "',N'" + row.id.ToString() + "',N'" + row.User_Avatar + "',N'" + row.uidname.ToString() + "',N'" + row.pwd.ToString() + "',N'" + row.name.ToString() + "',1, N'" + row.tel.ToString() + "',100,N'" + rc_id + "',N'" + row.active.ToString() + "',N'" + row.User_LaTP + "',N'" + quyen + "',N'" + chinhanh + "',1";
            RICDB.DB.RunSQLNoReturn(sql, ref msg, strcnn);

            return (msg == "100" ? true : false);
        }
        public bool Delete(string ids)
        {
            sql = "EXEC Admin.sp_" + tblname + "_Delete N'" + ids + "',N'" + rc_id + "',N'" + userId + "'";
            RICDB.DB.RunSQLNoReturn(sql, ref msg, strcnn);

            return (msg == "100" ? true : false);
        }

        public bool Import(dynamic arr, dynamic cols)
        {

            // dua vao 1 bang tam
            // thuc hien import
            int i = 0;
            sql = "delete from tblImport_Temp where rc_id='" + rc_id + "'";
            RICDB.DB.RunSQLNoReturn(sql, ref msg, strcnn);
            string uid, ten, matkhau, phone, status;

            sql = " INSERT INTO tblImport_Temp(tablename, RCID,s1, s2, s3, s4, i1 ) VALUES ";
            try
            {
                foreach (var row in arr)
                {
                    uid = row[cols[1].ToString()].ToString();
                    ten = row[cols[2].ToString()].ToString();
                    matkhau = row[cols[3].ToString()].ToString();
                    phone = row[cols[4].ToString()].ToString();
                    status = row[cols[5].ToString()].ToString();

                    uid = common.ChuanHoaXau(uid);
                    ten = common.ChuanHoaXau(ten);
                    matkhau = common.ChuanHoaXau(matkhau);
                    phone = common.ChuanHoaXau(phone);
                    status = (status == "0" ? "0" : "1");

                    sql += (i == 0 ? "" : ",");
                    sql += "('" + tblname + "','" + rc_id + "',N'" + uid + "',N'" + ten + "',N'" + matkhau + "',N'" + phone + "'," + status + " )";
                    i += 1;
                }
            }
            catch (Exception e)
            {
                msg = e.Message;
                return false;
            }

            common.RunSQLNoReturn(sql, ref msg, strcnn, ref iRowEffected);

            // xu ly import trong sql server
            sql = "EXEC Admin.sp_" + tblname + "_Import N'" + tblname + "',N'" + rc_id + "',N'" + userId + "'";
            common.RunSQLNoReturn(sql, ref msg, strcnn, ref iRowEffected);
            return (msg == "100" ? true : false);
        }


        public bool ActiveDeactive(ref string msg)
        {

            sql = "EXEC Admin.sp_" + tblname + "_ActiveDeactive N'" + Idlist + "',N'" + rc_id.ToString() + "',N'" + userId + "'";
            common.RunSQLNoReturn(sql, ref msg, strcnn, ref iRowEffected);
            return (msg == "100" ? true : false);

        }

        public bool checkCurrentPassword(string oldpass, ref string msg)
        {
            sql = "EXEC Admin.sp_" + tblname + "_GetPassword_ByID N'" + userId + "'";
            string kq = RICDB.DB.RunSQLReturn1Value(sql, ref msg, strcnn);

            if (msg != "100") return false;
            if (
                string.Compare(kq.Trim(), oldpass.Trim(), false) != 0
                )
            {
                msg = "Mật khẩu cũ không đúng.";
                return false;
            }
            return true;
        }

        public bool ChangePassword(string pass, ref string msg)
        {
            sql = "EXEC Admin.sp_" + tblname + "_SetPassword N'" + userId + "',N'" + pass + "'";
            RICDB.DB.RunSQLNoReturn(sql, ref msg, strcnn);
            return (msg == "100" ? true : false);
        }




    }

}