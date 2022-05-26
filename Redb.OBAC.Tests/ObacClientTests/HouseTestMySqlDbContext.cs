using Microsoft.EntityFrameworkCore;

namespace Redb.OBAC.Tests.ObacClientTests
{
    public class HouseTestMySqlDbContext : HouseTestDbContext
    {
        public HouseTestMySqlDbContext(DbContextOptions<HouseTestPgDbContext> options) : base(options) { }

        public HouseTestMySqlDbContext() { }

        public HouseTestMySqlDbContext(string connectionString) : base(connectionString) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMySql(ConnectionString, ServerVersion.AutoDetect(ConnectionString));
            }
        }
    }
}
