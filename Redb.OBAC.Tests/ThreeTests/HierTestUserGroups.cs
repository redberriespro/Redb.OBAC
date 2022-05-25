using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Redb.OBAC.Core;
using Redb.OBAC.Core.Models;
using Redb.OBAC.Tests.Utils;

namespace Redb.OBAC.Tests.ThreeTests
{
    public class HierTestUserGroups: TestBase
    {
        private int User1 = 744001;
        private int User2 = 744002;
        private int User3 = 744003;
        private int User4 = 744004;
        private int UserGroup1 = 744004;
        private int UserGroup2 = 744005;

        private int Node10 = 10;
        private int Node100 = 100;
        private int Node110 = 110;
        private int Node111 = 111;
        private int Node200 = 200;
        
        private Guid DeletePermId = new Guid("02DB5F72-8D70-41E5-A70A-922899A07601");
        private Guid UpdatePermId = new Guid("02C2BACE-52C8-4DD4-97C0-41A0837C8602");
        private Guid ViewPermId = new Guid("02869541-AE42-42D8-B482-CAACC1556F03");

        public HierTestUserGroups(string dbName) : base(dbName) { }

        private async Task EnsureObjects(IObacObjectManager om)
        {
            await om.EnsurePermission(DeletePermId, "_delete");
            await om.EnsurePermission(UpdatePermId, "_update");
            await om.EnsurePermission(ViewPermId, "_view");
            
            await om.EnsureUser(User1, "u1");
            await om.EnsureUser(User2, "u2");
            await om.EnsureUser(User3, "u3");
            await om.EnsureUser(User4, "u4");

            await om.EnsureUserGroup(UserGroup1, "ug1");
            await om.EnsureUserGroup(UserGroup2, "ug2");
        }

        private async Task EnsureTreeObjects(IObacObjectManager om, Guid treeId)
        {
            await om.EnsureTree(treeId, "_oug");
            await om.EnsureTreeNode(treeId, Node10, null, User1);
            await om.EnsureTreeNode(treeId, Node100, Node10, User1);
            await om.EnsureTreeNode(treeId, Node110, Node100, User1);
            await om.EnsureTreeNode(treeId, Node111, Node110, User1);
            await om.EnsureTreeNode(treeId, Node200, null, User1);
        }

        [Test]
        public async Task GroupTest1()
        {          
            var treeId = Guid.NewGuid();
                
            var conf = GetConfiguration();
            var om = conf.GetObjectManager();
            
            await EnsureObjects(om);
            await EnsureTreeObjects(om, treeId);
            
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
                        UserGroupId = UserGroup1,
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
                        PermissionId = UpdatePermId,
                        Kind = PermissionKindEnum.Deny
                    },
                    new AclItemInfo
                    {
                        UserGroupId = UserGroup2,
                        PermissionId = UpdatePermId,
                        Kind = PermissionKindEnum.Allow
                    }
                }
            });
            
            await om.SetTreeNodeAcl(treeId, Node111, new AclInfo
            {
                InheritParentPermissions = true,
                AclItems = new[]
                {
                    new AclItemInfo
                    {
                        UserId = User1,
                        PermissionId = UpdatePermId,
                        Kind = PermissionKindEnum.Allow
                    },
                    new AclItemInfo
                    {
                        UserGroupId = UserGroup1,
                        PermissionId = DeletePermId,
                        Kind = PermissionKindEnum.Allow
                    }
                }
            });

            var permCheckerUser1 = GetConfiguration().GetPermissionChecker(User1);
            var permCheckerUser2 = GetConfiguration().GetPermissionChecker(User2);
            var permCheckerUser3 = GetConfiguration().GetPermissionChecker(User3);
            var permCheckerUser4 = GetConfiguration().GetPermissionChecker(User4);

            
            await om.AddUserToUserGroup(UserGroup1, User2);
            await om.AddUserToUserGroup(UserGroup1, User3);
            
            await om.AddUserToUserGroup(UserGroup2, User3);
            await om.AddUserToUserGroup(UserGroup2, User4);
            
            // 100 - ug1: view, u1:view
            // 110 - ug2: edit, u2:edit:DENY
            // 111 - u1: edit, ug1:delete
            // ug1=2,3 ug2=3,4
            
            Assert.IsTrue(await permCheckerUser1.CheckObjectPermissions(treeId, Node111,  ViewPermId));
            Assert.IsTrue(await permCheckerUser1.CheckObjectPermissions(treeId, Node111,  UpdatePermId));
            Assert.IsFalse(await permCheckerUser1.CheckObjectPermissions(treeId, Node111,  DeletePermId));
            
            Assert.IsTrue(await permCheckerUser2.CheckObjectPermissions(treeId, Node111,  ViewPermId));
            Assert.IsFalse(await permCheckerUser2.CheckObjectPermissions(treeId, Node111,  UpdatePermId));
            Assert.IsTrue(await permCheckerUser2.CheckObjectPermissions(treeId, Node111,  DeletePermId));
            
            Assert.IsTrue(await permCheckerUser3.CheckObjectPermissions(treeId, Node111,  ViewPermId));
            Assert.IsTrue(await permCheckerUser3.CheckObjectPermissions(treeId, Node111,  UpdatePermId));
            Assert.IsTrue(await permCheckerUser3.CheckObjectPermissions(treeId, Node111,  DeletePermId));
            
            Assert.IsFalse(await permCheckerUser4.CheckObjectPermissions(treeId, Node111,  ViewPermId));
            Assert.IsTrue(await permCheckerUser4.CheckObjectPermissions(treeId, Node111,  UpdatePermId));
            Assert.IsFalse(await permCheckerUser4.CheckObjectPermissions(treeId, Node111,  DeletePermId));

            
            await om.RemoveUserFromUserGroup(UserGroup2, User4);
            await om.RemoveUserFromUserGroup(UserGroup1, User2);
            
            await AssertRights2(permCheckerUser1, treeId, permCheckerUser2, permCheckerUser3, permCheckerUser4);

            await om.RepairTreeNodeEffectivePermissions(treeId, Node10);
            
            await AssertRights2(permCheckerUser1, treeId, permCheckerUser2, permCheckerUser3, permCheckerUser4);
        }

        private async Task AssertRights2(IObacPermissionChecker permCheckerUser1, Guid treeId,
            IObacPermissionChecker permCheckerUser2, IObacPermissionChecker permCheckerUser3,
            IObacPermissionChecker permCheckerUser4)
        {
            Assert.IsTrue(await permCheckerUser1.CheckObjectPermissions(treeId, Node111, ViewPermId));
            Assert.IsTrue(await permCheckerUser1.CheckObjectPermissions(treeId, Node111, UpdatePermId));
            Assert.IsFalse(await permCheckerUser1.CheckObjectPermissions(treeId, Node111, DeletePermId));

            Assert.IsFalse(await permCheckerUser2.CheckObjectPermissions(treeId, Node111, ViewPermId));
            Assert.IsFalse(await permCheckerUser2.CheckObjectPermissions(treeId, Node111, UpdatePermId));
            Assert.IsFalse(await permCheckerUser2.CheckObjectPermissions(treeId, Node111, DeletePermId));

            Assert.IsTrue(await permCheckerUser3.CheckObjectPermissions(treeId, Node111, ViewPermId));
            Assert.IsTrue(await permCheckerUser3.CheckObjectPermissions(treeId, Node111, UpdatePermId));
            Assert.IsTrue(await permCheckerUser3.CheckObjectPermissions(treeId, Node111, DeletePermId));

            Assert.IsFalse(await permCheckerUser4.CheckObjectPermissions(treeId, Node111, ViewPermId));
            Assert.IsFalse(await permCheckerUser4.CheckObjectPermissions(treeId, Node111, UpdatePermId));
            Assert.IsFalse(await permCheckerUser4.CheckObjectPermissions(treeId, Node111, DeletePermId));
        }
    }
}