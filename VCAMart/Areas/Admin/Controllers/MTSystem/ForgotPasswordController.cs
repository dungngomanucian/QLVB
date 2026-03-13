using System;
using System.Resources;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using Newtonsoft.Json.Linq;
 
using RICTotalAdmin.Models;

namespace RICTotalAdmin.Controllers.MTSystem
{
    public class ForgotPasswordController : Controller
    {
        string msg = "";
        jsonClientPost jc = new jsonClientPost();
        Users mdl = new Users();
        // GET: ForgotPassword
        public ActionResult Index()
        {
            return View("ForgotPassword");
        }

        public JsonResult Recovery(string data)
        {
            dynamic row = JObject.Parse(data);
            string skeys = "email;";

            if (!common.CheckObjectEnoughProperies(row, skeys, ref msg))
            {
                jc.code = 121;
                jc.msg = "";
                return Json(jc, JsonRequestBehavior.AllowGet);
            }

            if (!common.IsEmail(row.email.ToString()))
            {
                jc.code = 110;
                jc.msg = "";
                return Json(jc, JsonRequestBehavior.AllowGet);
            }

            if (!mdl.existsEmail(row.email.ToString()))
            {
                jc.code = 111;
                jc.msg = "";
                return Json(jc, JsonRequestBehavior.AllowGet);
            }

            // thuc hien gui email 
            if (!common.emailMatkhau(row.email.ToString(), ref msg))
            {
                jc.code = 140;
                jc.msg = msg;
            }
            else
            {
                jc.code = 100;
                jc.msg = "";
            }
            return Json(jc, JsonRequestBehavior.AllowGet);
        }
 
        
    }
}