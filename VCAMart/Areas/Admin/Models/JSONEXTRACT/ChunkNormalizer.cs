using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RICTotalAdmin.Models
{
  
 
public static class RagHtmlNormalizer
    {
        //=================CHUẨN HÓA VĂN BẢN - LOẠI BỎ KÍ TỰ TRẮNG THỪA
        public static string NormalizeTextForRag(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            string text = input;

            // 1️⃣ Chuẩn hoá Unicode (tránh lỗi copy/paste)
            text = text.Normalize(NormalizationForm.FormC);

            // 2️⃣ Chuẩn hoá xuống dòng, tab → space
            text = Regex.Replace(text, @"[\r\n\t]+", " ");

            // 3️⃣ Loại bỏ khoảng trắng thừa
            text = Regex.Replace(text, @"\s{2,}", " ");

            // 4️⃣ Chuẩn hoá dấu câu lặp (.. ;; ,, ::)
            text = Regex.Replace(text, @"([\.]{2,})", ".");
            text = Regex.Replace(text, @"([;]{2,})", ";");
            text = Regex.Replace(text, @"([,]{2,})", ",");
            text = Regex.Replace(text, @"([:]{2,})", ":");

            // 4️⃣.5️⃣ 🔥 Loại bỏ ký tự đặc biệt trùng lặp ( +++ *** === !!! ??? )
            // Giữ lại 1 ký tự đại diện
            text = Regex.Replace(text, @"([^\p{L}\p{N}\s\.,;:])\1+", "$1");

            // 5️⃣ Loại bỏ tổ hợp dấu câu sai (. ; , :)
            text = Regex.Replace(text, @"\s*([\.|;|,|:])\s*([\.|;|,|:])+", "$1");

            // 6️⃣ Chuẩn hoá khoảng trắng quanh dấu câu
            text = Regex.Replace(text, @"\s*([,;:])\s*", "$1 ");
            text = Regex.Replace(text, @"\s*([\.])\s*", "$1 ");

            // 7️⃣ Loại bỏ dấu câu thừa ở cuối câu
            text = Regex.Replace(text, @"([,;:])\s*$", "");

            // 8️⃣ Chuẩn hoá khoảng trắng lần cuối
            text = Regex.Replace(text, @"\s{2,}", " ");

            return text.Trim();
        }
        // ===================== ENTRY POINT =====================

        //public static string NormalizeForRag(string html)
        //{
        //    if (string.IsNullOrWhiteSpace(html))
        //        return string.Empty;

        //    var doc = new HtmlDocument
        //    {
        //        OptionFixNestedTags = true,
        //        OptionAutoCloseOnEnd = true
        //    };
        //    doc.LoadHtml(html);

        //    // 1️⃣ Remove noise nodes
        //    RemoveNodes(doc, "//script|//style|//noscript|//svg|//canvas");

        //    // 2️⃣ Normalize structured blocks (DOM-level)
        //    NormalizeTableToRaw(doc);
        //    NormalizeLists(doc);

        //    // 3️⃣ DOM → plain text (ONE WAY)
        //    var raw = DomToText(doc.DocumentNode);

        //    // 4️⃣ Text-level normalization
        //    raw = NormalizeWhitespace(raw);
        //    raw = NormalizeUnicode(raw);
        //    raw = NormalizeVietnameseLegalPatterns(raw);

        //    return raw;
        //}
        public static string NormalizeForRag(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return string.Empty;

            var doc = new HtmlDocument
            {
                OptionFixNestedTags = true,
                OptionAutoCloseOnEnd = true
            };
            doc.LoadHtml(html);

            RemoveNodes(doc, "//script|//style|//noscript|//svg|//canvas");
            RemoveAnchorText(doc);
            NormalizeTableToRaw(doc);
            NormalizeLists(doc);

            var raw = DomToText(doc.DocumentNode);

            raw = NormalizeWhitespace(raw);
            raw = NormalizeUnicode(raw);
            raw = NormalizeVietnameseLegalPatterns(raw);
            
            // 🔥 CHỈ thêm đúng dòng này
            raw = NormalizeCurrencyAndUiAction(raw);
            raw = NormalizeUiActionBoundary(raw);
            return raw;
        }

        private static void RemoveAnchorText(HtmlDocument doc)
        {
            var anchors = doc.DocumentNode.SelectNodes("//a");
            if (anchors == null) return;

            foreach (var a in anchors)
            {
                // Nếu anchor nằm trong text flow → thay bằng khoảng trắng để tránh dính chữ
                var space = doc.CreateTextNode(" ");
                a.ParentNode.ReplaceChild(space, a);
            }
        }

        private static string NormalizeCurrencyAndUiAction(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            string text = input;

            // UI action: "Xem chi tiết", "Xem thêm", hoặc "Chi tiết"
            string uiAction = @"(?:xem\s+(?:chi\s+tiết|thêm)|chi\s+tiết)";

            // Currency/value pattern:
            // - "Miễn phí"
            // - số: 1000, 1.000.000, 1000.50 (nếu có)
            // - chữ: Đồng, USD, VND, VNĐ, EUR, JPY... (mở rộng được)
            string money = @"(?:miễn\s*phí|\d{1,3}(?:\.\d{3})*(?:,\d+)?|\d+(?:,\d+)?|đồng|usd|vnd|vnđ|eur|jpy|gbp|aud|cad|sgd|cny|thb|krw|inr)";

            // Tách: <money><optional spaces><uiAction>  ==>  <money> <uiAction>
            // Dùng named group để không bao giờ “mất Xem”
            text = Regex.Replace(
                text,
                $@"(?ix)
           (?<money>{money})
           \s*
           (?<action>{uiAction})
        ",
                "${money} ${action}"
            );

            return text;
        }

        //===================HAM XỬ LÝ CÁC NỘI DUNG KHÔNG THỂ TÁCH RỜI NHƯ Tên tài liêu: 1. Nội dung abc
      


        // ===================== DOM HELPERS =====================

        private static void RemoveNodes(HtmlDocument doc, string xpath)
        {
            var nodes = doc.DocumentNode.SelectNodes(xpath);
            if (nodes == null) return;

            foreach (var node in nodes)
                node.Remove();
        }

        // ===================== TABLE NORMALIZATION =====================

        private static void NormalizeTableToRaw(HtmlDocument doc)
        {
            var tables = doc.DocumentNode.SelectNodes("//table");
            if (tables == null) return;

            foreach (var table in tables.ToList())
            {
                string raw = ConvertTableNodeToRaw(table);
                var textNode = doc.CreateTextNode("\n" + raw + "\n");
                table.ParentNode.ReplaceChild(textNode, table);
            }
        }

        private static string ConvertTableNodeToRaw(HtmlNode table)
        {
            var sb = new StringBuilder();

            var headers = table
                .SelectNodes(".//thead//th")
                ?.Select(h => NormalizeWhitespace(HtmlEntity.DeEntitize(h.InnerText)))
                .ToList();

            if (headers == null || headers.Count == 0)
                return string.Empty;

            var rows = table.SelectNodes(".//tbody//tr");
            if (rows == null) return string.Empty;

            foreach (var row in rows)
            {
                var cells = row.SelectNodes("./td");
                if (cells == null || cells.Count != headers.Count)
                    continue;

                for (int i = 0; i < cells.Count; i++)
                {
                    string cellText = ExtractCellText(cells[i]);
                    if (!string.IsNullOrWhiteSpace(cellText))
                    {
                        sb.Append(headers[i]);
                        sb.Append(": ");
                        sb.Append(cellText);
                        sb.Append(". ");
                    }
                }

                sb.AppendLine();
                sb.AppendLine();
            }

            return sb.ToString().Trim();
        }
        private static string ExtractCellText(HtmlNode cell)
        {
            if (cell == null) return string.Empty;

            var brs = cell.SelectNodes(".//br");
            if (brs != null)
            {
                foreach (var br in brs)
                    br.ParentNode.ReplaceChild(
                        cell.OwnerDocument.CreateTextNode(" "),
                        br
                    );
            }

            string text = HtmlEntity.DeEntitize(cell.InnerText);
            return NormalizeWhitespace(text);
        }

       //   ===================== LIST NORMALIZATION =====================

        private static void NormalizeLists(HtmlDocument doc)
        {
            var lists = doc.DocumentNode.SelectNodes("//ul|//ol");
            if (lists == null) return;

            foreach (var list in lists.ToList())
            {
                var items = list.SelectNodes("./li");
                if (items == null) continue;

                var sb = new StringBuilder();

                foreach (var li in items)
                {
                    string text = NormalizeWhitespace(
                        HtmlEntity.DeEntitize(li.InnerText)
                    );

                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        sb.Append(text);
                        sb.Append(". ");
                    }
                }

                var textNode = doc.CreateTextNode("\n" + sb.ToString().Trim() + "\n");
                list.ParentNode.ReplaceChild(textNode, list);
            }
        }

        // ===================== DOM → TEXT =====================

        private static string DomToText(HtmlNode node)
        {
            if (node == null)
                return string.Empty;

            if (node.NodeType == HtmlNodeType.Text)
                return node.InnerText;

            var sb = new StringBuilder();
            foreach (var child in node.ChildNodes)
                sb.Append(DomToText(child));

            return sb.ToString();
        }

        // ===================== TEXT NORMALIZATION =====================

        private static string NormalizeWhitespace(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            string text = input;

            text = Regex.Replace(text, @"[\r\n\t]+", " ");
            text = Regex.Replace(text, @"\s{2,}", " ");

            return text.Trim();
        }

        private static string NormalizeUnicode(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            return input.Normalize(NormalizationForm.FormC);
        }

        private static string NormalizeVietnameseLegalPatterns(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            string text = input;

            // Chuẩn hoá Điều / Khoản / Điểm
            text = Regex.Replace(text, @"\bđiều\s+(\d+)", "Điều $1", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\bkhoản\s+(\d+)", "Khoản $1", RegexOptions.IgnoreCase);
            text = Regex.Replace(text, @"\bđiểm\s+([a-z])\b", "Điểm $1", RegexOptions.IgnoreCase);

            return text;
        }
        private static string NormalizeUiActionBoundary(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            string text = input;

            // Các nhãn nghiệp vụ thường gặp sau Xem chi tiết
            string nextLabel = @"(Phí|Lệ\s*phí|Mô\s*tả|Thời\s*hạn|Hình\s*thức|Đối\s*tượng|Cách\s*thực\s*hiện)\s*:";

            // Nếu sau "Xem chi tiết" dính liền nhãn → chèn ". "
            text = Regex.Replace(
                text,
                $@"(?ix)
        (xem\s+(?:chi\s+tiết|thêm)|chi\s+tiết)
        \s*
        (?={nextLabel})
        ",
                "$1. "
            );

            return text;
        }

    }

}