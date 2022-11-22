using System;
using System.Linq;
using System.Threading.Tasks;
using Redb.OBAC.Backends;
using Redb.OBAC.Backends.InMemory;
using Redb.OBAC.Core;
using Redb.OBAC.EF.DB;

namespace Redb.OBAC.EF.BL
{
    public class PermissionChecker : IObacPermissionChecker
    {
        private readonly ObjectStorage _store;
        private readonly IObacCacheBackend _cacheBackend;
        private readonly int _userId;

        public PermissionChecker(ObjectStorage storage, IObacCacheBackend cacheBackend, int currentUserId)
        {
            _store = storage;
            _cacheBackend = cacheBackend;
            _userId = currentUserId;
        }

       
        public async Task<Guid[]> GetObjectPermissions(Guid objectTypeId, int? objectId = null)
        {
            var permList = _cacheBackend.GetPermissionsFor(_userId, objectTypeId, objectId);
            if (permList != null)
                return permList;

            permList = await _store.GetEffectivePermissionsForUser(_userId, objectTypeId, objectId);
            _cacheBackend.SetPermissions(_userId, objectTypeId, objectId, permList);
            
            return permList;
        }


        public async Task<bool> CheckObjectPermissions(Guid objectTypeId, int objectId, Guid permissionId)
        {
            var effPermissions = await GetObjectPermissions(objectTypeId, objectId);
            return effPermissions.Contains(permissionId);
        }

        public async Task<bool> CheckObjectPermissions(Guid objectTypeId, int objectId, Guid[] permissionIds)
        {
            var effPermissions = await GetObjectPermissions(objectTypeId, objectId);
            return permissionIds.All(pid => effPermissions.Contains(pid));
        }
    }
}