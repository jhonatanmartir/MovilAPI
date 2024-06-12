using System.ComponentModel.DataAnnotations;

namespace AESMovilAPI.DTOs
{
    /// <summary>
    /// Datos para crear reclamo.
    /// </summary>
    public class Claim
    {
        /// <summary>
        /// Número de cuenta.
        /// </summary>
        [Required]
        public long NC { get; set; }

        /// <summary>
        /// Código que determina el medio donde se realizó el reclamo
        /// </summary>
        [Required]
        public string OrigenReclamo { get; set; }

        /// <summary>
        /// Código de la tipologia del reclamo
        /// </summary>
        /// <value>Por defecto <c>ZO400</c></value>
        public string TipoReclamo { get; set; } = "ZO400";

        /// <summary>
        /// Comentario del reclamo.
        /// </summary>
        [Required]
        public string ComentarioReclamo { get; set; }

        /// <summary>
        /// Punto de referencia de la dirección.
        /// </summary>
        [Required]
        public string ComentarioDireccion { get; set; }

        /// <summary>
        /// Código de peligro
        /// </summary>
        /// <value>Por defecto <c>SE001</c></value>
        public string Peligro { get; set; } = "SE001";

        /// <summary>
        /// En caso de haber vecinos afectados.
        /// </summary>
        /// <value><c>true</c> si hay vecinos afectados; de lo contrario, <c>false</c>. Por defecto <c>false</c></value>
        public bool VecinosAfectados { get; set; } = false;

        /// <summary>
        /// Empresa que pertenece.
        /// </summary>
        public string? Empresa { get; set; } = "";

        /// <summary>
        /// Departamento que pertenece.
        /// </summary>
        public string? Departamento { get; set; } = "";

        /// <summary>
        /// Distrito que pertenece.
        /// </summary>
        public string? Municipio { get; set; } = "";

        /// <summary>
        /// Usuario que hace el reclamo.
        /// </summary>
        [Required]
        public string Usuario { get; set; }

        /// <summary>
        /// Nombre del cliente.
        /// </summary>
        [Required]
        public string Nombre { get; set; }

        /// <summary>
        /// Teléfono o Celular del cliente.
        /// </summary>
        [Required]
        public string Telefono { get; set; }
    }
}
