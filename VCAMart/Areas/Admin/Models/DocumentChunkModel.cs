using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace RICTotalAdmin.Models
{
    public class DocumentChunkModel
    {
        string strcnn;
        public Guid userId;
        public Guid languageId;
        public string msg;
        public int iRowEffected;
        int pageSize;
        SqlCommand cmd;
        string tblname = "tblDocumentChunk";
        public string rc_id;

        public string Idlist { get; set; }
        public DocumentChunkModel()
        {
            msg = "";
            strcnn = System.Configuration.ConfigurationManager.AppSettings["cnn"].ToString();

            try
            {
                //rc_id = System.Web.HttpContext.Current.Session["rc_id"].ToString();
                userId = new Guid(System.Web.HttpContext.Current.Session["user_id"].ToString());
                pageSize = int.Parse(System.Web.HttpContext.Current.Session["userPageSize"].ToString());
                languageId = new Guid(System.Web.HttpContext.Current.Session["lang_id"].ToString());
            }
            catch (Exception)
            {
                pageSize = 10;
            }

        }

        public string GetList(string pageSize, string pageIndex, ref int total)
        {
            //string sql = "exec Admin.sp_tblDocumentChunk_Get_List_Page";
            //return common.RunSQLToJson(sql, ref msg);

            string msg = "";
            total = 0;
            string sql = "exec Admin.sp_tblDocumentChunk_Get_List_Page  " + pageSize + "," + pageIndex;// "exec [SDV_WorkerBasicInfor_Get_List]  " + page + "," + pageSize + "";
            DataTable dt;
            string strcnn = common.GetAppSetting("cnn");
            dt = RICDB.DB.RunSQL(sql, ref msg, strcnn);
            if (msg != "100") return "";
            if (dt.Rows.Count == 0) return "";
            total = int.Parse(dt.Rows[0]["total"].ToString());
            return JsonConvert.SerializeObject(dt);
        }

        public int GetDocumentNoChunk()
        {
            int n = 0;
            string msg = "";
            DataTable dt;
            string strcnn = common.GetAppSetting("cnn");
            string sql = "exec Admin.sp_Count_Document_NotChunked ";
            dt = RICDB.DB.RunSQL(sql, ref msg, strcnn);
            if (msg != "100") return 0;
            if (dt.Rows.Count == 0) return 0;
            n = int.Parse(dt.Rows[0]["total_not_chunked"].ToString());
            return n;
        }
        //Thủ tục để thực hiện căt văn bản theo batch
        public bool RunChunkBatch(int batchSize = 20)
        {
            msg = "";
            iRowEffected = 0;

            using (SqlConnection conn = new SqlConnection(strcnn))
            {
                conn.Open();
                SqlTransaction tran = conn.BeginTransaction();

                try
                {
                    DataTable docs = GetDocumentBatch(conn, tran, batchSize);

                    foreach (DataRow r in docs.Rows)
                    {
                        Guid docId = (Guid)r["doc_Id"];
                        string docCode = r["doc_code"].ToString();
                        string docContent = r["doc_content"].ToString();

                        MarkDocumentStatus(conn, tran, docId, 1); // PROCESSING

                        try
                        {
                            // (Tuỳ chọn) xoá chunk cũ nếu bạn cho phép re-chunk
                            // DeleteChunksByDocument(conn, tran, docId);

                            ChunkOneDocument(conn, tran, docId, docCode, docContent);

                            MarkDocumentStatus(conn, tran, docId, 2); // DONE
                            iRowEffected++;
                        }
                        catch
                        {
                            MarkDocumentStatus(conn, tran, docId, 9); // ERROR
                        }
                    }

                    tran.Commit();
                    msg = "100";
                    return true;
                }
                catch (Exception ex)
                {
                    try { tran.Rollback(); } catch { }
                    msg = ex.Message;
                    return false;
                }
            }
        }

        //Thủ tục để lấy văn bản theo batch
        private DataTable GetDocumentBatch(SqlConnection conn, SqlTransaction tran, int batchSize)
        {
            string sql = @"
                SELECT TOP (@BatchSize)
                    doc_Id, doc_code, doc_content
                FROM tblDocument WITH (READPAST)
                WHERE is_active = 1
                  AND is_chunk = 0
                  AND doc_content IS NOT NULL
                ORDER BY CreatedDate
            ";

            using (SqlCommand cmd = new SqlCommand(sql, conn, tran))
            {
                cmd.Parameters.Add("@BatchSize", SqlDbType.Int).Value = batchSize;

                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }

        //Thủ tục cập nhật trạng thái cho các bản ghi đang xử lý trong văn bản
        private void MarkDocumentStatus(SqlConnection conn, SqlTransaction tran, Guid docId, int status)
        {
            string sql = @"
                UPDATE tblDocument
                SET is_chunk = @status,
                    updated_at = GETDATE()
                WHERE doc_Id = @doc_Id
            ";

            using (SqlCommand cmd = new SqlCommand(sql, conn, tran))
            {
                cmd.Parameters.Add("@status", SqlDbType.Int).Value = status;
                cmd.Parameters.Add("@doc_Id", SqlDbType.UniqueIdentifier).Value = docId;
                cmd.ExecuteNonQuery();
            }
        }

        //Thu tục check và tìm chunk type
        private int? ResolveChunkType(string docTypeText, SqlConnection conn, SqlTransaction tran)
        {
            using (SqlCommand cmd = new SqlCommand("Admin.sp_MapChunkType", conn, tran))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@DocTypeText", SqlDbType.NVarChar, 500).Value = docTypeText;

                var outParam = new SqlParameter("@ChunkType_Code", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(outParam);

                cmd.ExecuteNonQuery();

                return outParam.Value == DBNull.Value ? (int?)null : Convert.ToInt32(outParam.Value);
            }
        }

        //Thủ tục cắt từng tài liệu thành các đoạn và lưu vào DB
        private void ChunkOneDocument(SqlConnection conn, SqlTransaction tran, Guid docId, string docCode, string html)
        {
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);
            var h2Nodes = doc.DocumentNode.SelectNodes("//h2");
            if (h2Nodes == null) return;

            int stepNo = 1;

            foreach (var h2 in h2Nodes)
            {
                string title = HtmlAgilityPack.HtmlEntity.DeEntitize(h2.InnerText.Trim());

                var sb = new StringBuilder();
                var node = h2.NextSibling;

                while (node != null && node.Name != "h2")
                {
                    if (node.NodeType == HtmlAgilityPack.HtmlNodeType.Element)
                        sb.Append(node.OuterHtml);

                    node = node.NextSibling;
                }
                string chunkContentHtml = sb.ToString();

                string chunkContentRaw = RagHtmlNormalizer.NormalizeForRag(chunkContentHtml);
                chunkContentRaw = RagHtmlNormalizer.NormalizeTextForRag(chunkContentRaw);
               
                int? chunkTypeCode = ResolveChunkType(title, conn, tran);
                if (chunkTypeCode == null) continue;

                InsertDocumentChunk(
                    conn, tran,
                    docId,
                    docCode,
                    chunkTypeCode.Value,
                    stepNo,
                    title,
                    chunkContentHtml,
                    chunkContentRaw
                );

                stepNo++;
            }
        }

        //Thủ tục insert
        private void InsertDocumentChunk(
        SqlConnection conn,
        SqlTransaction tran,
        Guid documentId,
        string docCode,
        int chunkTypeCode,
        int stepNo,
        string chunkTitle,
        string chunkContent,
        string chunkContentRaw)
        {
            string sql = @"
            INSERT INTO tblDocumentChunk
            (
                chunk_id,
                document_id,
                chunk_code,
                doc_code,
                chunk_type,
                step_no,
                chunk_title,
                chunk_content,
                chunk_content_raw,
                created_at
            )
            VALUES
            (
                NEWID(),
                @document_id,
                @chunk_code,
                @doc_code,
                @chunk_type,
                @step_no,
                @chunk_title,
                @chunk_content,
                @chunk_content_raw,
                GETDATE()
            )
        ";

            using (SqlCommand cmd = new SqlCommand(sql, conn, tran))
            {
                cmd.Parameters.Add("@document_id", SqlDbType.UniqueIdentifier)
                    .Value = documentId;
                cmd.Parameters.Add("@chunk_code", SqlDbType.NVarChar, 50)
                    .Value = docCode + "_" + stepNo.ToString("00");
                cmd.Parameters.Add("@doc_code", SqlDbType.NVarChar, 50)
                    .Value = docCode;
                cmd.Parameters.Add("@chunk_type", SqlDbType.Int)
                    .Value = chunkTypeCode;
                cmd.Parameters.Add("@step_no", SqlDbType.Int)
                    .Value = stepNo;
                cmd.Parameters.Add("@chunk_title", SqlDbType.NVarChar, 250)
                    .Value = chunkTitle;
                cmd.Parameters.Add("@chunk_content", SqlDbType.NText)
                    .Value = chunkContent;
                cmd.Parameters.Add("@chunk_content_raw", SqlDbType.NVarChar)
                    .Value = chunkContentRaw;
                cmd.ExecuteNonQuery();
            }
        }

        //Thủ tục xử lý loại bỏ thẻ HTML trong văn bản



        public static string StripHtml(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return string.Empty;

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            return HtmlEntity.DeEntitize(doc.DocumentNode.InnerText).Trim();
        }

}
}