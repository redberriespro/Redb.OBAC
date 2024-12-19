using System;
using System.Collections.Concurrent;

namespace Redb.OBAC.Backends.InMemory
{
    public class ObjectTypeCacheEntry
    {
        private ConcurrentDictionary<Guid, ConcurrentBag<Guid>> _otCache = new ConcurrentDictionary<Guid, ConcurrentBag<Guid>>();
        
        public void Set(Guid? objectId, Guid permission)
        {
            var oid = objectId ?? Guid.Empty;
            
            if (!_otCache.ContainsKey(oid))
            {
                var entry = new ConcurrentBag<Guid> { permission };
                if (!_otCache.TryAdd(oid, entry)) // 2nd time attempt in case of concurrent adding
                {
                    _otCache[oid].Add(permission);
                }
            }
            else
            {
                _otCache[oid].Add(permission);
            }
        }

        public void InvalidatePermissionsForObject(Guid? objectId)
        {
            _otCache.TryRemove(objectId ?? Guid.Empty, out _);
        }

        public Guid[] GetPermissions(Guid? objectId)
        {
            var oid = objectId ?? Guid.Empty;

            if (_otCache.ContainsKey(oid))
                return _otCache[oid].ToArray();

            return null;
        }
    }
}