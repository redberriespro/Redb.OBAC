using Microsoft.EntityFrameworkCore;

namespace Redb.OBAC.Tests.ObacClientTests
{
    public class HouseTestPgDbContext: HouseTestDbContext
    {
        public HouseTestPgDbContext(DbContextOptions<HouseTestPgDbContext> options) 
            : base(options)
        {
        }

        public HouseTestPgDbContext()
        {
        }

        public HouseTestPgDbContext(string connectionString) : base(connectionString)
        {
        }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql(ConnectionString);
            }
        }
    }
}