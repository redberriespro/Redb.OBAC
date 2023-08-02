using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Redb.OBAC.EF.DB;

namespace Redb.OBAC.PgSql
{
    public class DesignTimePgSqlDbContextFactory: IDesignTimeDbContextFactory<PgSqlObacDbContext>
    {
        public PgSqlObacDbContext CreateDbContext(string[] args)
        {
            var cs = Environment.GetEnvironmentVariable("OBAC_CONNECTION_STRING");
            var optionsBuilder = new DbContextOptionsBuilder<ObacDbContext>();
            optionsBuilder.UseNpgsql(cs);
            return new PgSqlObacDbContext(optionsBuilder.Options);
        }
    }
}