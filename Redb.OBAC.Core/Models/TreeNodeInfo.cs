using System;

namespace Redb.OBAC.Core.Models
{
    public class TreeNodeInfo
    {
        public Guid TreeObjectTypeId { get; set; }
        public int NodeId { get; set; }
        public int? ParentNodeId { get; set; }
        public bool InheritParentPermissions { 
            get => Acl.InheritParentPermissions;
            set { Acl.InheritParentPermissions = value; }
        }
        
        public AclInfo Acl { get; set; } 
        public int OwnerUserid { get; set; }
    }
}