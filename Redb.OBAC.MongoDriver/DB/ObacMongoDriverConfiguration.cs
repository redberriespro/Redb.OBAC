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


            await database.GetCollection<ObacUserPermissionsEntity>("obac_userpermissions").Indexes.
               CreateOneAsync(new CreateIndexModel<ObacUserPermissionsEntity>(Builders<ObacUserPermissionsEntity>.IndexKeys.Ascending(x => new { x.UserId, x.PermissionId, x.ObjectTypeId, x.ObjectId }))); //IsUnique

            await database.GetCollection<ObacUserPermissionsEntity>("obac_userpermissions").Indexes.
              CreateOneAsync(new CreateIndexModel<ObacUserPermissionsEntity>(Builders<ObacUserPermissionsEntity>.IndexKeys.Ascending(x => new { x.UserId, x.ObjectTypeId, x.ObjectId })));

            await database.GetCollection<ObacUserPermissionsEntity>("obac_userpermissions").Indexes.
             CreateOneAsync(new CreateIndexModel<ObacUserPermissionsEntity>(Builders<ObacUserPermissionsEntity>.IndexKeys.Ascending(x => new { x.ObjectTypeId, x.ObjectId })));



            //await database.GetCollection<ObacPermissionRoleEntity>("obac_permissions_in_roles").Indexes.
            // CreateOneAsync(new CreateIndexModel<ObacPermissionRoleEntity>(Builders<ObacPermissionRoleEntity>.IndexKeys.Ascending(x => new { x.PermissionId, x.RoleId}))); //keys


            //builder.Entity<ObacPermissionRoleEntity>()
            //    .HasKey(a => new { a.PermissionId, a.RoleId });

            //builder.Entity<ObacPermissionRoleEntity>()
            //    .HasOne(p => p.Role)
            //    .WithMany(r => r.Permissions)
            //    .HasForeignKey(p => p.RoleId);


            // trees

            await database.GetCollection<ObacTreeNodeEntity>("obac_tree_nodes").Indexes.
             CreateOneAsync(new CreateIndexModel<ObacTreeNodeEntity>(Builders<ObacTreeNodeEntity>.IndexKeys.Ascending(x => new { x.TreeId, x.Id, x.ParentId })));

            //TODO: add key

            //builder.Entity<ObacTreeNodeEntity>()
            //   .HasKey(p => new { p.TreeId, p.Id });

            await database.GetCollection<ObacTreeNodeEntity>("obac_tree_nodes").Indexes.
            CreateOneAsync(new CreateIndexModel<ObacTreeNodeEntity>(Builders<ObacTreeNodeEntity>.IndexKeys.Ascending(x => new { x.TreeId })));


            //TODO: add FK
            //builder.Entity<ObacTreeNodeEntity>()
            //    .HasOne(p => p.Tree)
            //    .WithMany()
            //    .HasForeignKey(p => p.TreeId);

            //builder.Entity<ObacTreeNodeEntity>()
            //    .HasOne(p => p.Parent)
            //    .WithMany()
            //    .HasForeignKey(p => new { p.TreeId, p.ParentId });

            //builder.Entity<ObacTreeNodeEntity>()
            //    .HasOne(p => p.Owner)
            //    .WithMany()
            //    .HasForeignKey(p => p.OwnerUserId);

            await database.GetCollection<ObacTreeNodePermissionEntity>("obac_tree_node_permissions").Indexes.
           CreateOneAsync(new CreateIndexModel<ObacTreeNodePermissionEntity>(Builders<ObacTreeNodePermissionEntity>.IndexKeys.Ascending(x => new { x.UserId, x.UserGroupId, x.TreeId, x.NodeId, x.PermissionId }))); //IsUnique

            await database.GetCollection<ObacTreeNodePermissionEntity>("obac_tree_node_permissions").Indexes.
           CreateOneAsync(new CreateIndexModel<ObacTreeNodePermissionEntity>(Builders<ObacTreeNodePermissionEntity>.IndexKeys.Ascending(x => new { x.TreeId, x.NodeId })));

            await database.GetCollection<ObacTreeNodePermissionEntity>("obac_tree_node_permissions").Indexes.
          CreateOneAsync(new CreateIndexModel<ObacTreeNodePermissionEntity>(Builders<ObacTreeNodePermissionEntity>.IndexKeys.Ascending(x => new { x.UserGroupId })));

            await database.GetCollection<ObacTreeNodePermissionEntity>("obac_tree_node_permissions").Indexes.
           CreateOneAsync(new CreateIndexModel<ObacTreeNodePermissionEntity>(Builders<ObacTreeNodePermissionEntity>.IndexKeys.Ascending(x => new { x.TreeId, x.NodeId, x.PermissionId })));

            //TODO:Add FK

            //builder.Entity<ObacTreeNodePermissionEntity>()
            //    .HasOne(p => p.User)
            //    .WithMany()
            //    .HasForeignKey(p => p.UserId)
            //    .OnDelete(DeleteBehavior.Cascade);


            //builder.Entity<ObacTreeNodePermissionEntity>()
            //    .HasOne(p => p.UserGroup)
            //    .WithMany()
            //    .HasForeignKey(p => p.UserGroupId)
            //    .OnDelete(DeleteBehavior.Cascade);


            //builder.Entity<ObacTreeNodePermissionEntity>()
            //    .HasOne(p => p.Node)
            //    .WithMany()
            //    .HasForeignKey(p => new { p.TreeId, p.NodeId })
            //    .OnDelete(DeleteBehavior.Cascade);



            // userGroups
            //builder.Entity<ObacUserInGroupEntity>().HasKey(x => new { x.UserId, x.GroupId });

            await database.GetCollection<ObacUserInGroupEntity>("obac_users_in_groups").Indexes.
          CreateOneAsync(new CreateIndexModel<ObacUserInGroupEntity>(Builders<ObacUserInGroupEntity>.IndexKeys.Ascending(x => new { x.UserId })));

            await database.GetCollection<ObacUserInGroupEntity>("obac_users_in_groups").Indexes.
          CreateOneAsync(new CreateIndexModel<ObacUserInGroupEntity>(Builders<ObacUserInGroupEntity>.IndexKeys.Ascending(x => new { x.GroupId })));


            //builder.Entity<ObacUserInGroupEntity>()
            //    .HasOne(pt => pt.User)
            //    .WithMany(p => p.Groups)
            //    .HasForeignKey(pt => pt.UserId)
            //    .OnDelete(DeleteBehavior.Cascade); ;

            //builder.Entity<ObacUserInGroupEntity>()
            //    .HasOne(pt => pt.Group)
            //    .WithMany(p => p.Users)
            //    .HasForeignKey(pt => pt.GroupId)
            //    .OnDelete(DeleteBehavior.Cascade); ;


        }
    }
}
