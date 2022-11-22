using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Redb.OBAC.ApiHost;
using Redb.OBAC.Core;
using Redb.OBAC.Core.Models;
using Redb.OBAC.EF.DB.Entities;
using Redb.OBAC.Exceptions;
using Redb.OBAC.Tests.Utils;
using Redberries.OBAC.Api;

namespace Redb.OBAC.Tests.ThreeTests
{
    public class HierTest: TestBase
    { 
        private Guid OrgUnitTreeId = new Guid("01FAEDB2-F149-4781-B32E-7F81993AD39F");
        private Guid DeletePermId = new Guid("01DB5F72-8D70-41E5-A70A-922899A076EF");
        private Guid UpdatePermId = new Guid("01C2BACE-52C8-4DD4-97C0-41A0837C8642");
        private Guid ViewPermId = new Guid("01869541-AE42-42D8-B482-CAACC1556F66");

        private Guid ManageRoleId = new Guid("01D6D727-F1C1-45D7-94CC-5D6D72012733");
        private Guid ViewRoleId = new Guid("01F0E312-9BA9-43A3-9A62-84D6B2101D05");


        private int User1 = 745001;
        private int User2 = 745002;
        private int User3 = 745003;
        
        private int Node100 = 100;
        private int Node110 = 110;
        private int Node120 = 120;
        private int Node130 = 130;
        private int Node200 = 200;

        public HierTest(string dbName) : base(dbName) { }

        private async Task EnsureObjects(IObacObjectManager om)
        {
            await om.EnsurePermission(DeletePermId, "_delete");
            await om.EnsurePermission(UpdatePermId, "_update");
            await om.EnsurePermission(ViewPermId, "_view");

            await om.EnsureRole(ManageRoleId, "_managerole", new[]
            {
                UpdatePermId, DeletePermId
            });

            await om.EnsureRole(ViewRoleId, "_viewrole", new[]
            {
                ViewRoleId
            });


            await om.EnsureUser(User1, "u1");
            await om.EnsureUser(User2, "u2");
            await om.EnsureUser(User3, "u3");
        }
        
        private async Task EnsureTreeObjects(IObacObjectManager om, Guid treeId)
        {
            await om.EnsureTree(treeId, "_ou");
            await om.EnsureTreeNode(treeId, Node100, null, User3);
            await om.EnsureTreeNode(treeId, Node110, Node100, User3);
            await om.EnsureTreeNode(treeId, Node120, Node100, User3);
            await om.EnsureTreeNode(treeId, Node130, Node100, User3);
            await om.EnsureTreeNode(treeId, Node200, null, User3);

        }

        [Test]
        public async Task Hier1()
        {
            var conf = GetConfiguration();
            var om = conf.GetObjectManager();
            
            await EnsureObjects(om);
            await EnsureTreeObjects(om, OrgUnitTreeId);

            // set ACL
            // 100 u1:view
            // 110 (X) u2:view
            // 120 u1:DENY view, u2:update
            // 130 u2:view
            
            await om.SetTreeNodeAcl(OrgUnitTreeId, Node100, new AclInfo
            {
                InheritParentPermissions = true,
                AclItems = new[]
                {
                    new AclItemInfo
                    {
                        UserId = User1,
                        PermissionId = ViewPermId,
                        Kind = PermissionKindEnum.Allow
                    }
                }
            });
            
            await om.SetTreeNodeAcl(OrgUnitTreeId, Node110, new AclInfo
            {
                InheritParentPermissions = false,
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

            var acl110 = await om.GetTreeNodeAcl(OrgUnitTreeId, Node110);
            Assert.IsFalse(acl110.InheritParentPermissions);
            Assert.AreEqual(1, acl110.AclItems.Length);
            
            await om.SetTreeNodeAcl(OrgUnitTreeId, Node120, new AclInfo
            {
                InheritParentPermissions = true,
                AclItems = new[]
                {
                    new AclItemInfo
                    {
                        UserId = User1,
                        PermissionId = ViewPermId,
                        Kind = PermissionKindEnum.Deny
                    },
                    new AclItemInfo
                    {
                        UserId = User2,
                        PermissionId = UpdatePermId,
                        Kind = PermissionKindEnum.Allow
                    }
                }
            });

            var acl120 = await om.GetTreeNodeAcl(OrgUnitTreeId, Node120);
            Assert.IsTrue(acl120.InheritParentPermissions);
            Assert.AreEqual(2, acl120.AclItems.Length);
            AssertContainAclNode(acl120.AclItems, User1, null, ViewPermId, PermissionKindEnum.Deny);
            AssertContainAclNode(acl120.AclItems, User2, null, UpdatePermId, PermissionKindEnum.Allow);

            
            await om.SetTreeNodeAcl(OrgUnitTreeId, Node130, new AclInfo
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

            await AssertTreeRights(OrgUnitTreeId, extraChecks:true);

            await om.RepairTreeNodeEffectivePermissions(OrgUnitTreeId, Node200);
            
            await AssertTreeRights(OrgUnitTreeId);

            await om.RepairTreeNodeEffectivePermissions(OrgUnitTreeId, Node110);
            await om.RepairTreeNodeEffectivePermissions(OrgUnitTreeId, Node120);

            await AssertTreeRights(OrgUnitTreeId);

            await om.RepairTreeNodeEffectivePermissions(OrgUnitTreeId, Node100);

            await AssertTreeRights(OrgUnitTreeId);
            
            // now try to broke acls
            Assert.ThrowsAsync<ObacException>(async () =>
                await om.SetTreeNodeAcl(OrgUnitTreeId, Node100, new AclInfo
                {
                    InheritParentPermissions = true,
                    AclItems = new[]
                    {
                        new AclItemInfo
                        {
                            UserId = 334834727,
                            PermissionId = Guid.NewGuid(),
                            Kind = PermissionKindEnum.Allow
                        }
                    }
                })
            );

            await AssertTreeRights(OrgUnitTreeId);
        }


        [Test]
        public async Task Hier2()
        {
            var conf = GetConfiguration();
            var om = conf.GetObjectManager();

            var treeId = Guid.NewGuid();
            
            await EnsureObjects(om);
            // set ACL
            // 100 u1:view
            // 130 u2:view

            await om.EnsureTree(treeId, "_ou_hier2");
            await om.EnsureTreeNode(treeId, Node100, null, User3);
            await om.EnsureTreeNode(treeId, Node130, Node100, User3);

            await om.SetTreeNodeAcl(treeId, Node100, new AclInfo
            {
                InheritParentPermissions = true,
                AclItems = new[]
                {
                    new AclItemInfo
                    {
                        UserId = User1,
                        PermissionId = ViewPermId,
                        Kind = PermissionKindEnum.Allow
                    }
                }
            });
            
            await om.SetTreeNodeAcl(treeId, Node130, new AclInfo
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

            var storage = GetObjectStorage();

            var nodeEp = await storage.GetEffectivePermissionsForAllUsers(treeId, Node130);
            Assert.AreEqual(2, nodeEp.Count);
        }

        [Test]
        public async Task HierSequentialAdd()
        {
            var conf = GetConfiguration();
            var om = conf.GetObjectManager();
            
            await EnsureObjects(om);
            
            // set ACL
            // 100 u1:view
            // 110 (X) u2:view
            // 120 u1:DENY view, u2:update
            // 130 u2:view

            var treeId = Guid.NewGuid();
            
            await om.EnsureTree(treeId, "_ou_seq");
            await om.EnsureTreeNode(treeId, Node100, null, User3);
            await om.SetTreeNodeAcl(treeId, Node100, new AclInfo
            {
                InheritParentPermissions = true,
                AclItems = new[]
                {
                    new AclItemInfo
                    {
                        UserId = User1,
                        PermissionId = ViewPermId,
                        Kind = PermissionKindEnum.Allow
                    }
                }
            });
            
            await om.EnsureTreeNode(treeId, Node110, Node100, User3);
            
            await om.SetTreeNodeAcl(treeId, Node110, new AclInfo
            {
                InheritParentPermissions = false,
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
            
            
            await om.EnsureTreeNode(treeId, Node120, Node100, User3);
            
            await om.SetTreeNodeAcl(treeId, Node120, new AclInfo
            {
                InheritParentPermissions = true,
                AclItems = new[]
                {
                    new AclItemInfo
                    {
                        UserId = User1,
                        PermissionId = ViewPermId,
                        Kind = PermissionKindEnum.Deny
                    },
                    new AclItemInfo
                    {
                        UserId = User2,
                        PermissionId = UpdatePermId,
                        Kind = PermissionKindEnum.Allow
                    }
                }
            });

            
            await om.EnsureTreeNode(treeId, Node130, Node100, User3);
            
            await om.SetTreeNodeAcl(treeId, Node130, new AclInfo
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
            
            await om.EnsureTreeNode(treeId, Node200, null, User3);


            await AssertTreeRights(treeId, extraChecks:true);
        }
        
          [Test]
        public async Task HierSequentialAdd2()
        {
            var conf = GetConfiguration();
            var om = conf.GetObjectManager();
            
            await EnsureObjects(om);
            
            // set ACL
            // 100 u1:view
            // 110 --
            
            var treeId = Guid.NewGuid();
            
            await om.EnsureTree(treeId, "_ou_seq2");
            await om.EnsureTreeNode(treeId, Node100, null, User3);
            await om.SetTreeNodeAcl(treeId, Node100, new AclInfo
            {
                InheritParentPermissions = true,
                AclItems = new[]
                {
                    new AclItemInfo
                    {
                        UserId = User1,
                        PermissionId = ViewPermId,
                        Kind = PermissionKindEnum.Allow
                    }
                }
            });
            
            await om.EnsureTreeNode(treeId, Node110, Node100, User3);
            await om.EnsureTreeNode(treeId, 111, Node110, User2);
            
            var storage = GetObjectStorage();

            var nodeEp = await storage.GetEffectivePermissionsForAllUsers(treeId, Node100);
            Assert.AreEqual(1, nodeEp.Count);
            AssertContainEffectivePermission(nodeEp, treeId, User1, ViewPermId, Node100);
            
            nodeEp = await storage.GetEffectivePermissionsForAllUsers(treeId, Node110);
            Assert.AreEqual(1, nodeEp.Count);
            AssertContainEffectivePermission(nodeEp, treeId, User1, ViewPermId, Node110);
            
            nodeEp = await storage.GetEffectivePermissionsForAllUsers(treeId, 111);
            Assert.AreEqual(1, nodeEp.Count);
            AssertContainEffectivePermission(nodeEp, treeId, User1, ViewPermId, 111);

        }
        
          [Test]
        public async Task HierUpdateAcls()
        {
            var conf = GetConfiguration();
            var om = conf.GetObjectManager();
            var u1checker = conf.GetPermissionChecker(User1);
            var u2checker = conf.GetPermissionChecker(User2);
            var u3checker = conf.GetPermissionChecker(User3);
            
            await EnsureObjects(om);
            
            // step1 acl
            // 100 u1:view
            // 110 u2:view
            
            var treeId = Guid.NewGuid();
            await om.EnsureTree(treeId, "_ou_updacl");
            await om.EnsureTreeNode(treeId, Node100, null, User3);
            await om.EnsureTreeNode(treeId, Node110, Node100, User3);
            
            await om.SetTreeNodeAcl(treeId, Node100, new AclInfo
            {
                InheritParentPermissions = true,
                AclItems = new[]
                {
                    new AclItemInfo
                    {
                        UserId = User1,
                        PermissionId = ViewPermId,
                        Kind = PermissionKindEnum.Allow
                    }
                }
            });
            
            await om.SetTreeNodeAcl(treeId, Node110, new AclInfo
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
            
            Assert.IsTrue(await u1checker.CheckObjectPermissions(treeId, Node100, ViewPermId));
            Assert.IsTrue(await u1checker.CheckObjectPermissions(treeId, Node110, ViewPermId));
            Assert.IsFalse(await u3checker.CheckObjectPermissions(treeId, Node110, UpdatePermId));

            Assert.IsFalse(await u2checker.CheckObjectPermissions(treeId, Node100, ViewPermId));
            Assert.IsTrue(await u2checker.CheckObjectPermissions(treeId, Node110, ViewPermId));
            
            // step2 acl
            // 100 u1:view u3:upd
            // 110 u1:view:DENY, u2:view
            
            await om.SetTreeNodeAcl(treeId, Node100, new AclInfo
            {
                InheritParentPermissions = true,
                AclItems = new[]
                {
                    new AclItemInfo
                    {
                        UserId = User1,
                        PermissionId = ViewPermId,
                        Kind = PermissionKindEnum.Allow
                    },
                    new AclItemInfo
                    {
                        UserId = User3,
                        PermissionId = UpdatePermId,
                        Kind = PermissionKindEnum.Allow
                    },
                }
            });
            
            await om.SetTreeNodeAcl(treeId, Node110, new AclInfo
            {
                InheritParentPermissions = true,
                AclItems = new[]
                {
                    new AclItemInfo
                    {
                        UserId = User1,
                        PermissionId = ViewPermId,
                        Kind = PermissionKindEnum.Deny
                    },
                    new AclItemInfo
                    {
                        UserId = User2,
                        PermissionId = ViewPermId,
                        Kind = PermissionKindEnum.Allow
                    }
                }
            });

            Assert.IsTrue(await u1checker.CheckObjectPermissions(treeId, Node100, ViewPermId));
            Assert.IsFalse(await u2checker.CheckObjectPermissions(treeId, Node100, ViewPermId));
            Assert.IsTrue(await u3checker.CheckObjectPermissions(treeId, Node110, UpdatePermId));
            
            Assert.IsFalse(await u1checker.CheckObjectPermissions(treeId, Node110, ViewPermId));
            Assert.IsTrue(await u2checker.CheckObjectPermissions(treeId, Node110, ViewPermId));
            Assert.IsTrue(await u3checker.CheckObjectPermissions(treeId, Node110, UpdatePermId));

        }
        
        [Test]
        public async Task HierNodeReassign()
        {
            var conf = GetConfiguration();
            var om = conf.GetObjectManager();
            
            await EnsureObjects(om);
            
            // step 1: 130 will be under 110 (in wrong place) and 200 under 130
            
            // set ACL
            // 100 u1:view
            // 110 (X) u2:view
            // 120 u1:DENY view, u2:update
            // 130 u2:view
            // 200

            var treeId = Guid.NewGuid();
            
            await om.EnsureTree(treeId, "_ou_seq");
            await om.EnsureTreeNode(treeId, Node100, null, User3);
            await om.SetTreeNodeAcl(treeId, Node100, new AclInfo
            {
                InheritParentPermissions = true,
                AclItems = new[]
                {
                    new AclItemInfo
                    {
                        UserId = User1,
                        PermissionId = ViewPermId,
                        Kind = PermissionKindEnum.Allow
                    }
                }
            });
            
            await om.EnsureTreeNode(treeId, Node110, Node100, User3);
            
            await om.SetTreeNodeAcl(treeId, Node110, new AclInfo
            {
                InheritParentPermissions = false,
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
            
            
            await om.EnsureTreeNode(treeId, Node120, Node100, User3);
            
            await om.SetTreeNodeAcl(treeId, Node120, new AclInfo
            {
                InheritParentPermissions = true,
                AclItems = new[]
                {
                    new AclItemInfo
                    {
                        UserId = User1,
                        PermissionId = ViewPermId,
                        Kind = PermissionKindEnum.Deny
                    },
                    new AclItemInfo
                    {
                        UserId = User2,
                        PermissionId = UpdatePermId,
                        Kind = PermissionKindEnum.Allow
                    }
                }
            });

            
            await om.EnsureTreeNode(treeId, Node130, Node110, User3);
            
            await om.SetTreeNodeAcl(treeId, Node130, new AclInfo
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
            
            await om.EnsureTreeNode(treeId, Node200, 130, User3);

            // step 2: 130 and 200 come to the right place, all permissions must be set

            await om.EnsureTreeNode(treeId, Node200, null, User3);
            await om.EnsureTreeNode(treeId, Node130, Node100, User3);

            await AssertTreeRights(treeId);

            var nd130 = await om.GetTreeNode(treeId, Node130);
            Assert.AreEqual(User3, nd130.OwnerUserid);

        }
      

        private async Task AssertTreeRights(Guid treeId, bool extraChecks = false)
        {
            var storage = GetObjectStorage();

            var nodeEp = await storage.GetEffectivePermissionsForAllUsers(treeId, Node100);
            Assert.AreEqual(1, nodeEp.Count);
            AssertContainEffectivePermission(nodeEp, treeId, User1, ViewPermId, Node100);

            nodeEp = await storage.GetEffectivePermissionsForAllUsers(treeId, Node110);
            Assert.AreEqual(1, nodeEp.Count);
            AssertContainEffectivePermission(nodeEp, treeId, User2, ViewPermId, Node110);

            nodeEp = await storage.GetEffectivePermissionsForAllUsers(treeId, Node120);
            Assert.AreEqual(1, nodeEp.Count);
            AssertContainEffectivePermission(nodeEp, treeId, User2, UpdatePermId, Node120);


            nodeEp = await storage.GetEffectivePermissionsForAllUsers(treeId, Node130);
            Assert.AreEqual(2, nodeEp.Count);
            AssertContainEffectivePermission(nodeEp, treeId, User1, ViewPermId, Node130);
            AssertContainEffectivePermission(nodeEp, treeId, User2, ViewPermId, Node130);

            if (!extraChecks) return;
            var permCheckerUser1 = GetConfiguration().GetPermissionChecker(User1);
            var permCheckerUser2 = GetConfiguration().GetPermissionChecker(User2);
            
            Assert.IsTrue(await permCheckerUser1.CheckObjectPermissions(treeId, Node100,  ViewPermId));
            Assert.IsFalse(await permCheckerUser1.CheckObjectPermissions(treeId, Node100,  UpdatePermId));
            Assert.IsFalse(await permCheckerUser2.CheckObjectPermissions(treeId, Node100,  ViewPermId));

            Assert.IsFalse(await permCheckerUser1.CheckObjectPermissions(treeId, Node110,  ViewPermId));
            Assert.IsTrue(await permCheckerUser2.CheckObjectPermissions(treeId, Node110,  ViewPermId));

            Assert.IsFalse(await permCheckerUser1.CheckObjectPermissions(treeId, Node120,  UpdatePermId));
            Assert.IsTrue(await permCheckerUser2.CheckObjectPermissions(treeId, Node120,  UpdatePermId));

            Assert.IsTrue(await permCheckerUser1.CheckObjectPermissions(treeId, Node130,  ViewPermId));
            Assert.IsTrue(await permCheckerUser2.CheckObjectPermissions(treeId, Node130,  ViewPermId));
            
        }

        private void AssertContainAclNode(AclItemInfo[] aclItems, int? userId, int? userGroupId, Guid permId, PermissionKindEnum kind)
        {
            var ep = aclItems.SingleOrDefault(
                e => e.UserId == userId
                     && e.UserGroupId == userGroupId
                     && e.PermissionId == permId
                     && e.Kind == kind
            );
            if (ep == null)
                throw new AssertionException(
                    $"ACL item not found: user {userId} group {userGroupId} perm {permId} kind {kind}");
        }
        
        private void AssertContainEffectivePermission(List<TreeNodePermissionInfo> eps, Guid objectTypeId, int userId, Guid permId, int objectId)
        {
            var ep = eps.SingleOrDefault(
                e =>  e.NodeId == objectId
                && e.PermissionId == permId
                && e.UserId == userId
            );
            if (ep == null)
                throw new AssertionException(
                    $"EP not found: type {objectTypeId} obj {objectId} user {userId} perm {permId}");
        }
    }
}