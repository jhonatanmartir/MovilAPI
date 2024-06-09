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

        public static string GetCompanyName(string source)
        {
            string companyName = string.Empty;
            if (!string.IsNullOrEmpty(source))
            {
                if (source.ToUpper().Contains("CAESS"))
                {
                    companyName = "CAESS";
                }
                else if (source.ToUpper().Contains("EEO"))
                {
                    companyName = "EEO";
                }
                else if (source.ToUpper().Contains("DEUSEM"))
                {
                    companyName = "DEUSEM";
                }
                else if (source.ToUpper().Contains("CLESA"))
                {
                    companyName = "CLESA";
                }

                if (string.IsNullOrEmpty(companyName))
                {
                    switch (source)
                    {
                        case "SV10":
                            companyName = "CAESS";
                            break;
                        case "SV20":
                            companyName = "EEO";
                            break;
                        case "SV30":
                            companyName = "DEUSEM";
                            break;
                        case "SV40":
                            companyName = "CLESA";
                            break;
                        default:
                            break;
                    }
                }
            }

            return companyName;
        }
    }
}
