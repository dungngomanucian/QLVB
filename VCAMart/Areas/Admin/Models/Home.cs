using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Sockets;
using System.Net;

namespace RICTotalAdmin.Models
{
    public class Home
    {
        public static string strMenu;
        string  strcnn;
        public Guid userId;
        public Guid languageId;
        public string msg;
        public int iRowEffected;
     
      


        public Home()
        {
            msg = "";
            strcnn = System.Configuration.ConfigurationManager.AppSettings["cnn"].ToString();
        }

        public DataTable GetSumary()
        {
            string sql = "exec [sp_tblVanBan_Sumary] ";
            return RICDB.DB.RunSQL(sql, ref msg, strcnn);
        }

       
    }
}