using System;
using System.Linq;
using NUnit.Framework;
using Redb.OBAC.Core.Models;
using Redb.OBAC.ModelsPrivate;
using Redb.OBAC.Tree;

namespace Redb.OBAC.Tests.MiscTests
{
    [TestFixture]
    public class AclComparerTests
    {
        private Guid ManageRoleId = new Guid("01D6D727-F1C1-45D7-94CC-5D6D72012733");
        private Guid ViewRoleId = new Guid("01F0E312-9BA9-43A3-9A62-84D6B2101D05");

        [Test]
        public void ComparerTest1()
        {
            var oldAcl = new AclInfo
            {
                InheritParentPermissions = false,
                AclItems = new[]
                {
                    new AclItemInfo
                    {
                        Kind = PermissionKindEnum.Allow, PermissionId = ViewRoleId, UserId = 1
                    },
                    new AclItemInfo
                    {
                        Kind = PermissionKindEnum.Allow, PermissionId = ViewRoleId, UserId = 10
                    },
                }
            };

            var newAcl = new AclInfo
            {
                InheritParentPermissions = true,
                AclItems = new[]
                {
                    new AclItemInfo
                    {
                        Kind = PermissionKindEnum.Deny, PermissionId = ViewRoleId, UserId = 1
                    },
                    new AclItemInfo
                    {
                        Kind = PermissionKindEnum.Deny, PermissionId = ManageRoleId, UserId = 1
                    },
                    new AclItemInfo
                    {
                        Kind = PermissionKindEnum.Allow, PermissionId = ViewRoleId, UserId = 10
                    },
                }
            };

            var res = AclComparer.CompareAcls(oldAcl, newAcl);

            Assert.IsTrue(res.InheritParentPermissionsAction == NodeParentPermissionInheritanceActionEnum.SetInherit);
            Assert.AreEqual(1, res.AclItemsToBeRemoved.Count);
            Assert.AreEqual(2, res.AclItemsToBeAdded.Count);

            var a1 = res.AclItemsToBeAdded.First(a => a.PermissionId == ViewRoleId);
            var a2 = res.AclItemsToBeAdded.First(a => a.PermissionId == ManageRoleId);
            Assert.AreEqual("1::01f0e312-9ba9-43a3-9a62-84d6b2101d05:D",a1.ToString());
            Assert.AreEqual("1::01d6d727-f1c1-45d7-94cc-5d6d72012733:D",a2.ToString());

            
            Assert.AreEqual("1::01f0e312-9ba9-43a3-9a62-84d6b2101d05:A",res.AclItemsToBeRemoved.First().ToString());
            
        }
    }
}