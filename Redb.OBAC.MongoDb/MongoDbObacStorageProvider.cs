using Redb.OBAC.MongoDriver;
using Redb.OBAC.MongoDriver.DB;
using Redb.OBAC.Permissions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Redb.OBAC.MongoDb
{
    public class MongoDbObacStorageProvider : IObacStorageProvider
    {
        private MongoDbObacConfig _config;

        public MongoDbObacStorageProvider(MongoDbObacConfig config)
        {
            if (config?.Connection == null) throw new ArgumentNullException(nameof(config));
            _config = config;
        }

        public MongoDbObacStorageProvider(string connectionString)
        {
            if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));
            _config = new MongoDbObacConfig() { Connection = connectionString };
        }

        public async Task EnsureDatabaseExists()
        {
            var ctx = CreateObacContext();
            await ObacMongoDriverConfiguration.ConfigureModel(ctx.Database);
        }
        public IEffectivePermissionStorage CreateDbDefaultEffectivePermissionsStorage()
        {
            throw new NotImplementedException();
        }

        public ObacMongoDriverContext CreateObacContext() => new MongoDbObacDbContext(_config.Connection);

    }
}
