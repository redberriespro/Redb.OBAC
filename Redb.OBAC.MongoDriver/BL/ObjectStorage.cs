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
    public class ObjectStorage : IObjectStorage
    {
        private readonly IObacStorageProvider _storageProvider;

        public ObjectStorage(IObacStorageProvider storageProvider)
        {
            _storageProvider = storageProvider;
        }

        public async Task RemoveAllObjectTypes(bool force)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            ctx.ObacObjectTypes.DeleteMany(x => true);
        }

        public async Task RemoveAllPermissions(bool force)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            ctx.ObacPermissions.DeleteMany(x => true);
        }

        public async Task RemoveAllUserSubjects(bool force)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            ctx.ObacUserSubjects.DeleteMany(x => true);
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
        }

        public async Task UpdatePermission(Guid permissionId, string description)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            await ctx.ObacPermissions.UpdateOneAsync(a => a.Id == permissionId, Builders<ObacPermissionEntity>.Update.Set(x => x.Description, description));
        }

        public async Task DeletePermission(Guid permissionId)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            await ctx.ObacPermissions.DeleteOneAsync(a => a.Id == permissionId);
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

            await ctx.ObacRoles.ReplaceOneAsync(a => a.Id == roleId, p);
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
            await ctx.ObacRoles.DeleteOneAsync(a => a.Id == roleId);
        }

        public async Task<IEnumerable<RoleInfo>> GetAllRoles()
        {
            await using var ctx = _storageProvider.CreateObacContext();

            return (await ctx.ObacRoles
                .Find(x => true).ToListAsync()).Select(p => new RoleInfo()
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
            var p = await ctx.ObacUserSubjects.Find(a => a.Id == subjectId).FirstOrDefaultAsync();
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
            var p = await cursor.FirstOrDefaultAsync();
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
            var p = await cursor.FirstOrDefaultAsync();
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
            await EnsureNoUserExternalIds(ctx, intId, stringId);

            await ctx.ObacUserSubjects.InsertOneAsync(new ObacUserSubjectEntity()
            {
                Id = subjectId,
                Description = description,
                ExternalIdInt = intId,
                ExternalIdString = stringId
            });
        }

        public async Task<int> AddUserSubject(string description, int? intId, string stringId)
        {
            await using var ctx = _storageProvider.CreateObacContext();

            await EnsureNoUserExternalIds(ctx, intId, stringId);

            var userSubjectEntity = new ObacUserSubjectEntity()
            {
                Description = description,
                ExternalIdInt = intId,
                ExternalIdString = stringId
            };
            await ctx.ObacUserSubjects.InsertOneAsync(userSubjectEntity);

            return userSubjectEntity.Id;
        }

        private async Task EnsureNoUserExternalIds(ObacMongoDriverContext ctx, int? intId, string stringId)
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
        }

        public async Task DeleteUserSubject(int subjectId)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            await ctx.ObacUserSubjects.DeleteOneAsync(a => a.Id == subjectId);
        }

        public async Task<SubjectInfo> GetGroupSubjectById(int subjectId)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            var cursor = await ctx.ObacGroupSubjects.FindAsync(a => a.Id == subjectId);
            var p = await cursor.FirstOrDefaultAsync();
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
        
        public async Task<SubjectInfo> GetGroupSubjectByExternalIntId(int externalId)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            var cursor = await ctx.ObacGroupSubjects.FindAsync(a => a.ExternalIdInt == externalId);
            var p = await cursor.FirstOrDefaultAsync();
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
        
        public async Task<SubjectInfo> GetGroupSubjectByExternalStringId(string externalId)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            var cursor = await ctx.ObacGroupSubjects.FindAsync(a => a.ExternalIdString == externalId);
            var p = await cursor.FirstOrDefaultAsync();
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
            try
            {
                await ctx.ObacUsersInGroups.ReplaceOneAsync(x => x.GroupId == userGroupId && x.UserId == userId, new ObacUserInGroupEntity
                {
                    GroupId = userGroupId,
                    UserId = userId
                }, new ReplaceOptions { IsUpsert = true });
                //await ctx.ObacUsersInGroups.InsertOneAsync(new ObacUserInGroupEntity
                //{
                //    GroupId = userGroupId,
                //    UserId = userId
                //});
            }
            catch (MongoWriteException) { }

        }

        public async Task DeleteUserFromGroup(int userGroupId, int userId)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            var ug = await ctx.ObacUsersInGroups
                .DeleteOneAsync(a => a.UserId == userId && a.GroupId == userGroupId);
        }


        private async Task EnsureNoGroupExternalIds(ObacMongoDriverContext ctx, int? intId, string stringId)
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
            await EnsureNoGroupExternalIds(ctx, intId, stringId);

            var groupSubjectEntity = new ObacGroupSubjectEntity
            {
                Id = subjectId,
                Description = description,
                ExternalIdInt = intId,
                ExternalIdString = stringId
            };
            await ctx.ObacGroupSubjects.InsertOneAsync(groupSubjectEntity);
        }

        public async Task UpdateGroupSubject(int subjectId, string description)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            await ctx.ObacGroupSubjects.
                UpdateOneAsync(a => a.Id == subjectId, Builders<ObacGroupSubjectEntity>.Update.Set(x => x.Description, description));
        }

        public async Task DeleteUserGroupSubject(int subjectId)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            await ctx.ObacGroupSubjects.DeleteOneAsync(a => a.Id == subjectId);
        }

        public async Task DeleteObjectType(Guid objectTypeId)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            await ctx.ObacObjectTypes.DeleteOneAsync(a => a.Id == objectTypeId);
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
        }

        public async Task UpdateObjectType(Guid objectTypeId, string description)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            await ctx.ObacObjectTypes.
                UpdateOneAsync(a => a.Id == objectTypeId, Builders<ObacObjectTypeEntity>.Update.Set(x => x.Description, description));
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
            var obj = await cursor.FirstOrDefaultAsync();

            return obj?.Id;
        }

        public async Task AddUserEffectivePermission(int subjectId, Guid[] permissionIds, Guid objectTypeId, int? objectId, bool deleteExisting)
        {
            await using var ctx = _storageProvider.CreateObacContext();

            if (deleteExisting)
            {
                ctx.ObacUserPermissions.DeleteMany(p => p.UserId == subjectId
                                                    && p.ObjectTypeId == objectTypeId
                                                    && p.ObjectId == objectId);
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
        }

        public async Task DeleteUserEffectivePermission(int subjectId, Guid permissionId, Guid objectTypeId, int? objectId)
        {
            await using var ctx = _storageProvider.CreateObacContext();

            var obj = await ctx.ObacUserPermissions.DeleteOneAsync(p =>
                p.UserId == subjectId && p.PermissionId == permissionId && p.ObjectTypeId == objectTypeId &&
                p.ObjectId == objectId);
        }

        public async Task<Guid[]> GetEffectivePermissionsForUser(int userId, Guid objectTypeId, int? objectId)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            return (await ctx.ObacUserPermissions
                .Find(p => p.UserId == userId && p.ObjectTypeId == objectTypeId && p.ObjectId == objectId)
                .ToListAsync()).Select(a => a.PermissionId).ToArray();
        }

        public async Task<List<TreeNodePermissionInfo>> GetEffectivePermissionsForAllUsers(Guid objectTypeId, int? objectId)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            return (await ctx.ObacUserPermissions
                .Find(p => p.ObjectTypeId == objectTypeId && p.ObjectId == objectId)
                .ToListAsync()).Where(e => e.ObjectId.HasValue).Select(x =>
                new TreeNodePermissionInfo
                {
                    NodeId = x.ObjectId.Value,
                    DenyPermission = false,
                    PermissionId = x.PermissionId,
                    UserId = x.UserId,
                    UserGroupId = null
                }).ToList();
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

        private async Task TreeEnsureNoExternalIds(ObacMongoDriverContext ctx, int? intId, string stringId)
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
        }

        public async Task UpdateObjectTree(Guid treeId, string description, int? intId, string stringId)
        {
            await using var ctx = _storageProvider.CreateObacContext();

            var entity = await ctx.ObacTree.Find(a => a.Id == treeId).FirstAsync();

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
        
        public async Task<TreeObjectTypeInfo> GetTreeObjectByExternalIntId(int externalId)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            var res = await ctx.ObacTree.Find(t => t.ExternalIdInt == externalId).FirstOrDefaultAsync();
            return res switch
            {
                null => null,
                _ => new TreeObjectTypeInfo
                {
                    TreeObjectTypeId = res.Id,
                    Description = res.Description
                }
            };
        }
        
        public async Task<TreeObjectTypeInfo> GetTreeObjectByExternalStringId(string externalId)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            var res = await ctx.ObacTree.Find(t => t.ExternalIdString == externalId).FirstOrDefaultAsync();
            return res switch
            {
                null => null,
                _ => new TreeObjectTypeInfo
                {
                    TreeObjectTypeId = res.Id,
                    Description = res.Description
                }
            };
        }

        public async Task<TreeNodeInfo> GetTreeNode(Guid treeId, int nodeId)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            var nd = await ctx.ObacTreeNodes.Find(tn => tn.TreeId == treeId && tn.NodeId == nodeId).FirstOrDefaultAsync();
            return nd switch
            {
                null => null,
                _ => MakeTreeNodeInfo(treeId, nd)
            };
        }
        
        public async Task<TreeNodeInfo> GetTreeNodeByExternalIntId(Guid treeId, int externalId)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            var nd = await ctx.ObacTreeNodes.Find(tn => tn.TreeId == treeId && tn.ExternalIdInt == externalId).FirstOrDefaultAsync();
            return nd switch
            {
                null => null,
                _ => MakeTreeNodeInfo(treeId, nd)
            };
        }
        
        public async Task<TreeNodeInfo> GetTreeNodeByExternalStringId(Guid treeId, string externalId)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            var nd = await ctx.ObacTreeNodes.Find(tn => tn.TreeId == treeId && tn.ExternalIdString == externalId).FirstOrDefaultAsync();
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
                NodeId = nd.NodeId,
                ParentNodeId = nd.ParentId,
                InheritParentPermissions = nd.InheritParentPermissions,
                OwnerUserid = nd.OwnerUserId
            };
        }

        public async Task CreateTreeNode(Guid treeId, int nodeId, int? parentId, int ownerUserId)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            var newNode = new ObacTreeNodeEntity
            {
                TreeId = treeId,
                NodeId = nodeId,
                ParentId = parentId,
                OwnerUserId = ownerUserId,
                InheritParentPermissions = true
            };
            try
            {
                await ctx.ObacTreeNodes.ReplaceOneAsync(x => x.NodeId == newNode.NodeId && x.TreeId == newNode.TreeId, newNode, new ReplaceOptions { IsUpsert = true });
                //await ctx.ObacTreeNodes.InsertOneAsync(newNode);
            }
            catch (MongoWriteException) { }

        }

        public async Task<int?> ReplaceTreeNode(Guid treeId, int nodeId, int? newParentNodeId)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            var nd = await ctx.ObacTreeNodes.Find(n => n.TreeId == treeId && n.NodeId == nodeId).FirstAsync();
            var oldParentId = nd.ParentId;

            if (nd.ParentId == newParentNodeId)
                return oldParentId;

            nd.ParentId = newParentNodeId;
            await ctx.ObacTreeNodes.ReplaceOneAsync(n => n.TreeId == treeId && n.NodeId == nodeId, nd);
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
            foreach (var addItem in ptoadd)
            {
                if (ctx.ObacUserSubjects.Find(x => x.Id == addItem.UserId).FirstOrDefault() == null &&
                    ctx.ObacGroupSubjects.Find(x => x.Id == addItem.UserGroupId).FirstOrDefault() == null)
                    throw new ObacException("Error when ACL set");
            }

            var nd = await ctx.ObacTreeNodes.
            UpdateOneAsync(n => n.TreeId == treeId && n.NodeId == treeNodeId, Builders<ObacTreeNodeEntity>.Update.Set(x => x.InheritParentPermissions, inheritParentPermissions));

            foreach (var delItem in ptodel)
            {
                var p = await ctx.ObacTreeNodePermissions.DeleteOneAsync(
                    p => p.TreeId == treeId
                    && p.NodeId == treeNodeId
                    && p.PermissionId == delItem.PermissionId
                    && p.UserId == delItem.UserId
                    && p.UserGroupId == delItem.UserGroupId
                    && p.Deny == delItem.DenyPermission);
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

        }



        public async Task<IEnumerable<TreeNodeInfo>> GetTreeSubnodesDeep(Guid treeId, int? startingNodeId = null)
        {
            await using var ctx = _storageProvider.CreateObacContext();

            List<ObacTreeNodeEntity> root = null;

            if (startingNodeId.HasValue)
            {
                root = ctx.ObacTreeNodes.Find(x => x.NodeId == startingNodeId && x.TreeId == treeId).ToList();

            }
            else
            {
                root = ctx.ObacTreeNodes.Find(x => x.ParentId == null && x.TreeId == treeId).ToList();
            }

            if (root != null)
            {
                var result = new List<ObacTreeNodeEntity>();
                foreach (var item in root)
                {
                    if (root.Count > 1)
                        result.Add(item);
                    var childs = GetTreeSubnodesImpl(ctx, item);
                    if (childs != null)
                        result.AddRange(childs);
                }

                return result.Select(nd => MakeTreeNodeInfo(treeId, nd));
            }
            return null;
        }

        public List<ObacTreeNodeEntity> GetTreeSubnodesImpl(ObacMongoDriverContext ctx, ObacTreeNodeEntity parent)
        {
            var childs = ctx.ObacTreeNodes.Find(x => x.ParentId == parent.NodeId && x.TreeId == parent.TreeId).ToList();
            if (childs.Count != 0)
            {
                var result = new List<ObacTreeNodeEntity>();
                result.AddRange(childs);
                foreach (var child in childs)
                {
                    var tempResult = GetTreeSubnodesImpl(ctx, child);
                    if (tempResult != null)
                        result.AddRange(tempResult);
                }
                return result;
            }
            return null;
        }

        public async Task<IEnumerable<TreeNodeInfo>> GetTreeSubnodesShallow(Guid treeId, int? startingNodeId = null)
        {
            await using var ctx = _storageProvider.CreateObacContext();

            return startingNodeId.HasValue
                    ? (await ctx.ObacTreeNodes.Find(x => x.ParentId == startingNodeId && x.TreeId == treeId).ToListAsync()).
                        OrderByDescending(x => x.NodeId).Select(x => MakeTreeNodeInfo(treeId, x))
                    : (await ctx.ObacTreeNodes.Find(x => x.ParentId == null && x.TreeId == treeId).ToListAsync()).
                        OrderByDescending(x => x.NodeId).Select(x => MakeTreeNodeInfo(treeId, x));
        }


        public async Task<long> GetTreeNodeCount(Guid treeId)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            return await ctx.ObacTreeNodes.CountDocumentsAsync(n => n.TreeId == treeId);
        }

        public async Task DeleteObjectTree(Guid treeId)
        {
            await using var ctx = _storageProvider.CreateObacContext();

            var tid = "{" + treeId + "}";

            await ctx.ObacTreeNodes.
                DeleteManyAsync(x => x.TreeId == treeId);

            await ctx.ObacTree.DeleteManyAsync(n => n.Id == treeId);

        }

        private const int EP_BATCH_SZ = 1;

        public async Task FeedWithActionList(IEnumerable<PermissionActionInfo> actions)
        {
            await using var ctx = _storageProvider.CreateObacContext();

            int n = EP_BATCH_SZ;

            foreach (var a in actions)
            {
                switch (a.Action)
                {
                    case PermissionActionEnum.RemoveAllObjectsDirectPermission:
                        {
                            await ctx.ObacUserPermissions.
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
            }
        }
    }
}