using System.ComponentModel.DataAnnotations;

namespace AESMovilAPI.DTOs
{
    public class Login
    {
        [Required]
        public string Auth { get; set; }
    }
}
