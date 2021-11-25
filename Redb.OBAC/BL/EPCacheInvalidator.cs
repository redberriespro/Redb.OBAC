using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Redb.OBAC.Backends;
using Redb.OBAC.Core;
using Redb.OBAC.Core.Ep;
using Redb.OBAC.Tree;

namespace Redb.OBAC.BL
{
    public class EPCacheInvalidator: IEffectivePermissionFeed
    {
        private readonly IObacCacheBackend _cache;

        public EPCacheInvalidator(IObacCacheBackend cache)
        {
            _cache = cache;
        }

        public async Task FeedWithActionList(IEnumerable<PermissionActionInfo> actions)
        {
            foreach (var ac in actions)
            {
                if (ac.UserId != 0)
                {
                        _cache.InvalidateForUser(ac.UserId, ac.ObjectTypeId, ac.ObjectId);
                }
                else
                {
                    _cache.InvalidatePermissionsForObject(ac.ObjectTypeId, ac.ObjectId);
                }
                
            }
        }
    }
}