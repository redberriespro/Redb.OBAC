using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Redb.OBAC.Core.Models;
using Redb.OBAC.Models;

namespace Redb.OBAC.Core
{
    public interface IObacObjectManager
    {
        // TODO - add Guid keys for every object, hide plain int IDs from major entities
        // TODO tree nodes - use both ints and Guids in overrides
        // TODO split subject-ish methods to UserXXX and UserGroupXXX
        
        #region tree objects
        
        Task<TreeObjectTypeInfo> GetTree(Guid? treeObjectTypeId, int? intId = null, string stringId=null);
        Task DeleteTree(Guid treeObjectTypeId, bool force=false);

        Task<TreeObjectTypeInfo> EnsureTree(Guid treeObjectTypeId, string description = null, 
            int? intId = null, string stringId=null);
        
        Task EnsureTreeNode(Guid treeId, int nodeId, int? parentId, int ownerUserId, int? intId = null, string stringId=null);

        Task<TreeNodeInfo> GetTreeNode(Guid treeId, int? treeNodeId, int? intId = null, string stringId=null);
        Task<List<TreeNodeInfo>> GetTreeNodes(Guid treeId, int? startingNodeId=null, bool deep=false);
        
        
        Task SetTreeNodeAcl(Guid treeId, int treeNodeId, AclInfo acl);
        Task<AclInfo> GetTreeNodeAcl(Guid treeId, int treeNodeId);
        
        Task RepairTreeNodeEffectivePermissions(Guid treeId, int treeNodeId);

        
        #endregion
        
        #region list objects
        
        Task<TreeObjectTypeInfo> GetList(Guid listObjectTypeId);
        Task DeleteList(Guid listObjectTypeId, bool force=false);

        Task<TreeObjectTypeInfo> EnsureList(Guid listObjectTypeId, string description = null, 
            int? intId = null, string stringId=null);
        
        Task EnsureListItem(Guid treeId, int objectId, int ownerUserId);

        Task<List<TreeNodeInfo>> GetListItems(Guid treeId);

        #endregion

        #region permission objects

        Task<IReadOnlyCollection<PermissionInfo>> GetPermissions();
        Task<PermissionInfo> GetPermission(Guid permissionId);
        Task EnsurePermission(Guid permissionId, string description, bool force = false);
        Task DeletePermission(Guid permissionId, bool force=false);

    
        #endregion
        
        #region role objects

        Task<RoleInfo> GetRole(Guid roleId);
        Task EnsureRole(Guid roleId, string description, Guid[] permissionIds, bool force = false);
        Task DeleteRole(Guid roleId);


        #endregion

        #region users
        
        Task EnsureUser(int userId, string description, int? intId = null, string stringId=null);
        Task DeleteUser(int userId, bool force=false);

        Task<SubjectInfo> GetUser(int? userId, int? intId = null, string stringId=null);

        
        #endregion

        #region users groups
        Task EnsureUserGroup(int userGroupId, string description, int? intId = null, string stringId=null);
        Task DeleteUserGroup(int userGroupId, bool force=false);

        Task<SubjectInfo> GetUserGroup(int? userGroupId, int? intId = null, string stringId = null);

        Task<int[]> GetUserGroupMembers(int userGroupId);

        Task AddUserToUserGroup(int userGroupId, int memberUserId);
        Task RemoveUserFromUserGroup(int userGroupId, int memberUserId);
        
        #endregion
        
        // todo sort these out        
        //Task<SubjectInfo> GetSubjectByExternalIntId(SubjectTypeEnum subjectType, int extId);
        //Task<SubjectInfo> GetSubjectByExternalStringId(SubjectTypeEnum subjectType, string extId);

    }
}