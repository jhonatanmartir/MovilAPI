using System.ComponentModel.DataAnnotations;

namespace AESMovilAPI.DTOs
{
    /// <summary>
    /// Datos para confirmación.
    /// </summary>
    public class Callback
    {
        /// <summary>
        /// Código de callback.
        /// </summary>
        [Required]
        public string Code { get; set; }
        /// <summary>
        /// Opción seleccionada del cliente.
        /// </summary>
        [Required]
        public int Option { get; set; }
    }
}
