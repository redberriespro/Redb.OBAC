using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Redb.OBAC.EF.DB;

namespace Redb.OBAC.PgSql
{
    public class PgSqlObacDbContext : ObacDbContext
    {
        public PgSqlObacDbContext(string connectionString) : base(connectionString)
        {
        }

        public PgSqlObacDbContext(DbContextOptions<ObacDbContext> options) : base(options)
        {
            
        }

        // uncomment base(...) when doing migrations  
        public PgSqlObacDbContext() //: base("Host=localhost;Port=5432;Database=obac_test;Username=postgres;Password=12345678")
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = ConnectionString;
                if (connectionString == null)
                {
                    var host = Environment.GetEnvironmentVariable("OBAC_HOST") ?? "localhost";
                    var port = Environment.GetEnvironmentVariable("OBAC_PORT") ?? "5432";
                    var db = Environment.GetEnvironmentVariable("OBAC_DB") ?? "obac";
                    var user = Environment.GetEnvironmentVariable("OBAC_USER") ?? "postgres";
                    var password = Environment.GetEnvironmentVariable("OBAC_PASSWORD") ?? "12345678";
                    connectionString = $"Host={host};Port={port};Database={db};Username={user};Password={password}";
                }
                optionsBuilder.UseNpgsql(connectionString);
            }
            
            if (false)
            {
                optionsBuilder.EnableDetailedErrors();
                optionsBuilder.EnableSensitiveDataLogging(); // show parameter values in sql log
                var lp = LoggerFactory.Create(builder =>
                    {
                        builder.SetMinimumLevel(LogLevel.Debug)
                            //.AddFilter("Microsoft", LogLevel.Debug)
                            // .AddFilter("System", LogLevel.Warning)
                            // .AddFilter("SampleApp.Program", LogLevel.Debug)
                            .AddConsole();
                    }
                );
                
                optionsBuilder.UseLoggerFactory(lp);
            }
        }
    }
}
