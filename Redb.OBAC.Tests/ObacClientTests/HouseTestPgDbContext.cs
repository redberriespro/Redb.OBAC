using Microsoft.EntityFrameworkCore;
using Redb.OBAC.Client;
using Redb.OBAC.Tests.ObacClientTests.Entities;

namespace Redb.OBAC.Tests.ObacClientTests
{
    public class HouseTestPgDbContext: ObacEpContextBase
    {
        protected readonly string ConnectionString;

        public HouseTestPgDbContext(DbContextOptions<HouseTestPgDbContext> options) : base(options)
        {
        }

        public HouseTestPgDbContext()
        {
        }

        public HouseTestPgDbContext(string connectionString)
        {
            ConnectionString = connectionString;
        }
        
        public DbSet<HouseTestEntity> Houses { get; set; }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql(ConnectionString);
            }
        }
    }
}