using System;

namespace Redb.OBAC.Core.Models
{
    public class TreeNodeInfo
    {
        public Guid TreeObjectTypeId { get; set; }
        public Guid NodeId { get; set; }
        public Guid? ParentNodeId { get; set; }
        
        public bool InheritParentPermissions { 
            get => Acl.InheritParentPermissions;
            set { Acl.InheritParentPermissions = value; }
        }

        public AclInfo Acl { get; set; } = new AclInfo();
        public int OwnerUserid { get; set; }
    }
}