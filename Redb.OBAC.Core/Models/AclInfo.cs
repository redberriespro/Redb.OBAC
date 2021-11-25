using System;

namespace Redb.OBAC.Core.Models
{
    public class AclItemInfo
    {
        public Guid PermissionId { get; set; }
        public int? UserId { get; set; }
        public int? UserGroupId { get; set; }
        public PermissionKindEnum Kind { get; set; } = PermissionKindEnum.Allow;
        
        public override string ToString()
        {
            var k = Kind==PermissionKindEnum.Allow ? 'A' : 'D';
            return $"{UserId}:{UserGroupId}:{PermissionId}:{k}";
        }
        
        public static AclItemInfo Parse(string aclInfo)
        {
            var parts = aclInfo.Split(":");
            if (parts.Length != 4) throw new ArgumentException("wrong aclInfo format");
            return new AclItemInfo
            {
                UserId = int.TryParse(parts[0], out var f) ? (int?)f : null, 
                UserGroupId = int.TryParse(parts[1], out var f2) ? (int?)f2 : null, 
                PermissionId = Guid.Parse(parts[2]),
                Kind = parts[3]=="A"? PermissionKindEnum.Allow: PermissionKindEnum.Deny
            };
        }
    }
    
    public class AclInfo
    {
        public AclItemInfo[] AclItems { get; set; }
        public bool InheritParentPermissions { get; set; }
    }
}