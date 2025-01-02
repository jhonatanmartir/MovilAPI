using System.Numerics;
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

        public static string ParseStrDate(string date, string format = "yyyyMMdd", string outFormat = "dd/MM/yyyy")
        {
            string result = string.Empty;
            try
            {
                result = DateTime.ParseExact(date, format, null).ToString(outFormat);
            }
            catch
            {
                result = "--";
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
                if (source.ToUpper().Contains(Constants.CAESS_NAME))
                {
                    companyName = Constants.CAESS_NAME;
                }
                else if (source.ToUpper().Contains(Constants.DEUSEM_NAME))
                {
                    companyName = Constants.DEUSEM_NAME;
                }
                else if (source.ToUpper().Contains(Constants.EEO_NAME))
                {
                    companyName = Constants.EEO_NAME;
                }
                else if (source.ToUpper().Contains(Constants.CLESA_NAME))
                {
                    companyName = Constants.CLESA_NAME;
                }

                if (string.IsNullOrEmpty(companyName))
                {
                    switch (source)
                    {
                        case Constants.SV10_CAESS_CODE:
                            companyName = Constants.CAESS_NAME;
                            break;
                        case Constants.SV20_DEUSEM_CODE:
                            companyName = Constants.DEUSEM_NAME;
                            break;
                        case Constants.SV30_EEO_CODE:
                            companyName = Constants.EEO_NAME;
                            break;
                        case Constants.SV40_CLESA_CODE:
                            companyName = Constants.CLESA_NAME;
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

        public static string CleanXml(string xmlString, string namespaceValue)
        {
            // Cargar el XML en un XDocument
            XDocument doc = XDocument.Parse(xmlString);

            // Remover los prefijos, pero manteniendo el namespace correcto
            XNamespace sapNamespace = namespaceValue;
            if (string.IsNullOrEmpty(namespaceValue))
            {
                sapNamespace = "http://sap.com/xi/SAPGlobal/Global";
            }

            var elements = doc.Descendants().ToList();

            foreach (var element in elements)
            {
                element.Name = sapNamespace + element.Name.LocalName; // Remover el prefijo y mantener el namespace
            }

            // Convertir de nuevo el documento a string limpio
            return doc.ToString();
        }
        public static string CleanXml(string? xmlString)
        {
            if (string.IsNullOrEmpty(xmlString))
            {
                return string.Empty;
            }

            // Cargar el XML en un XDocument
            XDocument doc = XDocument.Parse(xmlString);

            // Crear un diccionario para los espacios de nombres
            var namespaceMappings = new Dictionary<string, XNamespace>
            {
                { "global", "http://sap.com/xi/SAPGlobal/Global" },
                { "atom", "http://www.w3.org/2005/Atom" },
                { "m", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata" },
                { "d", "http://schemas.microsoft.com/ado/2007/08/dataservices" }
            };

            // Quitar los prefijos pero respetar los namespaces originales
            foreach (var element in doc.Descendants())
            {
                // Identificar el namespace actual
                XNamespace currentNamespace = element.Name.Namespace;

                // Reasignar el nombre del elemento sin prefijo
                string localName = element.Name.LocalName;
                if (namespaceMappings.ContainsValue(currentNamespace))
                {
                    element.Name = currentNamespace + localName;
                }
            }

            // Convertir de nuevo el documento a string limpio
            return doc.ToString();
        }

        public static double ParseToNegative(string value)
        {
            double amount = 0.00;
            if (value.Trim().EndsWith("-"))
            {
                amount = -double.Parse(value.Trim('-'));
            }
            else
            {
                amount = double.Parse(value);
            }

            return amount;
        }
        public static string MoneyValue(string value)
        {
            string result = ParseToNegative(value).ToString("F2");

            if (result.StartsWith("-"))
            {
                result = result.Replace("-", "").Insert(0, "-$ ");
            }
            else
            {
                result = "$ " + result;
            }

            return result;
        }

        public static bool IsCuentaContrato(string value)
        {
            if (value.Length == 12)
            {
                bool isNumber = BigInteger.TryParse(value, out BigInteger numeroEntero);
                if (isNumber)
                {
                    return true;
                }
            }
            return false;
        }

        public static decimal ToDecimal(string value)
        {
            decimal result;
            if (decimal.TryParse(value, out result))
            {
                return result;
            }
            else
            {
                return 0;
            }
        }

        public static string GetStyledNPE(string npe)
        {
            if (!string.IsNullOrEmpty(npe.Trim()))
            {
                // Eliminar espacios existentes
                npe = Regex.Replace(npe, @"\s+", "");

                // Formatear en bloques de 4
                npe = Regex.Replace(npe, ".{4}", "$0 ");
            }

            return npe;
        }
    }
}
