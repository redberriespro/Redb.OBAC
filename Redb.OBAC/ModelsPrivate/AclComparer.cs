using System;
using System.Collections.Generic;
using System.Linq;
using Redb.OBAC.Core.Ep;
using Redb.OBAC.Core.Models;
using Redb.OBAC.Tree;

namespace Redb.OBAC.ModelsPrivate
{
    public class AclCompareResult
    {
        public  NodeParentPermissionInheritanceActionEnum InheritParentPermissionsAction { get; set; }
        public List<AclItemInfo> AclItemsToBeAdded { get; set; }
        public List<AclItemInfo> AclItemsToBeRemoved { get; set; }
    }
    
    public class AclComparer
    {
        public static AclCompareResult CompareAcls(AclInfo oldAcl, AclInfo newAcl)
        {
            if (oldAcl == null) throw new ArgumentNullException(nameof(oldAcl));
            if (newAcl == null) throw new ArgumentNullException(nameof(newAcl));
            
            var res = new AclCompareResult
            {
                InheritParentPermissionsAction = (oldAcl.InheritParentPermissions == newAcl.InheritParentPermissions)
                ? NodeParentPermissionInheritanceActionEnum.KeepSame
                : (newAcl.InheritParentPermissions
                    ? NodeParentPermissionInheritanceActionEnum.SetInherit
                    : NodeParentPermissionInheritanceActionEnum.SetDoNotInherit),

                
                AclItemsToBeAdded = new List<AclItemInfo>(),
                AclItemsToBeRemoved = new List<AclItemInfo>()
            };

            var oldPermissionSet = new HashSet<string>();
            foreach (var p in oldAcl.AclItems)
            {
                oldPermissionSet.Add(p.ToString());
            }
            
            var newPermissionSet = new HashSet<string>();
            foreach (var p in newAcl.AclItems)
            {
                newPermissionSet.Add(p.ToString());
            }

            var toAdd = new HashSet<string>(newPermissionSet);
            toAdd.ExceptWith(oldPermissionSet);


            var toRemove = new HashSet<string>(oldPermissionSet);
            toRemove.ExceptWith(newPermissionSet);
            
            res.AclItemsToBeAdded.AddRange(toAdd.Select(AclItemInfo.Parse));
            res.AclItemsToBeRemoved.AddRange(toRemove.Select(AclItemInfo.Parse));
            
            
            return res;
        }
        
        public static IEnumerable<PermissionActionInfo> CompareEffectivePermissions(
            TreeNodePermissionInfo[] oldPermissions,
            TreeNodePermissionInfo[] newPermissions,
            Guid treeId)
        {
            var oldPermissionSet = new HashSet<string>();
            foreach (var p in oldPermissions)
            {
                oldPermissionSet.Add(p.ToString());
            }
            
            var newPermissionSet = new HashSet<string>();
            foreach (var p in newPermissions)
            {
                newPermissionSet.Add(p.ToString());
            }

            var toAdd = new HashSet<string>(newPermissionSet);
            toAdd.ExceptWith(oldPermissionSet);


            var toRemove = new HashSet<string>(oldPermissionSet);
            toRemove.ExceptWith(newPermissionSet);
            
            var actions = new List<PermissionActionInfo>();
            
            actions.AddRange(toAdd
                .Select(TreeNodePermissionInfo.Parse)
                .Where(p=>p.UserId.HasValue)
                .Select(p=>new PermissionActionInfo
                { 
                    Action = PermissionActionEnum.AddDirectPermission,
                    PermissionId = p.PermissionId,
                    UserId = p.UserId.Value, 
                    ObjectId = p.NodeId,
                    ObjectTypeId = treeId
                }));
            
            actions.AddRange(toRemove
                .Select(TreeNodePermissionInfo.Parse)
                .Where(p=>p.UserId.HasValue)
                .Select(p=>new PermissionActionInfo
                { 
                    Action = PermissionActionEnum.RemoveDirectPermission,
                    PermissionId = p.PermissionId,
                    UserId = p.UserId.Value, 
                    ObjectId = p.NodeId,
                    ObjectTypeId = treeId
                }));
            
            return actions;
        }

    }
}