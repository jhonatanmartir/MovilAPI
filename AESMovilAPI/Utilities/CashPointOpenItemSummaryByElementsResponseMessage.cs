using System.Xml.Serialization;

namespace AESMovilAPI.Utilities
{
    [XmlRoot("CashPointOpenItemSummaryByElementsResponseMessage", Namespace = "http://sap.com/xi/SAPGlobal/Global")]
    public class CashPointOpenItemSummaryByElementsResponseMessage
    {
        [XmlElement("MessageHeader")]
        public MessageHeader? MessageHeader { get; set; }

        [XmlElement("CashPointOpenItemSummary")]
        public CashPointOpenItemSummary? CashPointOpenItemSummary { get; set; }
    }

    public class MessageHeader
    {
        [XmlElement("ID")]
        public string? ID { get; set; }

        [XmlElement("CreationDateTime")]
        public DateTime CreationDateTime { get; set; }

        [XmlElement("SenderParty")]
        public Party? SenderParty { get; set; }

        [XmlElement("RecipientParty")]
        public Party? RecipientParty { get; set; }
    }

    public class Party
    {
        [XmlElement("InternalID")]
        public string? InternalID { get; set; }
    }

    public class CashPointOpenItemSummary
    {
        [XmlElement("PartyReference")]
        public PartyReference? PartyReference { get; set; }

        [XmlElement("CashPointOpenItem")]
        public List<CashPointOpenItem>? CashPointOpenItems { get; set; }
    }

    public class PartyReference
    {
        [XmlElement("InternalID")]
        public string? InternalID { get; set; }
    }

    public class CashPointOpenItem
    {
        [XmlElement("OpenAmount")]
        public Amount? OpenAmount { get; set; }

        [XmlElement("OpenTaxAmount")]
        public Amount? OpenTaxAmount { get; set; }

        [XmlElement("DueDate")]
        public DateTime DueDate { get; set; }

        [XmlElement("OpenItemTransactionDescription")]
        public OpenItemTransactionDescription? OpenItemTransactionDescription { get; set; }

        [XmlElement("ContractAccountDocumentID")]
        public string? ContractAccountDocumentID { get; set; }

        [XmlElement("ContractAccountID")]
        public string? ContractAccountID { get; set; }

        [XmlElement("ContractID")]
        public string? ContractID { get; set; }

        [XmlElement("InvoiceID")]
        public string? InvoiceID { get; set; }

        [XmlElement("PaymentFormID")]
        public string? PaymentFormID { get; set; }

        [XmlElement("ClearingProposalAmount")]
        public Amount? ClearingProposalAmount { get; set; }

        [XmlElement("LatePaymentCharge")]
        public Amount? LatePaymentCharge { get; set; }

        [XmlElement("DiscountAmount")]
        public Amount? DiscountAmount { get; set; }

        [XmlElement("DiscountDueDate")]
        public DateTime DiscountDueDate { get; set; }
    }

    public class Amount
    {
        [XmlAttribute("currencyCode")]
        public string? CurrencyCode { get; set; }

        [XmlText]
        public decimal Value { get; set; }
    }

    public class OpenItemTransactionDescription
    {
        [XmlAttribute("languageCode")]
        public string? LanguageCode { get; set; }

        [XmlText]
        public string? Value { get; set; }
    }
}
