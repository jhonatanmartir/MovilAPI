namespace AESMovilAPI.DTOs
{
    public class BillDto
    {
        public string Client { get; set; }
        public string Amount { get; set; }
        public DateTime ExpirationDate { get; set; }
        public DateTime IssueDate { get; set; }
        public bool MayoralPayment { get; set; }
        public bool ReconnectionPayment { get; set; }
        public string Company { get; set; }
        public string BP { get; set; }
    }
}
