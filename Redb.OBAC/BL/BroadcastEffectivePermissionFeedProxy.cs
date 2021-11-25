using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Redb.OBAC.Core;
using Redb.OBAC.Core.Ep;
using Redb.OBAC.Tree;

namespace Redb.OBAC.BL
{
    public class BroadcastEffectivePermissionFeedProxy: IEffectivePermissionFeed
    {
        private IEffectivePermissionFeed[] _children;

        public BroadcastEffectivePermissionFeedProxy(IEffectivePermissionFeed[] children)
        {
            _children = children;
        }
        public async Task FeedWithActionList(IEnumerable<PermissionActionInfo> actions)
        {
            var actionList = actions.ToArray();
            foreach (var feed in _children)
            {
                await feed.FeedWithActionList(actionList);
            }
        }
    }
}