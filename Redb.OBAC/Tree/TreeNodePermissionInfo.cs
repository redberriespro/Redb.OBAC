using System;

namespace Redb.OBAC.Core.Models
{
    public class TreeNodePermissionInfo : IEquatable<TreeNodePermissionInfo>
    {
        public bool Equals(TreeNodePermissionInfo other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return NodeId == other.NodeId && UserId == other.UserId && UserGroupId == other.UserGroupId && PermissionId.Equals(other.PermissionId) && DenyPermission == other.DenyPermission;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TreeNodePermissionInfo) obj);
        }

        public Guid NodeId;
        public int? UserId;
        public int? UserGroupId;
        public Guid PermissionId;
        public bool DenyPermission;

        public override int GetHashCode()
        {
            return HashCode.Combine(NodeId, UserId, UserGroupId, PermissionId, DenyPermission);
        }

        public override string ToString()
        {
            return $"{NodeId}:{UserId}:{UserGroupId}:{PermissionId}:{DenyPermission}";
        }

        public static TreeNodePermissionInfo Parse(string permInfo)
        {
            var parts = permInfo.Split(":");
            if (parts.Length != 5) throw new ArgumentException("wrong permInfo format");
            return new TreeNodePermissionInfo
            {
                NodeId = Guid.Parse(parts[0]),
                UserId = int.TryParse(parts[1], out var f) ? (int?)f : null, 
                UserGroupId = int.TryParse(parts[2], out var f2) ? (int?)f2 : null, 
                PermissionId = Guid.Parse(parts[3]),
                DenyPermission = bool.Parse(parts[4])
            };
        }
    }
}