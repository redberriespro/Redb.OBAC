using Microsoft.EntityFrameworkCore;
using Redb.OBAC.Client;
using Redb.OBAC.Tests.ObacClientTests.Entities;

namespace Redb.OBAC.Tests.ObacClientTests
{
    public abstract class HouseTestDbContext : ObacEpContextBase
    {
        protected readonly string ConnectionString;

        public HouseTestDbContext(DbContextOptions<HouseTestPgDbContext> options) : base(options)
        {
        }

        public HouseTestDbContext()
        {
        }

        public HouseTestDbContext(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public DbSet<HouseTestEntity> Houses { get; set; }

    }
}
