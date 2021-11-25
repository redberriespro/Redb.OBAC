using System;
using System.Linq;
using NUnit.Framework;
using Redb.OBAC.Backends.InMemory;

namespace Redb.OBAC.Tests.CacheTests
{
    [TestFixture]
    public class InMemoryCacheBackendTests
    {
        [Test]
        public void InvalidatePerms()
        {
            var ot = Guid.NewGuid();
            var p1 = Guid.NewGuid();
            var p2 = Guid.NewGuid();
            
            var cache = new InMemoryObacCacheBackend();
            
            cache.SetPermissions(1,ot , 10, new []{p1} );
            cache.SetPermissions(2,ot , 20, new []{p1,p2} );

            var pres = cache.GetPermissionsFor(1, ot, 10);
            Assert.AreEqual(p1, pres.Single());
            
            pres = cache.GetPermissionsFor(2, ot, 20);
            Assert.AreEqual(2, pres.Length);

            cache.InvalidateForUser(2);

            pres = cache.GetPermissionsFor(1, ot, 10);
            Assert.AreEqual(p1, pres.Single());
            
            pres = cache.GetPermissionsFor(2, ot, 20);
            Assert.IsNull(pres);

            cache.InvalidatePermissionsForObject(ot,99);

            pres = cache.GetPermissionsFor(1, ot, 10);
            Assert.AreEqual(p1, pres.Single());
            
            cache.SetPermissions(2,ot , 10, new []{p2} );
            cache.SetPermissions(2,ot , 20, new []{p1,p2} );
            
            cache.InvalidatePermissionsForObject(ot,10);

            pres = cache.GetPermissionsFor(2, ot, 20);
            Assert.AreEqual(2, pres.Length);

            Assert.IsNull(cache.GetPermissionsFor(1, ot, 10));
            Assert.IsNull(cache.GetPermissionsFor(2, ot, 10));
        }
    }
}