using System.Collections.Generic;
using System.Linq;
using Redb.OBAC.Core.Models;
using Redb.OBAC.MongoDriver.DB.Entities;

namespace Redb.OBAC.MongoDriver.ModelsPrivate
{
    public class TreeObjectMapper
    {
        public static AclItemInfo[] TreeObjectPermissionsToAclList(IEnumerable<ObacTreeNodePermissionEntity> perms) =>
            perms
                .Select(p => new AclItemInfo
                {
                    PermissionId = p.PermissionId,
                    UserGroupId = p.UserGroupId,
                    UserId = p.UserId,
                    Kind = p.Deny? PermissionKindEnum.Deny: PermissionKindEnum.Allow
                }).ToArray();

        public static TreeNodePermissionInfo EntityToPermissionInfo(ObacTreeNodePermissionEntity npe) =>
            new TreeNodePermissionInfo
            {
                NodeId = npe.NodeId,
                DenyPermission = npe.Deny,
                PermissionId = npe.PermissionId,
                UserId = npe.UserId,
                UserGroupId = npe.UserGroupId
            };

        public static TreeNodePermissionInfo AclToPermissionInfo(int nodeId, AclItemInfo aclItem)
            => new TreeNodePermissionInfo
            {
                NodeId = nodeId, PermissionId = aclItem.PermissionId, UserId = aclItem.UserId,
                UserGroupId = aclItem.UserGroupId,
                DenyPermission = aclItem.Kind == PermissionKindEnum.Deny
            };
    }
}