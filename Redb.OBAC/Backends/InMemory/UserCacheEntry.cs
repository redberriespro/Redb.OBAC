using System;
using System.Collections.Concurrent;

namespace Redb.OBAC.Backends.InMemory
{
    public class UserCacheEntry {
        private ConcurrentDictionary<Guid, ObjectTypeCacheEntry> _userCache = new ConcurrentDictionary<Guid, ObjectTypeCacheEntry>();

        public void Set(Guid objectTypeId, int? objectId, Guid permission)
        {
            if (!_userCache.ContainsKey(objectTypeId))
            {
                var entry = new ObjectTypeCacheEntry();
                entry.Set(objectId, permission);
                if (!_userCache.TryAdd(objectTypeId, entry)) // 2nd time attempt in case of concurrent adding
                {
                    _userCache[objectTypeId].Set(objectId, permission);
                }
            }
            else
            {
                _userCache[objectTypeId].Set(objectId, permission);
            }
        }

        public Guid[] GetPermissions(Guid objectTypeId, int? objectId)
        {
            if (_userCache.ContainsKey(objectTypeId))
                return _userCache[objectTypeId].GetPermissions(objectId);

            return null;
        }

        public void InvalidatePermissionsForObject(Guid objectTypeId, int? objectId)
        {
            if (!_userCache.TryGetValue(objectTypeId, out var otCache)) return;
            otCache.InvalidatePermissionsForObject(objectId);
        }
    }
}