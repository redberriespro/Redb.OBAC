using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Redb.OBAC.Core;
using Redb.OBAC.Core.Ep;
using Redb.OBAC.Core.Hierarchy;
using Redb.OBAC.Core.Models;
using Redb.OBAC.MongoDriver.DB;
using Redb.OBAC.MongoDriver.DB.Entities;
using Redb.OBAC.Exceptions;
using Redb.OBAC.Models;
using Redb.OBAC.Tree;
using MongoDB.Driver;

namespace Redb.OBAC.MongoDriver.BL
{
    public class ObjectStorage : ILazyTreeDataProvider, IEffectivePermissionFeed
    {
        private readonly IObacStorageProvider _storageProvider;

        public ObjectStorage(IObacStorageProvider storageProvider)
        {
            _storageProvider = storageProvider;
        }

        public async Task RemoveAllObjectTypes(bool force)
        {
            await using var ctx = _storageProvider.CreateObacContext();

            var ots = ctx.ObacObjectTypes.DeleteMany(x => true);

            //await ctx.SaveChangesAsync();
        }

        public async Task RemoveAllPermissions(bool force)
        {
            await using var ctx = _storageProvider.CreateObacContext();

            //var ots = await ctx.ObacPermissions.ToListAsync();
            ctx.ObacPermissions.DeleteMany(x => true);
            //await ctx.SaveChangesAsync();
        }

        public async Task RemoveAllUserSubjects(bool force)
        {
            await using var ctx = _storageProvider.CreateObacContext();

            //var ots = await ctx.ObacUserSubjects.ToListAsync();
            ctx.ObacUserSubjects.DeleteMany(x => true);
            //await ctx.SaveChangesAsync();
        }

        public async Task<PermissionInfo> GetPermissionById(Guid permissionId)
        {
            await using var ctx = _storageProvider.CreateObacContext();

            var p = ctx.ObacPermissions.Find(a => a.Id == permissionId).FirstOrDefault();
            return p == null ? null : new PermissionInfo { PermissionId = p.Id, Description = p.Description };
        }

        public async Task AddPermission(Guid permissionId, string description)
        {
            await using var ctx = _storageProvider.CreateObacContext();

            await ctx.ObacPermissions.InsertOneAsync(new ObacPermissionEntity
            {
                Id = permissionId,
                Description = description
            });
            //await ctx.SaveChangesAsync();
        }

        public async Task UpdatePermission(Guid permissionId, string description)
        {
            await using var ctx = _storageProvider.CreateObacContext();

            var p = await ctx.ObacPermissions.UpdateOneAsync(a => a.Id == permissionId, Builders<ObacPermissionEntity>.Update.Set(x => x.Description, description));
            //if (p == null) return;
            //p.Description = description;
            //await ctx.SaveChangesAsync();
        }

        public async Task DeletePermission(Guid permissionId)
        {
            await using var ctx = _storageProvider.CreateObacContext();

            var p = await ctx.ObacPermissions.DeleteOneAsync(a => a.Id == permissionId);
            //if (p == null) return;
            //ctx.ObacPermissions.Remove(p);
            //await ctx.SaveChangesAsync();
        }

        public async Task<RoleInfo> GetRoleById(Guid roleId)
        {
            await using var ctx = _storageProvider.CreateObacContext();

            var p = await ctx.ObacRoles
                .Find(a => a.Id == roleId).FirstOrDefaultAsync();
            return p == null ? null : new RoleInfo()
            {
                RoleId = p.Id,
                Description = p.Description,
                PermissionIds = p.Permissions.Select(p => p.PermissionId).ToArray()
            };
        }

        public async Task AddRole(Guid roleId, string description, Guid[] permissionIds)
        {
            await using var ctx = _storageProvider.CreateObacContext();

            await ctx.ObacRoles.InsertOneAsync(new ObacRoleEntity()
            {
                Id = roleId,
                Description = description,
                Permissions = permissionIds.Select(pid => new ObacPermissionRoleEntity
                {
                    RoleId = roleId,
                    PermissionId = pid
                }).ToList()
            });
            //await ctx.SaveChangesAsync();
        }

        public async Task AddPermissionToRole(Guid roleId, Guid permissionId)
        {
            await using var ctx = _storageProvider.CreateObacContext();

            var p = await ctx.ObacRoles
                .Find(a => a.Id == roleId).FirstOrDefaultAsync();
            if (p == null) return;

            p.Permissions.Add(new ObacPermissionRoleEntity
            {
                PermissionId = permissionId,
                RoleId = roleId
            });

            await ctx.ObacRoles.UpdateOneAsync(a => a.Id == roleId, Builders<ObacRoleEntity>.Update.Set(x => x.Permissions, p.Permissions));
        }

        public async Task UpdateRole(Guid roleId, string description, Guid[] permissionIds)
        {
            await using var ctx = _storageProvider.CreateObacContext();

            var p = await ctx.ObacRoles
                .Find(a => a.Id == roleId).FirstOrDefaultAsync();
            if (p == null) return;
            p.Description = description;
            p.Permissions.Clear();

            foreach (var pid in permissionIds)
            {
                p.Permissions.Add(new ObacPermissionRoleEntity
                {
                    PermissionId = pid,
                    RoleId = roleId
                });
            }

            await ctx.ObacRoles.ReplaceOneAsync(a => a.Id == roleId, p);
        }

        public async Task DeleteRole(Guid roleId)
        {
            await using var ctx = _storageProvider.CreateObacContext();

            var p = await ctx.ObacRoles.DeleteOneAsync(a => a.Id == roleId);
            //if (p == null) return;
            //ctx.ObacRoles.Remove(p);
            //await ctx.SaveChangesAsync();
        }

        public async Task<IEnumerable<RoleInfo>> GetAllRoles()
        {
            await using var ctx = _storageProvider.CreateObacContext();

            return (await ctx.ObacRoles
                .Find(x=>true).ToListAsync()).Select(p => new RoleInfo()
                {
                    RoleId = p.Id,
                    Description = p.Description,
                    PermissionIds = p.Permissions.Select(p => p.PermissionId).ToArray()
                });
        }

        public async Task<IEnumerable<SubjectInfo>> GetAllUserSubjects()
        {
            await using var ctx = _storageProvider.CreateObacContext();
            return (await ctx.ObacUserSubjects.Find(x => true).ToListAsync()).Select(p => new SubjectInfo
            {
                SubjectId = p.Id,
                Description = p.Description,
                ExternalIntId = p.ExternalIdInt,
                ExternalStringId = p.ExternalIdString,
                SubjectType = SubjectTypeEnum.User
            });
        }



        public async Task<SubjectInfo> GetUserSubjectById(int subjectId)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            var p = await ctx.ObacUserSubjects.Find(a => a.Id == subjectId).SingleOrDefaultAsync();
            return p == null
                ? null
                : new SubjectInfo
                {
                    SubjectId = p.Id,
                    SubjectType = SubjectTypeEnum.User,
                    Description = p.Description,
                    ExternalIntId = p.ExternalIdInt,
                    ExternalStringId = p.ExternalIdString
                };
        }

        public async Task<SubjectInfo> GetUserSubjectByExternalIntId(int extenalIntId)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            var cursor = await ctx.ObacUserSubjects.FindAsync(a => a.ExternalIdInt == extenalIntId);
            var p = await cursor.SingleOrDefaultAsync();
            return p == null ? null : new SubjectInfo
            {
                SubjectId = p.Id,
                SubjectType = SubjectTypeEnum.User,
                Description = p.Description,
                ExternalIntId = p.ExternalIdInt,
                ExternalStringId = p.ExternalIdString
            };
        }

        public async Task<SubjectInfo> GetUserSubjectByExternalStringId(string stringId)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            var cursor = await ctx.ObacUserSubjects.FindAsync(a => a.ExternalIdString == stringId);
            var p = await cursor.SingleOrDefaultAsync();
            return p == null
                ? null
                : new SubjectInfo
                {
                    SubjectId = p.Id,
                    SubjectType = SubjectTypeEnum.User,
                    Description = p.Description,
                    ExternalIntId = p.ExternalIdInt,
                    ExternalStringId = p.ExternalIdString
                };
        }

        public async Task AddUserSubject(int subjectId, string description, int? intId, string stringId)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            //await using var transaction = await ctx.Database.BeginTransactionAsync();

            await EnsureNoUserExternalIds(ctx, intId, stringId);

            await ctx.ObacUserSubjects.InsertOneAsync(new ObacUserSubjectEntity()
            {
                Id = subjectId,
                Description = description,
                ExternalIdInt = intId,
                ExternalIdString = stringId
            });
            //await ctx.SaveChangesAsync();
            //await transaction.CommitAsync();
        }
        public async Task<int> AddUserSubject(string description, int? intId, string stringId)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            //await using var transaction = await ctx.Database.BeginTransactionAsync();

            await EnsureNoUserExternalIds(ctx, intId, stringId);

            var userSubjectEntity = new ObacUserSubjectEntity()
            {
                Description = description,
                ExternalIdInt = intId,
                ExternalIdString = stringId
            };
            await ctx.ObacUserSubjects.InsertOneAsync(userSubjectEntity);
            //await ctx.SaveChangesAsync();
            //await transaction.CommitAsync();

            return userSubjectEntity.Id;
        }

        private async Task EnsureNoUserExternalIds(ObacMongoDbContext ctx, int? intId, string stringId)
        {
            if (intId.HasValue)
            {
                var cntInt = await ctx.ObacUserSubjects.CountDocumentsAsync(u => u.ExternalIdInt == intId.Value);
                if (cntInt > 0) throw new ObacException($"User with external int id {intId} already exists");
            }

            if (stringId != null)
            {
                var cntStr = await ctx.ObacUserSubjects.CountDocumentsAsync(u => u.ExternalIdString == stringId);
                if (cntStr > 0) throw new ObacException($"User with external string id {stringId} already exists");
            }
        }


        public async Task UpdateUserSubject(int subjectId, string description)
        {
            await using var ctx = _storageProvider.CreateObacContext();

            await ctx.ObacUserSubjects.
                UpdateOneAsync(a => a.Id == subjectId, Builders<ObacUserSubjectEntity>.Update.Set(x => x.Description, description));
            //if (p == null) return;
            //p.Description = description;
            //await ctx.SaveChangesAsync();
        }

        public async Task DeleteUserSubject(int subjectId)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            await ctx.ObacUserSubjects.DeleteOneAsync(a => a.Id == subjectId);
            //if (p == null) return;
            //ctx.ObacUserSubjects.Remove(p);
            //await ctx.SaveChangesAsync();
        }



        public async Task<SubjectInfo> GetGroupSubjectById(int subjectId)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            var cursor = await ctx.ObacGroupSubjects.FindAsync(a => a.Id == subjectId);
            var p = await cursor.SingleOrDefaultAsync();
            return p == null
                ? null
                : new SubjectInfo
                {
                    SubjectId = p.Id,
                    SubjectType = SubjectTypeEnum.UserGroup,
                    Description = p.Description,
                    ExternalIntId = p.ExternalIdInt,
                    ExternalStringId = p.ExternalIdString
                };
        }

        public async Task<int[]> GetGroupMembers(int userGroupId)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            var p = await ctx.ObacUsersInGroups
                .FindAsync(a => a.GroupId == userGroupId);
            return p.ToList().Select(p => p.UserId).ToArray();
        }

        public async Task AddUserToGroup(int userGroupId, int userId)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            await ctx.ObacUsersInGroups.InsertOneAsync(new ObacUserInGroupEntity
            {
                GroupId = userGroupId,
                UserId = userId
            });
            //await ctx.SaveChangesAsync();
        }

        public async Task DeleteUserFromGroup(int userGroupId, int userId)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            var ug = await ctx.ObacUsersInGroups
                .DeleteOneAsync(a => a.UserId == userId && a.GroupId == userGroupId);
            //if (ug == null) return;
            //ctx.ObacUsersInGroups.Remove(ug);
            //await ctx.SaveChangesAsync();
        }


        private async Task EnsureNoGroupExternalIds(ObacMongoDbContext ctx, int? intId, string stringId)
        {
            if (intId.HasValue)
            {
                var cntInt = await ctx.ObacGroupSubjects.CountDocumentsAsync(u => u.ExternalIdInt == intId.Value);
                if (cntInt > 0) throw new ObacException($"Group with external int id {intId} already exists");
            }

            if (stringId != null)
            {
                var cntStr = await ctx.ObacGroupSubjects.CountDocumentsAsync(u => u.ExternalIdString == stringId);
                if (cntStr > 0) throw new ObacException($"Group with external string id {stringId} already exists");
            }
        }

        public async Task AddGroupSubject(int subjectId, string description, int? intId, string stringId)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            //await using var transaction = await ctx.Database.BeginTransactionAsync();

            await EnsureNoGroupExternalIds(ctx, intId, stringId);

            var groupSubjectEntity = new ObacGroupSubjectEntity
            {
                Id = subjectId,
                Description = description,
                ExternalIdInt = intId,
                ExternalIdString = stringId
            };
            await ctx.ObacGroupSubjects.InsertOneAsync(groupSubjectEntity);
            //await ctx.SaveChangesAsync();
            //await transaction.CommitAsync();
        }

        public async Task UpdateGroupSubject(int subjectId, string description)
        {
            await using var ctx = _storageProvider.CreateObacContext();

            var p = await ctx.ObacGroupSubjects.
                UpdateOneAsync(a => a.Id == subjectId, Builders<ObacGroupSubjectEntity>.Update.Set(x => x.Description, description));
            //if (p == null) return;
            //p.Description = description;
            //await ctx.SaveChangesAsync();
        }

        public async Task DeleteUserGroupSubject(int subjectId)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            var p = await ctx.ObacGroupSubjects.DeleteOneAsync(a => a.Id == subjectId);
            //if (p == null) return;
            //ctx.ObacGroupSubjects.Remove(p);
            //await ctx.SaveChangesAsync();
        }

        public async Task DeleteObjectType(Guid objectTypeId)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            var p = await ctx.ObacObjectTypes.DeleteOneAsync(a => a.Id == objectTypeId);
        }

        public async Task<ObjectTypeInfo> GetObjectTypeById(Guid objectTypeId)
        {
            await using var ctx = _storageProvider.CreateObacContext();

            var cursor = await ctx.ObacObjectTypes.FindAsync(a => a.Id == objectTypeId);
            var p = await cursor.FirstOrDefaultAsync();
            return p == null ? null : new ObjectTypeInfo { ObjectTypeId = p.Id, Description = p.Description, ObjectType = p.Type };
        }

        public async Task AddObjectType(Guid objectTypeId, string description, ObjectTypeEnum objType)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            await ctx.ObacObjectTypes.InsertOneAsync(new ObacObjectTypeEntity
            {
                Id = objectTypeId,
                Description = description,
                Type = objType
            });
            //await ctx.SaveChangesAsync();
        }

        public async Task UpdateObjectType(Guid objectTypeId, string description)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            var p = await ctx.ObacObjectTypes.UpdateOneAsync(a => a.Id == objectTypeId, Builders<ObacObjectTypeEntity>.Update.Set(x => x.Description, description));
            //if (p == null) return;
            //p.Description = description;
            //await ctx.SaveChangesAsync();
        }

        public async Task<IEnumerable<ObjectTypeInfo>> GetAllObjectTypes()
        {
            await using var ctx = _storageProvider.CreateObacContext();
            return (await ctx.ObacObjectTypes.Find(x => true).ToListAsync()).Select(p => new ObjectTypeInfo
            {
                ObjectTypeId = p.Id,
                Description = p.Description,
                ObjectType = p.Type
            });
        }

        public async Task<Guid?> GetUserEffectivePermission(int subjectId, Guid permissionId, Guid objectTypeId, int? objectId)
        {
            await using var ctx = _storageProvider.CreateObacContext();

            var cursor = await ctx.ObacUserPermissions.FindAsync(p =>
                p.UserId == subjectId && p.PermissionId == permissionId && p.ObjectTypeId == objectTypeId &&
                p.ObjectId == objectId);
            var obj = await cursor.SingleOrDefaultAsync();

            return obj?.Id;
        }

        public async Task AddUserEffectivePermission(int subjectId, Guid[] permissionIds, Guid objectTypeId, int? objectId, bool deleteExisting)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            //await using var transaction = await ctx.Database.BeginTransactionAsync();

            if (deleteExisting)
            {
                /*var oldUserPermissions =*/
                ctx.ObacUserPermissions.DeleteMany(p =>
p.UserId == subjectId && p.ObjectTypeId == objectTypeId && p.ObjectId == objectId
);
                //ctx.ObacUserPermissions.RemoveRange(oldUserPermissions);
            }

            foreach (var permissionId in permissionIds)
            {
                await ctx.ObacUserPermissions.InsertOneAsync(new ObacUserPermissionsEntity
                {
                    Id = Guid.NewGuid(),
                    UserId = subjectId,
                    PermissionId = permissionId,
                    ObjectTypeId = objectTypeId,
                    ObjectId = objectId
                });
            }

            //await ctx.SaveChangesAsync();
            //await transaction.CommitAsync();
        }

        public async Task DeleteUserEffectivePermission(int subjectId, Guid permissionId, Guid objectTypeId, int? objectId)
        {
            await using var ctx = _storageProvider.CreateObacContext();

            var obj = await ctx.ObacUserPermissions.DeleteOneAsync(p =>
                p.UserId == subjectId && p.PermissionId == permissionId && p.ObjectTypeId == objectTypeId &&
                p.ObjectId == objectId);

            //if (obj == null) return;
            //ctx.ObacUserPermissions.Remove(obj);
            //await ctx.SaveChangesAsync();
        }

        public async Task<Guid[]> GetEffectivePermissionsForUser(int userId, Guid objectTypeId, int? objectId)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            return (await ctx.ObacUserPermissions
                .Find(p => p.UserId == userId && p.ObjectTypeId == objectTypeId && p.ObjectId == objectId)
                .ToListAsync()).Select(a => a.PermissionId).ToArray();
        }

        public async Task<List<ObacUserPermissionsEntity>> GetEffectivePermissionsForAllUsers(Guid objectTypeId, int? objectId)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            return (await ctx.ObacUserPermissions
                .Find(p => p.ObjectTypeId == objectTypeId && p.ObjectId == objectId)
                .ToListAsync());
        }

        public async Task<IEnumerable<UserPermissionInfo>> GetDirectPermissionsOnObject(Guid objectType, int objectId)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            return (await ctx.ObacUserPermissions
                    .Find(p => p.ObjectId == objectId && p.ObjectTypeId == objectType).ToListAsync())
                .GroupBy(x => x.UserId)
                .Select(x => new UserPermissionInfo { UserId = x.Key, Permissions = x.Select(y => y.PermissionId) })
                .ToArray();
        }

        // tree manipulations

        private async Task TreeEnsureNoExternalIds(ObacMongoDbContext ctx, int? intId, string stringId)
        {
            if (intId.HasValue)
            {
                var cntInt = await ctx.ObacTree.CountDocumentsAsync(u => u.ExternalIdInt == intId.Value);
                if (cntInt > 0) throw new ObacException($"Tree with external int id {intId} already exists");
            }

            if (stringId != null)
            {
                var cntStr = await ctx.ObacTree.CountDocumentsAsync(u => u.ExternalIdString == stringId);
                if (cntStr > 0) throw new ObacException($"Tree with external string id {stringId} already exists");
            }
        }

        public async Task CreateObjectTree(Guid treeId, string description, int? intId, string stringId)
        {
            await using var ctx = _storageProvider.CreateObacContext();

            await TreeEnsureNoExternalIds(ctx, intId, stringId);

            var entity = new ObacTreeEntity
            {
                Id = treeId,
                Description = description,
                ExternalIdInt = intId,
                ExternalIdString = stringId
            };
            await ctx.ObacTree.InsertOneAsync(entity);

            //await ctx.SaveChangesAsync();
        }

        public async Task UpdateObjectTree(Guid treeId, string description, int? intId, string stringId)
        {
            await using var ctx = _storageProvider.CreateObacContext();

            var entity = await ctx.ObacTree.Find(a => a.Id == treeId).SingleAsync();

            entity.Description = description;

            if (entity.ExternalIdInt != intId && intId != null)
            {
                // check no other is using this id
                var cntInt = await ctx
                    .ObacTree
                    .CountDocumentsAsync(u => u.ExternalIdInt == intId.Value && u.Id != treeId);
                if (cntInt > 0) throw new ObacException("external int id already used");
            }
            entity.ExternalIdInt = intId;

            if (entity.ExternalIdString != stringId && stringId != null)
            {
                // check no other is using this id
                var cntInt = await ctx
                    .ObacTree
                    .CountDocumentsAsync(u => u.ExternalIdString == stringId && u.Id != treeId);
                if (cntInt > 0) throw new ObacException("external string id already used");
            }
            entity.ExternalIdString = stringId;
            await ctx.ObacTree.ReplaceOneAsync(a => a.Id == treeId, entity);
            //await ctx.SaveChangesAsync();
        }

        public async Task<TreeObjectTypeInfo> GetObjectTreeById(Guid treeId)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            var res = await ctx.ObacTree.Find(t => t.Id == treeId).FirstOrDefaultAsync();
            return res switch
            {
                null => null,
                _ => new TreeObjectTypeInfo
                {
                    TreeObjectTypeId = treeId,
                    Description = res.Description
                }
            };
        }

        public async Task<TreeNodeInfo> GetTreeNode(Guid treeId, int nodeId)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            var nd = await ctx.ObacTreeNodes.Find(tn => tn.TreeId == treeId && tn.Id == nodeId).SingleOrDefaultAsync();
            return nd switch
            {
                null => null,
                _ => MakeTreeNodeInfo(treeId, nd)
            };
        }

        private static TreeNodeInfo MakeTreeNodeInfo(Guid treeId, ObacTreeNodeEntity nd)
        {
            return new TreeNodeInfo
            {
                TreeObjectTypeId = treeId,
                NodeId = nd.Id,
                ParentNodeId = nd.ParentId,
                InheritParentPermissions = nd.InheritParentPermissions,
                OwnerUserid = nd.OwnerUserId
            };
        }

        public async Task CreateTreeNode(Guid treeId, int nodeId, int? parentId, int ownerUserId)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            await ctx.ObacTreeNodes.InsertOneAsync(new ObacTreeNodeEntity
            {
                TreeId = treeId,
                Id = nodeId,
                ParentId = parentId,
                OwnerUserId = ownerUserId,
                InheritParentPermissions = true
            });
            //await ctx.SaveChangesAsync();
        }

        public async Task<int?> ReplaceTreeNode(Guid treeId, int nodeId, int? newParentNodeId)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            var nd = await ctx.ObacTreeNodes.Find(n => n.TreeId == treeId && n.Id == nodeId).SingleAsync();
            var oldParentId = nd.ParentId;

            if (nd.ParentId == newParentNodeId)
                return oldParentId;

            nd.ParentId = newParentNodeId;
            await ctx.ObacTreeNodes.ReplaceOneAsync(n => n.TreeId == treeId && n.Id == nodeId, nd);
            //ctx.Entry(nd).Property(a => a.ParentId).IsModified = true;
            //await ctx.SaveChangesAsync();
            return oldParentId;
        }

        public async Task<IEnumerable<ObacTreeNodePermissionEntity>> GetTreeNodePermissions(Guid treeId, int treeNodeId)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            return await ctx
                .ObacTreeNodePermissions
                .Find(p => p.TreeId == treeId && p.NodeId == treeNodeId)
                .ToListAsync();
        }

        public async Task<IEnumerable<ObacTreeNodePermissionEntity>> GetTreeNodePermissionsForGroup(int userGroupId)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            return await ctx
                .ObacTreeNodePermissions
                .Find(p => p.UserGroupId == userGroupId)
                .ToListAsync();
        }

        public async Task<IEnumerable<ObacTreeNodePermissionEntity>> GetTreeNodePermissionList(Guid treeId, int[] treeNodeIds, Guid[] permissionIds)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            return await ctx
                .ObacTreeNodePermissions
                .Find(p => p.TreeId == treeId
                            && treeNodeIds.Contains(p.NodeId)
                            && permissionIds.Contains(p.PermissionId)
                                )
                .ToListAsync();
        }

        public async Task SetTreeNodePermissions(Guid treeId,
            int treeNodeId,
            bool inheritParentPermissions,
            TreeNodePermissionInfo[] ptoadd,
            TreeNodePermissionInfo[] ptodel)
        {
            await using var ctx = _storageProvider.CreateObacContext();

            var nd = await ctx.ObacTreeNodes.
                UpdateOneAsync(n => n.TreeId == treeId && n.Id == treeNodeId, Builders<ObacTreeNodeEntity>.Update.Set(x => x.InheritParentPermissions, inheritParentPermissions));
            //nd.InheritParentPermissions = inheritParentPermissions;
            //ctx.Entry(nd).Property(a => a.InheritParentPermissions).IsModified = true;

            foreach (var delItem in ptodel)
            {
                var p = await ctx.ObacTreeNodePermissions.DeleteOneAsync(
                    p => p.TreeId == treeId
                    && p.NodeId == treeNodeId
                    && p.PermissionId == delItem.PermissionId
                    && p.UserId == delItem.UserId
                    && p.UserGroupId == delItem.UserGroupId
                    && p.Deny == delItem.DenyPermission);
                //ctx.ObacTreeNodePermissions.Remove(p);
            }

            foreach (var addItem in ptoadd)
            {
                await ctx.ObacTreeNodePermissions.InsertOneAsync(new ObacTreeNodePermissionEntity
                {
                    Id = Guid.NewGuid(),
                    TreeId = treeId,
                    NodeId = treeNodeId,
                    PermissionId = addItem.PermissionId,
                    UserId = addItem.UserId,
                    UserGroupId = addItem.UserGroupId,
                    Deny = addItem.DenyPermission
                });
            }

            //await ctx.SaveChangesAsync();
        }



        public async Task<IEnumerable<TreeNodeInfo>> GetTreeSubnodesDeep(Guid treeId, int? startingNodeId = null)
        {
            await using var ctx = _storageProvider.CreateObacContext();

            IQueryable<ObacTreeNodeEntity> res=null;

            //if (startingNodeId.HasValue)
            //{
            //    // start from root
            //    var qry = $"with {(ctx.DbType != DbType.MsSql ? "recursive" : "")} nodes(id, parent_id, external_id_int, external_id_str, inherit_parent_perms, owner_user_id) as (" +
            //@"select id, parent_id, external_id_int, external_id_str, inherit_parent_perms, owner_user_id
            //from obac_tree_nodes
            //where parent_id = {0} and tree_id={1}
            //union all
            //select o.id, o.parent_id, o.external_id_int, o.external_id_str, o.inherit_parent_perms, o.owner_user_id
            //    from obac_tree_nodes o
            //join nodes n on n.id = o.parent_id and o.tree_id={1}
            //    )
            //select *, {1} as tree_id
            //    from nodes
            //    order by id desc";

            //    res = ctx.ObacTreeNodes.FromSqlRaw(qry, startingNodeId, treeId);

            //}
            //else
            //{
            //    // start from given node
            //    var qry = $"with {(ctx.DbType != DbType.MsSql ? "recursive" : "")}  nodes(id, parent_id, external_id_int, external_id_str, inherit_parent_perms, owner_user_id) as (" +
            //@"select id, parent_id, external_id_int, external_id_str, inherit_parent_perms, owner_user_id
            //from obac_tree_nodes
            //where parent_id is null and tree_id={0}
            //union all
            //select o.id, o.parent_id, o.external_id_int, o.external_id_str, o.inherit_parent_perms, o.owner_user_id
            //    from obac_tree_nodes o
            //join nodes n on n.id = o.parent_id and o.tree_id={0}
            //    )
            //select *, {0} as tree_id
            //    from nodes
            //    order by id desc";
            //    res = ctx.ObacTreeNodes.FromSqlRaw(qry, treeId);

            //}

            return (await res.ToListAsync()).Select(nd => MakeTreeNodeInfo(treeId, nd));
        }

        public async Task<IEnumerable<TreeNodeInfo>> GetTreeSubnodesShallow(Guid treeId, int? startingNodeId = null)
        {
            await using var ctx = _storageProvider.CreateObacContext();

            IQueryable<ObacTreeNodeEntity> res=null;

            return startingNodeId.HasValue
                    ?(await ctx.ObacTreeNodes.Find(x => x.ParentId == startingNodeId && x.TreeId == treeId).ToListAsync()).
                        OrderByDescending(x => x.Id).Select(x => MakeTreeNodeInfo(treeId, x))
                    :(await ctx.ObacTreeNodes.Find(x => x.ParentId == null && x.TreeId == treeId).ToListAsync()).
                        OrderByDescending(x => x.Id).Select(x => MakeTreeNodeInfo(treeId, x));
            //if (startingNodeId.HasValue)
            //{

            //    //start from given node
            //    var qry = @"select id,  tree_id, parent_id, external_id_int, external_id_str, inherit_parent_perms, owner_user_id
            //from obac_tree_nodes
            //where parent_id = {0} and tree_id={1} order by id desc";

            //    res = ctx.ObacTreeNodes.FromSqlRaw(qry, startingNodeId, treeId);

            //}
            //else
            //{

            //    //start from root
            //   var qry = @"select id, tree_id, parent_id, external_id_int, external_id_str, inherit_parent_perms, owner_user_id
            //from obac_tree_nodes
            //where parent_id is null and tree_id={0} order by id desc";


            //    res = ctx.ObacTreeNodes.FromSqlRaw(qry, treeId);

            //}

            //return (await res.ToListAsync()).Select(nd => MakeTreeNodeInfo(treeId, nd));
        }


        public async Task<long> GetTreeNodeCount(Guid treeId)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            return await ctx.ObacTreeNodes.CountDocumentsAsync(n => n.TreeId == treeId);
        }

        public async Task DeleteObjectTree(Guid treeId)
        {
            /*await using*/
            var ctx = _storageProvider.CreateObacContext();

            var tid = "{" + treeId + "}";

            // await ctx.Database.ExecuteSqlCommandAsync( 
            //     $"DELETE FROM obac_tree_nodes WHERE tree_id = {tid}"); 
            await ctx.Database.GetCollection<ObacTreeNodeEntity>("obac_tree_nodes").
                DeleteManyAsync(x => x.TreeId == treeId);

            //ctx.ObacTreeNodes.RemoveRange(ctx.ObacTreeNodes.Where(n => n.TreeId == treeId));
            ctx.ObacTree.DeleteMany(n => n.Id == treeId);

        }

        private const int EP_BATCH_SZ = 1;

        public async Task FeedWithActionList(IEnumerable<PermissionActionInfo> actions)
        {
            /*await using*/
            var ctx = _storageProvider.CreateObacContext();
            // todo improve
            int n = EP_BATCH_SZ;

            foreach (var a in actions)
            {
                switch (a.Action)
                {
                    case PermissionActionEnum.RemoveAllObjectsDirectPermission:
                        {
                            await ctx.Database.GetCollection<ObacUserPermissionsEntity>("obac_userpermissions").
                                DeleteManyAsync(x => x.ObjectTypeId == a.ObjectTypeId && x.ObjectId == a.ObjectId);

                        }
                        break;

                    case PermissionActionEnum.RemoveDirectPermission:
                        {
                            var p = await ctx
                                .ObacUserPermissions
                                .DeleteOneAsync(
                                    p => p.ObjectTypeId == a.ObjectTypeId
                                        && p.ObjectId == a.ObjectId
                                        && p.PermissionId == a.PermissionId
                                        && p.UserId == a.UserId);
                            //if (p != null)
                            //{
                            //    ctx.ObacUserPermissions.Remove(p);
                            //}
                        }
                        break;
                    case PermissionActionEnum.AddDirectPermission:
                        {
                            await ctx.ObacUserPermissions.InsertOneAsync(new ObacUserPermissionsEntity
                            {
                                Id = new Guid(),
                                PermissionId = a.PermissionId,
                                ObjectTypeId = a.ObjectTypeId,
                                ObjectId = a.ObjectId,
                                UserId = a.UserId
                            });
                        }
                        break;

                    default: throw new ObacException($"missing action branch: {a.Action}");
                }

                n -= 1;
                if (n > 0) continue;

                n = EP_BATCH_SZ;
                //await ctx.SaveChangesAsync();
            }

            //if (n != EP_BATCH_SZ)
            //    await ctx.SaveChangesAsync();
        }


    }
}