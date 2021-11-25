using System;
using Redb.OBAC.BL;
using Redb.OBAC.Core;
using Redb.OBAC.DB;

namespace Redb.OBAC
{
    /// <summary>
    /// starting point of OBAC management
    /// </summary>
    public class ObacManager
    {
        /// <summary>
        /// configure with default in-memory storage and no caching
        /// </summary>
        public static IObacConfiguration CreateDemoConfiguration()
        {
            throw new NotImplementedException();
        }
        
        /// <summary>
        /// configure with default storage and no caching
        /// </summary>
        public static IObacConfiguration CreateConfiguration(IObacStorageProvider storageProvider)
        {
            var cfg = new ObacConfiguration();
            cfg.Initialize(storageProvider);
            return cfg;
        }
        
        /// <summary>
        /// configure with default storage, no caching and Extra EP receiver
        /// </summary>
        public static IObacConfiguration CreateConfiguration(IObacStorageProvider storageProvider, 
            IEffectivePermissionFeed extraFeed)
        {
            var cfg = new ObacConfiguration();
            cfg.Initialize(storageProvider, new []{extraFeed});
            return cfg;
        }
    }
}