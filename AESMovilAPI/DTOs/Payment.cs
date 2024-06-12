using System.ComponentModel.DataAnnotations;

namespace AESMovilAPI.DTOs
{
    /// <summary>
    /// Parametros para crear link de pago.
    /// </summary>
    public class Payment
    {
        /// <summary>
        /// Número de contrato
        /// </summary>
        [Required]
        public long NC { get; set; }

        /// <summary>
        /// Nombre del colector.
        /// </summary>
        /// <value>Pagadito, Payway</value>
        [Required]
        public string Collector { get; set; }

    }
}
