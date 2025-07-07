using System.Net.Mail;
using System.Text.RegularExpressions;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Helpers
{
    public static class ValidationHelper
    {
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Regex chuẩn RFC 5322 simplified version, phổ biến dùng trong Validate email
                string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

                return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        public static bool IsValidPhoneNumber(string phone)
        {
            return Regex.IsMatch(phone, @"^\d{9,11}$"); // Ví dụ: 9–11 chữ số
        }
    }
}
