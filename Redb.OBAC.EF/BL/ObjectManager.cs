using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Redb.OBAC.Backends;
using Redb.OBAC.Core;
using Redb.OBAC.Core.Models;
using Redb.OBAC.Exceptions;
using Redb.OBAC.Models;
using Redb.OBAC.EF.ObjectTypes;
using Redb.OBAC.Utils;

namespace Redb.OBAC.EF.BL
{
    /// <summary>
    /// aggregator class managing all types of objects with their
    /// respective sub-managers
    /// todo merge with EF.TreeObjectManager, introduce IObjectStorage interface, pull up the manager to OBAC.Core
    /// </summary>
    public class ObjectManager: IObacObjectManager
    {
        private readonly ObjectStorage _store;
        private readonly IObacCacheBackend _cacheBackend;
        private TreeObjectManager _treeObjectManager;

        public ObjectManager(ObjectStorage store, IObacCacheBackend cacheBackend,
            IEffectivePermissionFeed[] extraEpFeeds)
        {
            _store = store;
            _cacheBackend = cacheBackend;
            _treeObjectManager = new TreeObjectManager(store, cacheBackend, extraEpFeeds);
        }

        public async Task<TreeObjectTypeInfo> GetTree(Guid? treeObjectTypeId, int? intId = null, string stringId=null)
        {
            if (stringId != null)
                return await _treeObjectManager.GetTreeObjectByExternalStringId(stringId);
            if (intId.HasValue) 
                return await _treeObjectManager.GetTreeObjectByExternalIntId(intId.Value);
            if (treeObjectTypeId.HasValue)
                return await _treeObjectManager.GetTreeObjectById(treeObjectTypeId.Value);
            throw new ArgumentException("GetTree - no ID is provided");
        }

        public async Task DeleteTree(Guid treeObjectTypeId, bool force = false)
        {
            await _treeObjectManager.DeleteTreeObjectType(treeObjectTypeId, force);
        }

        public async Task<TreeObjectTypeInfo> EnsureTree(Guid treeObjectTypeId, string description = null, int? intId = null, string stringId = null)
        {
            return await _treeObjectManager.EnsureTreeObject(treeObjectTypeId, description,intId, stringId);
        }

        public async Task EnsureTreeNode(Guid treeId, int nodeId, int? parentId, int ownerUserId,  int? intId = null, string stringId=null)
        {
            await _treeObjectManager.EnsureTreeNode(treeId, nodeId, parentId, ownerUserId, intId, stringId);
        }

        public async Task DeleteTreeNode(Guid treeId, int nodeId)
        {
            await _treeObjectManager.DeleteTreeNode(treeId, nodeId);
        }


        public async Task<List<TreeNodeInfo>> GetTreeNodes(Guid treeId, int? startingNodeId = null, bool deep=false)
        {
            return await _treeObjectManager.GetTreeNodes(treeId, startingNodeId, deep);
        }
        
        public async Task<TreeNodeInfo> GetTreeNode(Guid treeId, int? treeNodeId, int? intId = null, string stringId=null)
        {
            if (stringId != null)
                return await _treeObjectManager.GetTreeNodeByExternalStringId(treeId, stringId);
            if (intId.HasValue) 
                return await _treeObjectManager.GetTreeNodeByExternalIntId(treeId, intId.Value);
            if (treeNodeId.HasValue)
                return await _treeObjectManager.GetTreeNode(treeId, treeNodeId.Value);
            throw new ArgumentException("GetTreeNode - no ID is provided");
        }


        public async Task<TreeObjectTypeInfo> GetList(Guid treeObjectTypeId)
        {
            return await _treeObjectManager.GetTreeObjectById(treeObjectTypeId);
        }

        public async Task DeleteList(Guid treeObjectTypeId, bool force = false)
        {
            await _treeObjectManager.DeleteTreeObjectType(treeObjectTypeId, force);
        }

        public async Task<TreeObjectTypeInfo> EnsureList(Guid treeObjectTypeId, string description = null,
            int? intId = null, string stringId = null)
        {
            return await _treeObjectManager.EnsureTreeObject(treeObjectTypeId, description,intId, stringId);
        }

        public async Task EnsureListItem(Guid treeId, int objectId, int ownerUserId)
        {
            await _treeObjectManager.EnsureTreeNode(treeId, objectId, null, ownerUserId);
        }

        public async Task<List<TreeNodeInfo>> GetListItems(Guid treeId)
        {
            return await _treeObjectManager.GetTreeNodes(treeId, null);
        }


        public async Task<IReadOnlyCollection<PermissionInfo>> GetPermissions()
        {
            return await _store.ListPermissions();
        }

        
        public async Task<PermissionInfo> GetPermission(Guid permissionId)
        {
            return await _store.GetPermissionById(permissionId);
        }

        public async Task EnsurePermission(Guid permissionId, string description, bool force = false)
        {
            var perm = await _store.GetPermissionById(permissionId);
            if (perm == null)
            {
                await _store.AddPermission(permissionId, description);
            }
            else
            {
                if (force)
                {
                    await _store.UpdatePermission(permissionId, description);
                }
                else
                {
                    if (perm.Description != description)
                        throw new ObacException($"EnsurePermission {permissionId}: name differs current {description} was {perm.Description}");
                }
            }
            
        }

        public async Task DeletePermission(Guid permissionId, bool force = false)
        {
            await _store.DeletePermission(permissionId);
        }

        // public Task GiveTreePermissions(SubjectTypeEnum subjectType, int subjectId, Guid objectTypeId, Guid[] permissionIds)
        // {
        //     throw new NotImplementedException();
        // }
        //
        // public async Task GiveTreeNodePermissions(SubjectTypeEnum subjectType, int subjectId, Guid objectTypeId, Guid[] permissionIds, int objectId)
        // {
        //     if (subjectType != SubjectTypeEnum.User) throw new NotImplementedException();
        //
        //     _cacheBackend.InvalidateForUser(subjectId, objectTypeId, objectId);
        //     await _treeObjectType2.AddUserPermissionsToAnObject(subjectId, permissionIds, objectTypeId, objectId, deleteExisting:true);
        // }

        public async Task RepairTreeNodeEffectivePermissions(Guid treeId, int treeNodeId)
        {
            await _treeObjectManager.RepairTreeNodeEffectivePermissions(treeId, treeNodeId);
        }

        
        public async Task SetTreeNodeAcl(Guid treeId, int treeNodeId, AclInfo acl)
        {
            await _treeObjectManager.SetTreeNodeAcl(treeId, treeNodeId, acl);
        }

        public async Task< AclInfo> GetTreeNodeAcl(Guid treeId, int treeNodeId)
        {
            return await _treeObjectManager.GetTreeNodeAcl(treeId, treeNodeId);
        }
        
        public async Task<RoleInfo> GetRole(Guid roleId)
        {
            return await _store.GetRoleById(roleId);
        }

        public async Task<IReadOnlyCollection<RoleInfo>> GetRoles()
        {
            return (await _store.GetAllRoles()).ToArray();
        }


        public async Task EnsureRole(Guid roleId, string description, Guid[] permissionIds, bool force = false)
        {
            var perm = await _store.GetRoleById(roleId);
            if (perm == null)
            {
                await _store.AddRole(roleId, description, permissionIds);
            }
            else
            {
                if (force)
                {
                    await _store.UpdateRole(roleId, description, permissionIds);
                }
                else
                {
                    var attrsDiff = perm.Description != description;
                    var permsDiff = !GuidUtils.ListsEquals(permissionIds, perm.PermissionIds);
                    if ( attrsDiff || permsDiff )
                        throw new ObacException(
                            $"EnsureRole {roleId}: differs current {description} was {perm.Description}");
                }
            }
        }

        public async Task DeleteRole(Guid roleId)
        {
            await _store.DeleteRole(roleId);
        }

        public async Task EnsureUser(int userId, string description = null, int? intId = null, string stringId = null)
        {
            // todo auto-generate user id if zero. consider always do it, protecting user id from being changed
            var perm = await _store.GetUserSubjectById(userId);
            if (perm == null)
            {
                _cacheBackend.InvalidateForUser(userId);
                await _store.AddUserSubject(userId, description, intId, stringId);
                _cacheBackend.SetUserId(new SubjectInfo
                {
                    Description = description, ExternalIntId = intId, ExternalStringId = stringId, SubjectId = userId,
                    SubjectType = SubjectTypeEnum.User
                });
            }
            else
            {
                await _store.UpdateUserSubject(userId, description);
            }
        }

        private void EnsureSubjectType(SubjectTypeEnum subjectType)
        {
            if (subjectType == SubjectTypeEnum.User) return;
            if (subjectType == SubjectTypeEnum.UserGroup) return;

            throw new NotImplementedException("Unsupported subject type: " + subjectType);
        }
        
        public async Task EnsureUserGroup(int userGroupId, string description = null, int? intId = null, string stringId = null)
        {
            var perm = await _store.GetGroupSubjectById(userGroupId);
            if (perm == null)
            {
                _cacheBackend.InvalidateForUserGroup(userGroupId);
                await _store.AddGroupSubject(userGroupId, description, intId, stringId);
                _cacheBackend.SetGroupId(new SubjectInfo
                {
                    Description = description, ExternalIntId = intId, ExternalStringId = stringId, SubjectId = userGroupId,
                    SubjectType = SubjectTypeEnum.User
                });
            }
            else
            {
                await _store.UpdateGroupSubject(userGroupId, description);
            }        
        }

        
        public async Task DeleteUser(int userId, bool force = false)
        {
            await _store.DeleteUserSubject(userId);
            _cacheBackend.InvalidateForUser(userId);
        }
        
        public async Task DeleteUserGroup(int userGroupId, bool force = false)
        {
            await _store.DeleteUserGroupSubject(userGroupId);
            _cacheBackend.InvalidateForUserGroup(userGroupId);
        }

        public async Task<int[]> GetUserGroupsForUser(int userId)
        {
            return await _store.GetGroupsForUser(userId);
        }

        
        public async Task<SubjectInfo> GetUser(int? userId, int? intId = null, string stringId=null)
        {
            if (!userId.HasValue && !intId.HasValue && stringId == null)
                throw new ObacException("GetUser: no ID provided");
            
            SubjectInfo user;
                
            if (stringId != null)
                user = _cacheBackend.GetUserByExternalStringId(stringId);
            else if (intId.HasValue)
                user = _cacheBackend.GetUserByExternalIntId(intId.Value);
            else user = _cacheBackend.GetGroupById(userId.Value);
            
            if (user == null)
            {
                if (stringId != null)
                    user = await _store.GetUserSubjectByExternalStringId(stringId);
                else if (intId.HasValue)
                    user = await _store.GetUserSubjectByExternalIntId(intId.Value);
                else user = await _store.GetUserSubjectById(userId.Value);
                
                if (user!=null)
                    _cacheBackend.SetUserId(user);
            }
    
            return user;
        }

        public async Task<IReadOnlyCollection<SubjectInfo>> GetUserGroups()
        {
            return await _store.GetGroupSubjects();
        }
        
        public async Task<SubjectInfo> GetUserGroup(int? userGroupId, int? intId, string stringId = null)
        {
            if (!userGroupId.HasValue && !intId.HasValue && stringId == null)
                throw new ObacException("GetUserGroup: no ID provided");
            
            SubjectInfo grp;
                
            if (stringId != null)
                grp = _cacheBackend.GetGroupByExternalStringId(stringId);
            else if (intId.HasValue)
                grp = _cacheBackend.GetGroupByExternalIntId(intId.Value);
            else grp = _cacheBackend.GetGroupById(userGroupId.Value);
            
            if (grp == null)
            {
                if (stringId != null)
                    grp = await _store.GetGroupSubjectByExternalStringId(stringId);
                else if (intId.HasValue)
                    grp = await _store.GetGroupSubjectByExternalIntId(intId.Value);
                else grp = await _store.GetGroupSubjectById(userGroupId.Value);
                
                if (grp!=null)
                    _cacheBackend.SetGroupId(grp);
            }
    
            return grp;
        }
        
        public async Task<int[]> GetUserGroupMembers(int userGroupId)
        {
            return await _store.GetGroupMembers(userGroupId);
        }

        public async Task AddUserToUserGroup(int userGroupId, int memberUserId)
        {
            try
            {
                await _store.AddUserToGroup(userGroupId, memberUserId);
            }
            catch (DbUpdateException)
            {
                // ignore any update exception (include duplicates etc)
            }        
            await _treeObjectManager.UpdatePermissionsUsersGroupChanged(userGroupId);

        }

        public async Task RemoveUserFromUserGroup(int userGroupId, int memberUserId)
        {
            await _store.DeleteUserFromGroup(userGroupId, memberUserId);
            await _treeObjectManager.UpdatePermissionsUsersGroupChanged(userGroupId);

        }
    }
}