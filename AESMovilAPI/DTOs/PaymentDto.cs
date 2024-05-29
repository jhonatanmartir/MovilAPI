using System.ComponentModel.DataAnnotations;

namespace AESMovilAPI.DTOs
{
    public class PaymentDto
    {
        [Required]
        public long NC { get; set; }
        [Required]
        public string Collector { get; set; }

    }
}
