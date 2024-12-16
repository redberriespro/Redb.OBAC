using Microsoft.EntityFrameworkCore;
using Redb.OBAC.Client;

namespace HelloObac
{
    public class HelloDbContext: ObacEpContextBase, IHelloDbContext
    {
        public DbSet<DocumentEntity> Documents { get; set; }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql(Program.TEST_CONNECTION);
            }
        }
    }
}