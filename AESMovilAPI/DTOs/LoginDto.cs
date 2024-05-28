using System.ComponentModel.DataAnnotations;

namespace AESMovilAPI.DTOs
{
    public class LoginDto
    {
        [Required]
        public string Key { get; set; }
    }
}
