using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace AESMovilAPI.Utilities
{
    public class Helper
    {
        public static DateTime ParseDate(string date, string format = "dd/MM/yyyy")
        {
            return DateTime.ParseExact(date, format, null);
        }
        public static string ParseStrDate(string date, string format = "yyyyMMdd")
        {
            string result = string.Empty;
            try
            {
                result = DateTime.ParseExact(date, format, null).ToString("dd/MM/yyyy");
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
                result = DateTime.ParseExact(date, format, null).ToString("MMMM 'de' yyyy");
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
                else if (source.ToUpper().Contains("DEUSEM"))
                {
                    companyName = "DEUSEM";
                }
                else if (source.ToUpper().Contains("EEO"))
                {
                    companyName = "EEO";
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
                            companyName = "DEUSEM";
                            break;
                        case "SV30":
                            companyName = "EEO";
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

        // Método para deserializar la respuesta XML
        public static T DeserializeXml<T>(string xml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));


            using (StringReader reader = new StringReader(xml))
            {
                return (T)serializer.Deserialize(reader);
            }
        }

        public static string CleanXml(string xmlString)
        {
            // Cargar el XML en un XDocument
            XDocument doc = XDocument.Parse(xmlString);

            // Remover los prefijos, pero manteniendo el namespace correcto
            XNamespace sapNamespace = "http://sap.com/xi/SAPGlobal/Global";
            var elements = doc.Descendants().ToList();

            foreach (var element in elements)
            {
                element.Name = sapNamespace + element.Name.LocalName; // Remover el prefijo y mantener el namespace
            }

            // Convertir de nuevo el documento a string limpio
            return doc.ToString();
        }
    }
}
