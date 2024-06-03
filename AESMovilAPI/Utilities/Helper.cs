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

        public static string RemoveWhitespaces(string source)
        {
            return Regex.Replace(source, @"\s", string.Empty);
        }
    }
}
