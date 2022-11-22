using MongoDB.Bson;
using MongoDB.Driver;

namespace Redb.OBAC.MongoDbClient.EffectivePermissionsReceiver
{
    public static class ObacEffectivePermissions
    {
        public static void ConfigureModel(IMongoDatabase database)
        {
            string cmdStr = "{ createIndexes: 'obac_ep', indexes: [ { key: { userid: 1, permid:1,objtypeid:1,objid:1 }, name: 'obac_ep_unique', unique: true } ] }";
            BsonDocument cmd = BsonDocument.Parse(cmdStr);
            database.RunCommand<BsonDocument>(cmd);

            cmdStr = "{ createIndexes: 'obac_ep', indexes: [ { key: { userid: 1, objtypeid:1,objid:1 }, name: 'obac_ep_3', unique: false } ] }";
            cmd = BsonDocument.Parse(cmdStr);
            database.RunCommand<BsonDocument>(cmd);


            cmdStr = "{ createIndexes: 'obac_ep', indexes: [ { key: { objtypeid:1,objid:1 }, name: 'obac_ep_2', unique: false } ] }";
            cmd = BsonDocument.Parse(cmdStr);
            database.RunCommand<BsonDocument>(cmd);
        }
    }
}