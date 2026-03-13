using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using RICTotal.Models;
using System.Data;
using System.Collections;
using System.Data.SqlClient;
 

namespace RICTotal.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        Home mdl = new Home();         
        jsonClientPost jc = new jsonClientPost();
        

        public string tblname;
        public string tbaoquyenxoa = "Bạn không có quyền xóa thông tin này";
        public string tbaoquyensua = "Bạn không có quyền sửa thông tin này";
        public string tbaoquyenthem = "Bạn không có quyền thêm thông tin này";
        public static string langId;
        public static string msg = "";
        SqlCommand cmd;
        string sql, strcnn;
        public HomeController()
        {
            try
            {
                ViewBag.ControllerName = this.GetType().Name.Replace("Controller", "");
                strcnn = System.Configuration.ConfigurationManager.AppSettings["cnn"].ToString();

                RICTotal.Models.common.InitDefault();

                string x = SaveUserInfoToDB();
               
            }
            catch (Exception e) { }

            try
            {
                if (System.Web.HttpContext.Current.Session["userId"] !=null)
                {
                    string u = System.Web.HttpContext.Current.Session["userId"].ToString();
                    if(System.Web.HttpContext.Current.Session["MBAvata"]!=null)
                        ViewBag.MB_avata = System.Web.HttpContext.Current.Session["MBAvata"].ToString();
                    if(System.Web.HttpContext.Current.Session["MBFullName"]!=null)
                        ViewBag.MB_fullname = System.Web.HttpContext.Current.Session["MBFullName"].ToString();
                    if(System.Web.HttpContext.Current.Session["MBName"]!=null)
                        ViewBag.MB_name = System.Web.HttpContext.Current.Session["MBName"].ToString();
                    if(System.Web.HttpContext.Current.Session["MBPwd"]!=null)
                        ViewBag.MBPass = System.Web.HttpContext.Current.Session["MBPwd"].ToString();
                    if(System.Web.HttpContext.Current.Session["MB_id"]!=null)
                        ViewBag.MBId = System.Web.HttpContext.Current.Session["MB_id"].ToString();
                    if(System.Web.HttpContext.Current.Session["MB_PB_Id"]!=null)
                        ViewBag.MB_PB_Id = System.Web.HttpContext.Current.Session["MB_PB_Id"].ToString();
                    if(System.Web.HttpContext.Current.Session["MB_isAdmin"]!=null)
                        ViewBag.MB_isAdmin = System.Web.HttpContext.Current.Session["MB_isAdmin"].ToString();
                    if(System.Web.HttpContext.Current.Session["userPageSize"]!=null)
                        ViewBag.pagesize = int.Parse(System.Web.HttpContext.Current.Session["userPageSize"].ToString());
                }
                else
                {
                   // (new LIB.Logout(common.strcnn)).RunLogout_Member();
                     
                }
            }
            catch (Exception e1)
            {
                ViewBag.pagesize = 10;
            }

            jc = new jsonClientPost();
            common.InitDefault();

           // RICTotal.Models.common.InitDefault();
            try
            {
                ViewBag.datachart = mdl.GetSumTrainResult();
                ViewBag.AIExpert = mdl.GetAIExpert();
                ViewBag.AIPro = mdl.GetAIPro();
            }
            catch { }

            if (System.Web.HttpContext.Current.Session["Cart"] != null)
                ViewBag.cartCount = common.countWordInString(System.Web.HttpContext.Current.Session["Cart"].ToString(), ',');
            else
                ViewBag.cartCount = 0;



        }
 
        public ActionResult Index(string id)
        {
            try

            {
                if (System.Web.HttpContext.Current.Session["lg_id"] != null)
                {
                    langId = System.Web.HttpContext.Current.Session["lg_id"].ToString();
                }
                ViewBag.id = id;
                
            }
            catch
            {
                langId = "1";
            }

            if (langId == "1")
            {                           
                System.Web.HttpContext.Current.Session["lg_id"] = 1;                 
            }
            else
            {                          
                System.Web.HttpContext.Current.Session["lg_id"] = 2;                
            }
            string strcnn = common.GetAppSetting("cnn");

            
            return View("index");
        }

       

        public void Logout()
        {
         
           // (new LIB.Logout()).RunLogout();
            
        }


        public ActionResult GetList()
        {
            
            jc.data = mdl.GetList();
            //jc.data = mdl.GetList(0);
            jc.msg = mdl.msg;
            jc.code = (jc.msg == "100" ? 100 : 101);
            string skq = "{\"code\":" + jc.code.ToString() + ",\"msg\":\"" + jc.msg.ToString() + "\", \"data\":" + (jc.data.ToString().Trim() == "" ? "[]" : jc.data.ToString()) + "}";
            return Content(skq, "application/json");
        }

        public string SaveUserInfoToDB()
        {
            try
            {
                string MB_id = "";

                Dictionary<String, String> mapData = new Dictionary<String, String>();
                string accessToken = (string)System.Web.HttpContext.Current.Session["access_token"];// Session["access_token"];
                string refreshToken = (string)System.Web.HttpContext.Current.Session["refresh_token"];// Session["refresh_token"];

                string userId = (string)System.Web.HttpContext.Current.Session["userId"]; // Session["userId"];
                string idToken = (string)System.Web.HttpContext.Current.Session["idToken"];// Session["idToken"];
                string familyName =(string)System.Web.HttpContext.Current.Session["familyName"];
       
                if (System.Web.HttpContext.Current.Session["userId"] != null)
                {
                    
                    string sql = "exec sp_tblMembers_CheckExistsUserName N'" + userId + "'";
                    int x = Convert.ToInt32(RICDB.DB.RunSQLReturn1Value(sql, ref msg, strcnn));

                    if (x == 0 && userId!="")
                    {
                        MB_id = Guid.NewGuid().ToString();
                        int iRowEffected = 0;
                        cmd = new SqlCommand("sp_tblMembers_Insert", new SqlConnection(strcnn));
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add(new SqlParameter("@MB_Id", MB_id));
                        cmd.Parameters.Add(new SqlParameter("@MB_UserName", userId.ToString()));
                        cmd.Parameters.Add(new SqlParameter("@MB_FullName", familyName.ToString()));
                       
                        cmd.Parameters.Add(new SqlParameter("@MB_Avatar", ""));

                        cmd.Parameters.Add(new SqlParameter("@MB_PB_Id", "00000000-0000-0000-0000-000000000000".ToString()));
                        cmd.Parameters.Add(new SqlParameter("@MB_CV_Id", "00000000-0000-0000-0000-000000000000".ToString()));
                        cmd.Parameters.Add(new SqlParameter("@MB_Password", "123"));
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
                        }

                    }


                    string uid = userId;
                    string pwd = "";
                    LIB.CLogin lg = new LIB.CLogin(uid, pwd, RICTotal.Models.common.strcnn, RICTotal.Models.common.MBCookieTokenName);
                   // msg = lg.runLoginMemberWithKnox();

                    if (lg.code == 100)
                    {
                        // kiem tra thu muc image da co chua, neu chua co thi tao
                        string thumuc = "PF" + (Session["user_id"] == null ? "NoLogin" : Session["user_id"].ToString().Replace("-", ""));
                        var sfl = Server.MapPath("~/") + "Images\\" + thumuc;
                        if (!System.IO.Directory.Exists(sfl))
                        {
                            try
                            {
                                System.IO.Directory.CreateDirectory(sfl);
                            }
                            catch (Exception e) { }
                        }
                    }
                    if (lg.code == 151)
                        msg = "Tài khoản chưa được Active";
                    else if (lg.code == 150) msg = "Tài khoản chưa có.";
                    else if (lg.code == 152) msg = "Mật khẩu chưa chính xác";
                    else if (lg.code != 100) msg = "Đăng nhập không thành công.Kiểm tra lại kết nội mạng INTERNET";
                    return msg == "100" ? "100" : msg;
                }
                else
                {
                    return "101";
                }

            }
            catch { return "101"; }

        }
        



    }
}