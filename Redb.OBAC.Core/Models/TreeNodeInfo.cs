using System;

namespace Redb.OBAC.Core.Models
{
    public class TreeNodeInfo
    {
        public Guid TreeObjectTypeId { get; set; }
        public int NodeId { get; set; }
        public int? ParentNodeId { get; set; }
        
        public int? ExternalIntId { get; set; }
        public string ExternalStringId { get; set; }
        
        public bool InheritParentPermissions { 
            get => Acl.InheritParentPermissions;
            set { Acl.InheritParentPermissions = value; }
        }

        public AclInfo Acl { get; set; } = new AclInfo();
        public int OwnerUserid { get; set; }
    }
}