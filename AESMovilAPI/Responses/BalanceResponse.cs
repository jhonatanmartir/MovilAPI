namespace AESMovilAPI.Responses
{
    public class BalanceResponse
    {
        public decimal Amount { get; set; }
        public decimal TotalAmount { get; set; }
        public string ExpirationDate { get; set; }
        public decimal MayoralAmount { get; set; }
        public decimal ReconnectionAmount { get; set; }
        public string DocumentNumber { get; set; }
    }
}
