using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Helpers
{
    public class SlugGeneratorHelper
    {
        public static string GenerateSlug(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "";

            // Bước 1: Chuyển sang chữ thường và trim
            text = text.ToLowerInvariant().Trim();

            // Bước 2: Loại bỏ dấu tiếng Việt
            text = RemoveDiacritics(text);

            // Bước 3: Xoá ký tự không hợp lệ và thay khoảng trắng bằng dấu gạch ngang
            text = Regex.Replace(text, @"[^a-z0-9\s-]", "");  // chỉ giữ chữ thường, số, khoảng trắng và dấu gạch ngang
            text = Regex.Replace(text, @"\s+", "-");          // thay khoảng trắng bằng gạch ngang
            text = Regex.Replace(text, @"-+", "-");           // gộp nhiều gạch ngang thành 1
            return text.Trim('-');
        }

        private static string RemoveDiacritics(string text)
        {
            text = text.Replace("đ", "d").Replace("Đ", "D");
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
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
