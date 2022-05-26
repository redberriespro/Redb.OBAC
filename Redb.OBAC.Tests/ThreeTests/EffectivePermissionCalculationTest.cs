using System;
using System.Linq;
using NUnit.Framework;
using Redb.OBAC.Core.Models;
using Redb.OBAC.Models;
using Redb.OBAC.Tests.Utils;
using Redb.OBAC.Tree;

namespace Redb.OBAC.Tests.ThreeTests
{
    public class EffectivePermissionCalculationTest: TestBase
    {
        private Guid Perm_Read = new Guid("7C74B8CF-FDFE-404E-972E-78C3E5145101");
        private Guid Perm_Change = new Guid("7C74B8CF-FDFE-404E-972E-78C3E5145102");
        private Guid Perm_Delete = new Guid("7C74B8CF-FDFE-404E-972E-78C3E5145103");

        public EffectivePermissionCalculationTest(string dbName) : base(dbName) { }

        [Test]
        public void Test1()
        {
            var c = new EffectivePermissionCalculator();
            
            var res = c.CalculateEffectivePermissions(88,true, new [] {
                new TreeNodePermissionInfo { PermissionId = Perm_Read, UserId = 1},
                new TreeNodePermissionInfo { PermissionId = Perm_Read, UserId = 1}
                }, new [] {
                    new TreeNodePermissionInfo { PermissionId = Perm_Read, UserId = 1},
                    new TreeNodePermissionInfo { PermissionId = Perm_Read, UserId = 1}
                }, new [] {
                    new TreeNodePermissionInfo { PermissionId = Perm_Read, UserId = 1},
                    new TreeNodePermissionInfo { PermissionId = Perm_Read, UserId = 1}
                });

            Assert.IsFalse(res.Any(r => r.NodeId!=88));
            
            AssertSamePermissions(res,new [] {
                new TreeNodePermissionInfo { PermissionId = Perm_Read, UserId = 1}
            });
            
            res = c.CalculateEffectivePermissions(88,true, new [] {
                    new TreeNodePermissionInfo { PermissionId = Perm_Read, UserId = 1}
                }, new [] {
                    new TreeNodePermissionInfo { PermissionId = Perm_Change, UserId = 1}
                }, new [] {
                    new TreeNodePermissionInfo { PermissionId = Perm_Read, UserId = 2}
                });

            AssertSamePermissions(res,new [] {
                new TreeNodePermissionInfo { PermissionId = Perm_Read, UserId = 1},
                new TreeNodePermissionInfo { PermissionId = Perm_Change, UserId = 1},
                new TreeNodePermissionInfo { PermissionId = Perm_Read, UserId = 2},
            });
            
            
            res = c.CalculateEffectivePermissions(88,false, new [] {
                new TreeNodePermissionInfo { PermissionId = Perm_Read, UserId = 1}
            }, new [] {
                new TreeNodePermissionInfo { PermissionId = Perm_Change, UserId = 1}
            }, new [] {
                new TreeNodePermissionInfo { PermissionId = Perm_Read, UserId = 2}
            });

            AssertSamePermissions(res,new [] {
                new TreeNodePermissionInfo { PermissionId = Perm_Read, UserId = 1},
                new TreeNodePermissionInfo { PermissionId = Perm_Change, UserId = 1},
            });
            
            res = c.CalculateEffectivePermissions(88,true, new [] {
                new TreeNodePermissionInfo { PermissionId = Perm_Read, UserId = 1}
            }, new [] {
                new TreeNodePermissionInfo { PermissionId = Perm_Change, UserId = 1},
                new TreeNodePermissionInfo { PermissionId = Perm_Change, UserId = 1, DenyPermission = true},
                new TreeNodePermissionInfo { PermissionId = Perm_Read, UserId = 2, DenyPermission = true}
            }, new [] {
                new TreeNodePermissionInfo { PermissionId = Perm_Read, UserId = 2}
            });

            AssertSamePermissions(res,new [] {
                new TreeNodePermissionInfo { PermissionId = Perm_Read, UserId = 1},
                new TreeNodePermissionInfo { PermissionId = Perm_Change, UserId = 1}
            });
            
            res = c.CalculateEffectivePermissions(88,true, new [] {
                new TreeNodePermissionInfo { PermissionId = Perm_Read, UserId = 1},
                new TreeNodePermissionInfo { PermissionId = Perm_Read, UserId = 1, DenyPermission = true},
                new TreeNodePermissionInfo { PermissionId = Perm_Change, UserId = 1, DenyPermission = true}
            }, new [] {
                new TreeNodePermissionInfo { PermissionId = Perm_Change, UserId = 1},
                new TreeNodePermissionInfo { PermissionId = Perm_Change, UserId = 1, DenyPermission = true},
                new TreeNodePermissionInfo { PermissionId = Perm_Read, UserId = 2, DenyPermission = true}
            }, new [] {
                new TreeNodePermissionInfo { PermissionId = Perm_Read, UserId = 2}
            });

            AssertSamePermissions(res,new [] {
                new TreeNodePermissionInfo { PermissionId = Perm_Read, UserId = 1}
            });

            res = c.CalculateEffectivePermissions(88, true,
                null,
                null,
                new TreeNodePermissionInfo[0]);
            Assert.IsFalse(res.Any());
        }

        private void AssertSamePermissions(TreeNodePermissionInfo[] p1,
            TreeNodePermissionInfo[] p2)
        {
            Assert.AreEqual(p2.Length, p1.Length, "permission lists are not the same size");
            var checkingP2 = p2.ToList();
            foreach (var p1p in p1)
            {
                TreeNodePermissionInfo toRemove = null;
                foreach (var p2p in checkingP2)
                {
                    if (p2p.DenyPermission == p1p.DenyPermission
                        && p2p.PermissionId.ToString() == p1p.PermissionId.ToString()
                        && p2p.UserId == p1p.UserId
                        && p2p.UserGroupId == p1p.UserGroupId)
                    {
                        toRemove = p2p;
                        break;
                    }
                }

                if (toRemove != null)
                    checkingP2.Remove(toRemove);
            }

            if (checkingP2.Any()) throw new AssertionException("permissions lists are not equal");
        }
    }
}