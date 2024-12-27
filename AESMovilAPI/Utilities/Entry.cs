using System.Xml.Serialization;

namespace AESMovilAPI.Utilities
{
    [XmlRoot(ElementName = "entry", Namespace = "http://www.w3.org/2005/Atom")]
    public class Entry
    {
        [XmlElement(ElementName = "id", Namespace = "http://www.w3.org/2005/Atom")]
        public string Id { get; set; }

        [XmlElement(ElementName = "title", Namespace = "http://www.w3.org/2005/Atom")]
        public string Title { get; set; }

        [XmlElement(ElementName = "updated", Namespace = "http://www.w3.org/2005/Atom")]
        public DateTime Updated { get; set; }

        [XmlElement(ElementName = "category", Namespace = "http://www.w3.org/2005/Atom")]
        public Category Category { get; set; }

        [XmlElement(ElementName = "link", Namespace = "http://www.w3.org/2005/Atom")]
        public Link Link { get; set; }

        [XmlElement(ElementName = "content", Namespace = "http://www.w3.org/2005/Atom")]
        public Content Content { get; set; }
    }
    public class Category
    {
        [XmlAttribute(AttributeName = "term")]
        public string Term { get; set; }

        [XmlAttribute(AttributeName = "scheme")]
        public string Scheme { get; set; }
    }

    public class Link
    {
        [XmlAttribute(AttributeName = "href")]
        public string Href { get; set; }

        [XmlAttribute(AttributeName = "rel")]
        public string Rel { get; set; }

        [XmlAttribute(AttributeName = "title")]
        public string Title { get; set; }
    }

    public class Content
    {
        [XmlElement(ElementName = "properties", Namespace = "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata")]
        public Properties Properties { get; set; }

        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }
    }

    public class Properties
    {
        [XmlElement(ElementName = "Opbel", Namespace = "http://schemas.microsoft.com/ado/2007/08/dataservices")]
        public string Opbel { get; set; }

        [XmlElement(ElementName = "Vkont", Namespace = "http://schemas.microsoft.com/ado/2007/08/dataservices")]
        public string Vkont { get; set; }

        [XmlElement(ElementName = "Json", Namespace = "http://schemas.microsoft.com/ado/2007/08/dataservices")]
        public string Json { get; set; }
    }
}
