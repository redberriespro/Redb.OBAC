using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Redb.OBAC.DB;
using Redb.OBAC.Permissions;

namespace Redb.OBAC.PgSql
{
    public class PgSqlObacStorageProvider: IObacStorageProvider
    {
        private readonly PgSqlObacConfig _config;

        public PgSqlObacStorageProvider(PgSqlObacConfig config)
        {
            if (config?.Connection == null) throw new ArgumentNullException(nameof(config));
            _config = config;
            
        }
        
        public PgSqlObacStorageProvider(string connectionString)
        {
            if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));
            _config = new PgSqlObacConfig { Connection = connectionString};
        }

        public ObacDbContext CreateObacContext() => new PgSqlObacDbContext(_config.Connection);

        public async Task EnsureDatabaseExists()
        {
            var ctx = CreateObacContext();
            if (_config.NoMigrate)
            {
                await ctx.Database.EnsureCreatedAsync();
            }
            else
            {
                await ctx.Database.MigrateAsync();
            }
        }
        
        public IEffectivePermissionStorage CreateDbDefaultEffectivePermissionsStorage()
        {
            throw new System.NotImplementedException();
        }
    }
}