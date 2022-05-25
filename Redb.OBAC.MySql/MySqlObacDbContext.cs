using Microsoft.EntityFrameworkCore;
using Redb.OBAC.DB;
using System;

namespace Redb.OBAC.MySql
{
    public class MySqlObacDbContext : ObacDbContext
    {
        // uncomment base(...) when doing migrations  
        public MySqlObacDbContext(): base("Host=192.168.2.12;Port=3306;Database=obac_test_user;Username=root;Password=12345678")
        { }
        
        public MySqlObacDbContext(string connectionString) : base(connectionString) { }

        public MySqlObacDbContext(DbContextOptions<ObacDbContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //! uuid var(16)
            //optionsBuilder.UseMySQL(
            //    string.IsNullOrEmpty(ConnectionString)
            //        ? GetDefaultConnectionString()
            //        : ConnectionString

            var connectionString = string.IsNullOrEmpty(ConnectionString)
                    ? GetDefaultConnectionString()
                    : ConnectionString;
            optionsBuilder.UseMySql(
                connectionString
                , ServerVersion.AutoDetect(connectionString)
                );
        }

        private string GetDefaultConnectionString()
        {
            return string.Format(
                "Host={0};Port={1};database={2};user={3};password={4}",
                Environment.GetEnvironmentVariable("OBAC_HOST") ?? "localhost",
                Environment.GetEnvironmentVariable("OBAC_PORT") ?? "3306",
                Environment.GetEnvironmentVariable("OBAC_DB") ?? "obac",
                Environment.GetEnvironmentVariable("OBAC_USER") ?? "myname",
                Environment.GetEnvironmentVariable("OBAC_PASSWORD") ?? "12345678"
                );
        }
    }
}
