using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Text;
using System.Web;

namespace RICTotalAdmin.Models
{
    public class RagUnitModel
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
        public RagUnitModel()
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
            
            string msg = "";
            total = 0;
            string sql = "exec Admin.sp_tblRagUnit_Get_List_Page  " + pageSize + "," + pageIndex;// "exec [SDV_WorkerBasicInfor_Get_List]  " + page + "," + pageSize + "";
            DataTable dt;
            string strcnn = common.GetAppSetting("cnn");
            dt = RICDB.DB.RunSQL(sql, ref msg, strcnn);
            if (msg != "100") return "";
            if (dt.Rows.Count == 0) return "";
            total = int.Parse(dt.Rows[0]["total"].ToString());
            return JsonConvert.SerializeObject(dt);
        }


        public int GetChunkNotProcess()
        {
            int n = 0;
            string msg = "";
            DataTable dt;
            string strcnn = common.GetAppSetting("cnn");
            string sql = "exec Admin.sp_Count_DocumentChunk_NotProcess ";
            dt = RICDB.DB.RunSQL(sql, ref msg, strcnn);
            if (msg != "100") return 0;
            if (dt.Rows.Count == 0) return 0;
            n = int.Parse(dt.Rows[0]["total_not_chunked"].ToString());
            return n;
        }

        public bool ProcessRagFromDocumentChunk(int batchSize)
        {
            int processed = 0;
            string sql = "exec Admin.sp_tblDocumentChunk_Get_ForRag " + batchSize;

            DataTable dt = RICDB.DB.RunSQL(sql, ref msg, strcnn);
            if (msg != "100" || dt == null || dt.Rows.Count == 0)
                return false;

            foreach (DataRow r in dt.Rows)
            {
                Guid chunkId = new Guid(r["chunk_id"].ToString());

                try
                {
                    // 1) Map row → DTO
                    DocumentChunkDto chunk = new DocumentChunkDto
                    {
                        ChunkId = chunkId,
                        DocumentId = new Guid(r["document_id"].ToString()),
                        ChunkCode = r["chunk_code"].ToString(),
                        DocCode = r["doc_code"].ToString(),
                        ChunkType = r["chunk_type"].ToString(),
                        StepNo = r["step_no"] == DBNull.Value ? 0 : Convert.ToInt32(r["step_no"]),
                        ChunkTitle = r["chunk_title"].ToString(),
                        ChunkContentRaw = r["chunk_content_raw"].ToString(),
                        doc_type = r["doc_type"].ToString()
                    };

                    // 2) Normalize văn bản
                    string normalizedText = chunk.ChunkContentRaw;//
                    
                    if (string.IsNullOrWhiteSpace(normalizedText))
                    {
                        MarkChunkProcessed(chunkId);
                        processed++;
                        continue;
                    }
                     
                    // 3) Split → RagUnit
                    List<RagUnitDto> units = RagChunkText.SplitChunkToRagUnits(chunk, normalizedText); //RagSemanticChunker.ChunkForRag(chunk, normalizedText);

                    // 4) Save vào tblRagUnit
                    bool ok = SaveUnits(chunkId, units);
                    if (!ok)
                    {
                        msg = "Lỗi cập nhật dữ liệu";
                        return false;
                    }

                    // 5) Mark chunk đã xử lý
                    MarkChunkProcessed(chunkId);

                    processed++;
                }
                catch (Exception ex)
                {
                    // log nếu bạn muốn
                    msg = ex.Message;
                    return false;
                    // KHÔNG mark processed → để batch sau xử lý lại
                }
                iRowEffected = processed;
            }

            return true;
        }

        private void MarkChunkProcessed(Guid chunkId)
        {
            string sql = @"
            UPDATE tblDocumentChunk
            SET is_process = 1,
                rag_processed_at = GETDATE()
            WHERE chunk_id = '" + chunkId + "'";
            RICDB.DB.RunSQL(sql, ref msg, strcnn);
        }

        public bool SaveUnits(Guid chunkId, List<RagUnitDto> units)
        {
            if (units == null || units.Count == 0)
                return true;
            using (SqlConnection cnn = new SqlConnection(strcnn))
            {
                cnn.Open();
                using (SqlTransaction tran = cnn.BeginTransaction())
                {
                    try
                    {
                        // 1️⃣ Deactivate RagUnit cũ của chunk
                        using (SqlCommand cmdDeactivate = new SqlCommand(
                            "UPDATE tblRagUnit SET is_active = 0 WHERE chunk_id = @chunk_id",
                            cnn, tran))
                        {
                            cmdDeactivate.Parameters.AddWithValue("@chunk_id", chunkId);
                            cmdDeactivate.ExecuteNonQuery();
                        }

                        // 2️⃣ Insert RagUnit mới
                        foreach (RagUnitDto u in units)
                        {
                            using (SqlCommand cmdInsert = new SqlCommand(@"
                            INSERT INTO tblRagUnit
                            (
                                rag_unit_id,
                                chunk_id,
                                document_id,
                                unit_code,
                                unit_anchor,
                                intent_type,
                                scenario_tags,
                                unit_title,
                                unit_content,
                                unit_hash,
                                token_len,
                                created_at,
                                is_active,
                                doc_type
                            )
                            VALUES
                            (
                                @rag_unit_id,
                                @chunk_id,
                                @document_id,
                                @unit_code,
                                @unit_anchor,
                                @intent_type,
                                @scenario_tags,
                                @unit_title,
                                @unit_content,
                                @unit_hash,
                                @token_len,
                                @created_at,
                                @is_active,
                                @doc_type
                            )", cnn, tran))
                            {
                                cmdInsert.Parameters.AddWithValue("@rag_unit_id", u.RagUnitId);
                                cmdInsert.Parameters.AddWithValue("@chunk_id", u.ChunkId);
                                cmdInsert.Parameters.AddWithValue("@document_id", u.DocumentId);

                                cmdInsert.Parameters.AddWithValue("@unit_code",
                                    string.IsNullOrEmpty(u.UnitCode) ? (object)DBNull.Value : u.UnitCode);

                                cmdInsert.Parameters.AddWithValue("@unit_anchor", u.UnitAnchor ?? "");
                                cmdInsert.Parameters.AddWithValue("@intent_type",
                                    string.IsNullOrEmpty(u.IntentType) ? (object)DBNull.Value : u.IntentType);

                                cmdInsert.Parameters.AddWithValue("@scenario_tags",
                                    string.IsNullOrEmpty(u.ScenarioTags) ? (object)DBNull.Value : u.ScenarioTags);

                                cmdInsert.Parameters.AddWithValue("@unit_title",
                                    string.IsNullOrEmpty(u.UnitTitle) ? (object)DBNull.Value : u.UnitTitle);

                                cmdInsert.Parameters.AddWithValue("@unit_content", u.UnitContent ?? "");
                                cmdInsert.Parameters.AddWithValue("@unit_hash", u.UnitHash ?? "");
                                cmdInsert.Parameters.AddWithValue("@token_len", u.TokenLen);
                                cmdInsert.Parameters.AddWithValue("@created_at", u.CreatedAt);
                                cmdInsert.Parameters.AddWithValue("@is_active", u.IsActive);
                                cmdInsert.Parameters.AddWithValue("@doc_type", u.DocType);
                                cmdInsert.ExecuteNonQuery();
                            }
                        }

                        tran.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        msg = ex.Message;
                        return false;
                    }
                }
            }
        }

        public int LastEmbeddedCount { get; private set; }

        public bool SyncRagUnitSearchEmbedding(int batchSize = 100)
        {
            LastEmbeddedCount = 0;
            msg = "";
            try
            {
                using (SqlConnection cnn = new SqlConnection(strcnn))
                {
                    cnn.Open();

                    // 1) Sync metadata từ vw_ragunit_for_index -> tblRagUnit_Search
                    using (SqlCommand cmdSync = new SqlCommand("sp_RagUnit_Search_Sync", cnn))
                    {
                        cmdSync.CommandType = CommandType.StoredProcedure;
                        cmdSync.ExecuteNonQuery();
                    }

                    // Đảm bảo doc_type mặc định = LEGAL cho bảng Search (nếu DB có cột này)
                    using (SqlCommand cmdFixDocType = new SqlCommand(@"
IF COL_LENGTH('dbo.tblRagUnit_Search','doc_type') IS NOT NULL
BEGIN
    UPDATE dbo.tblRagUnit_Search
    SET doc_type = 'LEGAL'
    WHERE doc_type IS NULL OR LTRIM(RTRIM(doc_type)) = '';
END
", cnn))
                    {
                        cmdFixDocType.ExecuteNonQuery();
                    }

                    // 2) Đảm bảo có cột embedding (tự tạo nếu chưa có)
                    using (SqlCommand cmdEnsure = new SqlCommand(@"
IF COL_LENGTH('dbo.tblRagUnit_Search', 'embedding_json') IS NULL
    ALTER TABLE dbo.tblRagUnit_Search ADD embedding_json NVARCHAR(MAX) NULL;
", cnn))
                    {
                        cmdEnsure.ExecuteNonQuery();
                    }

                    // 3) Lấy batch cần embed
                    DataTable dt = new DataTable();
                    using (SqlCommand cmdGet = new SqlCommand(@"
SELECT TOP (@batch)
    rag_unit_id,
    ISNULL(unit_title,'') AS unit_title,
    ISNULL(unit_content,'') AS unit_content,
    ISNULL(unit_hash,'') AS unit_hash
FROM dbo.tblRagUnit_Search
WHERE ISNULL(is_active,1) = 1
  AND (embedding_json IS NULL OR LTRIM(RTRIM(embedding_json)) = '')
ORDER BY last_sync_at DESC, rag_unit_id
", cnn))
                    {
                        cmdGet.Parameters.Add("@batch", SqlDbType.Int).Value = batchSize;
                        using (SqlDataAdapter da = new SqlDataAdapter(cmdGet))
                        {
                            da.Fill(dt);
                        }
                    }

                    if (dt.Rows.Count == 0)
                    {
                        msg = "100";
                        return true;
                    }

                    // 4) Chuẩn bị input cho python
                    var rows = new List<RagEmbedRow>();
                    foreach (DataRow r in dt.Rows)
                    {
                        string title = r["unit_title"].ToString().Trim();
                        string content = r["unit_content"].ToString().Trim();
                        string combined = (title == "" ? content : (title + "\n" + content));
                        rows.Add(new RagEmbedRow
                        {
                            RagUnitId = (Guid)r["rag_unit_id"],
                            UnitHash = r["unit_hash"].ToString(),
                            Text = combined
                        });
                    }

                    var pyReq = new RagEmbedRequest
                    {
                        model = "AITeamVN/Vietnamese_Embedding",
                        texts = rows.Select(x => x.Text).ToList()
                    };

                    var pyRes = RunEmbeddingPython(pyReq);
                    if (pyRes == null)
                    {
                        msg = "Embedding response rỗng.";
                        return false;
                    }
                    if (!string.IsNullOrWhiteSpace(pyRes.error))
                    {
                        msg = "Python embedding lỗi: " + pyRes.error;
                        return false;
                    }
                    if (pyRes.fallback_count > 0)
                    {
                        msg = "Embedding có fallback (" + pyRes.fallback_count + "/" + rows.Count + "). Lỗi đầu tiên: " + (pyRes.first_error ?? "");
                        return false;
                    }
                    var vectors = NormalizeEmbeddingCount(pyRes == null ? null : pyRes.embeddings, rows.Count, 768);

                    // 5) Cập nhật embedding về DB
                    using (SqlTransaction tran = cnn.BeginTransaction())
                    {
                        try
                        {
                            for (int i = 0; i < rows.Count; i++)
                            {
                                string embJson = JsonConvert.SerializeObject(vectors[i]);
                                using (SqlCommand cmdUp = new SqlCommand(@"
UPDATE dbo.tblRagUnit_Search
SET embedding_json = @embedding_json
WHERE rag_unit_id = @rag_unit_id
", cnn, tran))
                                {
                                    cmdUp.Parameters.Add("@embedding_json", SqlDbType.NVarChar, -1).Value = embJson;
                                    cmdUp.Parameters.Add("@rag_unit_id", SqlDbType.UniqueIdentifier).Value = rows[i].RagUnitId;
                                    cmdUp.ExecuteNonQuery();
                                }
                            }

                            tran.Commit();
                            LastEmbeddedCount = rows.Count;
                            msg = "100";
                            return true;
                        }
                        catch (Exception ex2)
                        {
                            try { tran.Rollback(); } catch { }
                            msg = ex2.Message;
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return false;
            }
        }

        public bool SyncRagUnitSearchEmbeddingByDocument(Guid documentId, int batchSize = 1000)
        {
            LastEmbeddedCount = 0;
            msg = "";
            try
            {
                using (SqlConnection cnn = new SqlConnection(strcnn))
                {
                    cnn.Open();

                    using (SqlCommand cmdSync = new SqlCommand("sp_RagUnit_Search_Sync", cnn))
                    {
                        cmdSync.CommandType = CommandType.StoredProcedure;
                        cmdSync.ExecuteNonQuery();
                    }

                    // Đảm bảo doc_type mặc định = LEGAL cho bảng Search (nếu DB có cột này)
                    using (SqlCommand cmdFixDocType = new SqlCommand(@"
IF COL_LENGTH('dbo.tblRagUnit_Search','doc_type') IS NOT NULL
BEGIN
    UPDATE dbo.tblRagUnit_Search
    SET doc_type = 'LEGAL'
    WHERE doc_type IS NULL OR LTRIM(RTRIM(doc_type)) = '';
END
", cnn))
                    {
                        cmdFixDocType.ExecuteNonQuery();
                    }

                    using (SqlCommand cmdEnsure = new SqlCommand(@"
IF COL_LENGTH('dbo.tblRagUnit_Search', 'embedding_json') IS NULL
    ALTER TABLE dbo.tblRagUnit_Search ADD embedding_json NVARCHAR(MAX) NULL;
", cnn))
                    {
                        cmdEnsure.ExecuteNonQuery();
                    }

                    DataTable dt = new DataTable();
                    using (SqlCommand cmdGet = new SqlCommand(@"
SELECT TOP (@batch)
    rag_unit_id,
    ISNULL(unit_title,'') AS unit_title,
    ISNULL(unit_content,'') AS unit_content,
    ISNULL(unit_hash,'') AS unit_hash
FROM dbo.tblRagUnit_Search
WHERE ISNULL(is_active,1) = 1
  AND document_id = @document_id
ORDER BY rag_unit_id
", cnn))
                    {
                        cmdGet.Parameters.Add("@batch", SqlDbType.Int).Value = batchSize;
                        cmdGet.Parameters.Add("@document_id", SqlDbType.UniqueIdentifier).Value = documentId;
                        using (SqlDataAdapter da = new SqlDataAdapter(cmdGet))
                        {
                            da.Fill(dt);
                        }
                    }

                    if (dt.Rows.Count == 0)
                    {
                        msg = "100";
                        return true;
                    }

                    var rows = new List<RagEmbedRow>();
                    foreach (DataRow r in dt.Rows)
                    {
                        string title = r["unit_title"].ToString().Trim();
                        string content = r["unit_content"].ToString().Trim();
                        string combined = (title == "" ? content : (title + "\n" + content));
                        rows.Add(new RagEmbedRow
                        {
                            RagUnitId = (Guid)r["rag_unit_id"],
                            UnitHash = r["unit_hash"].ToString(),
                            Text = combined
                        });
                    }

                    var pyReq = new RagEmbedRequest
                    {
                        model = "AITeamVN/Vietnamese_Embedding",
                        texts = rows.Select(x => x.Text).ToList()
                    };

                    var pyRes = RunEmbeddingPython(pyReq);
                    if (pyRes == null)
                    {
                        msg = "Embedding response rỗng.";
                        return false;
                    }
                    if (!string.IsNullOrWhiteSpace(pyRes.error))
                    {
                        msg = "Python embedding lỗi: " + pyRes.error;
                        return false;
                    }
                    if (pyRes.fallback_count > 0)
                    {
                        msg = "Embedding có fallback (" + pyRes.fallback_count + "/" + rows.Count + "). Lỗi đầu tiên: " + (pyRes.first_error ?? "");
                        return false;
                    }
                    var vectors = NormalizeEmbeddingCount(pyRes == null ? null : pyRes.embeddings, rows.Count, 768);

                    using (SqlTransaction tran = cnn.BeginTransaction())
                    {
                        try
                        {
                            for (int i = 0; i < rows.Count; i++)
                            {
                                string embJson = JsonConvert.SerializeObject(vectors[i]);
                                using (SqlCommand cmdUp = new SqlCommand(@"
UPDATE dbo.tblRagUnit_Search
SET embedding_json = @embedding_json
WHERE rag_unit_id = @rag_unit_id
", cnn, tran))
                                {
                                    cmdUp.Parameters.Add("@embedding_json", SqlDbType.NVarChar, -1).Value = embJson;
                                    cmdUp.Parameters.Add("@rag_unit_id", SqlDbType.UniqueIdentifier).Value = rows[i].RagUnitId;
                                    int affected = cmdUp.ExecuteNonQuery();
                                    if (affected <= 0)
                                        throw new Exception("Không update được embedding_json cho rag_unit_id=" + rows[i].RagUnitId);
                                }
                            }

                            tran.Commit();
                            LastEmbeddedCount = rows.Count;
                            msg = "100";
                            return true;
                        }
                        catch (Exception ex2)
                        {
                            try { tran.Rollback(); } catch { }
                            msg = ex2.Message;
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return false;
            }
        }

        private RagEmbedResponse RunEmbeddingPython(RagEmbedRequest request)
        {
            string scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AI", "embed_vietnamese.py");
            if (!File.Exists(scriptPath))
                throw new Exception("Không tìm thấy script embedding: " + scriptPath);

            string reqJson = JsonConvert.SerializeObject(request);
            string tempIn = Path.Combine(Path.GetTempPath(), "rag_embed_req_" + Guid.NewGuid().ToString("N") + ".json");
            string tempOut = Path.Combine(Path.GetTempPath(), "rag_embed_res_" + Guid.NewGuid().ToString("N") + ".json");
            File.WriteAllText(tempIn, reqJson, new UTF8Encoding(false));

            // Ưu tiên python, fallback py -3
            var runners = new List<(string exe, string args)>
            {
                ("python", "\"" + scriptPath + "\" --input \"" + tempIn + "\" --output \"" + tempOut + "\""),
                ("py", "-3 \"" + scriptPath + "\" --input \"" + tempIn + "\" --output \"" + tempOut + "\"")
            };

            Exception lastEx = null;
            foreach (var runner in runners)
            {
                try
                {
                    var psi = new ProcessStartInfo(runner.exe, runner.args)
                    {
                        RedirectStandardInput = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using (var p = Process.Start(psi))
                    {
                        string stdout = p.StandardOutput.ReadToEnd();
                        string stderr = p.StandardError.ReadToEnd();
                        p.WaitForExit();

                        if (p.ExitCode != 0)
                            throw new Exception("Python embedding lỗi: " + (string.IsNullOrWhiteSpace(stderr) ? stdout : stderr));

                        string payload = "";
                        if (File.Exists(tempOut))
                            payload = File.ReadAllText(tempOut, Encoding.UTF8);
                        else
                            payload = stdout;

                        var res = JsonConvert.DeserializeObject<RagEmbedResponse>(payload);
                        if (res == null)
                            throw new Exception("Không parse được output embedding.");
                        try { if (File.Exists(tempIn)) File.Delete(tempIn); } catch { }
                        try { if (File.Exists(tempOut)) File.Delete(tempOut); } catch { }
                        return res;
                    }
                }
                catch (Exception ex)
                {
                    lastEx = ex;
                }
            }

            try { if (File.Exists(tempIn)) File.Delete(tempIn); } catch { }
            try { if (File.Exists(tempOut)) File.Delete(tempOut); } catch { }
            throw lastEx ?? new Exception("Không chạy được Python embedding.");
        }

        private class RagEmbedRow
        {
            public Guid RagUnitId { get; set; }
            public string UnitHash { get; set; }
            public string Text { get; set; }
        }

        private class RagEmbedRequest
        {
            public string model { get; set; }
            public List<string> texts { get; set; }
        }

        private class RagEmbedResponse
        {
            public List<List<float>> embeddings { get; set; }
            public string error { get; set; }
            public int fallback_count { get; set; }
            public string first_error { get; set; }
        }

        private List<List<float>> NormalizeEmbeddingCount(List<List<float>> embeddings, int expectedCount, int dim)
        {
            var result = new List<List<float>>();
            if (embeddings != null)
                result.AddRange(embeddings);

            if (result.Count > expectedCount)
                result = result.Take(expectedCount).ToList();

            while (result.Count < expectedCount)
                result.Add(Enumerable.Repeat(0f, dim).ToList());

            // Chuẩn hóa null vector con
            for (int i = 0; i < result.Count; i++)
            {
                if (result[i] == null || result[i].Count == 0)
                    result[i] = Enumerable.Repeat(0f, dim).ToList();
            }

            return result;
        }

    }
}