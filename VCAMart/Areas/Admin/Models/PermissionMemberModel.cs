using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RICTotalAdmin.Models
{
    public class PermissionMemberModel
    {
        public string strcnn;
        public int iRowEffected;
        public string msg, sql;
        public string tableName = "tblNhomquyen";
     
     
        public System.Guid User_ID { get; set; }

        public string User_UID { get; set; }

        public Nullable<System.Guid> CH_ID_X { get; set; }
        public Nullable<System.Guid> RC_ID { get; set; }
        public string Idlist { get; set; }
        public int pageSize { get; set; }
        public int pageIndex { get; set; }
        public PermissionMemberModel()
        {
            sql = ""; msg = "";
            strcnn = System.Configuration.ConfigurationManager.AppSettings["cnn"].ToString();

            try
            {

                //  userId = new Guid(System.Web.HttpContext.Current.Session["user_id"].ToString());
                pageSize = int.Parse(System.Web.HttpContext.Current.Session["userPageSize"].ToString());
                //languageId = new Guid(System.Web.HttpContext.Current.Session["lang_id"].ToString());
            }
            catch (Exception)
            {
                pageSize = 10;
            }

        }

        public string GetList(int pageCurrent)
        {
            string sql = "exec sp_" + tableName + "_Get_List ";
            return common.RunSQLToJson(sql, ref msg);
        }

        public string GetPhanQuyen(string nq)
        {
            string sql = "exec sp_tblNhomQuyen_PhanQuyen_GetList_ContentCategory '" + nq + "'";
            return common.RunSQLToJson(sql, ref msg);
        }

        public bool Insert(dynamic row)
        {
            sql = "EXEC [sp_" + tableName + "_Insert] N'" + row.id.ToString() + "',N'" + row.name.ToString() + "', " + row.active.ToString() + ",N'" + User_ID.ToString() + "'";
            RICDB.DB.RunSQLNoReturn(sql, ref msg, strcnn);

            return (msg == "100" ? true : false);
        }
        public bool Update(dynamic row)
        {
            sql = "EXEC [sp_" + tableName + "_Update] N'" + row.id.ToString() + "',N'" + row.name.ToString() + "', " + row.active.ToString() + ",N'" + RC_ID.ToString() + "',N'" + User_ID.ToString() + "'";
            RICDB.DB.RunSQLNoReturn(sql, ref msg, strcnn);
            return (msg == "100" ? true : false);
        }
        public bool Delete(string ids)
        {
            sql = "EXEC [sp_" + tableName + "_Delete] N'" + ids + "',N'" + RC_ID.ToString() + "',N'" + User_ID.ToString() + "'";
            RICDB.DB.RunSQLNoReturn(sql, ref msg, strcnn);

            return (msg == "100" ? true : false);
        }

        public bool Import(dynamic arr, dynamic cols)
        {

            // dua vao 1 bang tam
            // thuc hien import
            int i = 0;
            sql = "delete from tblImport_Temp where rc_id='" + RC_ID.ToString() + "'";
            RICDB.DB.RunSQLNoReturn(sql, ref msg, strcnn);
            string ten, status;

            sql = " INSERT INTO tblImport_Temp(tablename, RCID,s1,  i1 ) VALUES ";
            try
            {
                foreach (var row in arr)
                {

                    ten = row[cols[1].ToString()].ToString();
                    status = row[cols[2].ToString()].ToString();

                    ten = common.ChuanHoaXau(ten);
                    status = (status == "0" ? "0" : "1");

                    sql += (i == 0 ? "" : ",");
                    sql += "('" + tableName + "','" + RC_ID.ToString() + "',N'" + ten + "'," + status + " )";
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
            sql = "EXEC [sp_" + tableName + "_Import] N'" + tableName + "',N'" + RC_ID.ToString() + "',N'" + User_ID.ToString() + "'";
            common.RunSQLNoReturn(sql, ref msg, strcnn, ref iRowEffected);
            return (msg == "100" ? true : false);
        }

        public bool SavePhanQuyen(dynamic data)
        {

            int i = 0;
            string nhomid, mnid, them, sua, xoa, xem, in1, status = "1", import = "1", export = "1", phanquyen = "0";
            nhomid = data.nhomid.ToString();
            string sql = "exec sp_tblNhomQuyen_Delete_PhanQuyen_ContentCategory '" + nhomid + "'";
            RICDB.DB.RunSQLNoReturn(sql, ref msg, strcnn);

            sql = "EXEC sp_tblNhomQuyen_ContentCategory_Insert ";
            try
            {
                foreach (var row in data.items)
                {
                    String sql1 = sql;
                    mnid = row.id.ToString();
                    xem = row.xem.ToString();
                    them = row.them.ToString();
                    sua = row.sua.ToString();
                    xoa = row.xoa.ToString();
                    in1 = row.in1.ToString();
                    if (row.status != null) status = row.status.ToString();
                    if (row.import != null) import = row.import.ToString();
                    if (row.export != null) export = row.export.ToString();
                    if (row.phanquyen != null) phanquyen = row.phanquyen.ToString();

                    //sql1 += "'" + RC_ID.ToString() + "'"
                    //        + ",'" + Guid.Empty.ToString() + "'" //chid
                   //         + ",'" + Guid.Empty.ToString() + "'" //userid
                    sql1+= "'" + Guid.NewGuid().ToString() + "'" //id
                            + ",'" + nhomid + "'" //nhomid
                            + ",'" + mnid + "'" //menuid
                            + ",'" + them + "'" //them
                            + ",'" + sua + "'" //sua
                            + ",'" + xoa + "'" //sua
                            + ",'" + in1 + "'" //sua
                            + ",'" + xem + "'" //sua
                            + ",'" + status + "'" //sua
                            + ",'" + import + "'" //sua
                            + ",'" + export + "'" //sua
                    + ",'" + phanquyen + "'"; //phan quyen care -dung rieng cho ric
                    common.RunSQLNoReturn(sql1, ref msg, strcnn, ref iRowEffected);
                    if (msg != "100") break;
                    i += 1;
                }


                return (msg == "100" ? true : false);
            }
            catch (Exception e)
            {
                msg = e.Message;
                return false;
            }

            //return true;
        }
    }
}