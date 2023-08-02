using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Redb.OBAC.Core;
using Redb.OBAC.Core.Ep;
using Redb.OBAC.Core.Hierarchy;
using Redb.OBAC.Core.Models;
using Redb.OBAC.EF.DB;
using Redb.OBAC.EF.DB.Entities;
using Redb.OBAC.Exceptions;
using Redb.OBAC.Models;
using Redb.OBAC.Tree;

namespace Redb.OBAC.EF.BL
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

            var ots = await ctx.ObacObjectTypes.ToListAsync();
            ctx.ObacObjectTypes.RemoveRange(ots);
            await ctx.SaveChangesAsync();
        }

        public async Task RemoveAllPermissions(bool force)
        {
            await using var ctx = _storageProvider.CreateObacContext();

            var ots = await ctx.ObacPermissions.ToListAsync();
            ctx.ObacPermissions.RemoveRange(ots);
            await ctx.SaveChangesAsync();
        }

        public async Task RemoveAllUserSubjects(bool force)
        {
            await using var ctx = _storageProvider.CreateObacContext();

            var ots = await ctx.ObacUserSubjects.ToListAsync();
            ctx.ObacUserSubjects.RemoveRange(ots);
            await ctx.SaveChangesAsync();
        }

        public async Task<IReadOnlyCollection<PermissionInfo>> ListPermissions()
        {
            await using var ctx = _storageProvider.CreateObacContext();

            return  (await ctx.ObacPermissions.ToListAsync())
                .Select(p => new PermissionInfo { PermissionId = p.Id, Description = p.Description })
                .ToList();
        }
        
        public async Task<PermissionInfo> GetPermissionById(Guid permissionId)
        {
            await using var ctx = _storageProvider.CreateObacContext();

            var p = await ctx.ObacPermissions.FirstOrDefaultAsync(a => a.Id == permissionId);
            return p == null ? null : new PermissionInfo { PermissionId = p.Id, Description = p.Description };
        }

        public async Task AddPermission(Guid permissionId, string description)
        {
            await using var ctx = _storageProvider.CreateObacContext();

            await ctx.ObacPermissions.AddAsync(new ObacPermissionEntity
            {
                Id = permissionId,
                Description = description
            });
            await ctx.SaveChangesAsync();
        }

        public async Task UpdatePermission(Guid permissionId, string description)
        {
            await using var ctx = _storageProvider.CreateObacContext();

            var p = await ctx.ObacPermissions.FirstOrDefaultAsync(a => a.Id == permissionId);
            if (p == null) return;
            p.Description = description;
            await ctx.SaveChangesAsync();
        }

        public async Task DeletePermission(Guid permissionId)
        {
            await using var ctx = _storageProvider.CreateObacContext();

            var p = await ctx.ObacPermissions.FirstOrDefaultAsync(a => a.Id == permissionId);
            if (p == null) return;
            ctx.ObacPermissions.Remove(p);
            await ctx.SaveChangesAsync();
        }

        public async Task<RoleInfo> GetRoleById(Guid roleId)
        {
            await using var ctx = _storageProvider.CreateObacContext();

            var p = await ctx.ObacRoles
                .Include(r => r.Permissions)
                .FirstOrDefaultAsync(a => a.Id == roleId);
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

            await ctx.ObacRoles.AddAsync(new ObacRoleEntity()
            {
                Id = roleId,
                Description = description,
                Permissions = permissionIds.Select(pid => new ObacPermissionRoleEntity
                {
                    RoleId = roleId,
                    PermissionId = pid
                }).ToList()
            });
            await ctx.SaveChangesAsync();
        }

        public async Task AddPermissionToRole(Guid roleId, Guid permissionId)
        {
            await using var ctx = _storageProvider.CreateObacContext();

            var p = await ctx.ObacRoles
                .Include(r => r.Permissions)
                .FirstOrDefaultAsync(a => a.Id == roleId);
            if (p == null) return;

            p.Permissions.Add(new ObacPermissionRoleEntity
            {
                PermissionId = permissionId,
                RoleId = roleId
            });

            await ctx.SaveChangesAsync();
        }

        public async Task UpdateRole(Guid roleId, string description, Guid[] permissionIds)
        {
            await using var ctx = _storageProvider.CreateObacContext();

            var p = await ctx.ObacRoles
                .Include(r => r.Permissions)
                .FirstOrDefaultAsync(a => a.Id == roleId);
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

            await ctx.SaveChangesAsync();
        }

        public async Task DeleteRole(Guid roleId)
        {
            await using var ctx = _storageProvider.CreateObacContext();

            var p = await ctx.ObacRoles.FirstOrDefaultAsync(a => a.Id == roleId);
            if (p == null) return;
            ctx.ObacRoles.Remove(p);
            await ctx.SaveChangesAsync();
        }

        public async Task<IEnumerable<RoleInfo>> GetAllRoles()
        {
            await using var ctx = _storageProvider.CreateObacContext();

            return (await ctx.ObacRoles
                .Include(r => r.Permissions)
                .ToListAsync()).Select(p => new RoleInfo()
                {
                    RoleId = p.Id,
                    Description = p.Description,
                    PermissionIds = p.Permissions.Select(p => p.PermissionId).ToArray()
                });
        }

        public async Task<IEnumerable<SubjectInfo>> GetAllUserSubjects()
        {
            await using var ctx = _storageProvider.CreateObacContext();
            return (await ctx.ObacUserSubjects.ToListAsync()).Select(p => new SubjectInfo
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
            var p = await ctx.ObacUserSubjects.SingleOrDefaultAsync(a => a.Id == subjectId);
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
            var p = await ctx.ObacUserSubjects.SingleOrDefaultAsync(a => a.ExternalIdInt == extenalIntId);
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
            var p = await ctx.ObacUserSubjects.SingleOrDefaultAsync(a => a.ExternalIdString == stringId);
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
            await using var transaction = await ctx.Database.BeginTransactionAsync();

            await EnsureNoUserExternalIds(ctx, intId, stringId);

            await ctx.ObacUserSubjects.AddAsync(new ObacUserSubjectEntity()
            {
                Id = subjectId,
                Description = description,
                ExternalIdInt = intId,
                ExternalIdString = stringId
            });
            await ctx.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        public async Task<int> AddUserSubject(string description, int? intId, string stringId)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            await using var transaction = await ctx.Database.BeginTransactionAsync();

            await EnsureNoUserExternalIds(ctx, intId, stringId);

            var userSubjectEntity = new ObacUserSubjectEntity()
            {
                Description = description,
                ExternalIdInt = intId,
                ExternalIdString = stringId
            };
            await ctx.ObacUserSubjects.AddAsync(userSubjectEntity);
            await ctx.SaveChangesAsync();
            await transaction.CommitAsync();

            return userSubjectEntity.Id;
        }

        private async Task EnsureNoUserExternalIds(ObacDbContext ctx, int? intId, string stringId)
        {
            if (intId.HasValue)
            {
                var cntInt = await ctx.ObacUserSubjects.CountAsync(u => u.ExternalIdInt == intId.Value);
                if (cntInt > 0) throw new ObacException($"User with external int id {intId} already exists");
            }

            if (stringId != null)
            {
                var cntStr = await ctx.ObacUserSubjects.CountAsync(u => u.ExternalIdString == stringId);
                if (cntStr > 0) throw new ObacException($"User with external string id {stringId} already exists");
            }
        }


        public async Task UpdateUserSubject(int subjectId, string description)
        {
            await using var ctx = _storageProvider.CreateObacContext();

            var p = await ctx.ObacUserSubjects.FirstOrDefaultAsync(a => a.Id == subjectId);
            if (p == null) return;
            p.Description = description;
            await ctx.SaveChangesAsync();
        }

        public async Task DeleteUserSubject(int subjectId)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            var p = await ctx.ObacUserSubjects.FirstOrDefaultAsync(a => a.Id == subjectId);
            if (p == null) return;
            ctx.ObacUserSubjects.Remove(p);
            await ctx.SaveChangesAsync();
        }



        public async Task<SubjectInfo> GetGroupSubjectById(int subjectId)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            var p = await ctx.ObacGroupSubjects.SingleOrDefaultAsync(a => a.Id == subjectId);
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
            var p = await ctx.ObacGroupSubjects.SingleOrDefaultAsync(a => a.ExternalIdInt == externalId);
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
        
        public async Task<SubjectInfo> GetGroupSubjectByExternalStringId(string stringId)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            var p = await ctx.ObacGroupSubjects.SingleOrDefaultAsync(a => a.ExternalIdString == stringId);
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
                .Where(a => a.GroupId == userGroupId)
                .ToListAsync();
            return p.Select(p => p.UserId).ToArray();
        }

        public async Task AddUserToGroup(int userGroupId, int userId)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            await ctx.ObacUsersInGroups.AddAsync(new ObacUserInGroupEntity
            {
                GroupId = userGroupId,
                UserId = userId
            });
            await ctx.SaveChangesAsync();
        }

        public async Task DeleteUserFromGroup(int userGroupId, int userId)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            var ug = await ctx.ObacUsersInGroups
                .SingleOrDefaultAsync(a => a.UserId == userId && a.GroupId == userGroupId);
            if (ug == null) return;
            ctx.ObacUsersInGroups.Remove(ug);
            await ctx.SaveChangesAsync();
        }


        private async Task EnsureNoGroupExternalIds(ObacDbContext ctx, int? intId, string stringId)
        {
            if (intId.HasValue)
            {
                var cntInt = await ctx.ObacGroupSubjects.CountAsync(u => u.ExternalIdInt == intId.Value);
                if (cntInt > 0) throw new ObacException($"Group with external int id {intId} already exists");
            }

            if (stringId != null)
            {
                var cntStr = await ctx.ObacGroupSubjects.CountAsync(u => u.ExternalIdString == stringId);
                if (cntStr > 0) throw new ObacException($"Group with external string id {stringId} already exists");
            }
        }

        public async Task AddGroupSubject(int subjectId, string description, int? intId, string stringId)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            await using var transaction = await ctx.Database.BeginTransactionAsync();

            await EnsureNoGroupExternalIds(ctx, intId, stringId);

            var groupSubjectEntity = new ObacGroupSubjectEntity
            {
                Id = subjectId,
                Description = description,
                ExternalIdInt = intId,
                ExternalIdString = stringId
            };
            await ctx.ObacGroupSubjects.AddAsync(groupSubjectEntity);
            await ctx.SaveChangesAsync();
            await transaction.CommitAsync();
        }

        public async Task UpdateGroupSubject(int subjectId, string description)
        {
            await using var ctx = _storageProvider.CreateObacContext();

            var p = await ctx.ObacGroupSubjects.FirstOrDefaultAsync(a => a.Id == subjectId);
            if (p == null) return;
            p.Description = description;
            await ctx.SaveChangesAsync();
        }

        public async Task DeleteUserGroupSubject(int subjectId)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            var p = await ctx.ObacGroupSubjects.FirstOrDefaultAsync(a => a.Id == subjectId);
            if (p == null) return;
            ctx.ObacGroupSubjects.Remove(p);
            await ctx.SaveChangesAsync();
        }

        public async Task DeleteObjectType(Guid objectTypeId)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            var p = await ctx.ObacObjectTypes.FirstOrDefaultAsync(a => a.Id == objectTypeId);
            if (p == null) return;
            ctx.ObacObjectTypes.Remove(p);
            await ctx.SaveChangesAsync();
        }

        public async Task<ObjectTypeInfo> GetObjectTypeById(Guid objectTypeId)
        {
            await using var ctx = _storageProvider.CreateObacContext();

            var p = await ctx.ObacObjectTypes.FirstOrDefaultAsync(a => a.Id == objectTypeId);
            return p == null ? null : new ObjectTypeInfo { ObjectTypeId = p.Id, Description = p.Description, ObjectType = p.Type };
        }

        public async Task AddObjectType(Guid objectTypeId, string description, ObjectTypeEnum objType)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            await ctx.ObacObjectTypes.AddAsync(new ObacObjectTypeEntity
            {
                Id = objectTypeId,
                Description = description,
                Type = objType
            });
            await ctx.SaveChangesAsync();
        }

        public async Task UpdateObjectType(Guid objectTypeId, string description)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            var p = await ctx.ObacObjectTypes.FirstOrDefaultAsync(a => a.Id == objectTypeId);
            if (p == null) return;
            p.Description = description;
            await ctx.SaveChangesAsync();
        }

        public async Task<IEnumerable<ObjectTypeInfo>> GetAllObjectTypes()
        {
            await using var ctx = _storageProvider.CreateObacContext();
            return (await ctx.ObacObjectTypes.ToListAsync()).Select(p => new ObjectTypeInfo
            {
                ObjectTypeId = p.Id,
                Description = p.Description,
                ObjectType = p.Type
            });
        }

        public async Task<Guid?> GetUserEffectivePermission(int subjectId, Guid permissionId, Guid objectTypeId, int? objectId)
        {
            await using var ctx = _storageProvider.CreateObacContext();

            var obj = await ctx.ObacUserPermissions.FirstOrDefaultAsync(p =>
                p.UserId == subjectId && p.PermissionId == permissionId && p.ObjectTypeId == objectTypeId &&
                p.ObjectId == objectId);

            return obj?.Id;
        }

        public async Task AddUserEffectivePermission(int subjectId, Guid[] permissionIds, Guid objectTypeId, int? objectId, bool deleteExisting)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            await using var transaction = await ctx.Database.BeginTransactionAsync();

            if (deleteExisting)
            {
                var oldUserPermissions = ctx.ObacUserPermissions.Where(p =>
                    p.UserId == subjectId && p.ObjectTypeId == objectTypeId && p.ObjectId == objectId
                );
                ctx.ObacUserPermissions.RemoveRange(oldUserPermissions);
            }

            foreach (var permissionId in permissionIds)
            {
                await ctx.ObacUserPermissions.AddAsync(new ObacUserPermissionsEntity
                {
                    Id = Guid.NewGuid(),
                    UserId = subjectId,
                    PermissionId = permissionId,
                    ObjectTypeId = objectTypeId,
                    ObjectId = objectId
                });
            }

            await ctx.SaveChangesAsync();
            await transaction.CommitAsync();
        }

        public async Task DeleteUserEffectivePermission(int subjectId, Guid permissionId, Guid objectTypeId, int? objectId)
        {
            await using var ctx = _storageProvider.CreateObacContext();

            var obj = await ctx.ObacUserPermissions.FirstOrDefaultAsync(p =>
                p.UserId == subjectId && p.PermissionId == permissionId && p.ObjectTypeId == objectTypeId &&
                p.ObjectId == objectId);

            if (obj == null) return;
            ctx.ObacUserPermissions.Remove(obj);
            await ctx.SaveChangesAsync();
        }

        public async Task<Guid[]> GetEffectivePermissionsForUser(int userId, Guid objectTypeId, int? objectId)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            return (await ctx.ObacUserPermissions
                .Where(p => p.UserId == userId && p.ObjectTypeId == objectTypeId && p.ObjectId == objectId)
                .ToListAsync()).Select(a => a.PermissionId).ToArray();
        }

        public async Task<List<TreeNodePermissionInfo>> GetEffectivePermissionsForAllUsers(Guid objectTypeId, int? objectId)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            return (await ctx.ObacUserPermissions
                .Where(p => p.ObjectTypeId == objectTypeId && p.ObjectId == objectId)
                .ToListAsync()).Where(e => e.ObjectId.HasValue).Select(x=> 
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
                    .Where(p => p.ObjectId == objectId && p.ObjectTypeId == objectType).ToListAsync())
                .GroupBy(x => x.UserId)
                .Select(x => new UserPermissionInfo { UserId = x.Key, Permissions = x.Select(y => y.PermissionId) })
                .ToArray();
        }

        // tree manipulations

        private async Task TreeEnsureNoExternalIds(ObacDbContext ctx, int? intId, string stringId)
        {
            if (intId.HasValue)
            {
                var cntInt = await ctx.ObacTree.CountAsync(u => u.ExternalIdInt == intId.Value);
                if (cntInt > 0) throw new ObacException($"Tree with external int id {intId} already exists");
            }

            if (stringId != null)
            {
                var cntStr = await ctx.ObacTree.CountAsync(u => u.ExternalIdString == stringId);
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
            await ctx.ObacTree.AddAsync(entity);

            await ctx.SaveChangesAsync();
        }

        public async Task UpdateObjectTree(Guid treeId, string description, int? intId, string stringId)
        {
            await using var ctx = _storageProvider.CreateObacContext();

            var entity = await ctx.ObacTree.SingleAsync(a => a.Id == treeId);

            entity.Description = description;

            if (entity.ExternalIdInt != intId && intId != null)
            {
                // check no other is using this id
                var cntInt = await ctx
                    .ObacTree
                    .CountAsync(u => u.ExternalIdInt == intId.Value && u.Id != treeId);
                if (cntInt > 0) throw new ObacException("external int id already used");
            }
            entity.ExternalIdInt = intId;

            if (entity.ExternalIdString != stringId && stringId != null)
            {
                // check no other is using this id
                var cntInt = await ctx
                    .ObacTree
                    .CountAsync(u => u.ExternalIdString == stringId && u.Id != treeId);
                if (cntInt > 0) throw new ObacException("external string id already used");
            }
            entity.ExternalIdString = stringId;

            await ctx.SaveChangesAsync();
        }

        public async Task<TreeObjectTypeInfo> GetObjectTreeById(Guid treeId)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            var res = await ctx.ObacTree.FirstOrDefaultAsync(t => t.Id == treeId);
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
            var res = await ctx.ObacTree.FirstOrDefaultAsync(t => t.ExternalIdInt == externalId);
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
            var res = await ctx.ObacTree.FirstOrDefaultAsync(t => t.ExternalIdString == externalId);
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
            var nd = await ctx.ObacTreeNodes.SingleOrDefaultAsync(tn => tn.TreeId == treeId && tn.Id == nodeId);
            return nd switch
            {
                null => null,
                _ => MakeTreeNodeInfo(treeId, nd)
            };
        }
        
        public async Task<TreeNodeInfo> GetTreeNodeByExternalIntId(Guid treeId, int externalId)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            var nd = await ctx.ObacTreeNodes.SingleOrDefaultAsync(tn => tn.TreeId == treeId && tn.ExternalIdInt == externalId);
            return nd switch
            {
                null => null,
                _ => MakeTreeNodeInfo(treeId, nd)
            };
        }
        
        public async Task<TreeNodeInfo> GetTreeNodeByExternalStringId(Guid treeId, string externalId)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            var nd = await ctx.ObacTreeNodes.SingleOrDefaultAsync(tn => tn.TreeId == treeId && tn.ExternalIdString == externalId);
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
            await ctx.ObacTreeNodes.AddAsync(new ObacTreeNodeEntity
            {
                TreeId = treeId,
                Id = nodeId,
                ParentId = parentId,
                OwnerUserId = ownerUserId,
                InheritParentPermissions = true
            });
            await ctx.SaveChangesAsync();
        }

        public async Task<int?> ReplaceTreeNode(Guid treeId, int nodeId, int? newParentNodeId)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            var nd = await ctx.ObacTreeNodes.SingleAsync(n => n.TreeId == treeId && n.Id == nodeId);
            var oldParentId = nd.ParentId;

            if (nd.ParentId == newParentNodeId)
                return oldParentId;
            nd.ParentId = newParentNodeId;
            ctx.Entry(nd).Property(a => a.ParentId).IsModified = true;
            await ctx.SaveChangesAsync();
            return oldParentId;
        }

        public async Task<IEnumerable<ObacTreeNodePermissionEntity>> GetTreeNodePermissions(Guid treeId, int treeNodeId)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            return await ctx
                .ObacTreeNodePermissions
                .Where(p => p.TreeId == treeId && p.NodeId == treeNodeId)
                .ToListAsync();
        }

        public async Task<IEnumerable<ObacTreeNodePermissionEntity>> GetTreeNodePermissionsForGroup(int userGroupId)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            return await ctx
                .ObacTreeNodePermissions
                .Where(p => p.UserGroupId == userGroupId)
                .ToListAsync();
        }

        public async Task<IEnumerable<ObacTreeNodePermissionEntity>> GetTreeNodePermissionList(Guid treeId, int[] treeNodeIds, Guid[] permissionIds)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            return await ctx
                .ObacTreeNodePermissions
                .Where(p => p.TreeId == treeId
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

            var nd = await ctx.ObacTreeNodes.SingleAsync(n => n.TreeId == treeId
                                                            && n.Id == treeNodeId
                );
            nd.InheritParentPermissions = inheritParentPermissions;
            ctx.Entry(nd).Property(a => a.InheritParentPermissions).IsModified = true;

            foreach (var delItem in ptodel)
            {
                var p = await ctx.ObacTreeNodePermissions.SingleAsync(
                    p => p.TreeId == treeId
                    && p.NodeId == treeNodeId
                    && p.PermissionId == delItem.PermissionId
                    && p.UserId == delItem.UserId
                    && p.UserGroupId == delItem.UserGroupId
                    && p.Deny == delItem.DenyPermission);
                ctx.ObacTreeNodePermissions.Remove(p);
            }

            foreach (var addItem in ptoadd)
            {
                await ctx.ObacTreeNodePermissions.AddAsync(new ObacTreeNodePermissionEntity
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

            await ctx.SaveChangesAsync();
        }



        public async Task<IEnumerable<TreeNodeInfo>> GetTreeSubnodesDeep(Guid treeId, int? startingNodeId = null)
        {
            await using var ctx = _storageProvider.CreateObacContext();

            IQueryable<ObacTreeNodeEntity> res;

            if (startingNodeId.HasValue)
            {
                // start from root
                var qry = ctx.GetTreeSubnodesDeepQueryRoot();
                res = ctx.ObacTreeNodes.FromSqlRaw(qry, startingNodeId, treeId);

            }
            else
            {
                // start from given node
                var qry = ctx.GetTreeSubnodesDeepQueryGivenNode();
                res = ctx.ObacTreeNodes.FromSqlRaw(qry, treeId);

            }

            return (await res.ToListAsync()).Select(nd => MakeTreeNodeInfo(treeId, nd));
        }

        public async Task<IEnumerable<TreeNodeInfo>> GetTreeSubnodesShallow(Guid treeId, int? startingNodeId = null)
        {
            await using var ctx = _storageProvider.CreateObacContext();

            IQueryable<ObacTreeNodeEntity> res;

            if (startingNodeId.HasValue)
            {
                // start from given node
                var qry = @"select id,  tree_id, parent_id, external_id_int, external_id_str, inherit_parent_perms, owner_user_id
            from obac_tree_nodes
            where parent_id = {0} and tree_id={1} order by id desc";

                res = ctx.ObacTreeNodes.FromSqlRaw(qry, startingNodeId, treeId);

            }
            else
            {
                // start from root
                var qry = @"select id, tree_id, parent_id, external_id_int, external_id_str, inherit_parent_perms, owner_user_id
            from obac_tree_nodes
            where parent_id is null and tree_id={0} order by id desc";


                res = ctx.ObacTreeNodes.FromSqlRaw(qry, treeId);

            }

            return (await res.ToListAsync()).Select(nd => MakeTreeNodeInfo(treeId, nd));
        }


        public async Task<int> GetTreeNodeCount(Guid treeId)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            return await ctx.ObacTreeNodes.CountAsync(n => n.TreeId == treeId);
        }

        public async Task DeleteObjectTree(Guid treeId)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            await using var transaction = await ctx.Database.BeginTransactionAsync();


            var tid = "{" + treeId + "}";

            // await ctx.Database.ExecuteSqlCommandAsync( 
            //     $"DELETE FROM obac_tree_nodes WHERE tree_id = {tid}"); 
            await ctx.Database.ExecuteSqlRawAsync(
            "DELETE FROM obac_tree_nodes WHERE tree_id = {0}", treeId);

            //ctx.ObacTreeNodes.RemoveRange(ctx.ObacTreeNodes.Where(n => n.TreeId == treeId));
            ctx.ObacTree.RemoveRange(ctx.ObacTree.Where(n => n.Id == treeId));

            await transaction.CommitAsync();
        }

        private const int EP_BATCH_SZ = 1;

        public async Task FeedWithActionList(IEnumerable<PermissionActionInfo> actions)
        {
            await using var ctx = _storageProvider.CreateObacContext();
            // todo improve
            int n = EP_BATCH_SZ;

            foreach (var a in actions)
            {
                switch (a.Action)
                {
                    case PermissionActionEnum.RemoveAllObjectsDirectPermission:
                        {
                            await ctx.Database.ExecuteSqlRawAsync(
                                "DELETE FROM obac_userpermissions WHERE objtypeid = {0} and objid = {1}", a.ObjectTypeId, a.ObjectId);

                        }
                        break;

                    case PermissionActionEnum.RemoveDirectPermission:
                        {
                            var p = await ctx
                                .ObacUserPermissions
                                .SingleOrDefaultAsync(
                                    p => p.ObjectTypeId == a.ObjectTypeId
                                        && p.ObjectId == a.ObjectId
                                        && p.PermissionId == a.PermissionId
                                        && p.UserId == a.UserId);
                            if (p != null)
                            {
                                ctx.ObacUserPermissions.Remove(p);
                            }
                        }
                        break;
                    case PermissionActionEnum.AddDirectPermission:
                        {
                            await ctx.ObacUserPermissions.AddAsync(new ObacUserPermissionsEntity
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
                await ctx.SaveChangesAsync();
            }

            if (n != EP_BATCH_SZ)
                await ctx.SaveChangesAsync();
        }


    }
}