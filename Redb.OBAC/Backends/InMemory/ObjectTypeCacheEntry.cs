using System;
using System.Collections.Concurrent;

namespace Redb.OBAC.Backends.InMemory
{
    public class ObjectTypeCacheEntry
    {
        private ConcurrentDictionary<int, ConcurrentBag<Guid>> _otCache = new ConcurrentDictionary<int, ConcurrentBag<Guid>>();
        
        public void Set(int? objectId, Guid permission)
        {
            var oid = objectId ?? int.MinValue;
            
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

        public void InvalidatePermissionsForObject(int? objectId)
        {
            _otCache.TryRemove(objectId ?? Int32.MinValue, out _);
        }

        public Guid[] GetPermissions(int? objectId)
        {
            var oid = objectId ?? int.MinValue;

            if (_otCache.ContainsKey(oid))
                return _otCache[oid].ToArray();

            return null;
        }
    }
}