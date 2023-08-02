using System;
using Microsoft.EntityFrameworkCore.Design;

namespace Redb.OBAC.MsSql
{
    // ReSharper disable once UnusedType.Global
    public class DesignTimeMsSqlDbContextFactory: IDesignTimeDbContextFactory<MsSqlObacDbContext>
    {
        public MsSqlObacDbContext CreateDbContext(string[] args)
        {
            var cs = Environment.GetEnvironmentVariable("OBAC_CONNECTION_STRING");
            return new MsSqlObacDbContext(cs);
        }
    }
}