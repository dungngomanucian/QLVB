using System;
using System.Data;
using System.Data.SqlClient;
using System.Web;

namespace LIB
{
    /// <summary>
    /// Lightweight fallback for legacy external LIB assembly.
    /// Keeps the project buildable when the original private DLL is unavailable.
    /// </summary>
    public static class ConfigInfo
    {
        public const string userLoginedConfirm = "100";
        public const string MBLoginedConfirm = "100";
    }

    public class CLogin
    {
        public string userId { get; set; } = "";
        public string userUid { get; set; } = "";
        public string userFullName { get; set; } = "";
        public string userAvata { get; set; } = "";
        public string userCreated { get; set; } = "";
        public int songaycondung { get; set; } = 0;
        public string congty_ten { get; set; } = "";
        public string congty_diachi { get; set; } = "";
        public string congty_tel { get; set; } = "";
        public string rc_id { get; set; } = "";
        public string PB_Code { get; set; } = "";
        public int code { get; set; } = 150;

        private readonly string _uid;
        private readonly string _pwd;

        public CLogin(string connectionString, string cookieTokenName)
        {
            _uid = "";
            _pwd = "";
        }

        public CLogin(string uid, string pwd, string connectionString, string cookieTokenName)
        {
            _uid = uid ?? "";
            _pwd = pwd ?? "";
        }

        public bool checkTokenLogin()
        {
            try
            {
                return HttpContext.Current?.Session?["user_id"] != null;
            }
            catch
            {
                return false;
            }
        }

        public void GotoLoginPage()
        {
            try
            {
                HttpContext.Current?.Response?.Redirect("~/Admin/Login", false);
            }
            catch
            {
                // no-op fallback
            }
        }

        public string runLogin()
        {
            // Fallback behavior: accept non-empty credentials and stamp session values.
            // Replace with the original LIB implementation when available.
            if (string.IsNullOrWhiteSpace(_uid) || string.IsNullOrWhiteSpace(_pwd))
            {
                code = 152;
                return "152";
            }

            code = 100;
            userId = _uid;
            userUid = _uid;
            userFullName = _uid;
            userCreated = DateTime.Now.ToString("dd/MM/yyyy");
            rc_id = "00000000-0000-0000-0000-000000000000";

            try
            {
                var session = HttpContext.Current?.Session;
                if (session != null)
                {
                    session["user_id"] = userId;
                    session["user_uid"] = userUid;
                    session["user_logined"] = ConfigInfo.userLoginedConfirm;
                    session["rc_id"] = rc_id;
                }
            }
            catch
            {
                // no-op fallback
            }

            return "100";
        }
    }

    public class Logout
    {
        public Logout(string connectionString)
        {
        }

        public void RunLogout()
        {
            try
            {
                HttpContext.Current?.Session?.Clear();
                HttpContext.Current?.Session?.Abandon();
            }
            catch
            {
                // no-op fallback
            }
        }
    }
}

namespace RICDB
{
    /// <summary>
    /// Replacement for legacy ric.db helper methods.
    /// </summary>
    public static class DB
    {
        public static DataTable RunSQL(string sql, ref string msg, string connectionString)
        {
            var dt = new DataTable();
            msg = "100";
            try
            {
                using (var cn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(sql, cn))
                using (var da = new SqlDataAdapter(cmd))
                {
                    cn.Open();
                    da.Fill(dt);
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }
            return dt;
        }

        public static string RunSQLReturn1Value(string sql, ref string msg, string connectionString)
        {
            msg = "100";
            try
            {
                using (var cn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(sql, cn))
                {
                    cn.Open();
                    var result = cmd.ExecuteScalar();
                    return result?.ToString() ?? "";
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return "";
            }
        }

        public static void RunSQLNoReturn(string sql, ref string msg, string connectionString)
        {
            msg = "100";
            try
            {
                using (var cn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(sql, cn))
                {
                    cn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }
        }

        public static void RunProcNoReturn(string sql, ref string msg, string connectionString)
        {
            // Legacy calls pass SQL text (often EXEC ...), keep same behavior.
            RunSQLNoReturn(sql, ref msg, connectionString);
        }
    }
}
