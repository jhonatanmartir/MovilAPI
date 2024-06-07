using System.Globalization;
using System.Text.RegularExpressions;

namespace AESMovilAPI.Utilities
{
    public class Helper
    {
        public static DateTime ParseDate(string date, string format = "dd/MM/yyyy")
        {
            CultureInfo provider = CultureInfo.InvariantCulture;
            return DateTime.ParseExact(date, format, provider);
        }
        public static string ParseStrDate(string date, string format = "yyyyMMdd")
        {
            string result = string.Empty;
            try
            {
                CultureInfo provider = CultureInfo.InvariantCulture;
                result = DateTime.ParseExact(date, format, provider).ToString("dd/MM/yyyy");
            }
            catch
            {
                result = "--/--/----";
            }
            return result;
        }
        public static string ParseStrDateMonth(string date, string format = "yyyyMMdd")
        {
            string result = string.Empty;
            try
            {
                CultureInfo provider = CultureInfo.InvariantCulture;
                result = DateTime.ParseExact(date, format, provider).ToString("MMMM 'de' yyyy");
            }
            catch
            {
                result = "--/--/----";
            }
            return result;
        }

        public static string RemoveWhitespaces(string source)
        {
            return Regex.Replace(source, @"\s", string.Empty);
        }

        public static string Base64Encode(string source)
        {
            // Encoding
            byte[] bytesToEncode = System.Text.Encoding.UTF8.GetBytes(source);
            return Convert.ToBase64String(bytesToEncode);
        }
    }
}
