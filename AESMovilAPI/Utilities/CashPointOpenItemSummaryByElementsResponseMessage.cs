using System.Xml.Serialization;

namespace AESMovilAPI.Utilities
{
    [XmlRoot(ElementName = "CashPointOpenItemSummaryByElementsResponseMessage", Namespace = "http://sap.com/xi/SAPGlobal/Global")]
    public class CashPointOpenItemSummaryByElementsResponseMessage
    {
        [XmlElement(ElementName = "MessageHeader", Namespace = "http://sap.com/xi/SAPGlobal/Global")]
        public MessageHeader MessageHeader { get; set; }

        [XmlElement(ElementName = "CashPointOpenItemSummary", Namespace = "http://sap.com/xi/SAPGlobal/Global")]
        public CashPointOpenItemSummary CashPointOpenItemSummary { get; set; }
    }

    public class MessageHeader
    {
        [XmlElement(ElementName = "ID", Namespace = "http://sap.com/xi/SAPGlobal/Global")]
        public string ID { get; set; }

        [XmlElement(ElementName = "CreationDateTime", Namespace = "http://sap.com/xi/SAPGlobal/Global")]
        public DateTime CreationDateTime { get; set; }

        [XmlElement(ElementName = "SenderParty", Namespace = "http://sap.com/xi/SAPGlobal/Global")]
        public Party SenderParty { get; set; }

        [XmlElement(ElementName = "RecipientParty", Namespace = "http://sap.com/xi/SAPGlobal/Global")]
        public Party RecipientParty { get; set; }
    }

    public class Party
    {
        [XmlElement(ElementName = "InternalID", Namespace = "http://sap.com/xi/SAPGlobal/Global")]
        public string InternalID { get; set; }
    }

    public class CashPointOpenItemSummary
    {
        [XmlElement(ElementName = "PartyReference", Namespace = "http://sap.com/xi/SAPGlobal/Global")]
        public PartyReference PartyReference { get; set; }

        [XmlElement(ElementName = "CashPointOpenItem", Namespace = "http://sap.com/xi/SAPGlobal/Global")]
        public List<CashPointOpenItem> CashPointOpenItems { get; set; }
    }

    public class PartyReference
    {
        [XmlElement(ElementName = "InternalID", Namespace = "http://sap.com/xi/SAPGlobal/Global")]
        public string InternalID { get; set; }
    }

    public class CashPointOpenItem
    {
        [XmlElement(ElementName = "OpenAmount", Namespace = "http://sap.com/xi/SAPGlobal/Global")]
        public OpenAmount OpenAmount { get; set; }

        [XmlElement(ElementName = "OpenTaxAmount", Namespace = "http://sap.com/xi/SAPGlobal/Global")]
        public OpenAmount OpenTaxAmount { get; set; }

        [XmlElement(ElementName = "DueDate", Namespace = "http://sap.com/xi/SAPGlobal/Global")]
        public DateTime DueDate { get; set; }

        [XmlElement(ElementName = "OpenItemTransactionDescription", Namespace = "http://sap.com/xi/SAPGlobal/Global")]
        public OpenItemTransactionDescription OpenItemTransactionDescription { get; set; }

        [XmlElement(ElementName = "ContractAccountDocumentID", Namespace = "http://sap.com/xi/SAPGlobal/Global")]
        public string ContractAccountDocumentID { get; set; }

        [XmlElement(ElementName = "ContractAccountID", Namespace = "http://sap.com/xi/SAPGlobal/Global")]
        public string ContractAccountID { get; set; }

        [XmlElement(ElementName = "ContractID", Namespace = "http://sap.com/xi/SAPGlobal/Global")]
        public string ContractID { get; set; }

        [XmlElement(ElementName = "InvoiceID", Namespace = "http://sap.com/xi/SAPGlobal/Global")]
        public string InvoiceID { get; set; }

        [XmlElement(ElementName = "PaymentFormID", Namespace = "http://sap.com/xi/SAPGlobal/Global")]
        public string PaymentFormID { get; set; }

        [XmlElement(ElementName = "ClearingProposalAmount", Namespace = "http://sap.com/xi/SAPGlobal/Global")]
        public OpenAmount ClearingProposalAmount { get; set; }

        [XmlElement(ElementName = "LatePaymentCharge", Namespace = "http://sap.com/xi/SAPGlobal/Global")]
        public OpenAmount LatePaymentCharge { get; set; }

        [XmlElement(ElementName = "DiscountAmount", Namespace = "http://sap.com/xi/SAPGlobal/Global")]
        public OpenAmount DiscountAmount { get; set; }

        [XmlElement(ElementName = "DiscountDueDate", Namespace = "http://sap.com/xi/SAPGlobal/Global")]
        public DateTime DiscountDueDate { get; set; }
    }

    public class OpenAmount
    {
        [XmlAttribute(AttributeName = "currencyCode")]
        public string CurrencyCode { get; set; }

        [XmlText]
        public decimal Amount { get; set; }
    }

    public class OpenItemTransactionDescription
    {
        [XmlAttribute(AttributeName = "languageCode")]
        public string LanguageCode { get; set; }

        [XmlText]
        public string Description { get; set; }
    }
}
