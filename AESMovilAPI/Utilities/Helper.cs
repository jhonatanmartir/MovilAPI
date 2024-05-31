using System.Globalization;

namespace AESMovilAPI.Utilities
{
    public class Helper
    {
        public static DateTime ParseDate(string date, string format = "dd/MM/yyyy")
        {
            CultureInfo provider = CultureInfo.InvariantCulture;
            return DateTime.ParseExact(date, format, provider);
        }
    }
}
