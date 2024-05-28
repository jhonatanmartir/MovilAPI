using System.ComponentModel.DataAnnotations;

namespace AESMovilAPI.DTOs
{
    public class ClaimDto
    {
        [Required]
        public long NC { get; set; }
        [Required]
        public string OrigenReclamo { get; set; }
        public string TipoReclamo { get; set; } = "ZO400";
        [Required]
        public string ComentarioReclamo { get; set; }
        [Required]
        public string ComentarioDireccion { get; set; }
        public string Peligro { get; set; } = "SE001";
        public bool VecinosAfectados { get; set; } = false;
        public string? Empresa { get; set; } = "";
        public string? Departamento { get; set; } = "";
        public string? Municipio { get; set; } = "";
        [Required]
        public string Usuario { get; set; }
        [Required]
        public string Nombre { get; set; }
        [Required]
        public string Telefono { get; set; }
    }
}
