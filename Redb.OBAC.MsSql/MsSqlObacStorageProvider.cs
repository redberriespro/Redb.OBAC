using Microsoft.EntityFrameworkCore;
using Redb.OBAC.EF.DB;
using Redb.OBAC.Permissions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Redb.OBAC.MsSql
{
    public class MsSqlObacStorageProvider : IObacStorageProvider
    {
        private MsSqlObacConfig _config;

        public MsSqlObacStorageProvider(MsSqlObacConfig config)
        {
            if (config?.Connection == null) throw new ArgumentNullException(nameof(config));
            _config = config;
        }

        public MsSqlObacStorageProvider(string connectionString)
        {
            if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));
            _config = new MsSqlObacConfig() { Connection = connectionString };
        }

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
            throw new NotImplementedException();
        }

        public ObacDbContext CreateObacContext() => new MsSqlObacDbContext(_config.Connection);


    }
}
