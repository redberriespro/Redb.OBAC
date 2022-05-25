using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Redb.OBAC.Core;
using Redb.OBAC.Core.Models;
using Redb.OBAC.Exceptions;
using Redb.OBAC.Tests.ObacClientTests.Entities;
using Redb.OBAC.Tests.Utils;

namespace Redb.OBAC.Tests.ObacClientTests
{
    public class EpReceiverTests: TestBase
    {
        private Guid HouseCaseTreeId = new Guid("15D11960-4D04-4961-8C5E-E7B357BFB671");
        private Guid ViewPermId = new Guid("01869541-AE42-42D8-B482-CAACC1556F66");
        
        private int User1 = 745101;
        private int User2 = 745102;
        private int Node100 = 100;

        public EpReceiverTests(string dbName) : base(dbName) { }

        private async Task EnsureObjects(IObacObjectManager om)
        {
            await om.EnsurePermission(ViewPermId, "_view");

            await om.EnsureUser(User1, "houseuser1");
            await om.EnsureUser(User2, "houseuser2");

            await om.EnsureTree(HouseCaseTreeId, "_hctree");
            await om.EnsureTreeNode(HouseCaseTreeId, Node100, null, User1);
        }
        
        [Test]
        public async Task SimpleEpTest()
        {
            var conf = GetConfiguration();
            var om = conf.GetObjectManager();
            
            await EnsureObjects(om);

            var h1 = await TestDbContext.Houses.SingleOrDefaultAsync(a => a.Id == Node100);
            if (h1 != null)
            {
                TestDbContext.Houses.Remove(h1);
                await TestDbContext.SaveChangesAsync();
            }

            h1 = new HouseTestEntity
            {
                Id = Node100,
                Name = "house100"
            };
            await TestDbContext.Houses.AddAsync(h1);
            await TestDbContext.SaveChangesAsync();
            
            // set ACL house 100 user 2 view
            await om.SetTreeNodeAcl(HouseCaseTreeId, Node100, new AclInfo
            {
                InheritParentPermissions = true,
                AclItems = new[]
                {
                    new AclItemInfo
                    {
                        UserId = User2,
                        PermissionId = ViewPermId,
                        Kind = PermissionKindEnum.Allow
                    }
                }
            });

            var c = TestDbContext;
            var houses = from h in c.Houses
                join p in c.EffectivePermissions
                    on h.Id equals p.ObjectId
                where
                    p.ObjectTypeId == HouseCaseTreeId && p.UserId == User2
                select h;

            var h2 = await houses.ToListAsync();
            Assert.AreEqual(1, h2.Count);
            
            houses = from h in c.Houses
                join p in c.EffectivePermissions
                    on h.Id equals p.ObjectId
                where
                    p.ObjectTypeId == HouseCaseTreeId && p.UserId == User1
                select h;

            h2 = await houses.ToListAsync();
            Assert.AreEqual(0, h2.Count);

        }

    }
}