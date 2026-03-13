using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;

namespace RICTotal.Models
{
    

   public class Home
    {
        public static int CH_Id = 0;
        public static string strMenu;
        string sql, strcnn;
        public Guid userId;
        public Guid languageId;
        public string msg;
        public int iRowEffected;
        int pageSize;
        SqlCommand cmd;

        public Guid MBId;
        


        public Home()
        {
            sql = ""; msg = "";
            strcnn = System.Configuration.ConfigurationManager.AppSettings["cnn"].ToString();
            try
            {
                if (System.Web.HttpContext.Current.Session["MB_id"] != null)
                {
                    MBId = new Guid(System.Web.HttpContext.Current.Session["MB_id"].ToString());
                }
                if (System.Web.HttpContext.Current.Session["MBPageSize"] != null)
                {
                    pageSize = int.Parse(System.Web.HttpContext.Current.Session["MBPageSize"].ToString());
                }
                else pageSize = 10;
            }
            catch (Exception e1)
            {
                pageSize = 10;
            }
        }


        public string GetList()
        {
            string sql = "exec sp_tblMemberProject_Get_List_All";
            return common.RunSQLToJson(sql, ref msg);
            //return common.RunSQLToJsonManual(sql, ";MN_Parent_Name;MN_Id;MN_Ten;MN_Url;MN_Icon;MN_Thutu, active;", ref msg);
        }


        public DataTable GetSumary()
        {
            string sql = "exec [SDV_Sumary] ";
            return RICDB.DB.RunSQL(sql, ref msg, strcnn);
        }

        public DataTable GetSumTrainResult()
        {
            string sql = "exec [sp_tblSumTrainResult_Get_data_chart] ";
            return RICDB.DB.RunSQL(sql, ref msg, strcnn);
        }
        public DataTable GetAIExpert()
        {
            string sql = "exec sp_tblAICertificate_Get_Rank 1 ";
            return RICDB.DB.RunSQL(sql, ref msg, strcnn);
        }
        public DataTable GetAIPro()
        {
            string sql = "exec sp_tblAICertificate_Get_Rank 2 ";
            return RICDB.DB.RunSQL(sql, ref msg, strcnn);
        }

        public string getProjectOnHome()
        {
            string msg = "";
            string strcnn = common.GetAppSetting("cnn");
            string strList = "";
            string sql = "Select * From tblProject Where  PRJ_Display > 0 And PRJ_Display <1000 Order By PRJ_Display";// "Select * From tblProject Where PRJ_Display!=0 Order By PRJ_Display";
            DataTable dt = RICDB.DB.RunSQL(sql, ref msg, strcnn);
            int n = dt.Rows.Count;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string id = dt.Rows[i]["PRJ_SOTT"].ToString();
                strList += "<li class='linkheader1'><a class='linkheader1' href=/ProjectDetail/1?prj=" + id + ">" + (i + 1).ToString() + " - " + dt.Rows[i]["PRJ_Name"].ToString() + "(" + dt.Rows[i]["PRJ_Module"].ToString() + ")" + "</a></li>";
                //strList += "<smart-button style='width:100%;' class='flat menu-btn'><i style='margin-right:15px;' class='" + dt.Rows[i]["PRJ_Icon"].ToString() + "'></i><a style='font-size:14px;' href='#'>" + dt.Rows[i]["PRJ_Name"].ToString() + "</a></smart-button>";
            }
            strList += "<li class='linkheader1'><a class='linkheader1' href=/ProjectEfficiency/1" + ">" + (n + 1).ToString() + " - All project" + "</a></li>";
            return strList;
        }

        public string getUpdateTime()
        {

            string strDate = "";
            string msg = "";
            string strcnn = common.GetAppSetting("cnn");
             
            string sql = "exec sp_tblUpdatedTime_Get_List";
            DataTable dt = RICDB.DB.RunSQL(sql, ref msg, strcnn);
            if (dt.Rows.Count > 0)
            {
                string strTime = dt.Rows[0]["UpdatedTimeDetail"].ToString(); //00 00 00 00 00 00
                int month = Convert.ToInt32(strTime.Substring(2, 2));
                strDate = strTime.Substring(4, 2) + "-" + common.GetMonthName(month) + "-" + strTime.Substring(0, 2) + ", " + strTime.Substring(6, 2) + ":" + strTime.Substring(8, 2) + ":" + strTime.Substring(10, 2);
            }
            return strDate;
        }

    }
}