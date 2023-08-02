using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Redb.OBAC.EF.DB;

namespace Redb.OBAC.MySql;

// ReSharper disable once UnusedType.Global
public class DesignTimeMySqlDbContextFactory: IDesignTimeDbContextFactory<MySqlObacDbContext>
{
    public MySqlObacDbContext CreateDbContext(string[] args)
    {
        var cs = Environment.GetEnvironmentVariable("OBAC_CONNECTION_STRING");
        return new MySqlObacDbContext(cs);
    }
}