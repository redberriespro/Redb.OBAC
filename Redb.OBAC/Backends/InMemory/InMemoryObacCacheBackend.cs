using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Redb.OBAC.Core.Hierarchy;
using Redb.OBAC.Models;

namespace Redb.OBAC.Backends.InMemory
{
    // trivial implementation of cache mechanism

    public class InMemoryObacCacheBackend: IObacCacheBackend
    {
        private ConcurrentDictionary<int, UserCacheEntry> _cache = new ConcurrentDictionary<int, UserCacheEntry>();
        
        private ConcurrentDictionary<int, SubjectInfo> _userCache = new ConcurrentDictionary<int, SubjectInfo>();
        private ConcurrentDictionary<int, SubjectInfo> _userByExtIdCache = new ConcurrentDictionary<int, SubjectInfo>();
        private ConcurrentDictionary<string, SubjectInfo> _userByStringIdCache = new ConcurrentDictionary<string, SubjectInfo>();
        

        private ConcurrentDictionary<int, SubjectInfo> _groupCache = new ConcurrentDictionary<int, SubjectInfo>();
        private ConcurrentDictionary<int, SubjectInfo> _groupByExtIdCache = new ConcurrentDictionary<int, SubjectInfo>();
        private ConcurrentDictionary<string, SubjectInfo> _groupByStringIdCache = new ConcurrentDictionary<string, SubjectInfo>();

        public InMemoryObacCacheBackend()
        {
        }

        public void InvalidatePermissionsForObject(Guid objectTypeId, int? objectId = null)
        {
            foreach (var userCache in _cache.Values)
            {
                userCache.InvalidatePermissionsForObject(objectTypeId, objectId);
            }
        }
        
        public void InvalidateForUser(int userId, Guid? objectTypeId = null, int? objectId = null)
        {
            _cache.TryRemove(userId, out _);
            
            lock (_userCache)
            {
                if (_userCache.TryGetValue(userId, out var si))
                {
                    if (si.ExternalIntId.HasValue)
                    {
                        _userByExtIdCache.TryRemove(si.ExternalIntId.Value, out _);
                    }

                    if (!String.IsNullOrEmpty(si.ExternalStringId))
                    {
                        _userByStringIdCache.TryRemove(si.ExternalStringId, out _);
                    }

                    _userCache.TryRemove(userId, out _);
                }
            }

            if (objectTypeId.HasValue && objectId.HasValue)
            {
            }
        }

        private void SetPermission(int userId, Guid objectTypeId, int? objectId, Guid permission)
        {
            if (!_cache.ContainsKey(userId))
            {
                var entry = new UserCacheEntry();
                entry.Set(objectTypeId, objectId, permission);
                if (!_cache.TryAdd(userId, entry)) // 2nd time attempt in case of concurrent adding
                {
                    _cache[userId].Set(objectTypeId, objectId, permission); 
                }
            }
            else
            {
                _cache[userId].Set(objectTypeId, objectId, permission); 
            }
        }
        
        public void InvalidateForUserGroup(int groupId)
        {
            //_cache.TryRemove(userId, out _);
            
            lock (_groupCache)
            {
                if (_groupCache.TryGetValue(groupId, out var si))
                {
                    if (si.ExternalIntId.HasValue)
                    {
                        _groupByExtIdCache.TryRemove(si.ExternalIntId.Value, out _);
                    }

                    if (!String.IsNullOrEmpty(si.ExternalStringId))
                    {
                        _groupByStringIdCache.TryRemove(si.ExternalStringId, out _);
                    }

                    _groupCache.TryRemove(groupId, out _);
                }
            }

            // if (objectTypeId.HasValue && objectId.HasValue)
            // {
            //     _directPermissionsOnObject.TryRemove(new Tuple<Guid, int>(objectTypeId.Value, objectId.Value), out _);
            // }
        }

        public void SetPermissions(int userId, Guid objectTypeId, int? objectId, Guid[] permissionIds)
        {
            foreach (var permissionId in permissionIds)
            {
                SetPermission(userId, objectTypeId, objectId, permissionId);
            }
        }

        public Guid[] GetPermissionsFor(int userId, Guid objectTypeId, int? objectId)
        {
            if (_cache.ContainsKey(userId))
                return _cache[userId].GetPermissions(objectTypeId, objectId);

            return null;
            
        }

        public void SetUserId(SubjectInfo subjectInfo)
        {
            if (subjectInfo.ExternalIntId.HasValue)
            {
                _userByExtIdCache[subjectInfo.ExternalIntId.Value] = subjectInfo;
            }

            if (!String.IsNullOrEmpty(subjectInfo.ExternalStringId))
            {
                _userByStringIdCache[subjectInfo.ExternalStringId] = subjectInfo;
            }

            _userCache[subjectInfo.SubjectId] = subjectInfo;
        }

        public SubjectInfo GetUserById(int userId) 
            => _userCache.TryGetValue(userId, out var si) ? si : null;

        public SubjectInfo GetUserByExternalStringId(string extId) 
            => _userByStringIdCache.TryGetValue(extId, out var si) ? si : null;

        public SubjectInfo GetUserByExternalIntId(in int extId) 
            => _userByExtIdCache.TryGetValue(extId, out var si) ? si : null;

        
        public void SetGroupId(SubjectInfo subjectInfo)
        {
            if (subjectInfo.ExternalIntId.HasValue)
            {
                _groupByExtIdCache[subjectInfo.ExternalIntId.Value] = subjectInfo;
            }

            if (!String.IsNullOrEmpty(subjectInfo.ExternalStringId))
            {
                _groupByStringIdCache[subjectInfo.ExternalStringId] = subjectInfo;
            }

            _groupCache[subjectInfo.SubjectId] = subjectInfo;
        }

        public SubjectInfo GetGroupById(int groupId)
            => _groupCache.TryGetValue(groupId, out var si) ? si : null;

        public SubjectInfo GetGroupByExternalStringId(string extId)
            => _groupByStringIdCache.TryGetValue(extId, out var si) ? si : null;

        public SubjectInfo GetGroupByExternalIntId(in int extId)
            => _groupByExtIdCache.TryGetValue(extId, out var si) ? si : null;
        
    }
}