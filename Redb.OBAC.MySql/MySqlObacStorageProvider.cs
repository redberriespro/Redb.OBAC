using Redb.OBAC.EF.DB;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Redb.OBAC.Permissions;

namespace Redb.OBAC.MySql
{
    public class MySqlObacStorageProvider : IObacStorageProvider
    {
        private readonly MySqlObacConfig _config;

        public MySqlObacStorageProvider(MySqlObacConfig config)
        {
            if (config?.Connection == null) throw new ArgumentNullException(nameof(config));
            _config = config;
        }

        public MySqlObacStorageProvider(string connectionString)
        {
            if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));
            _config = new MySqlObacConfig { Connection = connectionString };
        }

        public ObacDbContext CreateObacContext() => new MySqlObacDbContext(_config.Connection);


        public async Task EnsureDatabaseExists()
        {
            var ctx = CreateObacContext();
            if (_config.NoMigrate)
            {
                //await ctx.Database.EnsureCreatedAsync();
                await Task.Run(() => ctx.Database.EnsureCreated());  // https://bugs.mysql.com/bug.php?id=102937
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
