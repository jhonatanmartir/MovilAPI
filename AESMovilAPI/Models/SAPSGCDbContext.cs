using Microsoft.EntityFrameworkCore;

namespace AESMovilAPI.Models
{
    public class SAPSGCDbContext : DbContext
    {
        public SAPSGCDbContext(DbContextOptions<SAPSGCDbContext> options)
        : base(options)
        {
        }
        public DbSet<SapData> SAPData { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SapData>().ToTable("SAP_DATA", "OSBILLING").HasNoKey();     //No requiere PK
        }
    }
}
