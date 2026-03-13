using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RICTotalAdmin.Models
{
    public class RootJson
    {
        public Meta meta { get; set; }
        public List<GroupNode> data { get; set; }
    }
    public class Meta
    {
        public string source { get; set; }
    }
    public class GroupNode
    {
        public string group_id { get; set; }
        public string group_title { get; set; }
        public List<EventNode> events { get; set; }
    }
    public class EventNode
    {
        public string event_id { get; set; }
        public string category { get; set; }
        public List<DocumentNode> children { get; set; }
    }

    public class DocumentNode
    {
        public string title { get; set; }
        public string url { get; set; }
    }

    public class DetailRootJson
    {
        public Meta meta { get; set; }
        public List<DetailDocumentNode> documents { get; set; }
    }

    public class DetailDocumentNode
    {
        public string procedure_code { get; set; }
        public List<DetailSectionNode> sections { get; set; }
    }
    public class DetailSectionNode
    {
        public string heading { get; set; }
        public string html { get; set; }
    }

    public class Document
    {
        public Guid Doc_Id { get; set; }
        public string Doc_Code { get; set; }
        public string Doc_Content { get; set; }
    }
}