using Microsoft.EntityFrameworkCore;

namespace AESMovilAPI.Models
{
    [Index(nameof(Nic))]
    [Index(nameof(CuentaContrato))]
    public class Associations
    {
        public long Nic { get; set; }
        public long CuentaContrato { get; set; }
    }
}
