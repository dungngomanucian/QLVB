using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RICTotalAdmin.Models
{

    public class OneValue
    {
        public string key { get; set; }
        public string value { get; set; }
        public string code { get; set; }
        public OneValue()
        {
            key = "";
            value = "";
            code = "";
        }
    }
    public class ApiSecret
    {
        public string apikey { get; set; }
        public string apisecret { get; set; }
    }
    public class ObjActiveCode
    {
        public string activeCode { get; set; }
        public string userId { get; set; }

        public ObjActiveCode(string uid, string code) { activeCode = code; userId = uid; }
    }

    public class jsonClientPost
    {
        public int code;
        public string msg;
        public dynamic data;
        public string message;
        public jsonClientPost()
        {
            data = new OneValue();
            code = 100;
            msg = "Done";
        }
    }

    public class jsonClientPostUser
    {

        public string userId;
        public string langId;
        public string userAvata;
        public string userEmail;
        public string userFullName;
        public int userMultiLogin;
        public string userTel;
        public string userUid;
        public string token;


    }

    [JsonObject]
    [Serializable]
    public class UserSignup : ApiSecret
    {
        public string id;
        public string name { get; set; }
        public string pwd { get; set; }
        public string tel { get; set; }
        public int type { get; set; }
        public string email { get; set; }
        public string faceid { get; set; }
        public string googleid { get; set; }
        public string apikey { get; set; }
        public string apisecret { get; set; }

        public UserSignup()
        {
            id = Guid.NewGuid().ToString();
            type = 1; apikey = "";
            apisecret = "";
        }

    }

    /// <summary>
    /// class sms
    /// </summary>
    public class SMSObject : ApiSecret
    {

        /// <summary>
        /// list of receiver number, sperate by ;
        /// </summary>
        public string tolist { get; set; }
        public string content { get; set; }

        public SMSObject()
        {
            tolist = ""; content = "";
            apikey = "";
            apisecret = "";
        }

    }
}
