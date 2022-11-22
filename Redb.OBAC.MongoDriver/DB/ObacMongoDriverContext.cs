using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Driver.Core;
using Redb.OBAC.MongoDriver.DB.Entities;

namespace Redb.OBAC.MongoDriver.DB
{
    public abstract class ObacMongoDriverContext : IAsyncDisposable
    {
        private MongoClient _client;
        private string _dbName;

        public IMongoCollection<ObacPermissionEntity> ObacPermissions { get; set; }
        public IMongoCollection<ObacRoleEntity> ObacRoles { get; set; }
        public IMongoCollection<ObacObjectTypeEntity> ObacObjectTypes { get; set; }
        public IMongoCollection<ObacUserSubjectEntity> ObacUserSubjects { get; set; }
        public IMongoCollection<ObacGroupSubjectEntity> ObacGroupSubjects { get; set; }
        public IMongoCollection<ObacUserInGroupEntity> ObacUsersInGroups { get; set; }

        public IMongoCollection<ObacTreeEntity> ObacTree { get; set; }
        public IMongoCollection<ObacTreeNodeEntity> ObacTreeNodes { get; set; }
        public IMongoCollection<ObacTreeNodePermissionEntity> ObacTreeNodePermissions { get; set; }

        public IMongoCollection<ObacUserPermissionsEntity> ObacUserPermissions { get; set; }
        public IMongoDatabase Database { get; private set; }

        public MongoClient Client => _client;

        public ObacMongoDriverContext()
        {
            _client = new MongoClient();
        }

        public ObacMongoDriverContext(string connectionString)
        {
            _client = new MongoClient(connectionString);
            _dbName = new MongoUrl(connectionString).DatabaseName;
            Initialize();
        }

        public ObacMongoDriverContext(string connectionString,string dbName)
        {
            _client = new MongoClient(connectionString);
            _dbName = dbName;
            Initialize();
        }

        public void Initialize()
        {
            Database = _client.GetDatabase(_dbName);
            ObacPermissions = Database.GetCollection<ObacPermissionEntity>("obac_permissions");
            ObacRoles = Database.GetCollection<ObacRoleEntity>("obac_roles");
            ObacObjectTypes = Database.GetCollection<ObacObjectTypeEntity>("obac_objecttypes");
            ObacUserSubjects = Database.GetCollection<ObacUserSubjectEntity>("obac_users");
            ObacGroupSubjects = Database.GetCollection<ObacGroupSubjectEntity>("obac_user_groups");
            ObacUsersInGroups = Database.GetCollection<ObacUserInGroupEntity>("obac_users_in_groups");
            ObacTree = Database.GetCollection<ObacTreeEntity>("obac_trees");
            ObacTreeNodes = Database.GetCollection<ObacTreeNodeEntity>("obac_tree_nodes");
            ObacTreeNodePermissions = Database.GetCollection<ObacTreeNodePermissionEntity>("obac_tree_node_permissions");
            ObacUserPermissions = Database.GetCollection<ObacUserPermissionsEntity>("obac_userpermissions");
        }

        public async ValueTask DisposeAsync()
        {
            GC.SuppressFinalize(this);
        }
    }

}
