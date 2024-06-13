using System.ComponentModel.DataAnnotations;

namespace AESMovilAPI.DTOs
{
    /// <summary>
    /// Consultar datos
    /// </summary>
    public class Query
    {
        /// <summary>
        /// NIC, NPE, o NC a consultar
        /// </summary>
        [Required]
        public string Cuenta { get; set; }
    }
}
