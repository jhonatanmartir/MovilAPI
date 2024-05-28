using System.ComponentModel.DataAnnotations;

namespace AESMovilAPI.DTOs
{
    public class LoginDto
    {
        [Required]
        public string Auth { get; set; }
    }
}
