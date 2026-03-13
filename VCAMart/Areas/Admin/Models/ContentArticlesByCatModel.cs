using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Text;
using System.Text.RegularExpressions;

namespace RICTotalAdmin.Models
{
    public class ContentArticlesByCatModel
    {
        public const string ROOT_ID = "00000000-0000-0000-0000-000000000000";
        string strcnn;
        public Guid userId;
        public Guid languageId;
        public string msg;
        public int iRowEffected;
        int pageSize;
        SqlCommand cmd;
        string tblname = "tblContentArticles";
        public string rc_id;

        public static  string URL="";
        public  static string DATA=""; 

        public string Idlist { get; set; }
        public ContentArticlesByCatModel()
        {
            
            strcnn = System.Configuration.ConfigurationManager.AppSettings["cnn"].ToString();

            try
            {
               // rc_id = System.Web.HttpContext.Current.Session["rc_id"].ToString();
                userId = new Guid(System.Web.HttpContext.Current.Session["user_id"].ToString());
                pageSize = int.Parse(System.Web.HttpContext.Current.Session["userPageSize"].ToString());
                languageId = new Guid(System.Web.HttpContext.Current.Session["lang_id"].ToString());
            }
            catch (Exception)
            {
                pageSize = 10;
            }

        }

        public string GetList(string pageSize, string pageIndex, ref int total, string CategoryID)
        {
            //string sql = "exec sp_" + tblname + "_Get_List_By_Cat " + id  + ", " + System.Web.HttpContext.Current.Session["lg_id"].ToString();
            //return common.RunSQLToJson(sql, ref msg);

            string msg = "";
            total = 0;
            string sql = "exec Admin.sp_tblContentCategory_Get_List_Page  " + pageSize + "," + pageIndex + ",'" + CategoryID + "'";// "exec [SDV_WorkerBasicInfor_Get_List]  " + page + "," + pageSize + "";
            DataTable dt;
            string strcnn = common.GetAppSetting("cnn");
            dt = RICDB.DB.RunSQL(sql, ref msg, strcnn);
            if (msg != "100") return "";
            if (dt.Rows.Count == 0) return "";
            total = int.Parse(dt.Rows[0]["total"].ToString());
            return JsonConvert.SerializeObject(dt);
        }
        public string ListParent()
        {
            string sql = "exec Admin.sp_tblContentArticles_Get_List_Parent " + System.Web.HttpContext.Current.Session["lang_id"].ToString();
            return common.RunSQLToJsonManual(sql, "", ref msg);
        }
        
        public string GetCategoryID(string CategoryID)
        {

            cmd = new SqlCommand("Admin.sp_tblContentCategory_GetID", new SqlConnection(strcnn));
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@CC_Identify", CategoryID));
            SqlParameter param = cmd.Parameters.Add("@CategoryID", SqlDbType.NVarChar, 100);

            cmd.Parameters["@CategoryID"].Direction = ParameterDirection.Output;
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
                return "";
            }

            string x = param.Value.ToString().ToUpper();
            return x;
        }
        public string GetCategoryName(string CategoryID)
        {
            cmd = new SqlCommand("Admin.sp_tblContentCategory_Name", new SqlConnection(strcnn));
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@CC_Identify", CategoryID));
            SqlParameter param = cmd.Parameters.Add("@CC_Name", SqlDbType.NVarChar, 100);

            cmd.Parameters["@CC_Name"].Direction = ParameterDirection.Output;
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
                return "";
            }

            string x = param.Value.ToString().ToUpper();
            return x;
        }

        public bool Insert(dynamic row, string uid)
        {
            string userId = uid;
            iRowEffected = 0;
            cmd = new SqlCommand("Admin.sp_" + tblname + "_Insert", new SqlConnection(strcnn));
            cmd.CommandType = CommandType.StoredProcedure;

            //data: { "MN_Id":"","MN_Parent_Id":"00000000-0000-0000-0000-000000000000","MN_Ten":"111","MN_Mota":"","MN_Url":"222","MN_Icon":"","MN_Thutu":"0","MN_Status":"0"}

            cmd.Parameters.Add(new SqlParameter("@CA_Id", row.id.ToString()));
            cmd.Parameters.Add(new SqlParameter("@CA_LG_Id", HttpContext.Current.Session["lang_id"].ToString()));
            cmd.Parameters.Add(new SqlParameter("@CA_CapHoc", row.CA_CapHoc.ToString()));
            cmd.Parameters.Add(new SqlParameter("@CA_CC_Id", row.CA_CC_Id.ToString()));
            cmd.Parameters.Add(new SqlParameter("@CA_Title", row.CA_Title.ToString()));

            cmd.Parameters.Add(new SqlParameter("@CA_SubTitle", row.CA_SubTitle.ToString()));
            cmd.Parameters.Add(new SqlParameter("@CA_FriendlyUrl", row.CA_FriendlyUrl.ToString()));

            cmd.Parameters.Add(new SqlParameter("@CA_Introduction", row.CA_Introduction.ToString()));
            cmd.Parameters.Add(new SqlParameter("@CA_ContentBody", row.CA_ContentBody.ToString()));
            cmd.Parameters.Add(new SqlParameter("@CA_Author", row.CA_Author.ToString()));
            cmd.Parameters.Add(new SqlParameter("@CA_ThumbnailPath", row.CA_ThumbnailPath.ToString()));

            cmd.Parameters.Add(new SqlParameter("@CA_Published", row.CA_Published.ToString()));
            cmd.Parameters.Add(new SqlParameter("@CA_AllowComment", row.CA_AllowComment.ToString()));
            cmd.Parameters.Add(new SqlParameter("@CA_IsHot", row.CA_IsHot.ToString()));
            cmd.Parameters.Add(new SqlParameter("@CA_ShowDate", row.CA_ShowDate.ToString()));


            cmd.Parameters.Add(new SqlParameter("@CA_PageTitle", row.CA_PageTitle.ToString()));
            cmd.Parameters.Add(new SqlParameter("@CA_MetaKeyword", row.CA_MetaKeyword.ToString()));
            cmd.Parameters.Add(new SqlParameter("@CA_MetaDescription", row.CA_MetaDescription.ToString()));
            cmd.Parameters.Add(new SqlParameter("@CA_Notes", row.CA_Notes.ToString()));
            cmd.Parameters.Add(new SqlParameter("@CA_FilePdf", row.CA_FilePdf.ToString()));
            cmd.Parameters.Add(new SqlParameter("@CA_CreatedDate", row.CA_CreatedDate.ToString()));
            cmd.Parameters.Add(new SqlParameter("@CA_UpdatedDate", row.CA_CreatedDate.ToString()));
            cmd.Parameters.Add(new SqlParameter("@CA_DisplayOnApp", row.CA_DisplayOnApp.ToString()));

            cmd.Parameters.Add(new SqlParameter("@CA_CreatedById", userId));



            //cmd.Parameters.Add(new SqlParameter("@userId", userId));
            try
            {
                cmd.Connection.Open();
                iRowEffected = cmd.ExecuteNonQuery();
                cmd.Connection.Close();
                cmd.Connection.Dispose();
                cmd.Dispose();
                try
                {
                    string noidung = row.noidung;


                    //Goi API push tin len khay tren app
                    if (row.CA_DisplayOnApp.ToString() == "1")
                        CreateObject(row.CA_Title.ToString(), noidung, row.id.ToString());
                }
                catch (Exception e2)
                {
                    msg = e2.Message;
                }
                msg = "100";
                //

            }
            catch (Exception e1)
            {
                msg = e1.Message;
                return false;
            }

            return true;
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


            iRowEffected = 0;
            cmd = new SqlCommand("Admin.sp_" + tblname + "_Update", new SqlConnection(strcnn));
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add(new SqlParameter("@CA_Id", row.id.ToString()));
            cmd.Parameters.Add(new SqlParameter("@CA_LG_Id", HttpContext.Current.Session["lang_id"].ToString()));
            cmd.Parameters.Add(new SqlParameter("@CA_CapHoc", row.CA_CapHoc.ToString()));
            cmd.Parameters.Add(new SqlParameter("@CA_CC_Id", row.CA_CC_Id.ToString()));
            cmd.Parameters.Add(new SqlParameter("@CA_Title", row.CA_Title.ToString()));

            cmd.Parameters.Add(new SqlParameter("@CA_SubTitle", row.CA_SubTitle.ToString()));
            cmd.Parameters.Add(new SqlParameter("@CA_FriendlyUrl", row.CA_FriendlyUrl.ToString()));

            cmd.Parameters.Add(new SqlParameter("@CA_Introduction", row.CA_Introduction.ToString()));
            cmd.Parameters.Add(new SqlParameter("@CA_ContentBody", row.CA_ContentBody.ToString()));
            cmd.Parameters.Add(new SqlParameter("@CA_Author", row.CA_Author.ToString()));
            cmd.Parameters.Add(new SqlParameter("@CA_ThumbnailPath", row.CA_ThumbnailPath.ToString()));

            cmd.Parameters.Add(new SqlParameter("@CA_Published", row.CA_Published.ToString()));
            cmd.Parameters.Add(new SqlParameter("@CA_AllowComment", row.CA_AllowComment.ToString()));
            cmd.Parameters.Add(new SqlParameter("@CA_IsHot", row.CA_IsHot.ToString()));
            cmd.Parameters.Add(new SqlParameter("@CA_ShowDate", row.CA_ShowDate.ToString()));


            cmd.Parameters.Add(new SqlParameter("@CA_PageTitle", row.CA_PageTitle.ToString()));
            cmd.Parameters.Add(new SqlParameter("@CA_MetaKeyword", row.CA_MetaKeyword.ToString()));
            cmd.Parameters.Add(new SqlParameter("@CA_MetaDescription", row.CA_MetaDescription.ToString()));
            cmd.Parameters.Add(new SqlParameter("@CA_Notes", row.CA_Notes.ToString()));
            cmd.Parameters.Add(new SqlParameter("@CA_FilePdf", row.CA_FilePdf.ToString()));
            cmd.Parameters.Add(new SqlParameter("@CA_UpdatedDate", row.CA_CreatedDate.ToString()));
            cmd.Parameters.Add(new SqlParameter("@CA_DisplayOnApp", row.CA_DisplayOnApp.ToString()));
            cmd.Parameters.Add(new SqlParameter("@CA_UpdatedById", userId));

            //cmd.Parameters.Add(new SqlParameter("@userId", userId));
            try
            {
                cmd.Connection.Open();
                iRowEffected = cmd.ExecuteNonQuery();
                cmd.Connection.Close();
                cmd.Connection.Dispose();
                cmd.Dispose();
                msg = "100";

                try
                {
                    string noidung = row.noidung;


                    //Goi API push tin len khay tren app
                    if (row.CA_DisplayOnApp.ToString() == "1")
                        CreateObject(row.CA_Title.ToString(), noidung, row.id.ToString());
                }
                catch (Exception e2)
                {
                    msg = e2.Message;
                }


            }
            catch (Exception e1)
            {
                msg = e1.Message;
                return false;
            }



            return (msg == "100" ? true : false);
        }

        //---------------Delete record
        public bool Delete(string ids)
        {
            iRowEffected = 0;
            cmd = new SqlCommand("Admin.sp_" + tblname + "_Delete", new SqlConnection(strcnn));
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

        public List<ContentCategoryNode> GetCategoryTree()
        {
            string sql = "exec Admin.sp_tblContentArticles_Get_List_Parent_TreeView "
                       + HttpContext.Current.Session["lang_id"].ToString();

            DataTable dt;
            string msg = "";

            dt = RICDB.DB.RunSQL(sql, ref msg, strcnn);
            if (msg != "100" || dt == null)
                return new List<ContentCategoryNode>();

            List<ContentCategoryNode> list = new List<ContentCategoryNode>();

            foreach (DataRow r in dt.Rows)
            {
                string parentId = r["CC_ParentId"] == DBNull.Value
                    ? ROOT_ID   // <<< QUAN TRỌNG
                    : r["CC_ParentId"].ToString();

                list.Add(new ContentCategoryNode
                {
                    CC_Id = r["CC_Id"].ToString(),
                    CC_Name = r["CC_Name"].ToString(),
                    CC_ParentId = parentId
                });
            }

            return list;
        }

        public List<object> BuildTreeDropdown(
            List<ContentCategoryNode> all,
            string parentId,
            string prefix = ""
        )
        {
            List<object> result = new List<object>();

            var children = all
                .Where(x => x.CC_ParentId == parentId)
                .OrderBy(x => x.CC_Name)
                .ToList();

            foreach (var item in children)
            {
                result.Add(new
                {
                    id = item.CC_Id,
                    ten = prefix + item.CC_Name
                });

                result.AddRange(
                    BuildTreeDropdown(all, item.CC_Id, prefix + "-- ")
                );
            }

            return result;
        }

        private static void CreateObject(string t, string c, string ma)
        {
            WebRequest tRequest = WebRequest.Create("https://fcm.googleapis.com/fcm/send");
            tRequest.Method = "post";
            //serverKey - Key from Firebase cloud messaging server  
            tRequest.Headers.Add(string.Format("Authorization: key=AAAAduI5_W0:APA91bGanHtl5tf3dv_O5QPEkprDdd5aXFEn4WQnhx1MhOU53t1zMApjkZCm2dmAaa5gSS2BgD1kUVSKNbfVoeUTAegSj7-al4kJpT5FXqniKrYuIrbtR2GXTIwJlSlv3oLZoNmqLV7M", "AIXXXXXX...."));
            //Sender Id - From firebase project setting  
            tRequest.Headers.Add(string.Format("Sender: id={0}", "XXXXX.."));
            tRequest.ContentType = "application/json";
            var payload = new
            {
                to = "/topics/cpa_all",
                priority = "high",
                content_available = true,
                notification = new
                {
                    body = c,
                    title = t,
                    badge = 1, 
                    id = ma
                },
                data = new
                {
                    body = c,
                    title = t,
                    id = ma
                }

            };

            string postbody = JsonConvert.SerializeObject(payload).ToString();
            Byte[] byteArray = Encoding.UTF8.GetBytes(postbody);
            tRequest.ContentLength = byteArray.Length;
            using (Stream dataStream = tRequest.GetRequestStream())
            {
                dataStream.Write(byteArray, 0, byteArray.Length);
                using (WebResponse tResponse = tRequest.GetResponse())
                {
                    using (Stream dataStreamResponse = tResponse.GetResponseStream())
                    {
                        if (dataStreamResponse != null) using (StreamReader tReader = new StreamReader(dataStreamResponse))
                            {
                                String sResponseFromServer = tReader.ReadToEnd();
                                //result.Response = sResponseFromServer;
                            }
                    }
                }
            }

        }

    }
}