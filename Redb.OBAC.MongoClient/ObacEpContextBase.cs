using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using Redb.OBAC.MongoDbClient.EffectivePermissionsReceiver;

namespace Redb.OBAC.MongoDbClient
{
    public abstract class ObacEpContextBase : IEffectivePermissionsAware
    {
        private MongoClient _client;
        public IMongoDatabase Database { get; private set; }
        protected ObacEpContextBase()
        {
        }

        protected ObacEpContextBase(string connectionString)
        {
            _client = new MongoClient(connectionString);
            Database = _client.GetDatabase(new MongoUrl(connectionString).DatabaseName);
            EffectivePermissions = Database.GetCollection<ObacEffectivePermissionsEntity>("obac_ep");
            ObacEffectivePermissions.ConfigureModel(Database);
        }



        public IMongoCollection<ObacEffectivePermissionsEntity> EffectivePermissions { get; set; }

        Task IEffectivePermissionsAware.SaveChangesAsync()
        {
            return null;
            //return SaveChangesAsync();
        }

        public async Task DropEffectivePermissions(Guid objectTypeId, Guid objectId)
        {
            await EffectivePermissions.DeleteManyAsync(x=>x.ObjectTypeId==objectTypeId&&x.ObjectId==objectId);

        }
    }
}