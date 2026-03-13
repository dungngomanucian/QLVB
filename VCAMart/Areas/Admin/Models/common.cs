using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Text.RegularExpressions;
using System.IO;
 
using System.Net.Mail;
using System.Data.SqlClient;
using System.Net.Sockets;
using System.Net;
 
using System.Web.Mvc;

namespace RICTotalAdmin.Models
{
    public class Comment
    {
        public int value { get; set; }
        public string text { get; set; }
        public int ParentId { get; set; }
        public string hierarchy { get; set; }
        public List<Comment> Children { get; set; }
    }
    public class SubjectNode
    {
        public int value { get; set; }
        public string text { get; set; }
        public int ParentId { get; set; }
        public string hierarchy { get; set; }
        public List<SubjectNode> Children { get; set; }
    }
    public class common
    {
        public static string strcnn = System.Configuration.ConfigurationManager.AppSettings["cnn"].ToString();
        public static string userCookieTokenName = "RICTotal_Token";
        public static string urlHome = "Home";
        public static string app_title = "";
        public static string app_title_short = "";
        public static string app_description = "";
        public static string app_keywords = "";
        public static string msg;
        public static string strMenu;
        public static string controlURL = "";
        public static DateTime LastSentMailToPLD= Convert.ToDateTime("10/1/2017 23:59:59");
        public common() {
            app_title = System.Configuration.ConfigurationManager.AppSettings["app_title"].ToString();
            app_title_short = System.Configuration.ConfigurationManager.AppSettings["app_title_short"].ToString();
            app_description = System.Configuration.ConfigurationManager.AppSettings["app_description"].ToString();
            app_keywords = System.Configuration.ConfigurationManager.AppSettings["app_keywords"].ToString();
        }
        public static void InitDefault()
        {
            app_title = System.Configuration.ConfigurationManager.AppSettings["app_title"].ToString();
            app_title_short = System.Configuration.ConfigurationManager.AppSettings["app_title_short"].ToString();
            app_description = System.Configuration.ConfigurationManager.AppSettings["app_description"].ToString();
            app_keywords = System.Configuration.ConfigurationManager.AppSettings["app_keywords"].ToString();

          
        }



        public string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
       
        public bool check_code(string tableName, string codeName, string code)
        {
            bool b = false;
            string strSQL = "Select * From " + tableName + " Where " + codeName + "='" + code + "'";
            DataTable dt = new DataTable();
            DataSet ds = new DataSet();
            
            SqlConnection conn = new SqlConnection(strcnn);
            try
            {
                conn.Open();
                SqlDataAdapter da = new SqlDataAdapter(strSQL, conn);
                da.Fill(ds, "data");
                dt = ds.Tables[0];
                if (dt.Rows.Count > 0)
                    b = true;
                else
                    b = false;
                dt.Dispose();
                conn.Close();
                conn.Dispose();
            }
            catch
            {
            }
            return b;
        }

        public static string GUIDRoot(string sid)
        {
            try
            {
                return sid.Substring(0, 8) + "-" + sid.Substring(8, 4) + "-" + sid.Substring(12, 4) + "-" + sid.Substring(16, 4) + "-" + sid.Substring(20);
            }
            catch  
            {
                return Guid.Empty.ToString();
            }
            //return sid.Substring(0, 8) + "-" + sid.Substring(8, 4) + "-" + sid.Substring(12, 4) + "-" + sid.Substring(16, 4) + "-" + sid.Substring(20);
        }


        public static bool IsEmail(string s)
        {
            ///^ ([\w -\.] +)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$/;
            Regex rx = new Regex(
        @"^[-!#$%&'*+/0-9=?A-Z^_a-z{|}~](\.?[-!#$%&'*+/0-9=?A-Z^_a-z{|}~])*@[a-zA-Z](-?[a-zA-Z0-9])*(\.[a-zA-Z](-?[a-zA-Z0-9])*)+$");
            return rx.IsMatch(s);
        }


        public static string ChuanHoaXau(string s, string pass = "")
        {
            s =  s.Replace("'", "").Replace("<script", "").Replace("</script>", "").Replace("<","").Replace(">","");
            if (pass != "/") s = s.Replace("/", "");

            return s;
        }
        /// <summary>
        /// Run SQL 
        /// </summary>
        /// <param name="sql">SqlCommandText</param>
        /// <returns>Json String</returns>
        public static string RunSQLToJson(string sql, ref string msg)
        {
            System.Data.DataTable dt;
            string strcnn = common.GetAppSetting("cnn");
            dt = RICDB.DB.RunSQL(sql, ref msg, strcnn);
            if (msg != "100") return "";
            if (dt.Rows.Count == 0) return "";
            return JsonConvert.SerializeObject(dt); //, Formatting.Indented);
        }

        public static bool IsNumericType(object o)
        {
            switch (Type.GetTypeCode(o.GetType()))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// thuc hien sql tra ve json, 1 so truong object de nguyen
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="objcols">cac truong object,ko xu ly, format: ,f1,f2,f3,</param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static string RunSQLToJsonManual(string sql, string objcols,  ref string msg)
        {
            // xu ly lay ra json ca cac truong object 
            System.Data.DataTable dt;
            string strcnn = common.GetAppSetting("cnn");
            dt = RICDB.DB.RunSQL(sql, ref msg, strcnn);

            if (msg != "100") return "";
            if (dt.Rows.Count == 0) return "";

            string data = "",   row = "";
            objcols += ";";
            foreach (DataRow r in dt.Rows)
            {
                data += (data == "" ? "" : ",") + "{";
                row = "";
                foreach (DataColumn c in dt.Columns)
                {
                    if (objcols.ToLower().IndexOf(";"+ c.ColumnName.ToString().ToLower() + ";") >= 0)
                    {
                       row += (row==""?"":",") + "\"" + c.ColumnName + "\":" +( r[c.ColumnName].ToString().Trim() ==""?"[]": r[c.ColumnName].ToString().Trim()) + "";
                    }
                    else if ( IsNumericType(r[c]))
                        row += (row == "" ? "" : ",") + "\"" + c.ColumnName + "\":" + r[c.ColumnName].ToString() + "" ;
                    else
                        row += (row == "" ? "" : ",") + "\"" + c.ColumnName + "\":\"" + r[c.ColumnName].ToString() + "\"";
                }
                data += row + "}";
            }
            return "[" +  data + "]"; //, Formatting.Indented);
        }
        public static string RunSQLToJsonStandart(string sql, string objcols, ref string msg)
        {
            // xu ly lay ra json ca cac truong object 
            System.Data.DataTable dt;
            string strcnn = common.GetAppSetting("cnn");
            dt = RICDB.DB.RunSQL(sql, ref msg, strcnn);

            if (msg != "100") return "";
            if (dt.Rows.Count == 0) return "";

            string data = "", row = "";
            objcols += ";";
            foreach (DataRow r in dt.Rows)
            {
                data += (data == "" ? "" : ",") + "{";
                row = "";
                foreach (DataColumn c in dt.Columns)
                {
                    if (objcols.ToLower().IndexOf(";" + c.ColumnName.ToString().ToLower() + ";") >= 0)
                    {
                        row += (row == "" ? "" : ",") + "\"" + c.ColumnName + "\":" + (r[c.ColumnName].ToString().Trim() == "" ? "[]" : r[c.ColumnName].ToString().Trim()) + "";
                    }
                    else if (IsNumericType(r[c]))
                        row += (row == "" ? "" : ",") + c.ColumnName + ":" + r[c.ColumnName].ToString() + "";
                    else
                        row += (row == "" ? "" : ",")   + c.ColumnName + ":'" + r[c.ColumnName].ToString() + "'";
                }
                data += row + "}";
            }
            return "[" + data + "]"; //, Formatting.Indented);
        }
        public static void RunSQLNoReturn(string sql, ref string msg, string strcnn, ref int irow)
        {
            irow = 0;
            System.Data.SqlClient.SqlCommand  cmd = new System.Data.SqlClient.SqlCommand(sql, new System.Data.SqlClient.SqlConnection(strcnn));
            cmd.CommandType = CommandType.Text;
            try
            {
                cmd.Connection.Open();
                irow = cmd.ExecuteNonQuery();
                cmd.Connection.Close();
                cmd.Connection.Dispose();
                cmd.Dispose();
                msg = "100";
            }
            catch (Exception e1)
            {
                msg = e1.Message;
            }
        }
        public static string GetMenuMembers()
        {
            string mnu = "";
            mnu += "<li><a href = '/" + "Member/Profile/1" + "' ><i class='" + "fa fa-user-o" + "'></i> " + "Thông tin cá nhân" + "</a></li>";

            //mnu += "<li class='treeview'>";
            //mnu += "          <a href = '/" + "MemberProductGroups" + "' >";
            //mnu += "              <i class='" + "fa fa-archive" + "'></i> <span> " + "Quản lý sản phẩm" + "</span>";
            //mnu += "              <span class='pull-right-container'>";
            //mnu += "                  <i class='fa fa-angle-left pull-right'></i>";
            //mnu += "              </span>";
            //mnu += "          </a>";
            //mnu += "<ul class='treeview-menu'>";
            //mnu += "<li><a href = '/" + "Member/MemberProductGroups/2" + "' ><i class='" + "fa fa-address-card-o" + "'></i> " + "Nhóm hàng hóa" + "</a></li>";
            //mnu += "<li><a href = '/" + "Member/MemberProductTypes/3" + "' ><i class='" + "fa fa-bar-chart" + "'></i> " + "Loại hàng hóa" + "</a></li>";
            //mnu += "<li><a href = '/" + "Member/MemberProducts/4" + "' ><i class='" + "fa fa-clone" + "'></i> " + "Danh mục hàng hóa" + "</a></li>";
            //mnu += "</ul>";
            //mnu += "</li>";

            //mnu += "<li class='treeview'>";
            //mnu += "          <a href = '/" + "URL" + "' >";
            //mnu += "              <i class='" + "fa fa-bank" + "'></i> <span> " + "Quản lý Shop" + "</span>";
            //mnu += "              <span class='pull-right-container'>";
            //mnu += "                  <i class='fa fa-angle-left pull-right'></i>";
            //mnu += "              </span>";
            //mnu += "          </a>";
            //mnu += "<ul class='treeview-menu'>";
            //mnu += "<li><a href = '/" + "Member/MemberStore/5" + "' ><i class='" + "fa fa-address-card-o" + "'></i> " + "Quản lý gian hàng" + "</a></li>";

            //mnu += "</ul>";
            //mnu += "</li>";

            return mnu;
        }

        public static string GetParentMenus()
        { // get danh sach menu cha

            string msg = "";
            string sql = "exec [sp_tblSysMenu_Get_List_Parent]  '" + System.Web.HttpContext.Current.Session["user_id"].ToString() + "'  ";
            return RunSQLToJson(sql, ref msg);

        }
        
        public static string GetPermisionGroups()
        { // get nhom quyen de dua vao user chon

            string msg = "";
            string sql = "exec Admin.sp_tblNhomQuyen_Get_List_2Select null, '" + System.Web.HttpContext.Current.Session["user_id"].ToString() + "','" + System.Web.HttpContext.Current.Session["rc_id"].ToString() + "'";
            return RunSQLToJson(sql, ref msg);

        }
        public static string GetAllUser()
        { // get nhom quyen de dua vao user chon

            string msg = "";
            string sql = "exec sp_tblUser_Get_List_2Select  ";//, '" + System.Web.HttpContext.Current.Session["user_id"].ToString() + "','" + System.Web.HttpContext.Current.Session["rc_id"].ToString() + "'";
            return RunSQLToJson(sql, ref msg);

        }
        public static string GetTinh()
        {
            string msg="";
            string sql = "exec sp_tblTinh_GetList N'VN'";
            return RunSQLToJson(sql, ref msg);
        }
        

        public static bool checkQuyenSudung(string userId, string mnuUrl, ref string msg )
        {
            // check xem nguoi dung co duoc dung menu hay ko
            string sql ;
             
            string strcnn = common.GetAppSetting("cnn");
            msg = "";
            sql = "exec [sp_tblUsers_Check_Quyen] '" + userId + "',N'" + mnuUrl +"'";
            sql  = RICDB.DB.RunSQLReturn1Value(sql, ref msg, strcnn);

            if (msg != "100")
            {
                msg = "{\"msg\":\"" + msg + "\"}" ;
                return false;
            }
            if (sql.Trim() == "0")
            {
                msg = "{\"msg\":\"Have no using permision.\"}";
                return false;
            }
            msg = "{\"msg\":\"100\"}";
            return true;
        }
        public static bool CheckAccessMenu(string uid, string mn_url)
        {
            string sql = "exec sp_check_permission_access_menu N'" + uid + "',N'" + mn_url + "'";
            string strcnn = System.Configuration.ConfigurationManager.AppSettings["cnn"].ToString();
            DataTable dt = RICDB.DB.RunSQL(sql, ref msg, strcnn);
            if (Convert.ToInt32(dt.Rows[0]["NumRow"]) <= 0) return false;
            else return true;
        }
        public static string GetMenuAdmin(string userId)
        {
            // may menu -- fill ra dang html
            string sql, msg="";
            System.Data.DataTable dt;
            string strcnn = System.Configuration.ConfigurationManager.AppSettings["cnn"].ToString();

            string mnu = "";
            sql = "exec [sp_get_menu_list] '" + userId+ "'";
            dt = RICDB.DB.RunSQL(sql, ref msg, strcnn);
            if (msg != "100") return "";
            if (dt.Rows.Count == 0) return "";

            int i;
            int mn_tt;
            string muc = dt.Rows[0]["mn_muc"].ToString();
            int dau = 0;
            int j = 0;
            for (i = 0; i < dt.Rows.Count; i++)
            {
                // truong hop la parent 
                mn_tt = Convert.ToInt32(dt.Rows[i]["mn_tt"].ToString());
                if (dt.Rows[i]["mn_muc"].ToString() == "0")
                {
                    if (dau != 0)
                    {
                        dau = 1;
                        mnu += "</li>";
                    }
                    if (dt.Rows[i]["mn_havechild"].ToString() == "0")
                    { // ko co child => link truc tiep
                        mnu += "<li>";
                        mnu += "          <a href = '/" + dt.Rows[i]["mn_url"].ToString() +  "' class='" + (dt.Rows[i]["mn_url"].ToString()=="Bill/21" || dt.Rows[i]["mn_url"].ToString() == "POS" ? "pos_button' target='_blank":"") + "'>";
                        mnu += "              <i class='mnu_parent " + dt.Rows[i]["mn_icon"].ToString() + "'></i> <span> " + dt.Rows[i]["mn_ten"].ToString() + "</span>";
                        mnu += "          </a>";
                    }
                    else
                    {
                        mnu += "<li class='treeview menu-open'>";
                        mnu += "          <a href = '/SubMenus/" + dt.Rows[i]["mn_url"].ToString() + "' >";
                        mnu += "              <i class='mnu_parent " + dt.Rows[i]["mn_icon"].ToString() + "'></i> <span> " + dt.Rows[i]["mn_ten"].ToString() + "</span>";
                        mnu += "              <span class='pull-right-container'>";
                        mnu += "                  <i class='fa fa-angle-left pull-right'></i>";
                        mnu += "              </span>";
                        mnu += "          </a>";
                    }

                }
                else // la con - muc1
                {
                    // duyet cac con cua 1 muc
                    mnu += "  <ul class='treeview-menu' style='display:block'> "; // bat dau menu con
                    for (j = i ; j < dt.Rows.Count; j++)
                    {
                        mn_tt = Convert.ToInt32(dt.Rows[j]["mn_tt"].ToString());
                        if (dt.Rows[j]["mn_muc"].ToString() == "0") break;
                        // submenu item
                        mnu += "             <li><a href = '/" + dt.Rows[j]["mn_url"].ToString() + "/" + mn_tt  + "' ><i  class='mnu_child " + dt.Rows[j]["mn_icon"].ToString() + "'></i> " + dt.Rows[j]["mn_ten"].ToString() + "</a></li>";
                    }
                    mnu += "  </ul>"; // cua sub menu
                    i = j - 1; // de chay tiep
                }

            }
            if (mnu != "") mnu += "</li>"; // end menu cha

      // '     mnu += "<li><a href=#><i class='mnu_parent fa fa-edit'></i> <span>VAT Điện tử</span></a></li>";
            return mnu;
   }

        public static string GetSubMenus(string userId, string parentURL)
        {
            // may menu -- fill ra dang html
            string sql, msg = "";
            System.Data.DataTable dt;
            string strcnn = System.Configuration.ConfigurationManager.AppSettings["cnn"].ToString();

            string mnu = "";
            sql = "exec [sp_get_menu_list_sub] '" + System.Web.HttpContext.Current.Session["rc_id"].ToString() + "','" + userId + "','" + parentURL + "'";
            dt = RICDB.DB.RunSQL(sql, ref msg, strcnn);
            if (msg != "100") return "";
            if (dt.Rows.Count == 0) return "";

            int i;

            for (i = 0; i < dt.Rows.Count; i++)
            {
                    mnu += "<li><a target=_blank href = '/" + dt.Rows[i]["mn_url"].ToString() + "' ><div><i style='width:30px;' class=' " + dt.Rows[i]["mn_icon"].ToString() + "'></i> " + dt.Rows[i]["mn_ten"].ToString() + "</div></a></li>";
            }
           
            return "<ul>" + mnu + "</ul>";
        }
    
        public int code;

        public static string GetAppSetting(string key)
        {
            try
            {
                return System.Configuration.ConfigurationManager.AppSettings[key].ToString();
            }
            catch  
            {
                return "";
            }
        }


        /// <summary>
        /// Hàm check xem 1 đối tượng có đủ các thuộc tính hay ko, nếu ko đủ => tra về false và thuộc tính thiếu trong key
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="skeys"></param>
        /// <returns></returns>
        public static bool CheckObjectEnoughProperies(dynamic obj, string skeys, ref string key)
        {
            key = "";
            string[] s = skeys.Split(';');
            int i;
            for (i = 0; i < s.Length; i++)
            {
                s[i] = s[i].Trim();
                if (s[i] == "") continue;
                if (obj[s[i]] == null)
                {
                    key = s[i];
                    return false;
                }
            }
            return true;
        }
        public static bool checkApiRunable(string apikey, string apipass)
        {
            var key = "rictotal-dev";
            var pass = "67fa4bc6-cecb-46cb-ae49-8682d6d30beb";

            if (
                String.Compare(apikey, key, false) == 0
                && String.Compare(pass, apipass, false) == 0
                )
                return true;
            return false;
        }

        public static bool IsPhoneNumber(ref string so)
        {
            so = so.Trim();
            so = so.Trim().Replace("+", "").Replace("-", "").Replace(".", "").Replace(" ", "");

            if (so.Length >= 12 || so.Length < 10) return false;

            if (so.StartsWith("0"))
            {
                so = "84" + so.Substring(1);
            }
            return true;
        }
        public static bool IsPhoneNumberValid(string so, ref string tel)
        {
            so = so.Trim().Replace("+", "").Replace("-", "").Replace(".", "").Replace(" ", "");

            if (so.Length >= 12 || so.Length < 10) return false;

            double kq = 0;
            if (!double.TryParse(so, out kq)) return false;
            tel = so;
            return true;
        }
        /// <summary>
        /// send sms api, no statistic (no insert to db)
        /// </summary>
        /// <param name="denSo">numberList, sperate by ;</param>
        /// <param name="noidung"></param>
        /// <param name="msg"></param>
        /// <param name="isonhan"></param>
        /// <returns></returns>
        public static bool SendSMSAPI(string denSo, string noidung, ref string msg)
        {
            string url = "http://210.245.26.64/SMSBrandName/sendSMS.php?secret=df89ej2&brandname=TransTender&mobile=" + denSo + "&content=" + noidung + "";
            System.Net.WebClient client = new System.Net.WebClient();
            string s = client.DownloadString(url);
            return (s == "0" ? true : false);

            //WebRequest request = WebRequest.Create("http://www.google.com");
            //WebResponse response = request.GetResponse();
            //Stream data = response.GetResponseStream();
            //string html = String.Empty;
            //using (StreamReader sr = new StreamReader(data))
            //{
            //    html = sr.ReadToEnd();
            //}
        }

        /// <summary>
        /// send sms and insert to db to statistic
        /// </summary>
        /// <param name="denSo"></param>
        /// <param name="noidung"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static int SendSMS(string denSo, string noidung, ref string msg)
        {
            if (noidung.Trim() == "")
            {
                msg = "No content to send SMS"; return 0;
            }

            if (denSo.Trim() == "")
            {
                msg = "No number list to send SMS"; return 0;
            }

            int dem = 0;
            string[] so = denSo.Split(';');
            string s1;
            string sql;
            for (int i = 0; i < so.Length; i++)
            {
                s1 = so[i];
                if (IsPhoneNumber(ref s1))
                { // gui tin
                    if (SendSMSAPI(s1, noidung, ref msg))
                    {
                        sql = "exec sp_tblSMSSendAuto_Insert N'" + denSo + "',N'" + noidung + "',N'TransTender'";
                        RICDB.DB.RunSQLNoReturn(sql, ref msg, strcnn);
                        dem += 1;

                    }
                }
            }
            return dem;
        }

        public static bool SendEmail(string emailTo, string subject, string noidung, ref string msg)
        {
            string sql;
            sql = "exec sp_tblEmailSendAuto_Insert '',N'" + subject + "',N'" + noidung + "',N'" + emailTo + "',N'TransTender'";
            RICDB.DB.RunProcNoReturn(sql, ref msg, strcnn);
            return (msg == "100" ? true : false);

        }


        public static bool SendEmailDirect(string emailTo, string subject, string noidung, ref string msg)
        {
            msg = "100";
            try
            { 
                SmtpClient SmtpServer = new SmtpClient();
                string email = GetAppSetting("mail_email");

                SmtpServer.Credentials = new System.Net.NetworkCredential(email, GetAppSetting("mail_pwd"));
                SmtpServer.Port =  int.Parse( GetAppSetting("mail_port"));
                SmtpServer.Host = GetAppSetting("mail_server");
                SmtpServer.EnableSsl = true;
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(email, "Nhà xuất bản đại học Sư Phạm", System.Text.Encoding.UTF8);
                mail.To.Add(emailTo);
                mail.Subject = subject;
                mail.Body = noidung;
                mail.IsBodyHtml = true;
                mail.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
                SmtpServer.Send(mail);
                SmtpServer.Dispose();
            }
            catch (Exception e)
            {
                msg = e.Message;
                return false;
            }
            return true;
        }


        public static string ActiveMember(string mbid, string code)
        {

            string msg = "", sql;
            sql = "exec sp_tblMembers_Active '" + mbid + "','" + code + "'";

            string strcnn = common.GetAppSetting("cnn");
            RICDB.DB.RunSQLNoReturn(sql, ref msg, strcnn);
            return msg;
        }

        public static string DeleteMember(string mbid, string phone)
        {

            string msg = "", sql;
            sql = "exec [sp_tblMembers_Delete1] '" + mbid + "','" + phone + "'";

            string strcnn = common.GetAppSetting("cnn");
            RICDB.DB.RunSQLNoReturn(sql, ref msg, strcnn);
            return msg;
        }

        public static string GetActiveCode(string mbid, ref string msg)
        {

            string sql;
            sql = "exec [sp_tblMembers_GetActiveCode] '" + mbid + "'";

            string strcnn = common.GetAppSetting("cnn");
            sql = RICDB.DB.RunSQLReturn1Value(sql, ref msg, strcnn);
            return sql;
        }

        public static jsonClientPostUser GetMemberIdByPhoneOREmail(string value, ref string msg)
        {
            jsonClientPostUser clientData = new jsonClientPostUser();
            DataTable dt;
            string sql;
            sql = "exec [sp_tblMembers_Get_ByEmailPhone] '" + value + "'";

            string strcnn = common.GetAppSetting("cnn");
            dt = RICDB.DB.RunSQL(sql, ref msg, strcnn);

            if (msg == "100")
                if (dt.Rows.Count > 0)
                {

                    clientData.userId = dt.Rows[0]["userId"].ToString();
                    clientData.userEmail = dt.Rows[0]["userEmail"].ToString();
                    clientData.userFullName = dt.Rows[0]["userFullName"].ToString();
                    clientData.userTel = dt.Rows[0]["userTel"].ToString();
                    clientData.userUid = dt.Rows[0]["userUid"].ToString();
                    clientData.token = "";
                    clientData.userAvata = dt.Rows[0]["userAvata"].ToString();
                    clientData.langId = dt.Rows[0]["MB_LG_ID"].ToString(); ;
                    try
                    {
                        clientData.userMultiLogin = int.Parse(dt.Rows[0]["userMultiLogin"].ToString());
                    }
                    catch  
                    {
                        clientData.userMultiLogin = 1;

                    }
                    dt.Dispose();
                }
                else
                    msg = "110";
            return clientData;
        }
        public static string GetMenuTopAdmin(int Position, string lg_id)
        {
            strMenu = "";
            int CC_Identify = 0;
            DataTable dt;

            string sql = "exec Admin.sp_tblContentCategory_Get_List_Top_Menu_Admin " + Position + "," + lg_id; ;


            dt = RICDB.DB.RunSQL(sql, ref msg, strcnn);
            controlURL = "Admin/ContentArticlesByCat";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                //controlURL = dt.Rows[i]["CC_FriendlyUrl"].ToString().Trim();

                CC_Identify = Convert.ToInt32(dt.Rows[i]["CC_Identify"]);
                if (dt.Rows[i]["CC_ParentId"].ToString() == "00000000-0000-0000-0000-000000000000")
                {
                    string s = "exec Admin.sp_tblContentCategory_Get_PerentList '" + dt.Rows[i]["CC_Id"].ToString() + "'";
                    DataTable dt1 = RICDB.DB.RunSQL(s, ref msg, strcnn);
                    if (dt1.Rows.Count > 0)
                    {
                        strMenu += "<li class='dropdown default-dropdown'>";
                        strMenu += "<a class='dropdown-toggle' data-toggle='dropdown' aria-expanded='true'>" + dt.Rows[i]["CC_Name"].ToString() + "&nbsp;&nbsp;<i class='fa fa-caret-down'></i></a>";


                    }
                    else
                    {
                        strMenu += "<li><a href='/" + controlURL + "/" + CC_Identify + "'>" + dt.Rows[i]["CC_Name"].ToString() + "</a></li>";
                    }

                    string value = dt.Rows[i]["CC_Id"].ToString();
                    FillChildAdmin(value);
                    dt1.Dispose();
                }
                else
                {

                }
            }

            return strMenu;
        }
        public static int FillChildAdmin(string IID)
        {
            int CC_Identify = 0;

            string sql = "exec Admin.sp_tblContentCategory_Get_PerentList '" + IID + "'";
            // return common.RunSQLToJson(sql);
            DataTable dt = RICDB.DB.RunSQL(sql, ref msg, strcnn);
            controlURL = "Admin/ContentArticlesByCat";
            if (dt.Rows.Count > 0)
            {

                strMenu += "<ul class='custom-menu'>";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    // controlURL = dt.Rows[i]["CC_FriendlyUrl"].ToString().Trim();
                    CC_Identify = Convert.ToInt32(dt.Rows[i]["CC_Identify"]);
                    strMenu += "<li style='list-style: none;'><a href='/" + controlURL + "/" + CC_Identify + "'>" + dt.Rows[i]["CC_Name"].ToString() + "</a></li>";
                    string temp = dt.Rows[i]["CC_Id"].ToString();
                    FillChildAdmin(temp);
                }
                strMenu += "</ul>";
                strMenu += "</li>";
            }


            return 0;
        }

        //public static string GetMenuAdmin(string userId)
        //{
        //    // may menu -- fill ra dang html
        //    string sql, msg = "";
        //    System.Data.DataTable dt;
        //    string strcnn = System.Configuration.ConfigurationManager.AppSettings["cnn"].ToString();

        //    string mnu = "";
        //    sql = "exec [sp_get_menu_list_4admin] '" + userId + "'";
        //    dt = RICDB.DB.RunSQL(sql, ref msg, strcnn);
        //    if (msg != "100") return "";
        //    if (dt.Rows.Count == 0) return "";

        //    int i;

        //    string muc = dt.Rows[0]["mn_muc"].ToString();
        //    int dau = 0;
        //    int j = 0;
        //    for (i = 0; i < dt.Rows.Count; i++)
        //    {
        //        // truong hop la parent 
        //        if (dt.Rows[i]["mn_muc"].ToString() == "0")
        //        {
        //            if (dau != 0)
        //            {
        //                dau = 1;
        //                mnu += "</li>";
        //            }
        //            mnu += "<li class='treeview'>";
        //            mnu += "          <a href = '" + dt.Rows[i]["mn_url"].ToString() + "' >";
        //            mnu += "              <i class='" + dt.Rows[i]["mn_icon"].ToString() + "'></i> <span> " + dt.Rows[i]["mn_ten"].ToString() + "</span>";
        //            mnu += "              <span class='pull-right-container'>";
        //            mnu += "                  <i class='fa fa-angle-left pull-right'></i>";
        //            mnu += "              </span>";
        //            mnu += "          </a>";

        //        }
        //        else // la con - muc1
        //        {
        //            // duyet cac con cua 1 muc
        //            mnu += "  <ul class='treeview-menu'>"; // bat dau menu con
        //            for (j = i; j < dt.Rows.Count; j++)
        //            {
        //                if (dt.Rows[j]["mn_muc"].ToString() == "0") break;
        //                // submenu item
        //                mnu += "             <li><a href = '" + dt.Rows[j]["mn_url"].ToString() + "' ><i class='" + dt.Rows[j]["mn_icon"].ToString() + "'></i> " + dt.Rows[j]["mn_ten"].ToString() + "</a></li>";
        //            }
        //            mnu += "  </ul>"; // cua sub menu
        //            i = j - 1; // de chay tiep
        //        }

        //    }
        //    if (mnu != "") mnu += "</li>"; // end menu cha

        //    return mnu;
        //}


        // khi quen 

        /// <summary>
        /// gui mat khau cho 1 email
        /// </summary>
        /// <param name="mbid"></param>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static bool emailMatkhau(string email, ref string msg)
        {
            string fs = System.Web.HttpContext.Current.Server.MapPath("~/") + "templatefiles\\forgotpassword.html";

            // doc csdl 
            string sql;
            sql = "exec [sp_tblUser_GetPassword] N'" + email+ "'";
            string strcnn = GetAppSetting("cnn");
            sql = RICDB.DB.RunSQLReturn1Value(sql, ref msg, strcnn);

            if (msg != "100") return false;
            
            string snd;
            try
            {
                snd = File.ReadAllText(fs, System.Text.Encoding.UTF8);
            }
            catch (Exception e)
            {
                msg = e.Message;
                return false;
            }
            snd = snd.Replace("{{UID}}", email).Replace("{{PASSWORD}}", sql.Trim());
            return SendEmailDirect(email, "RICTotal: Lấy lại mật khẩu", snd, ref msg);
        }

        public static bool ImageInsert(string path, string loai, string referenceId, string alt,string rc_id, ref string msg, long size = 0, int w = 0, int h = 0)
        {
            string sql;
            sql = "exec [sp_tblAnh_Insert] N'" + path + "','" + loai +"','" + (referenceId.Trim() ==""?Guid.Empty.ToString():referenceId) + "',N'" + alt +"'," +size.ToString() +"," + w.ToString()  + "," + h.ToString()  + ",'" + rc_id + "'";
            string strcnn = GetAppSetting("cnn");
            sql = RICDB.DB.RunSQLReturn1Value(sql, ref msg, strcnn);
            if (msg != "100") return false;
            sql = "";
            path = path.Replace(System.Web.HttpContext.Current.Server.MapPath("~/"), "");
            path = "/" + path.Replace("\\", "/");
            switch (loai)
            {
                case "logo":
                    sql  = "EXEC [sp_tblRicCustomerList_Update_Logo] '" + rc_id + "','" + path + "'";
                    break;
                case "product":
                   
                    break;
                case "background":
                   
                    break;
                case "category":
                
                    break;
            }

            RICDB.DB.RunSQLNoReturn(sql, ref msg, strcnn);
            if (msg != "100") return false;
            return true;
        }

        public static bool ImageDelete(string id, string path,string rc_id, ref string msg)
        {
            string sql;
            
            sql = "exec [sp_tblAnh_Delete] N'" + id + "','" + path + "','" + rc_id + "'";

            string strcnn = GetAppSetting("cnn");
            sql = RICDB.DB.RunSQLReturn1Value(sql, ref msg, strcnn);

            if (msg != "100") return false;
            return true;
        }


        public static System.Drawing.Bitmap ImageResize(System.Drawing.Image image_file, int max_height, int max_width)
        {
            // string fileName = Server.HtmlEncode(File1.FileName);
            // string extension = System.IO.Path.GetExtension(fileName);
            //System.Drawing.Image image_file = System.Drawing.Image.FromStream(File1.PostedFile.InputStream);
            int image_height = image_file.Height;
            int image_width = image_file.Width;

            image_height = (image_height * max_width) / image_width;
            image_width = max_width;

            if (image_height > max_height)
            {
                image_width = (image_width * max_height) / image_height;
                image_height = max_height;
            }
            System.Drawing.Bitmap bitmap_file = new System.Drawing.Bitmap(image_file, image_width, image_height);
            // bitmap_file.Save(filename, System.Drawing.Imaging.ImageFormat.Png);
            return bitmap_file;
            //  return true;
        }


        public static string ImageSavePath(string type)
        { // lay thu muc luu anh
            string s = "/Uploads/";
            switch (type)
            {
                case "logo":
                    s += "Logos/";
                    break;
                case "product":
                    s += "Products/";
                    break;
                case "background":
                    s += "Backgrounds/";
                    break;
                case "category":
                    s += "Categories/";
                    break;
            }
            return s;
        }

        public static string ImageFileName(string type, string rc_id)
        { // dat ten file tuong ung voi muc dich su dung
            string s = "FL_" + Guid.NewGuid().ToString().Replace("-", "").ToLower() + "_" + ".png";
            switch (type)
            {
                case "logo":
                    s = "LG_" + rc_id.Replace("-", "").ToLower() + ".png";
                    break;
                case "product":
                    s = "P_" + Guid.NewGuid().ToString().Replace("-", "").ToLower() + ".png";
                    break;
                case "background":
                    s = "BG_" + Guid.NewGuid().ToString().Replace("-", "").ToLower() + ".png";
                    break;
                case "category":
                    s = "C_" + Guid.NewGuid().ToString().Replace("-", "").ToLower() + ".png";
                    break;
            }
            return s;
        }
        public static DateTime DMYtoMDY(string s)
        {
            string [] s1 = s.Split('/');
            try
            {
                DateTime d1 = new DateTime(int.Parse(s1[2]), int.Parse(s1[1]), int.Parse(s1[0]));
            }catch  
            {
                return DateTime.Now;
            }
            //YYYY-MM-DDThh:mm:ss.sTZD 
            return new DateTime(int.Parse( s1[2]), int.Parse(s1[1]), int.Parse(s1[0]), DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second, DateTime.Now.Millisecond);
                
        }
        public static string formatDateTime(string format, DateTime dt)
        {
            string s = "";
            s = format.Replace("hh", dt.Hour < 10 ? "0" : "" + dt.Hour.ToString());
                s = s.Replace("mmm", dt.Minute < 10 ? "0" : "" + dt.Minute.ToString());
                s = s.Replace("ss", dt.Second < 10 ? "0" : "" + dt.Second.ToString());
                s = s.Replace("ms", (dt.Second < 10 ? "00" : (dt.Second < 100 ? "0" : "")) + dt.Millisecond.ToString());

                s = s.Replace("YYYY", dt.Year.ToString());
                s = s.Replace("MM", dt.Month < 10 ? "0" : "" + dt.Month.ToString());
                s = s.Replace("DD", dt.Day < 10 ? "0" : "" + dt.Day.ToString());
                
            return s;
        }

        private static readonly string[] VietNamChar = new string[] 
        { 
            "aAeEoOuUiIdDyY", 
            "áàạảãâấầậẩẫăắằặẳẵ", 
            "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ", 
            "éèẹẻẽêếềệểễ", 
            "ÉÈẸẺẼÊẾỀỆỂỄ", 
            "óòọỏõôốồộổỗơớờợởỡ", 
            "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ", 
            "úùụủũưứừựửữ", 
            "ÚÙỤỦŨƯỨỪỰỬỮ", 
            "íìịỉĩ", 
            "ÍÌỊỈĨ", 
            "đ", 
            "Đ", 
            "ýỳỵỷỹ", 
            "ÝỲỴỶỸ" 
         };
        public  string LocDau(string str)
        {
            //Thay thế và lọc dấu từng char      
            for (int i = 1; i < VietNamChar.Length; i++)
            {
                for (int j = 0; j < VietNamChar[i].Length; j++)
                    str = str.Replace(VietNamChar[i][j], VietNamChar[0][i - 1]);
            }
            return str;
        }



    } // end class
} // end namespace


