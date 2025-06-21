using System.Text.RegularExpressions;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Helpers
{
    public class SlugGeneratorHelper
    {
        public static string GenerateSlug(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return "";

            // Normalize and clean the string
            text = text.ToLowerInvariant().Trim();

            // Remove accents
            text = System.Text.Encoding.ASCII.GetString(
                System.Text.Encoding.GetEncoding("Cyrillic").GetBytes(text)
            );

            // Replace invalid characters with dash
            text = Regex.Replace(text, @"[^a-z0-9\s-]", "");
            text = Regex.Replace(text, @"\s+", "-").Trim('-');
            return text;
        }
    }
}
