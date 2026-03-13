using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace RICTotalAdmin.Models
{
    public class ContentCategoryNode
    {
        public string CC_Id { get; set; }
        public string CC_Name { get; set; }
        public string CC_ParentId { get; set; } // NULL nếu root
    }
}