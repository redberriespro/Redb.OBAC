using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Redb.OBAC.Core;
using Redb.OBAC.Core.Ep;
using Redb.OBAC.Tree;

namespace Redb.OBAC.BL
{
    public class InMemoryEffectivePermissionQueue:IEffectivePermissionFeed
    {
        public ConcurrentQueue<PermissionActionInfo> Actions = new ConcurrentQueue<PermissionActionInfo>();

        public PermissionActionInfo[] GetAll()
        {
            return Actions.ToArray();
        }
        public async Task FeedWithActionList(IEnumerable<PermissionActionInfo> actions)
        {
            foreach (var a in actions)
            {
                Actions.Enqueue(a);
            }
        }
    }
}