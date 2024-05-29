using Microsoft.EntityFrameworkCore;

namespace AESMovilAPI.Models
{
    public class SAPSGCDbContext : DbContext
    {
        public SAPSGCDbContext(DbContextOptions<SAPSGCDbContext> options)
        : base(options)
        {
        }
        public DbSet<Associations> Associations { get; set; }
    }
}
