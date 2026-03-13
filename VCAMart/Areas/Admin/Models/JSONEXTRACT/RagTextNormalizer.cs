using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
 
namespace RICTotalAdmin.Models
{
    public class DocumentChunkDto
    {
        public Guid ChunkId { get; set; }
        public Guid DocumentId { get; set; }
        public string ChunkCode { get; set; }
        public string DocCode { get; set; }
        public string ChunkType { get; set; }
        public int StepNo { get; set; }
        public string ChunkTitle { get; set; }
        public string ChunkContentRaw { get; set; }
        public DateTime CreatedAt { get; set; }
        public string doc_type { get; set; }
    }

    public class RagUnitDto
    {
        public Guid RagUnitId { get; set; }
        public Guid ChunkId { get; set; }
        public Guid DocumentId { get; set; }
        public string UnitCode { get; set; }
        public string UnitAnchor { get; set; }
        public string IntentType { get; set; }
        public string ScenarioTags { get; set; }
        public string UnitTitle { get; set; }
        public string UnitContent { get; set; }
        public string UnitHash { get; set; }
        public int TokenLen { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public byte IsActive { get; set; } = 1;
        public string DocType { get; set; }
    }
    
 
public static class RagChunkText
    {
        private const int MAX_TOKEN = 300; // tùy chỉnh

        public static List<RagUnitDto> SplitChunkToRagUnits(DocumentChunkDto chunk, string normalizedText)
        {
            var units = new List<RagUnitDto>();
            if (chunk == null) return units;

            var text = NormalizeText(normalizedText);
            if (string.IsNullOrWhiteSpace(text)) return units;

            // 1) Tách theo STEP (nếu có). Nếu không có STEP, coi toàn bộ là 1 block.
            var stepBlocks = SplitByStepsKeepAll(text);
            if (stepBlocks.Count == 0)
            {
                stepBlocks.Add(text);
            }

            int globalSeq = 1;

            // 2) Mỗi STEP block: tiếp tục tách bullet; nếu không có bullet thì tách theo câu.
            for (int i = 0; i < stepBlocks.Count; i++)
            {
                var stepBlock = stepBlocks[i];

                // Nếu block bắt đầu bằng "Bước x", thường là HOW (nhưng vẫn cho detect theo từng câu/bullet bên trong)
                var bulletBlocks = SplitByBulletsKeepAll(stepBlock);

                if (bulletBlocks.Count > 0)
                {
                    // Với mỗi bullet block, tách thành các câu hoàn chỉnh, rồi pack theo token
                    for (int b = 0; b < bulletBlocks.Count; b++)
                    {
                        var bulletText = bulletBlocks[b];
                        var sentences = ExtractCompleteSentences(bulletText); // đảm bảo không cắt lửng câu
                        PackSentencesToUnits(chunk, sentences, units, ref globalSeq);
                    }
                }
                else
                {
                    var sentences = ExtractCompleteSentences(stepBlock);
                    PackSentencesToUnits(chunk, sentences, units, ref globalSeq);
                }
            }

            return units;
        }

        // =========================
        // Core packing (token control but no mid-sentence cut)
        // =========================
        private static void PackSentencesToUnits(
            DocumentChunkDto chunk,
            List<string> sentences,
            List<RagUnitDto> units,
            ref int globalSeq)
        {
            if (sentences == null || sentences.Count == 0) return;

            var outlineBlocks = new List<List<string>>();
            List<string> currentBlock = null;
            int currentOutlineLevel = 0;

            // =========================
            // 1️⃣ Group theo outline
            // =========================
            foreach (var raw in sentences)
            {
                var s = CleanSpace(raw);
                if (string.IsNullOrWhiteSpace(s)) continue;

                int level = GetOutlineLevel(s);

                if (level > 0)
                {
                    // gặp outline mới → đóng block cũ
                    if (currentBlock != null && currentBlock.Count > 0)
                        outlineBlocks.Add(currentBlock);

                    currentBlock = new List<string>();
                    currentOutlineLevel = level;
                    currentBlock.Add(s);
                }
                else
                {
                    if (currentBlock == null)
                        currentBlock = new List<string>();

                    currentBlock.Add(s);
                }
            }

            if (currentBlock != null && currentBlock.Count > 0)
                outlineBlocks.Add(currentBlock);

            // =========================
            // 2️⃣ Pack từng outline block
            // =========================
            foreach (var block in outlineBlocks)
            {
                var buffer = new StringBuilder();
                string bufferIntent = null;

                foreach (var s in block)
                {
                    var intent = DetectIntent(s);

                    if (buffer.Length == 0)
                    {
                        buffer.Append(s);
                        bufferIntent = intent;
                        continue;
                    }

                    // đổi intent → flush
                    if (!string.Equals(bufferIntent, intent, StringComparison.OrdinalIgnoreCase))
                    {
                        units.Add(BuildUnit(chunk, buffer.ToString(), bufferIntent, globalSeq++));
                        buffer.Clear();
                        buffer.Append(s);
                        bufferIntent = intent;
                        continue;
                    }

                    var candidate = buffer + " " + s;
                    if (EstimateToken(candidate) <= MAX_TOKEN)
                    {
                        buffer.Append(" ");
                        buffer.Append(s);
                    }
                    else
                    {
                        units.Add(BuildUnit(chunk, buffer.ToString(), bufferIntent, globalSeq++));
                        buffer.Clear();
                        buffer.Append(s);
                        bufferIntent = intent;
                    }
                }

                if (buffer.Length > 0)
                {
                    units.Add(BuildUnit(chunk, buffer.ToString(), bufferIntent, globalSeq++));
                }
            }
        }

        // =========================
        // STEP split (keep all, do NOT require EndsWith("."))
        // =========================
        private static List<string> SplitByStepsKeepAll(string text)
        {
            var results = new List<string>();
            if (string.IsNullOrWhiteSpace(text)) return results;

            // Bước 1: / Bước 1. / Bước 1) / Bước 01:
            var regex = new Regex(
                @"(?is)(Bước\s+\d+\s*[:\.\)]\s*.*?)(?=(?:\n\s*Bước\s+\d+\s*[:\.\)])|$)",
                RegexOptions.Singleline | RegexOptions.IgnoreCase);

            var matches = regex.Matches(text);
            foreach (Match m in matches)
            {
                var v = CleanSpace(m.Value);
                if (!string.IsNullOrWhiteSpace(v))
                    results.Add(v);
            }

            return results;
        }
        private static string NormalizeDotInsideBrackets(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            string result = text;

            // =========================
            // 1️⃣ Xoá dấu chấm NGAY SAU ngoặc mở
            // (.Nội dung) → (Nội dung)
            // [.Nội dung] → [Nội dung]
            // {.Nội dung} → {Nội dung}
            // =========================
            result = Regex.Replace(
                result,
                @"([\(\[\{])\s*\.(?=\s*\p{L})",
                "$1"
            );

            // =========================
            // 2️⃣ Xoá dấu chấm NGAY TRƯỚC ngoặc đóng
            // (Nội dung.) → (Nội dung)
            // [Nội dung.] → [Nội dung]
            // {Nội dung.} → {Nội dung}
            //
            // ⚠️ KHÔNG xoá nếu trước dấu chấm là số (Điều 3.2.)
            // =========================
            result = Regex.Replace(
                result,
                @"(?<!\d)\.(\s*[\)\]\}])",
                "$1"
            );

            return result;
        }


        // =========================
        // BULLET split (keep all; bullet item may span multiple lines until next bullet)
        // Supports: -, •, +, *, 1., 1), a), (1)
        // =========================
        private static List<string> SplitByBulletsKeepAll(string text)
        {
            var results = new List<string>();
            if (string.IsNullOrWhiteSpace(text)) return results;

            var lines = text.Split('\n');
            var cur = new StringBuilder();
            bool inBullet = false;

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var t = line.Trim();
                if (string.IsNullOrWhiteSpace(t)) continue;

                bool isBulletStart =
                    // 1) 🔥 Số phân cấp: 1.1. / 1.1.1.
                    Regex.IsMatch(t, @"^\(?\d+(?:\.\d+)+\)?\.\s+")
                    // 2) Số đơn: 1. / (1). / 1)
                    || Regex.IsMatch(t, @"^\(?\d+\)?[\.|\)]\s+")
                    // 3) Chữ: a. / b)
                    || Regex.IsMatch(t, @"^[a-zA-Z][\)|\.]\s+")
                    // 4) Ký hiệu: -, •, +, *
                    || Regex.IsMatch(t, @"^[-•\+\*]\s+");


                if (isBulletStart)
                {
                    // flush bullet trước
                    if (inBullet && cur.Length > 0)
                    {
                        var prev = CleanSpace(cur.ToString());
                        if (!string.IsNullOrWhiteSpace(prev))
                            results.Add(prev);
                        cur.Clear();
                    }

                    inBullet = true;

                    // remove bullet marker
                    t = Regex.Replace(t, @"^\(?\d+(?:\.\d+)+\)?\.\s+", "");
                    t = Regex.Replace(t, @"^\(?\d+\)?[\.|\)]\s+", "");
                    t = Regex.Replace(t, @"^[a-zA-Z][\)|\.]\s+", "");
                    t = Regex.Replace(t, @"^[-•\+\*]\s+", "");


                    cur.Append(t);
                }
                else
                {
                    // continuation line của bullet
                    if (inBullet)
                    {
                        cur.Append(" ");
                        cur.Append(t);
                    }
                }
            }

            // flush cuối
            if (inBullet && cur.Length > 0)
            {
                var last = CleanSpace(cur.ToString());
                if (!string.IsNullOrWhiteSpace(last))
                    results.Add(last);
            }

            return results;
        }

        // =========================
        // Sentence extraction:
        // - "Câu kết thúc bởi dấu chấm"
        // - Không cắt giữa câu
        // - Nếu cuối cùng còn dư (không có dấu chấm), gắn vào câu trước nếu có để tránh bỏ lửng
        // =========================
        private static List<string> ExtractCompleteSentences(string text)
        {
            var results = new List<string>();
            if (string.IsNullOrWhiteSpace(text)) return results;

           
            var t = CleanSpace(text);

            t = NormalizeDotInsideBrackets(t);
            // 🔥 FIX toàn bộ dữ liệu có cấu trúc (số, ngày, văn bản, điều khoản)
            t = FixSpacedStructuredData(t);

            // Sau khi đã “lành lặn” mới đi protect
            var protectedRanges = GetProtectedRanges(t);

            var rawSentences = new List<string>();
            var sb = new StringBuilder();

            // =========================
            // 1️⃣ Tách câu thô (như hiện tại)
            // =========================
            for (int i = 0; i < t.Length; i++)
            {
                char c = t[i];
                sb.Append(c);

                if (c == '.')
                {
                    if (IsInsideProtectedRange(i, protectedRanges))
                        continue;

                    // 🔥 NEW: nếu dấu '.' là của marker/outline => KHÔNG cắt
                    if (IsDotBelongsToOutlineMarker(t, i))
                        continue;

                    var sentence = CleanSpace(sb.ToString());
                    if (!string.IsNullOrWhiteSpace(sentence))
                        rawSentences.Add(sentence);

                    sb.Clear();
                }

            }

            var tail = CleanSpace(sb.ToString());
            if (!string.IsNullOrWhiteSpace(tail))
                rawSentences.Add(tail);

            // =========================
            // 2️⃣ MERGE marker-only sentence
            // =========================
            // =========================
            // 2️⃣ MERGE marker-only sentence
            // =========================
            for (int i = 0; i < rawSentences.Count; i++)
            {
                var cur = rawSentences[i];

                // Case 1: marker đơn lẻ "1." / "a." / "I."
                if (IsPureOutlineMarker(cur) && i + 1 < rawSentences.Count)
                {
                    results.Add(CleanSpace(cur + " " + rawSentences[i + 1]));
                    i++;
                    continue;
                }

                // 🔥 Case 1.5: Section marker "Bước 1." / "Mục 2.1." / "Điều 3."
                if (IsSectionMarkerOnly(cur) && i + 1 < rawSentences.Count)
                {
                    results.Add(CleanSpace(cur + " " + rawSentences[i + 1]));
                    i++;
                    continue;
                }

                // Case 2: "Tên tài liệu: 4."
                if (IsLabeledEnumerationOnly(cur) && i + 1 < rawSentences.Count)
                {
                    results.Add(CleanSpace(cur + " " + rawSentences[i + 1]));
                    i++;
                    continue;
                }

                results.Add(cur);
            }


            return results;
        }
        private static bool IsPureOutlineMarker(string sentence)
        {
            if (string.IsNullOrWhiteSpace(sentence)) return false;

            var s = sentence.Trim();

            // Roman: I.
            if (Regex.IsMatch(s, @"^[IVXLCDM]+\.$", RegexOptions.IgnoreCase))
                return true;

            // Number: 1.
            if (Regex.IsMatch(s, @"^\d+\.$"))
                return true;
            if (Regex.IsMatch(s, @"^\d+(?:\.\d+)+\.$"))
                return true;
            // Letter: a.
            if (Regex.IsMatch(s, @"^[a-zA-Z]\.$"))
                return true;

            return false;
        }

        private static List<(int start, int end)> GetProtectedRanges(string text)
        {
            var ranges = new List<(int start, int end)>();

            // =========================
            // 1️⃣ URL chuẩn
            // =========================
            AddRanges(ranges, text,
                @"(https?:\/\/|www\.)[^\s\)]+");

            // =========================
            // 2️⃣ URL bị vỡ (có khoảng trắng)
            // Ví dụ: https: /dichvucong.gov. vn
            // =========================
            AddRanges(ranges, text,
                @"https?\s*:\s*\/?\s*\/?\s*[a-zA-Z0-9\-]+(\s*\.\s*[a-zA-Z0-9\-]+)+");

            // =========================
            // 3️⃣ Domain bị vỡ, không protocol
            // dichvucong.gov. vn
            // =========================
            AddRanges(ranges, text,
                @"\b[a-zA-Z0-9\-]+(\s*\.\s*[a-zA-Z]{2,}){1,}\b");

            // =========================
            // 4️⃣ Email (kể cả bị vỡ nhẹ)
            // =========================
            AddRanges(ranges, text,
                @"[A-Z0-9._%+\-]+\s*@\s*[A-Z0-9.\-]+\s*\.\s*[A-Z]{2,}",
                RegexOptions.IgnoreCase);

            // =========================
            // 5️⃣ Version / số kỹ thuật (v1.2.3, 2.4.1)
            // =========================
            AddRanges(ranges, text,
                @"\b(v?\d+(\.\d+)+)\b");

            // =========================
            // 6️⃣ Viết tắt hành chính (TP.HCM, Q.1, P.5)
            // =========================
            AddRanges(ranges, text,
                @"\b[A-ZĐ]{1,5}\.\s*[A-Z0-9]{1,5}\b");

            // =========================
            // 🔥 7️⃣ SỐ TIỀN CÓ PHÂN CÁCH HÀNG NGHÌN
            // 4.500.000 | 1.234.567.890
            // =========================
            AddRanges(ranges, text,
                @"\b\d{1,3}(\.\d{3})+\b");

            // =========================
            // 8️⃣ SỐ THẬP PHÂN THỰC
            // 3.14 | 0.5 | 2.75
            // =========================
            AddRanges(ranges, text,
                @"\b\d+\.\d+\b");

            // =========================
            // 9️⃣ NGÀY THÁNG DẠNG dd.mm.yyyy
            // =========================
            AddRanges(ranges, text,
                @"\b\d{1,2}\.\d{1,2}\.\d{4}\b");

            // =========================
            // 🔟 SỐ HIỆU VĂN BẢN PHÁP LUẬT
            // 104/2022/NĐ-CP | 02/2025/TT-BGDĐT
            // =========================
            AddRanges(ranges, text,
                @"\b\d{1,4}\/\d{4}\/[A-ZĐ\-]+\b");

            // =========================
            // 1️⃣1️⃣ Điều / Khoản / Điểm rút gọn
            // điểm a.1 | khoản 2.3
            // =========================
            AddRanges(ranges, text,
                @"\b(điểm|khoản|điều)\s+[a-z0-9]+(\.[a-z0-9]+)+",
                RegexOptions.IgnoreCase);
            // Nháy kép chuẩn "
            AddRanges(ranges, text, "\"([^\"]+)\"");

            // Nháy kép Unicode “ ”
            AddRanges(ranges, text, "“([^”]+)”");
            // =========================
            // 1️⃣3️⃣ PARENTHESIS / BRACKETS
            // =========================

            // ( ... )
            AddRanges(ranges, text, @"\([^()]+\)");

            // [ ... ]
            AddRanges(ranges, text, @"\[[^\[\]]+\]");

            // { ... }
            AddRanges(ranges, text, @"\{[^{}]+\}");
            return ranges;
        }
         
        private static void AddRanges(
        List<(int start, int end)> ranges,
        string text,
        string pattern,
        RegexOptions options = RegexOptions.None)
        {
            foreach (Match m in Regex.Matches(text, pattern, options))
            {
                ranges.Add((m.Index, m.Index + m.Length - 1));
            }
        }

        private static bool IsInsideProtectedRange(
        int index,
        List<(int start, int end)> ranges)
        {
            for (int i = 0; i < ranges.Count; i++)
            {
                if (index >= ranges[i].start && index <= ranges[i].end)
                    return true;
            }
            return false;
        }

        //private static List<string> ExtractCompleteSentences(string text)
        //{
        //    var results = new List<string>();
        //    if (string.IsNullOrWhiteSpace(text)) return results;

        //    var t = CleanSpace(text);

        //    var sb = new StringBuilder();
        //    for (int i = 0; i < t.Length; i++)
        //    {
        //        char c = t[i];
        //        sb.Append(c);

        //        if (c == '.')
        //        {
        //            var sentence = CleanSpace(sb.ToString());
        //            if (!string.IsNullOrWhiteSpace(sentence))
        //                results.Add(sentence);
        //            sb.Clear();
        //        }
        //    }

        //    // phần dư không có dấu chấm -> gắn vào câu trước (nếu có), để không tạo unit "bỏ lửng"
        //    var tail = CleanSpace(sb.ToString());
        //    if (!string.IsNullOrWhiteSpace(tail))
        //    {
        //        if (results.Count > 0)
        //        {
        //            results[results.Count - 1] = CleanSpace(results[results.Count - 1] + " " + tail);
        //        }
        //        else
        //        {
        //            // trường hợp hiếm: cả đoạn không có dấu chấm -> vẫn giữ làm 1 đoạn (không thể “kết thúc bởi dấu chấm”)
        //            // để tránh mất dữ liệu. Nếu bạn muốn cứng 100% theo dấu chấm: có thể return empty ở đây.
        //            results.Add(tail);
        //        }
        //    }

        //    return results;
        //}

        // =========================
        // Intent detection (rule-based)
        // =========================
        private static string DetectIntent(string text)
        {
            var s = (text ?? "").ToLowerInvariant();

            // WHERE
            if (s.Contains("nộp tại") || s.Contains("nộp hồ sơ tại") || s.Contains("cơ quan tiếp nhận") || s.Contains("địa điểm nộp"))
                return "WHERE";

            // HOW
            if (s.Contains("cách thực hiện") || s.Contains("thực hiện như sau") || s.Contains("bước") || s.Contains("nộp trực tuyến") || s.Contains("qua bưu chính"))
                return "HOW";

            // WHAT_DOCUMENT
            if (s.Contains("hồ sơ gồm") || s.Contains("thành phần hồ sơ") || s.Contains("giấy tờ") || s.Contains("tài liệu") || s.Contains("đơn đề nghị"))
                return "WHAT_DOCUMENT";

            // WHO
            if (s.Contains("đối tượng") || s.Contains("người thực hiện") || s.Contains("người nộp") || s.Contains("ủy quyền") || s.Contains("nhận thay"))
                return "WHO";

            // TIME
            if (s.Contains("thời hạn") || s.Contains("trong vòng") || s.Contains("không quá") || s.Contains("ngày làm việc") || s.Contains("giải quyết trong"))
                return "TIME";

            // FEE
            if (s.Contains("lệ phí") || s.Contains("phí") || s.Contains("mức thu") || s.Contains("nộp phí"))
                return "FEE";

            // CONDITION
            if (s.Contains("điều kiện") || s.Contains("yêu cầu") || s.Contains("trường hợp đủ") || s.Contains("chỉ áp dụng khi"))
                return "CONDITION";

            // EXCEPTION
            if (s.Contains("ngoại lệ") || s.Contains("trừ trường hợp") || s.Contains("không áp dụng") || s.Contains("trường hợp không"))
                return "EXCEPTION";

            return "HOW";
        }

        // =========================
        // Build unit
        // =========================
        private static RagUnitDto BuildUnit(DocumentChunkDto chunk, string content, string intent, int seq)
        {
            content = CleanSpace(content);

            return new RagUnitDto
            {
                RagUnitId = Guid.NewGuid(),
                ChunkId = chunk.ChunkId,
                DocumentId = chunk.DocumentId,
                UnitCode = $"{chunk.ChunkCode}_{seq:D2}",
                UnitAnchor = $"{chunk.ChunkCode}_{seq:D2}",
                IntentType = intent,
                UnitTitle = chunk.ChunkTitle,
                UnitContent = content,
                TokenLen = EstimateToken(content),
                UnitHash = ComputeHash(content),
                CreatedAt = DateTime.Now,
                IsActive = 1,
                DocType = string.IsNullOrWhiteSpace(chunk.doc_type) ? "PROCEDURE" : chunk.doc_type
            };
        }

        private static int EstimateToken(string text)
        {
            // Ước lượng đơn giản: 1 token ~ 4 ký tự
            if (string.IsNullOrEmpty(text)) return 0;
            return Math.Max(1, text.Length / 4);
        }

        private static string ComputeHash(string text)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(text ?? ""));
                return BitConverter.ToString(bytes).Replace("-", "");
            }
        }

        // =========================
        // Helpers
        // =========================
        private static string NormalizeText(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "";

            text = text.Replace("\r\n", "\n").Replace("\r", "\n");

            // 🔥 FIX bullet phân cấp bị thừa khoảng trắng:
            // 1. 1. 2.  -> 1.1.2.
            // 1 . 2 . 3 -> 1.2.3
            text = Regex.Replace(text, @"(?<=\d)\s*\.\s*(?=\d)", ".");
            
            return text.Trim();
        }

        private static bool IsDotBelongsToOutlineMarker(string text, int dotIndex)
        {
            // dotIndex là vị trí ký tự '.' trong text
            // Ta nhìn ngược lại một đoạn ngắn trước dấu chấm để xem có phải marker không
            int start = Math.Max(0, dotIndex - 30);
            string left = text.Substring(start, dotIndex - start + 1).TrimEnd(); // gồm cả dấu '.'

            // 1) Bước 1. / Bước 1.1. / Mục I.2. / Điều 3.2.
            if (Regex.IsMatch(left, @"(?i)(Bước|Mục|Phần|Chương|Điều|Khoản|Điểm)\s+(\d+(?:\.\d+)*|[IVXLCDM]+(?:\.\d+)*|[a-zA-Z](?:\.\d+)*)\.$"))
                return true;

            // 2) 1. / 12.
            if (Regex.IsMatch(left, @"\b\d+\.$"))
                return true;

            // 3) 1.1. / 1.1.2.
            if (Regex.IsMatch(left, @"\b\d+(?:\.\d+)+\.$"))
                return true;

            // 4) a. / b.
            if (Regex.IsMatch(left, @"\b[a-zA-Z]\.$"))
                return true;

            // 5) I. / II. (Roman)
            if (Regex.IsMatch(left, @"\b[IVXLCDM]+\.$", RegexOptions.IgnoreCase))
                return true;

            return false;
        }

        private static string CleanSpace(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "";
            s = s.Replace("\t", " ");
            s = Regex.Replace(s, @"[ ]{2,}", " ");
            s = Regex.Replace(s, @"\n{2,}", "\n");
            return s.Trim();
        }

        private static int GetOutlineLevel(string sentence)
        {
            if (string.IsNullOrWhiteSpace(sentence)) return 0;

            var s = sentence.TrimStart();

            // La Mã: I.
            if (Regex.IsMatch(s, @"^[IVXLCDM]+\.\s+", RegexOptions.IgnoreCase))
                return 1;

            // Số đơn: 1.
            if (Regex.IsMatch(s, @"^\d+\.\s+"))
                return 2;

            // 🔥 Số phân cấp: 1.1 / 1.1.1 / 1.1.1.
            var m = Regex.Match(s, @"^(\d+(?:\.\d+)+)\.?\s+");
            if (m.Success)
            {
                int dotCount = m.Groups[1].Value.Count(c => c == '.'); // 1.1 -> 1 dot, 1.1.1 -> 2 dots
                return 3 + dotCount; // 1.1 -> 4, 1.1.1 -> 5
            }

            // Chữ: a.
            if (Regex.IsMatch(s, @"^[a-zA-Z]\.\s+"))
                return 3;

            return 0;
        }


        //=============HAM NAY ĐỂ GIẢI QUYẾT CÁC VẤN ĐỀ VỀ BỊ CHUNK GIỮA CÂU KIỂU Tên tài liệu: 1. Nội dung thứ nhất,...
        private static bool IsLabeledEnumerationOnly(string sentence)
        {
            if (string.IsNullOrWhiteSpace(sentence))
                return false;

            var s = sentence.Trim();

            /*
             * LABELS – các nhãn thường gặp (có & không dấu, OCR-safe)
             */
            string[] labels =
            {
        // Hồ sơ / tài liệu
        "Tên giấy tờ", "Ten giay to",
        "Tên tài liệu", "Ten tai lieu",
        "Danh mục giấy tờ", "Danh muc giay to",
        "Danh mục tài liệu", "Danh muc tai lieu",
        "Thành phần hồ sơ", "Thanh phan ho so",
        "Hồ sơ gồm", "Ho so gom",
        "Hồ sơ yêu cầu", "Ho so yeu cau",

        // Nội dung / mô tả
        "Nội dung", "Noi dung",
        "Mô tả", "Mo ta",

        // Mục / Phần (có & không dấu)
        "Mục", "Muc",
        "Phần", "Phan"
    };

            string labelPattern = string.Join("|", labels.Select(Regex.Escape));

            /*
             * ENUMERATION:
             *  - 1
             *  - 1.2
             *  - 1.2.3
             *  - I | II | III | IV | V | X
             *  - I.2 | II.3.1
             */
            string enumPattern = @"
        (
            \d+(?:\.\d+)* |
            [IVXLCDM]+ |
            [IVXLCDM]+\.\d+(?:\.\d+)*
        )
        \.
    ";

            /*
             * FINAL:
             *
             *  Label : ENUM . [Nội dung có thể có hoặc không]
             *
             *  Match:
             *   - Mục: I.
             *   - Mục: I. Nội dung
             *   - Mục: I.2.
             *   - Mục: I.2. Nội dung
             */
            string pattern = $@"
                ^
                (?:{labelPattern})
                \s*
                :
                \s*
                {enumPattern}
                (\s+.+)?     # <-- CHO PHÉP CÓ NỘI DUNG PHÍA SAU
                $
            ";

            return Regex.IsMatch(
                s,
                pattern,
                RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace
            );
        }


        //=================HÀM NÀY ĐỂ FIX LỖI VỀ KHOẢNG TRẮNG TRONG CÁC DỮ LIỆU SỐ
        private static string FixSpacedStructuredData(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            string text = input;

            // =========================
            // A️⃣ SỐ & SỐ TIỀN
            // =========================

            // 1) Ghép dấu chấm phân tách hàng nghìn: 4. 500. 000 → 4.500.000
            text = Regex.Replace(
                text,
                @"(?<=\d)\s*\.\s*(?=\d{3}\b)",
                "."
            );

            // 2) Ghép số thập phân: 3 .14 → 3.14
            text = Regex.Replace(
                text,
                @"(?<=\d)\s*\.\s*(?=\d+\b)",
                "."
            );

            // 3) Ghép dấu phẩy thập phân kiểu VN: 12 , 5 → 12,5
            text = Regex.Replace(
                text,
                @"(?<=\d)\s*,\s*(?=\d+\b)",
                ","
            );

            // =========================
            // B️⃣ NGÀY THÁNG
            // =========================

            // dd . mm . yyyy
            text = Regex.Replace(
                text,
                @"(?<=\d)\s*\.\s*(?=\d)",
                "."
            );

            // dd / mm / yyyy
            text = Regex.Replace(
                text,
                @"(?<=\d)\s*\/\s*(?=\d)",
                "/"
            );

            // dd - mm - yyyy
            text = Regex.Replace(
                text,
                @"(?<=\d)\s*-\s*(?=\d)",
                "-"
            );

            // =========================
            // C️⃣ SỐ HIỆU VĂN BẢN PHÁP LUẬT
            // =========================

            // 02 / 2025 / TT - BGDĐT
            text = Regex.Replace(
                text,
                @"(?<=\d)\s*\/\s*(?=\d{4})",
                "/"
            );

            text = Regex.Replace(
                text,
                @"(?<=\d{4})\s*\/\s*(?=[A-ZĐ])",
                "/"
            );

            text = Regex.Replace(
                text,
                @"(?<=[A-ZĐ])\s*-\s*(?=[A-ZĐ])",
                "-"
            );

            // =========================
            // D️⃣ VERSION / ĐÁNH SỐ KỸ THUẬT
            // =========================

            // v 1 . 2 . 3 → v1.2.3
            text = Regex.Replace(
                text,
                @"(?<=v)\s*(\d)",
                "$1",
                RegexOptions.IgnoreCase
            );

            text = Regex.Replace(
                text,
                @"(?<=\d)\s*\.\s*(?=\d)",
                "."
            );

            // =========================
            // E️⃣ ĐIỀU / KHOẢN / ĐIỂM
            // =========================

            // Điều 3 . 2 → Điều 3.2
            text = Regex.Replace(
                text,
                @"(?<=\b(điều|khoản|điểm)\s+\d+)\s*\.\s*(?=\d)",
                ".",
                RegexOptions.IgnoreCase
            );

            // khoản 1 . a → khoản 1.a
            text = Regex.Replace(
                text,
                @"(?<=\b(điều|khoản|điểm)\s+\d+)\s*\.\s*(?=[a-z])",
                ".",
                RegexOptions.IgnoreCase
            );

            // =========================
            // F️⃣ SỐ + ĐƠN VỊ
            // =========================

            // 10 . 000 đồng → 10.000 đồng
            text = Regex.Replace(
                text,
                @"(?<=\d)\s*\.\s*(?=\d+\s*(đồng|vnđ|vnd|usd|eur|%))",
                ".",
                RegexOptions.IgnoreCase
            );

            // 10 . 5 % → 10.5%
            text = Regex.Replace(
                text,
                @"(?<=\d)\s*\.\s*(?=\d+\s*%)",
                "."
            );

            // =========================
            // G️⃣ CLEAN LẠI SPACE THỪA
            // =========================
            text = Regex.Replace(text, @"\s{2,}", " ");

            return text.Trim();
        }

        //============================HAM NÀY XỬ LÝ CÁC THÔNG TIN BƯỚC 1., BƯỚC 2.,...
        private static bool IsSectionMarkerOnly(string sentence)
        {
            if (string.IsNullOrWhiteSpace(sentence))
                return false;

            var s = sentence.Trim();

            /*
             * LABELS (whitelist – các nhãn phân đoạn thông dụng):
             *
             * Quy trình / hướng dẫn:
             *  - Bước
             *  - Mục
             *  - Phần
             *  - Chương
             *
             * Pháp lý / hành chính:
             *  - Điều
             *  - Khoản
             *  - Điểm
             *
             * NUMBERING:
             *  - Số: 1 | 2 | 10
             *  - Đa cấp: 1.1 | 1.2.3 | 10.4.2
             *  - Chữ: a | b
             *  - Chữ + số: a.1 | b.2
             *  - La Mã: I | II | III | IV | V | X | L | C | D | M
             *
             * TERMINATOR:
             *  - .
             *  - :
             *  - )
             */

            string pattern = @"
        ^
                (Bước|Mục|Phần|Chương|Điều|Khoản|Điểm)
                \s+
                (
                    # 1 | 1.1 | 1.2.3
                    \d+(?:\.\d+)*

                    | # a | a.1 | b.2
                    [a-zA-Z](?:\.\d+)*

                    | # I | II | III | IV | V | X | ...
                    [IVXLCDM]+
                )
                \s*
                [\.\:\)]
                $
            ";

            return Regex.IsMatch(
                s,
                pattern,
                RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace
            );
        }



    }

}