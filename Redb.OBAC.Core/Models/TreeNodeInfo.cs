using System;

namespace Redb.OBAC.Core.Models
{
    public class TreeNodeInfo
    {
        public Guid TreeObjectTypeId { get; set; }
        public int NodeId { get; set; }
        public int? ParentNodeId { get; set; }
        public bool InheritParentPermissions { get; set; }
        
        public int OwnerUserid { get; set; }
    }
}