using System.Collections.Generic;
using System.Threading.Tasks;
using Redb.OBAC.Core.Ep;

namespace Redb.OBAC.Core
{
    public interface IEffectivePermissionFeed
    {
        public Task FeedWithActionList(IEnumerable<PermissionActionInfo> actions);
    }
}