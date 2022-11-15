using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redb.OBAC.Tests.ObacClientTests
{
    public class HouseTestMsSqlDbContext:HouseTestDbContext
    {
        public HouseTestMsSqlDbContext(DbContextOptions<HouseTestPgDbContext> options) : base(options) { }

        public HouseTestMsSqlDbContext() { }

        public HouseTestMsSqlDbContext(string connectionString) : base(connectionString) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(ConnectionString);
            }
        }
    }
}
