using System;
using System.Collections.Generic;

namespace Redb.OBAC.Core.Hierarchy
{
    public class UserPermissionInfo
    {
        public int UserId { get; set; }
        public IEnumerable<Guid> Permissions { get; set; }
    }
}
