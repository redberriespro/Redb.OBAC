using System;
using System.Collections.Generic;
using System.Linq;
using Redb.OBAC.Core.Models;
using Redb.OBAC.Models;

namespace Redb.OBAC.Tree
{
    public class EffectivePermissionCalculator
    {
        public TreeNodePermissionInfo[] CalculateEffectivePermissions(
            Guid nodeId,
            bool inheritParentPermissions,
            TreeNodePermissionInfo[] nodeDirectPermissionsRaw,
            TreeNodePermissionInfo[] nodeGroupPermissionsRaw,
            TreeNodePermissionInfo[] parentEffectivePermissionsRaw)
        {
            // levels of permissions (higher level means higher priority) 
            // 1. direct permissions to users
            // 2. indirect permissions given to users via groups
            // 3. permissions to parent (if permission inheritance flag is set)
            // DENY permission flag set for higher priority means
            // comes to less lveles will not been considered
            // on each level ALLOW has priority under DENY

            var nodeDirectPermissions = nodeDirectPermissionsRaw ?? new TreeNodePermissionInfo[0];
            var nodeGroupPermissions = nodeGroupPermissionsRaw ?? new TreeNodePermissionInfo[0];
            var parentEffectivePermissions = parentEffectivePermissionsRaw ?? new TreeNodePermissionInfo[0];
            
            // strip all group-defined permissions
            nodeDirectPermissions = nodeDirectPermissions.Where(p => !p.UserGroupId.HasValue).ToArray(); 
            nodeGroupPermissions = nodeGroupPermissions.Where(p => !p.UserGroupId.HasValue).ToArray(); 

            
            var l2p = inheritParentPermissions
                ? FoldPair(nodeId,nodeGroupPermissions, parentEffectivePermissions)
                : nodeGroupPermissions;
            return FoldPair(nodeId,nodeDirectPermissions, l2p);
        }

        private TreeNodePermissionInfo[] FoldPair(
            Guid nodeId,
            TreeNodePermissionInfo[] higher, TreeNodePermissionInfo[] lower)
        {
            var res = new HashSet<(Guid, int)>(); // permid + userid
            foreach (var lp in lower.Where(l=>!l.DenyPermission && l.UserId.HasValue))
            {
                res.Add((lp.PermissionId, lp.UserId.Value));
            }

            // apply denials
            foreach (var h in higher.Where(h=> h.UserId.HasValue && h.DenyPermission))
            {
                res.Remove((h.PermissionId, h.UserId.Value));
            }
            
            // apply allows
            foreach (var h in higher.Where(h=> h.UserId.HasValue && !h.DenyPermission))
            {
                res.Add((h.PermissionId, h.UserId.Value));
            }

            return res.Select(r => new TreeNodePermissionInfo
            {
                NodeId = nodeId,
                DenyPermission = false, 
                PermissionId = r.Item1,
                UserId = r.Item2
            }).ToArray();

        }
    }
}