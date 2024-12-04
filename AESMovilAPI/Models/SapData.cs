using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace AESMovilAPI.Models
{
    [Index(nameof(NisRad))]
    public class SapData
    {
        [Column("NIC")]
        public int Nic { get; set; }
        [Column("NIS_RAD")]
        public int NisRad { get; set; }
        [Column("PARTNER")]
        public string Partner { get; set; }
        [Column("NAME_FIRST")]
        public string NameFirst { get; set; }
        [Column("NAME_LAST")]
        public string NameLast { get; set; }
        [Column("VKONT")]
        public string Vkont { get; set; }
        [Column("VERTRAG")]
        public string Vertrag { get; set; }
        [Column("TARIFTYP")]
        public string Tariftyp { get; set; }
        [Column("TARIFTYP_DESC")]
        public string TariftypDesc { get; set; }
        [Column("ABLEINH")]
        public string Ableinh { get; set; }
        [Column("PORTION")]
        public string Portion { get; set; }
        [Column("SERNR")]
        public string Sernr { get; set; }
        [Column("VSTELLE")]
        public string Vstelle { get; set; }
        [Column("HAUS")]
        public string Haus { get; set; }
        [Column("OPBUK")]
        public string Opbuk { get; set; }
        [Column("ANLAGE")]
        public string Anlage { get; set; }
    }
}
