using MongoDB.Bson;
using MongoDB.Driver;
using Redb.OBAC.MongoDriver.DB.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Redb.OBAC.MongoDriver.DB
{
    public class ObacMongoDriverConfiguration
    {
        public static async Task ConfigureModel(IMongoDatabase database)
        {

            await database.GetCollection<ObacUserSubjectEntity>("obac_users").Indexes.
                CreateOneAsync(new CreateIndexModel<ObacUserSubjectEntity>(Builders<ObacUserSubjectEntity>.IndexKeys.Ascending(x => x.ExternalIdInt)));

            await database.GetCollection<ObacUserSubjectEntity>("obac_users").Indexes.
               CreateOneAsync(new CreateIndexModel<ObacUserSubjectEntity>(Builders<ObacUserSubjectEntity>.IndexKeys.Ascending(x => x.ExternalIdString)));

            string cmdStr = "{ createIndexes: 'obac_userpermissions', indexes: [ { key: { userid: 1, permid:1,objtypeid:1,objid:1 }, name: 'obac_userpermissions_unique', unique: true } ] }";
            BsonDocument cmd = BsonDocument.Parse(cmdStr);
            await database.RunCommandAsync<BsonDocument>(cmd);

            cmdStr = "{ createIndexes: 'obac_userpermissions', indexes: [ { key: { userid: 1, objtypeid:1,objid:1 }, name: 'obac_userpermissions_3', unique: false } ] }";
            cmd = BsonDocument.Parse(cmdStr);
            await database.RunCommandAsync<BsonDocument>(cmd);

            cmdStr = "{ createIndexes: 'obac_userpermissions', indexes: [ { key: { objtypeid:1,objid:1 }, name: 'obac_userpermissions_2', unique: false } ] }";
            cmd = BsonDocument.Parse(cmdStr);
            await database.RunCommandAsync<BsonDocument>(cmd);

            cmdStr = "{ createIndexes: 'obac_permissions_in_roles', indexes: [ { key: { role_id:1,perm_id:1 }, name: 'obac_obac_permissions_in_roles_unique', unique: true } ] }";
            cmd = BsonDocument.Parse(cmdStr);
            await database.RunCommandAsync<BsonDocument>(cmd);

            // trees
            cmdStr = "{ createIndexes: 'obac_tree_nodes', indexes: [ { key: { tree_id:1,id:1,parent_id:1 }, name: 'obac_tree_nodes_3', unique: false } ] }";
            cmd = BsonDocument.Parse(cmdStr);
            await database.RunCommandAsync<BsonDocument>(cmd);

            cmdStr = "{ createIndexes: 'obac_tree_nodes', indexes: [ { key: { tree_id:1,node_id:1 }, name: 'obac_tree_nodes_unique', unique: true } ] }";
            cmd = BsonDocument.Parse(cmdStr);
            await database.RunCommandAsync<BsonDocument>(cmd);


            await database.GetCollection<ObacTreeNodeEntity>("obac_tree_nodes").Indexes.
            CreateOneAsync(new CreateIndexModel<ObacTreeNodeEntity>(Builders<ObacTreeNodeEntity>.IndexKeys.Ascending(x =>  x.TreeId )));

            cmdStr = "{ createIndexes: 'obac_tree_node_permissions', indexes: [ { key: { user_id:1,user_group_id:1,tree_id:1,tree_node_id:1,perm_id:1 }, name: 'obac_tree_node_permissions_5', unique: true } ] }";
            cmd = BsonDocument.Parse(cmdStr);
            await database.RunCommandAsync<BsonDocument>(cmd);

            cmdStr = "{ createIndexes: 'obac_tree_node_permissions', indexes: [ { key: { tree_id:1,tree_node_id:1 }, name: 'obac_tree_node_permissions_2', unique: false } ] }";
            cmd = BsonDocument.Parse(cmdStr);
            await database.RunCommandAsync<BsonDocument>(cmd);

            cmdStr = "{ createIndexes: 'obac_tree_node_permissions', indexes: [ { key: { user_group_id:1 }, name: 'obac_tree_node_permissions_1', unique: false } ] }";
            cmd = BsonDocument.Parse(cmdStr);
            await database.RunCommandAsync<BsonDocument>(cmd);

            cmdStr = "{ createIndexes: 'obac_tree_node_permissions', indexes: [ { key: { tree_id:1,tree_node_id:1,perm_id:1 }, name: 'obac_tree_node_permissions_3', unique: false } ] }";
            cmd = BsonDocument.Parse(cmdStr);
            await database.RunCommandAsync<BsonDocument>(cmd);


            // userGroups
            //builder.Entity<ObacUserInGroupEntity>().HasKey(x => new { x.UserId, x.GroupId });

            cmdStr = "{ createIndexes: 'obac_users_in_groups', indexes: [ { key: { user_id: 1, group_id:1 }, name: 'obac_users_in_groups_key', unique: true } ] }";
            cmd = BsonDocument.Parse(cmdStr);
            await database.RunCommandAsync<BsonDocument>(cmd);

            await database.GetCollection<ObacUserInGroupEntity>("obac_users_in_groups").Indexes.
          CreateOneAsync(new CreateIndexModel<ObacUserInGroupEntity>(Builders<ObacUserInGroupEntity>.IndexKeys.Ascending(x =>  x.UserId )));

            await database.GetCollection<ObacUserInGroupEntity>("obac_users_in_groups").Indexes.
          CreateOneAsync(new CreateIndexModel<ObacUserInGroupEntity>(Builders<ObacUserInGroupEntity>.IndexKeys.Ascending(x => x.GroupId)));

        }
    }
}
