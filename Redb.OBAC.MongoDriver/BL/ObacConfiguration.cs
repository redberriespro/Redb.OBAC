using System;
using System.Threading.Tasks;
using Redb.OBAC.Backends.InMemory;
using Redb.OBAC.Core;
using Redb.OBAC.MongoDriver.DB;
using Redb.OBAC.Exceptions;

namespace Redb.OBAC.MongoDriver.BL
{
    public class ObacConfiguration: IObacConfiguration
    {
        private IObacStorageProvider _storageProvider;
        private ObjectManager _objectManager;
        private InMemoryObacCacheBackend _cacheBackend;
        private ObjectStorage _objectStorage;

        public IObacObjectManager GetObjectManager()
        {
            if (_objectManager == null)
                throw new ObacException("configuration must be initialized first");
            return _objectManager;
        }

        public IObacPermissionChecker GetPermissionChecker(int currentUserId)
        {
            if (_objectManager == null)
                throw new ObacException("configuration must be initialized first");

            return new PermissionChecker(_objectStorage, _cacheBackend, currentUserId);
        }

        public void Initialize(IObacStorageProvider storageProvider, IEffectivePermissionFeed[] extraFeeds = null)
        {
            _cacheBackend = new InMemoryObacCacheBackend();
            _objectStorage = new ObjectStorage(storageProvider);

            var exFeeds = extraFeeds ?? Array.Empty<IEffectivePermissionFeed>();
            
            _objectManager = new ObjectManager(_objectStorage, _cacheBackend, exFeeds);
        }
    }
}