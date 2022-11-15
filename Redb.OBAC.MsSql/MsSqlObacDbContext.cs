using System;
using Microsoft.EntityFrameworkCore;
using Redb.OBAC.EF.DB;

namespace Redb.OBAC.MsSql
{
    public class MsSqlObacDbContext : ObacDbContext
    {
        public MsSqlObacDbContext():base()
        {
            DbType = DbType.MsSql;
        }

        public MsSqlObacDbContext(string connectionString) : base(connectionString)
        {
            DbType = DbType.MsSql;
        }

        public MsSqlObacDbContext(DbContextOptions<ObacDbContext> options) : base(options)
        {
            DbType = DbType.MsSql;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = string.IsNullOrEmpty(ConnectionString) ?
                    $"Server={Environment.GetEnvironmentVariable("OBAC_HOST") ?? "localhost"},{Environment.GetEnvironmentVariable("OBAC_PORT") ?? "1433"};" +
                    $"Database={Environment.GetEnvironmentVariable("OBAC_DB") ?? "obac"};" +
                    $"User Id={Environment.GetEnvironmentVariable("OBAC_USER") ?? "sa"};" +
                    $"Password={Environment.GetEnvironmentVariable("OBAC_PASSWORD") ?? "12345678"};" :
                    ConnectionString;
                optionsBuilder.UseSqlServer(connectionString);
            }

        }
    }
}
