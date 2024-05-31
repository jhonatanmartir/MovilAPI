using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AESMovilAPI.Models
{
    [Index(nameof(Nic))]
    [Index(nameof(Partner))]
    [Index(nameof(Vkont))]
    [Table("AssociationsSAP")]
    public class Associations
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int Nic { get; set; }
        public int NisRad { get; set; }
        public long Partner { get; set; }
        public string NameFirst { get; set; }
        public string NameLast { get; set; }
        public long Vkont { get; set; }
        public long Vertrag { get; set; }
        public string Tariftyp { get; set; }
        public string Ableinh { get; set; }
        public string Portion { get; set; }
        public string Sernr { get; set; }
        public string Vstelle { get; set; }
        public string Haus { get; set; }
        public string Opbuk { get; set; }
    }
}
