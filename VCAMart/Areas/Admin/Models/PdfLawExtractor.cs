using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace RICTotalAdmin.Models
{
    /// <summary>
    /// Tiện ích xử lý file PDF Luật/Bộ luật:
    /// - Đọc toàn bộ text từ PDF
    /// - Tách đoạn nội dung chính (từ "Phần thứ nhất ..." đến trước "CHỦ TỊCH QUỐC HỘI Nguyễn Sinh Hùng")
    /// - Trích ra "Luật số: ..." để làm doc_code
    /// - Ghi bản ghi vào tblDocument với doc_type = LEGAL
    /// 
    /// Lưu ý: Để đọc nội dung PDF, dự án cần tham chiếu 1 thư viện PDF .NET
    /// (ví dụ UglyToad.PdfPig hoặc iTextSharp). Phần ReadAllText hiện để trống,
    /// bạn có thể cài package và bổ sung sau.
    /// </summary>
    public static class PdfLawExtractor
    {
        private const string LEGAL_DOC_TYPE = "LEGAL";

        public class ExtractResult
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public string LawNumber { get; set; } // Luật số: ...
            public string Body { get; set; }      // Nội dung từ "Phần thứ nhất"...
        }

        public static string ReadAllText(string pdfPath)
        {
            if (string.IsNullOrWhiteSpace(pdfPath) || !File.Exists(pdfPath))
                return string.Empty;

            var sb = new StringBuilder();

            // PdfPig đọc text theo Unicode, hỗ trợ tốt tiếng Việt nếu PDF là text thật
            using (var document = PdfDocument.Open(pdfPath))
            {
                foreach (Page page in document.GetPages())
                {
                    // Page.Text đã là text đã ghép dòng cơ bản
                    sb.AppendLine(page.Text);
                    sb.AppendLine(); // khoảng trắng giữa các trang
                }
            }

            return sb.ToString();
        }

        public static ExtractResult ExtractLegalSegment(string fullText)
        {
            if (string.IsNullOrWhiteSpace(fullText))
            {
                return new ExtractResult
                {
                    Success = false,
                    Message = "Không đọc được nội dung từ PDF."
                };
            }

            var normalized = NormalizeWhitespace(fullText);

            // 1. Lấy doc_code từ "Luật số: ..." hoặc "Số: ..."
            string lawNumber = ExtractLawNumber(normalized);
            if (string.IsNullOrWhiteSpace(lawNumber))
            {
                return new ExtractResult
                {
                    Success = false,
                    Message = "Không tìm thấy mẫu 'Luật số: ...' hoặc 'Số: ...' trong file PDF."
                };
            }

            // 2. Cắt nội dung từ "Phần thứ nhất" tới trước "CHỦ TỊCH QUỐC HỘI Nguyễn Sinh Hùng"
            string body = ExtractBody(normalized);
            if (string.IsNullOrWhiteSpace(body))
            {
                return new ExtractResult
                {
                    Success = false,
                    Message = "Không tách được phần nội dung chính từ PDF."
                };
            }

            return new ExtractResult
            {
                Success = true,
                Message = "100",
                LawNumber = lawNumber.Trim(),
                Body = body.Trim()
            };
        }

        private static string NormalizeWhitespace(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;

            // Chuẩn hóa xuống dòng
            text = text.Replace("\r\n", "\n").Replace("\r", "\n");
            // Gom nhiều khoảng trắng
            text = Regex.Replace(text, "[ \t]{2,}", " ");
            // Gom nhiều dòng trống
            text = Regex.Replace(text, "\n{2,}", "\n\n");
            return text.Trim();
        }

        private static string ExtractLawNumber(string text)
        {
            // Tìm dòng chứa "Luật số" hoặc "Số" (có hoặc không dấu, hoa/thường)
            var lines = text.Split('\n');
            foreach (var raw in lines.Take(80)) // chỉ quét phần đầu văn bản
            {
                var line = raw.Trim();
                if (string.IsNullOrWhiteSpace(line)) continue;

                var noDiacritics = RemoveDiacritics(line).ToLowerInvariant();
                bool hasLawSo = noDiacritics.Contains("luat so");
                bool hasSoOnly = Regex.IsMatch(noDiacritics, @"(^|\s)so\s*[:\-]");
                if (!hasLawSo && !hasSoOnly) continue;

                // Regex lấy phần sau "Luật số:" hoặc "Số:"
                var m = Regex.Match(line, @"(?:Luật\s+)?Số\s*[:\-]?\s*(.+)", RegexOptions.IgnoreCase);
                if (m.Success)
                {
                    var tail = m.Groups[1].Value.Trim();

                    // Ưu tiên pattern số hiệu văn bản chuẩn: 91/2015/QH13, 104/2022/NĐ-CP, 02/2025/TT-BGDĐT, ...
                    var lawMatch = Regex.Match(
                        tail,
                        @"\b\d{1,3}\/\d{4}\/[A-Z0-9Đ\-]+",
                        RegexOptions.IgnoreCase);

                    if (lawMatch.Success)
                    {
                        return lawMatch.Value.Trim();
                    }

                    // Fallback: lấy token đầu tiên tới khoảng trắng / dấu chấm phẩy / dấu chấm
                    var val = tail.Split(new[] { ' ', '\t', ';', '.' }, StringSplitOptions.RemoveEmptyEntries)
                                  .FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(val))
                        return val.Trim();
                }
            }
            return null;
        }

        private static string ExtractBody(string text)
        {
            // Làm việc trên bản bỏ dấu để tìm chỉ số, sau đó cắt trên text gốc để giữ nguyên tiếng Việt.
            var noDiacritics = RemoveDiacritics(text).ToLowerInvariant();

            // 1) Mốc bắt đầu (ưu tiên "Phần thứ nhất", fallback "Phần I", "Chương I")
            int start = IndexOfAny(noDiacritics, new[]
            {
                "phan thu nhat",
                "phan i",
                "chuong i"
            });
            if (start < 0) return null;

            // 2) Mốc kết thúc (không khóa cứng tên người ký)
            // Dùng LastIndex để lấy mốc cuối tài liệu. Chỉ dùng "CHỦ TỊCH QUỐC HỘI" để tránh cắt nhầm
            // khi có các cụm như "Nơi nhận" hoặc "TM. Ủy ban thường vụ..." xuất hiện sớm hơn.
            int end = LastIndexOfAny(noDiacritics, new[]
            {
                "chu tich quoc hoi"
            });

            // Nếu không tìm thấy mốc kết thúc thì lấy đến hết văn bản để không fail cứng.
            if (end <= start) end = noDiacritics.Length;

            // Map index từ chuỗi bỏ dấu về chuỗi gốc: độ dài giữ nguyên nên dùng trực tiếp
            return text.Substring(start, end - start);
        }

        private static string RemoveDiacritics(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;

            var normalized = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (var c in normalized)
            {
                var uc = CharUnicodeInfo.GetUnicodeCategory(c);
                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }

            return sb.ToString()
                .Normalize(NormalizationForm.FormC)
                .Replace('đ', 'd')
                .Replace('Đ', 'D');
        }

        private static int IndexOfAny(string source, IEnumerable<string> markers)
        {
            int min = -1;
            foreach (var marker in markers)
            {
                int idx = source.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
                if (idx >= 0 && (min < 0 || idx < min)) min = idx;
            }
            return min;
        }

        private static int LastIndexOfAny(string source, IEnumerable<string> markers)
        {
            int max = -1;
            foreach (var marker in markers)
            {
                int idx = source.LastIndexOf(marker, StringComparison.OrdinalIgnoreCase);
                if (idx > max) max = idx;
            }
            return max;
        }

        public static Guid InsertLegalDocumentToDb(
            string lawNumber,
            string categoryId,
            string fileUrl,
            string fileNameWithoutExt,
            string bodyContent)
        {
            // Giới hạn độ dài để tránh lỗi "String or binary data would be truncated"
            lawNumber = Limit(lawNumber, 50);
            categoryId = Limit(categoryId, 50);
            // Không cần lưu source_url => để NULL
            fileUrl = null;
            fileNameWithoutExt = Limit(fileNameWithoutExt, 255);
            var documentId = Guid.NewGuid();

            var strcnn = ConfigurationManager.AppSettings["cnn"].ToString();
            using (var conn = new SqlConnection(strcnn))
            {
                conn.Open();
                using (var tran = conn.BeginTransaction())
                {
                    try
                    {
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.Transaction = tran;
                            cmd.CommandText = @"
INSERT INTO tblDocument
(
    doc_Id,
    doc_code,
    category_id,
    doc_title,
    source_url,
    is_active,
    CreatedDate,
    updated_at,
    doc_type,
    is_chunk,
    doc_content
)
VALUES
(
    @doc_Id,
    @doc_code,
    @category_id,
    @doc_title,
    @source_url,
    1,
    GETDATE(),
    GETDATE(),
    @doc_type,
    0,
    @doc_content
);";

                            cmd.Parameters.Add("@doc_Id", SqlDbType.UniqueIdentifier).Value = documentId;
                            cmd.Parameters.Add("@doc_code", SqlDbType.NVarChar, 50).Value = (object)lawNumber ?? DBNull.Value;
                            cmd.Parameters.Add("@category_id", SqlDbType.NVarChar, 50).Value = (object)categoryId ?? DBNull.Value;
                            cmd.Parameters.Add("@doc_title", SqlDbType.NVarChar, 255).Value = (object)fileNameWithoutExt ?? DBNull.Value;
                            cmd.Parameters.Add("@source_url", SqlDbType.NVarChar, 500).Value = DBNull.Value;
                            cmd.Parameters.Add("@doc_type", SqlDbType.NVarChar, 50).Value = LEGAL_DOC_TYPE;
                            cmd.Parameters.Add("@doc_content", SqlDbType.NText).Value = (object)bodyContent ?? DBNull.Value;
                            cmd.ExecuteNonQuery();
                        }

                        // Tách và lưu chunk theo từng Điều
                        var chunks = BuildLegalChunks(bodyContent);
                        var insertedChunks = InsertLegalChunks(conn, tran, documentId, lawNumber, chunks);

                        // Tách rag units từ mỗi chunk và lưu tblRagUnit
                        InsertLegalRagUnits(conn, tran, documentId, insertedChunks);

                        // Đồng bộ sang bảng search index metadata
                        SyncRagUnitSearch(conn, tran);

                        tran.Commit();
                        return documentId;
                    }
                    catch
                    {
                        try { tran.Rollback(); } catch { }
                        throw;
                    }
                }
            }
        }

        private class LegalChunkItem
        {
            public string ChunkTitle { get; set; }
            public string PartNumber { get; set; }      // "thứ nhất", "thứ hai", ...
            public string ChapterNumber { get; set; }   // "Chương I", "Chương II", ...
            public string SectionNumber { get; set; }   // "1", "2", ...
            public string ChunkContentRaw { get; set; }
        }

        private class InsertedChunkRow
        {
            public Guid ChunkId { get; set; }
            public Guid DocumentId { get; set; }
            public string ChunkCode { get; set; }
            public string DocCode { get; set; }
            public string ChunkTitle { get; set; }
            public string ChunkContentRaw { get; set; }
        }

        private static List<LegalChunkItem> BuildLegalChunks(string bodyContent)
        {
            var result = new List<LegalChunkItem>();
            if (string.IsNullOrWhiteSpace(bodyContent)) return result;

            var text = bodyContent.Replace("\r\n", "\n").Replace("\r", "\n");

            // Mốc PHẦN và CHƯƠNG theo vị trí để áp ngữ cảnh cho từng Điều.
            // Không phụ thuộc xuống dòng vì PDF text có thể dính 1 dòng dài.
            // part_number: chỉ lấy "thứ nhất/thứ hai/..." (không ăn cả tiêu đề chương)
            var partAnchors = Regex.Matches(text, @"(?i)\b(?:Phần|Phan)\s+(?:thứ|thu)\s+([A-Za-zÀ-ỹ0-9]+)\b")
                .Cast<Match>()
                .Select(m => new { Index = m.Index, Value = ("thứ " + m.Groups[1].Value.Trim().ToLower()) })
                .ToList();

            var chapterAnchors = Regex.Matches(text, @"(?i)\b(?:Chương|Chuong)\s+([IVXLCDM0-9]+)\b")
                .Cast<Match>()
                .Select(m => new { Index = m.Index, Value = ("Chương " + m.Groups[1].Value.Trim().ToUpper()) })
                .ToList();

            var sectionAnchors = Regex.Matches(text, @"(?i)\b(?:Mục|Muc)\s+(\d+)\b")
                .Cast<Match>()
                .Where(m => IsLikelySectionHeading(text, m.Index, m.Length))
                .Select(m => new
                {
                    Index = m.Index,
                    Value = m.Groups[1].Value.Trim()
                })
                .ToList();

            // Mỗi chunk tương ứng một Điều.
            // Không phụ thuộc xuống dòng vì PDF text có thể bị dính dòng.
            var articleStartRegex = new Regex(
                @"(?i)\b(Điều|Dieu)\s+\d+[A-Za-z]?\.\s*",
                RegexOptions.IgnoreCase);

            var starts = articleStartRegex.Matches(text).Cast<Match>().ToList();
            if (starts.Count == 0) return result;

            for (int idx = 0; idx < starts.Count; idx++)
            {
                int startIndex = starts[idx].Index;
                int endIndex = (idx + 1 < starts.Count) ? starts[idx + 1].Index : text.Length;
                if (endIndex <= startIndex) continue;

                var block = text.Substring(startIndex, endIndex - startIndex).Trim();
                if (string.IsNullOrWhiteSpace(block)) continue;

                // Loại phần heading cấu trúc (Mục/Tiểu mục/Chương/Phần) bị dính ở cuối block
                // trước Điều kế tiếp.
                block = TrimTrailingStructureHeadings(block);
                if (string.IsNullOrWhiteSpace(block)) continue;

                // Lấy chunk_title theo "Điều n. <tên điều>" và loại phần mở đầu nội dung bị dính.
                string title = ExtractChunkTitleFromBlock(block);

                // Giới hạn title để an toàn với cột DB
                title = Limit(title, 250);

                var part = partAnchors.Where(x => x.Index <= startIndex).LastOrDefault();
                var chapter = chapterAnchors.Where(x => x.Index <= startIndex).LastOrDefault();
                var chapterStart = chapter?.Index ?? -1;
                var section = sectionAnchors
                    .Where(x => x.Index <= startIndex && x.Index > chapterStart)
                    .LastOrDefault();

                var raw = Regex.Replace(block, @"\s+", " ").Trim();

                result.Add(new LegalChunkItem
                {
                    ChunkTitle = title,
                    PartNumber = part == null ? null : Limit(part.Value, 50),
                    ChapterNumber = chapter == null ? null : Limit(chapter.Value, 50),
                    SectionNumber = section == null ? null : Limit(section.Value, 10),
                    ChunkContentRaw = raw
                });
            }

            return result;
        }

        private static List<InsertedChunkRow> InsertLegalChunks(
            SqlConnection conn,
            SqlTransaction tran,
            Guid documentId,
            string docCode,
            List<LegalChunkItem> chunks)
        {
            var inserted = new List<InsertedChunkRow>();
            if (chunks == null || chunks.Count == 0) return inserted;

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
    part_number,
    chapter_number,
    section_number,
    chunk_content,
    chunk_content_raw,
    created_at
)
VALUES
(
    @chunk_id,
    @document_id,
    @chunk_code,
    @doc_code,
    @chunk_type,
    @step_no,
    @chunk_title,
    @part_number,
    @chapter_number,
    @section_number,
    @chunk_content,
    @chunk_content_raw,
    GETDATE()
);";

            for (int i = 0; i < chunks.Count; i++)
            {
                var item = chunks[i];
                string chunkCode = (docCode ?? "") + "_" + (i + 1).ToString("000");
                var chunkId = Guid.NewGuid();

                using (var cmd = new SqlCommand(sql, conn, tran))
                {
                    cmd.Parameters.Add("@chunk_id", SqlDbType.UniqueIdentifier).Value = chunkId;
                    cmd.Parameters.Add("@document_id", SqlDbType.UniqueIdentifier).Value = documentId;
                    cmd.Parameters.Add("@chunk_code", SqlDbType.NVarChar, 80).Value = Limit(chunkCode, 80);
                    cmd.Parameters.Add("@doc_code", SqlDbType.NVarChar, 50).Value = (object)docCode ?? DBNull.Value;
                    // chunk_type và step_no cùng mang số thứ tự chunk 1..n
                    int seq = i + 1;
                    cmd.Parameters.Add("@chunk_type", SqlDbType.Int).Value = seq;
                    // step_no cần unique theo document_id (ràng buộc UX_DocumentChunk_DocStep)
                    cmd.Parameters.Add("@step_no", SqlDbType.Int).Value = seq;
                    cmd.Parameters.Add("@chunk_title", SqlDbType.NVarChar, 250).Value = (object)item.ChunkTitle ?? DBNull.Value;
                    cmd.Parameters.Add("@part_number", SqlDbType.NVarChar, 50).Value = (object)item.PartNumber ?? DBNull.Value;
                    cmd.Parameters.Add("@chapter_number", SqlDbType.NVarChar, 50).Value = (object)item.ChapterNumber ?? DBNull.Value;
                    cmd.Parameters.Add("@section_number", SqlDbType.NVarChar, 20).Value = (object)item.SectionNumber ?? DBNull.Value;
                    cmd.Parameters.Add("@chunk_content", SqlDbType.NText).Value = (object)item.ChunkContentRaw ?? DBNull.Value;
                    cmd.Parameters.Add("@chunk_content_raw", SqlDbType.NText).Value = (object)item.ChunkContentRaw ?? DBNull.Value;
                    cmd.ExecuteNonQuery();
                }

                inserted.Add(new InsertedChunkRow
                {
                    ChunkId = chunkId,
                    DocumentId = documentId,
                    ChunkCode = Limit(chunkCode, 80),
                    DocCode = docCode,
                    ChunkTitle = item.ChunkTitle,
                    ChunkContentRaw = item.ChunkContentRaw
                });
            }

            return inserted;
        }

        private static void InsertLegalRagUnits(
            SqlConnection conn,
            SqlTransaction tran,
            Guid documentId,
            List<InsertedChunkRow> chunks)
        {
            if (chunks == null || chunks.Count == 0) return;

            string sql = @"
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
    GETDATE(),
    1,
    @doc_type
);";

            foreach (var chunk in chunks)
            {
                var unitTitle = ExtractUnitTitleFromChunkTitle(chunk.ChunkTitle);
                var segments = SplitChunkToLegalUnits(chunk.ChunkContentRaw, unitTitle);

                for (int i = 0; i < segments.Count; i++)
                {
                    var content = segments[i].Trim();
                    if (string.IsNullOrWhiteSpace(content)) continue;

                    string unitCode = (chunk.ChunkCode ?? "") + "_" + (i + 1).ToString("000");
                    int tokenLen = CountWords(content);
                    string unitHash = ComputeSha256(content);

                    using (var cmd = new SqlCommand(sql, conn, tran))
                    {
                        cmd.Parameters.Add("@rag_unit_id", SqlDbType.UniqueIdentifier).Value = Guid.NewGuid();
                        cmd.Parameters.Add("@chunk_id", SqlDbType.UniqueIdentifier).Value = chunk.ChunkId;
                        cmd.Parameters.Add("@document_id", SqlDbType.UniqueIdentifier).Value = documentId;
                        cmd.Parameters.Add("@unit_code", SqlDbType.NVarChar, 120).Value = Limit(unitCode, 120);
                        cmd.Parameters.Add("@unit_anchor", SqlDbType.NVarChar, 120).Value = Limit(unitCode, 120);
                        cmd.Parameters.Add("@intent_type", SqlDbType.NVarChar, 50).Value = DBNull.Value;
                        cmd.Parameters.Add("@scenario_tags", SqlDbType.NVarChar, 250).Value = DBNull.Value;
                        cmd.Parameters.Add("@unit_title", SqlDbType.NVarChar, 500).Value = (object)Limit(unitTitle, 500) ?? DBNull.Value;
                        cmd.Parameters.Add("@unit_content", SqlDbType.NText).Value = content;
                        cmd.Parameters.Add("@unit_hash", SqlDbType.NVarChar, 128).Value = unitHash;
                        cmd.Parameters.Add("@token_len", SqlDbType.Int).Value = tokenLen;
                        cmd.Parameters.Add("@doc_type", SqlDbType.NVarChar, 50).Value = LEGAL_DOC_TYPE;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        private static string ExtractUnitTitleFromChunkTitle(string chunkTitle)
        {
            if (string.IsNullOrWhiteSpace(chunkTitle)) return "";

            // "Điều 14. Bảo vệ quyền dân sự..." -> "Bảo vệ quyền dân sự..."
            var m = Regex.Match(chunkTitle.Trim(), @"^(?i)(Điều|Dieu)\s+\d+[A-Za-z]?\.\s*(.+)$");
            if (m.Success)
                return m.Groups[2].Value.Trim();

            return chunkTitle.Trim();
        }

        private static List<string> SplitChunkToLegalUnits(string chunkContentRaw, string unitTitle)
        {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(chunkContentRaw)) return result;

            string text = Regex.Replace(chunkContentRaw, @"\s+", " ").Trim();

            // Bỏ heading Điều + tiêu đề điều (nếu có)
            if (!string.IsNullOrWhiteSpace(unitTitle))
            {
                var titlePattern = Regex.Escape(unitTitle.Trim());
                text = Regex.Replace(
                    text,
                    @"^(?i)(Điều|Dieu)\s+\d+[A-Za-z]?\.\s*" + titlePattern + @"\s*",
                    "").Trim();
            }
            else
            {
                text = Regex.Replace(text, @"^(?i)(Điều|Dieu)\s+\d+[A-Za-z]?\.\s*", "").Trim();
            }

            // Một số trường hợp title bị cắt thiếu 1-2 từ cuối khi extract heading,
            // làm phần đuôi còn sót lại ở đầu nội dung (vd: "nhà nước Phong tỏa ...").
            text = TrimResidualTitleTail(text, unitTitle);
            if (string.IsNullOrWhiteSpace(text)) return result;

            // Tách theo khoản 1., 2., 3...
            // Chỉ nhận khoản 1-3 chữ số để tránh nhầm năm (vd: 2017.)
            var clauseStart = Regex.Matches(text, @"(?<!\S)([1-9]\d{0,2})\.\s+").Cast<Match>().ToList();
            if (clauseStart.Count == 0)
            {
                // Không có khoản số => dùng toàn bộ như 1 unit
                result.Add(text);
                return result;
            }

            // Có đoạn mở đầu trước khoản 1 -> lưu thành 1 unit riêng
            int firstClauseIndex = clauseStart[0].Index;
            if (firstClauseIndex > 0)
            {
                string preamble = text.Substring(0, firstClauseIndex).Trim();
                if (!string.IsNullOrWhiteSpace(preamble))
                {
                    result.Add(preamble);
                }
            }

            for (int i = 0; i < clauseStart.Count; i++)
            {
                int start = clauseStart[i].Index;
                int end = (i + 1 < clauseStart.Count) ? clauseStart[i + 1].Index : text.Length;
                if (end <= start) continue;

                string clause = text.Substring(start, end - start).Trim();
                if (string.IsNullOrWhiteSpace(clause)) continue;
                // Rule mới: mỗi khoản đánh số là 1 RagUnit duy nhất (không bẻ nhỏ theo câu).
                result.Add(clause);
            }

            return result;
        }

        private static int CountWords(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return 0;
            return Regex.Matches(text.Trim(), @"\S+").Count;
        }

        private static void SyncRagUnitSearch(SqlConnection conn, SqlTransaction tran)
        {
            using (var cmd = new SqlCommand("sp_RagUnit_Search_Sync", conn, tran))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.ExecuteNonQuery();
            }

            // Đảm bảo doc_type mặc định = LEGAL cho bảng Search (nếu DB có cột này)
            using (var cmdFix = new SqlCommand(@"
IF COL_LENGTH('dbo.tblRagUnit_Search','doc_type') IS NOT NULL
BEGIN
    UPDATE dbo.tblRagUnit_Search
    SET doc_type = 'LEGAL'
    WHERE doc_type IS NULL OR LTRIM(RTRIM(doc_type)) = '';
END
", conn, tran))
            {
                cmdFix.ExecuteNonQuery();
            }
        }

        private static string TrimTrailingStructureHeadings(string block)
        {
            if (string.IsNullOrWhiteSpace(block)) return block;

            var text = Regex.Replace(block, @"\s+", " ").Trim();

            // Lặp nhiều lần để bóc lần lượt nếu cuối block có chuỗi heading dính nhau:
            // "... Mục 2. ... Tiểu mục 1. ..." hoặc "... Chương III ..."
            bool changed;
            do
            {
                changed = false;

                // 1) Tiểu mục cuối
                var m1 = Regex.Match(
                    text,
                    @"(?is)\s+(?:Tiểu\s*mục|Tieu\s*muc)\s+\d+\.\s*[A-ZÀ-Ỹ0-9][A-ZÀ-Ỹ0-9\s,\-–—\(\)]+$");
                if (m1.Success)
                {
                    text = text.Substring(0, m1.Index).Trim();
                    changed = true;
                }

                // 2) Mục cuối
                var m2 = Regex.Match(
                    text,
                    @"(?is)\s+(?:Mục|Muc)\s+\d+\.\s*[A-ZÀ-Ỹ0-9][A-ZÀ-Ỹ0-9\s,\-–—\(\)]+$");
                if (m2.Success)
                {
                    text = text.Substring(0, m2.Index).Trim();
                    changed = true;
                }

                // 3) Chương cuối
                var m3 = Regex.Match(
                    text,
                    @"(?is)\s+(?:Chương|Chuong)\s+[IVXLCDM0-9]+\s+[A-ZÀ-Ỹ0-9][A-ZÀ-Ỹ0-9\s,\-–—\(\)]+$");
                if (m3.Success)
                {
                    text = text.Substring(0, m3.Index).Trim();
                    changed = true;
                }

                // 4) Phần cuối
                var m4 = Regex.Match(
                    text,
                    @"(?is)\s+(?:Phần|Phan)\s+(?:thứ|thu)\s+[A-Za-zÀ-ỹ0-9]+\s+[A-ZÀ-Ỹ0-9][A-ZÀ-Ỹ0-9\s,\-–—\(\)]+$");
                if (m4.Success)
                {
                    text = text.Substring(0, m4.Index).Trim();
                    changed = true;
                }
            }
            while (changed);

            return text;
        }

        private static string ComputeSha256(string text)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(text ?? ""));
                var sb = new StringBuilder(bytes.Length * 2);
                foreach (var b in bytes)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }

        private static string Limit(string value, int maxLen)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLen ? value : value.Substring(0, maxLen);
        }

        private static string ExtractChunkTitleFromBlock(string block)
        {
            if (string.IsNullOrWhiteSpace(block)) return "";

            string normalized = block.Replace("\r\n", "\n").Replace("\r", "\n").Trim();
            var headerMatch = Regex.Match(normalized, @"^(?i)(Điều|Dieu)\s+(\d+[A-Za-z]?)\.\s*(.*)$", RegexOptions.Singleline);
            if (!headerMatch.Success)
                return Limit(normalized, 250);

            string dieuNo = headerMatch.Groups[2].Value.Trim();
            string rest = (headerMatch.Groups[3].Value ?? "").Trim();

            // Trường hợp tốt nhất: PDF còn xuống dòng đúng sau tên điều.
            int lineBreak = rest.IndexOf('\n');
            if (lineBreak > 0)
            {
                string lineTitle = rest.Substring(0, lineBreak).Trim();
                return Limit("Điều " + dieuNo + ". " + lineTitle, 250);
            }

            // Nếu dính thành 1 dòng, tách theo trước khoản đầu tiên (1., 2., ...)
            int clauseStart = -1;
            // Khoản trong luật thường là 1-3 chữ số; tránh nhầm năm 4 chữ số.
            var clauseMatch = Regex.Match(rest, @"\s+[1-9]\d{0,2}\.\s+");
            if (clauseMatch.Success) clauseStart = clauseMatch.Index;
            string preClause = (clauseStart > 0 ? rest.Substring(0, clauseStart) : rest).Trim();

            // Nếu preClause quá dài hoặc có dấu ":" thì thường đang dính câu mở đầu thân điều.
            // Cắt tại các cụm bắt đầu nội dung phổ biến.
            if (preClause.Length > 120 || preClause.Contains(":"))
            {
                // Ưu tiên cắt theo mẫu đặc trưng mở đầu thân điều trong Bộ luật:
                // "Bộ luật này quy định ...", "Luật này quy định ...", ...
                var bodyStartLaw = Regex.Match(
                    preClause,
                    @"\s+((Bộ\s+luật|Luật|Nghị\s*định|Thông\s*tư|Điều|Khoản)\s+này\s+quy\s+định)\b",
                    RegexOptions.IgnoreCase);
                if (bodyStartLaw.Success && bodyStartLaw.Index > 10)
                {
                    preClause = preClause.Substring(0, bodyStartLaw.Index).Trim();
                }

                var bodyStart = Regex.Match(
                    preClause,
                    @"\s+(Hội\s+đồng|Cơ\s+quan|Tòa\s+án|Viện\s+kiểm\s+sát|Trong\s+trường\s+hợp|Khi|Nếu|Việc)\b",
                    RegexOptions.IgnoreCase);

                if (bodyStart.Success && bodyStart.Index > 10)
                {
                    preClause = preClause.Substring(0, bodyStart.Index).Trim();
                }
            }

            if (string.IsNullOrWhiteSpace(preClause))
                preClause = rest.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Take(10).Aggregate("", (a, b) => (a + " " + b).Trim());

            return Limit("Điều " + dieuNo + ". " + preClause, 250);
        }

        private static string TrimResidualTitleTail(string text, string unitTitle)
        {
            if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(unitTitle))
                return text;

            string cleanedText = Regex.Replace(text, @"\s+", " ").Trim();
            string title = Regex.Replace(unitTitle, @"^\s*(?i)(Điều|Dieu)\s+\d+[A-Za-z]?\.\s*", "").Trim();
            title = Regex.Replace(title, @"\s+", " ").Trim();
            if (string.IsNullOrWhiteSpace(title))
                return cleanedText;

            var words = title.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length < 3)
                return cleanedText;

            string firstWord = words[0];

            // Nhận diện pattern: "<đuôi title> <từ đầu title> ..."
            // Ví dụ: "nhà nước Phong tỏa tài khoản ..."
            for (int n = Math.Min(4, words.Length - 1); n >= 2; n--)
            {
                string suffix = string.Join(" ", words.Skip(words.Length - n));
                string pattern = @"^(?i)" + Regex.Escape(suffix) + @"\s+" + Regex.Escape(firstWord) + @"\b";
                if (Regex.IsMatch(cleanedText, pattern))
                {
                    cleanedText = Regex.Replace(cleanedText, @"^(?i)" + Regex.Escape(suffix) + @"\s+", "");
                    break;
                }
            }

            return cleanedText.Trim();
        }

        private static bool IsLikelySectionHeading(string text, int index, int length)
        {
            // Chặn nhầm "Tiểu mục n" thành "Mục n"
            int prevStart = Math.Max(0, index - 20);
            string prev = text.Substring(prevStart, index - prevStart);
            if (Regex.IsMatch(prev, @"(?i)(tiểu|tieu)\s*$"))
                return false;

            // Lấy chuỗi ngay sau "Mục n" để phân biệt heading và cụm tham chiếu trong câu.
            int nextStart = index + length;
            if (nextStart >= text.Length) return false;

            int lookLen = Math.Min(80, text.Length - nextStart);
            string after = text.Substring(nextStart, lookLen).TrimStart();
            if (string.IsNullOrWhiteSpace(after)) return false;

            // Loại trừ các mẫu tham chiếu thường gặp: "Mục 1 Điều ...", "Mục 2 Khoản ...", ...
            if (Regex.IsMatch(after, @"^(?i)(Điều|Dieu|Khoản|Khoan|Điểm|Diem)\b"))
                return false;

            // Heading mục thật thường đi kèm tiêu đề in hoa/ngắn ngay sau đó.
            // Ví dụ: "Mục 1. NĂNG LỰC PHÁP LUẬT..."
            // Chấp nhận dấu "." hoặc ":" sau số mục.
            if (Regex.IsMatch(after, @"^(?i)[\.\:]\s*"))
                return true;

            // Nếu không có dấu câu, yêu cầu token đầu tiên sau đó là chữ in hoa (hoặc số La Mã).
            if (Regex.IsMatch(after, @"^[A-ZÀ-Ỹ0-9]"))
                return true;

            return false;
        }
    }
}
