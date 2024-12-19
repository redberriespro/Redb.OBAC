using Redb.OBAC.Core;
using Redb.OBAC.Core.Models;
using Redb.OBAC.Tree;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Redb.OBAC
{
    public interface IObjectStorage: ILazyTreeDataProvider, IEffectivePermissionFeed
    {
        Task<List<TreeNodePermissionInfo>> GetEffectivePermissionsForAllUsers(Guid objectTypeId, Guid? objectId);
    }
}
