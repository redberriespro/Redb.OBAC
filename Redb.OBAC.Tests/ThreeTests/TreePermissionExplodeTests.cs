using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;
using Redb.OBAC.BL;
using Redb.OBAC.Core.Ep;
using Redb.OBAC.Core.Models;
using Redb.OBAC.Models;
using Redb.OBAC.Tests.Utils;
using Redb.OBAC.Tree;

namespace Redb.OBAC.Tests.ThreeTests
{
    public class TreePermissionExplodeTests : TestBase
    {
        private Guid FakeTreeId = new Guid("7C74B8CF-FDFE-404E-972E-000000000001");

        private Guid Perm_Read = new Guid("7C74B8CF-FDFE-404E-972E-78C3E5145101");
        private Guid Perm_Change = new Guid("7C74B8CF-FDFE-404E-972E-78C3E5145102");
        private Guid Perm_Delete = new Guid("7C74B8CF-FDFE-404E-972E-78C3E5145103");

        public TreePermissionExplodeTests(string dbName) : base(dbName) { }

        [Test]
        public async Task RepairTest1TreeOrigState()
        {
            var tc = new TreePermissionCalculator();

            var permissions = MakePermSet1();
            
            var tr = MakeCtx1(permissions);
            var feed = new InMemoryEffectivePermissionQueue();

            await tc.RepairNodePermissions(feed,tr, 100);
            var fullPermissions = feed.GetAll();

            AssertNodeActions(100, fullPermissions, true, new[]
            {
                new TreeNodePermissionInfo {NodeId = 100, UserId = 1, PermissionId = Perm_Read},
                new TreeNodePermissionInfo {NodeId = 100, UserId =  2, PermissionId = Perm_Read},
                new TreeNodePermissionInfo {NodeId = 100,  UserId = 3, PermissionId = Perm_Read},
                new TreeNodePermissionInfo {NodeId = 100,  UserId = 4, PermissionId = Perm_Read}
            });
            
            AssertNodeActions(110, fullPermissions, true, new[]
            {
                new TreeNodePermissionInfo {NodeId = 110, UserId = 5, PermissionId = Perm_Delete}
            });
            
            AssertNodeActions(111, fullPermissions, true, new[]
            {
                new TreeNodePermissionInfo {NodeId = 111, UserId =5, PermissionId = Perm_Delete},
                new TreeNodePermissionInfo {NodeId = 111, UserId =10, PermissionId = Perm_Read},
                new TreeNodePermissionInfo {NodeId = 111, UserId =10, PermissionId = Perm_Change},
            });
            
            AssertNodeActions(120, fullPermissions, true, new[]
            {
                new TreeNodePermissionInfo {NodeId = 120, UserId = 1, PermissionId = Perm_Read},
                new TreeNodePermissionInfo {NodeId = 120, UserId =  2, PermissionId = Perm_Read},
                new TreeNodePermissionInfo {NodeId = 120,  UserId = 3, PermissionId = Perm_Read},
                new TreeNodePermissionInfo {NodeId = 120,  UserId = 4, PermissionId = Perm_Read},
                new TreeNodePermissionInfo {NodeId = 120,  UserId = 2, PermissionId = Perm_Change},
            });
        }

     
        [Test]
        public async Task RepairTest2()
        {
            var tc = new TreePermissionCalculator();

            var permissions = new List<TreeNodePermissionInfo>
            {
                new TreeNodePermissionInfo {NodeId = 100, UserGroupId = 1, PermissionId = Perm_Read},
                new TreeNodePermissionInfo {NodeId = 100, UserGroupId = 2, PermissionId = Perm_Read},
                new TreeNodePermissionInfo {NodeId = 100, UserId = 4, PermissionId = Perm_Read},
                new TreeNodePermissionInfo {NodeId = 110, UserId = 5, PermissionId = Perm_Delete},
                new TreeNodePermissionInfo {NodeId = 120, UserId = 2, PermissionId = Perm_Change},
                new TreeNodePermissionInfo {NodeId = 120, UserId = 4, PermissionId = Perm_Read, DenyPermission = true},
                new TreeNodePermissionInfo {NodeId = 120, UserId = 4, PermissionId = Perm_Change, DenyPermission = true},
                new TreeNodePermissionInfo {NodeId = 111, UserId = 10, PermissionId = Perm_Read},
                new TreeNodePermissionInfo {NodeId = 111, UserId = 10, PermissionId = Perm_Change}
            };

            var tr = MakeCtx1(permissions);
            var feed = new InMemoryEffectivePermissionQueue();
            await tc.RepairNodePermissions(feed, tr, 100);
            var fullPermissions = feed.GetAll();

            AssertNodeActions(120, fullPermissions, true, new[]
            {
                new TreeNodePermissionInfo {NodeId = 120, UserId = 1, PermissionId = Perm_Read},
                new TreeNodePermissionInfo {NodeId = 120, UserId =  2, PermissionId = Perm_Read},
                new TreeNodePermissionInfo {NodeId = 120,  UserId = 3, PermissionId = Perm_Read},
                new TreeNodePermissionInfo {NodeId = 120,  UserId = 2, PermissionId = Perm_Change},
            });
        }

        [Test]
        public async Task RepairTest3()
        {
            var tc = new TreePermissionCalculator();

            var permissions = MakePermSet1();
            
            var tr = MakeCtx1(permissions);
            var feed = new InMemoryEffectivePermissionQueue();
            await tc.RepairNodePermissions(feed, tr, 110);

            var fullPermissions = feed.GetAll();
            
            AssertNodeActions(110, fullPermissions, true, new[]
            {
                new TreeNodePermissionInfo {NodeId = 110, UserId = 5, PermissionId = Perm_Delete}
            });
            
            AssertNodeActions(111, fullPermissions, true, new[]
            {
                new TreeNodePermissionInfo {NodeId = 111, UserId =5, PermissionId = Perm_Delete},
                new TreeNodePermissionInfo {NodeId = 111, UserId =10, PermissionId = Perm_Read},
                new TreeNodePermissionInfo {NodeId = 111, UserId =10, PermissionId = Perm_Change},
            });
        }

        [Test]
        public async Task IncrementalAddTo110and111()
        {
            // add read_perm to 110 for user 77
            // make sure node 111 gets the same read permission
            
            var tc = new TreePermissionCalculator();
            var permissions = MakePermSet1();
            
            var tr = MakeCtx1(permissions);

            var feed = new InMemoryEffectivePermissionQueue();
            await tc.ChangePermissions(feed, tr, 110,
                new[]
                {
                    new TreeNodePermissionInfo
                    {
                        NodeId = 110, UserId = 77, PermissionId = Perm_Read
                    } 
                }, null);
            var addPermissions = feed.GetAll();

            AssertNodeActions(110, addPermissions, false, new[]
            {
                new TreeNodePermissionInfo {NodeId = 110, UserId = 77, PermissionId = Perm_Read},
            });
            AssertNodeActions(111, addPermissions, false, new[]
            {
                new TreeNodePermissionInfo {NodeId = 111, UserId = 77, PermissionId = Perm_Read},
            });
        }
        
        [Test]
        public async Task IncrementalAddTo110reAddOn111()
        {
            // make sure if a permission exists on parent level (110) UserId = 5, PermissionId = Perm_Delete
            // AND inheritParentPermission set to false to 111
            // AND permission should be added at level 111
            // it is really added
            
            var tc = new TreePermissionCalculator();
            var permissions = MakePermSet1();
            
            var tr = MakeCtx1(permissions);

            var feed = new InMemoryEffectivePermissionQueue();
            await tc.ChangePermissions(feed, tr, 111,
                new[]
                {
                    new TreeNodePermissionInfo
                    {
                        NodeId = 111, UserId = 5, PermissionId = Perm_Delete
                    } 
                }, null, NodeParentPermissionInheritanceActionEnum.SetDoNotInherit);
            var addPermissions = feed.GetAll();

            AssertNodeActions(111, addPermissions, false, new[]
            {
                new TreeNodePermissionInfo {NodeId = 111, UserId = 5, PermissionId = Perm_Delete},
            });
        }
        

        [Test]
        public async Task IncrementalAdd1()
        {
            var tc = new TreePermissionCalculator();

            var permissions = MakePermSet1();
            
            // 100 (actual UG:1,2:R, U:4:R, eff U:1,2,3,4:R)
            // 110 [x] (actual U:5:D eff U:5:D)
            // 111 (actual U:10:R,C eff U:10:R,C U:5:D) 
            
            var tr = MakeCtx1(permissions);

            var feed = new InMemoryEffectivePermissionQueue();
            await tc.ChangePermissions(feed, tr, 100,
                new[]
                {
                    new TreeNodePermissionInfo
                    {
                        NodeId = 100, UserGroupId = 10, PermissionId = Perm_Read
                    } // users 4,5,6, for Read
                }, null);
            var addPermissions = feed.GetAll();

            
            AssertNodeActions(100, addPermissions, false, new[]
            {
                new TreeNodePermissionInfo {NodeId = 100, UserId = 5, PermissionId = Perm_Read},
                new TreeNodePermissionInfo {NodeId = 100, UserId = 6, PermissionId = Perm_Read},
            });
            
            AssertNodeActions(110, addPermissions, false, new TreeNodePermissionInfo[0]);
            AssertNodeActions(111, addPermissions, false, new TreeNodePermissionInfo[0]);
            
            AssertNodeActions(120, addPermissions, false, new[]
            {
                new TreeNodePermissionInfo {NodeId = 120, UserId = 5, PermissionId = Perm_Read},
                new TreeNodePermissionInfo {NodeId = 120, UserId = 6, PermissionId = Perm_Read},
            });
            
            // now we check starting node 120, should be the same
            feed = new InMemoryEffectivePermissionQueue();
            await tc.ChangePermissions(feed, tr, 120,
                new[]
                {
                    new TreeNodePermissionInfo
                    {
                        NodeId = 120, UserGroupId = 10, PermissionId = Perm_Read
                    } // users 4,5,6 for Read
                }, null);
            addPermissions = feed.GetAll();
            
            AssertNodeActions(120, addPermissions, false, new[]
            {
                new TreeNodePermissionInfo {NodeId = 120, UserId = 5, PermissionId = Perm_Read},
                new TreeNodePermissionInfo {NodeId = 120, UserId = 6, PermissionId = Perm_Read},
            });
            
            // now we try to add u2: change to the root, node 120 should remain the same
            feed = new InMemoryEffectivePermissionQueue();
            await tc.ChangePermissions(feed, tr, 100,
                new[]
                {
                    new TreeNodePermissionInfo
                    {
                        NodeId = 100, UserId = 2, PermissionId = Perm_Change
                    }
                }, null);
            addPermissions = feed.GetAll();
            
            AssertNodeActions(100, addPermissions, false, new[]
            {
                new TreeNodePermissionInfo {NodeId = 100, UserId = 2, PermissionId = Perm_Change}
            });
            
            AssertNodeActions(120, addPermissions, false, Array.Empty<TreeNodePermissionInfo>());

        }

        [Test]
        public async Task IncrementalAdd2NothingChanges()
        {
            var tc = new TreePermissionCalculator();
            var permissions = MakePermSet1();
            var tr = MakeCtx1(permissions);

            var feed = new InMemoryEffectivePermissionQueue();
            await tc.ChangePermissions(feed, tr, 100,
                new[]
                {
                    new TreeNodePermissionInfo {NodeId = 100, UserId = 1, PermissionId = Perm_Read}
                }, new[]
                {
                    new TreeNodePermissionInfo {NodeId = 100, UserId = 999, PermissionId = Perm_Change}
                }
            );
            var addPermissions = feed.GetAll();
            
            AssertNodeActions(100, addPermissions, false, new TreeNodePermissionInfo[]
            { });
            AssertNodeActions(110, addPermissions, false, new TreeNodePermissionInfo[0]);
            AssertNodeActions(111, addPermissions, false, new TreeNodePermissionInfo[0]);
            AssertNodeActions(120, addPermissions, false, new TreeNodePermissionInfo[]
            { });
        }
        
        [Test]
        public async Task IncrementalAdd3AddAfterInheritance()
        {
            var tc = new TreePermissionCalculator();
            var permissions = MakePermSet1();
            var tr = MakeCtx1(permissions);

            var feed = new InMemoryEffectivePermissionQueue();
            await tc.ChangePermissions(feed, tr, 110,
                new[]
                {
                    new TreeNodePermissionInfo {NodeId = 110, UserId = 33, PermissionId = Perm_Delete},
                    new TreeNodePermissionInfo {NodeId = 110, UserId = 10, PermissionId = Perm_Read}
                }, new[]
                {
                    new TreeNodePermissionInfo {NodeId = 110, UserId = 5, PermissionId = Perm_Delete}
                }
            );
            var changePermissions = feed.GetAll();


            AssertNodeActions(110, changePermissions, false, new TreeNodePermissionInfo[]
            {
                new TreeNodePermissionInfo {NodeId = 110, UserId = 33, PermissionId = Perm_Delete},
                new TreeNodePermissionInfo {NodeId = 110, UserId = 10, PermissionId = Perm_Read},
                new TreeNodePermissionInfo {NodeId = 110, UserId = 5, PermissionId = Perm_Delete, DenyPermission = true},
            });

            AssertNodeActions(111, changePermissions, false, new TreeNodePermissionInfo[]
            {
                new TreeNodePermissionInfo {NodeId = 111, UserId = 33, PermissionId = Perm_Delete},
                new TreeNodePermissionInfo {NodeId = 111, UserId = 5, PermissionId = Perm_Delete, DenyPermission = true},
            });
        }

        [Test]
        public async Task IncrementalAddAndRemove()
        {
            var tc = new TreePermissionCalculator();

            var permissions = MakePermSet1();


            // 100 (actual UG:1,2:R, U:4:R, eff U:1,2,3,4:R)
            // 110 [x] (actual U:5:D eff U:5:D)
            // 111 (actual U:10:R,C eff U:10:R,C U:5:D) 
            
            var tr = MakeCtx1(permissions);

            var feed = new InMemoryEffectivePermissionQueue();
            await tc.ChangePermissions(feed, tr, 100,
                new[]
                {
                    new TreeNodePermissionInfo {NodeId = 100, UserId = 100, PermissionId = Perm_Change}
                },
                new[]
                {
                    new TreeNodePermissionInfo {NodeId = 100, UserGroupId = 2, PermissionId = Perm_Read}
                }
            );
            var changePermissions = feed.GetAll();

            
            AssertNodeActions(100, changePermissions, false, new[]
            {
                new TreeNodePermissionInfo {NodeId = 100, UserId = 100, PermissionId = Perm_Change},
                new TreeNodePermissionInfo {NodeId = 100, UserId = 3, PermissionId = Perm_Read, DenyPermission = true},
            });
            
            AssertNodeActions(110, changePermissions, false, new TreeNodePermissionInfo[0]);
            AssertNodeActions(111, changePermissions, false, new TreeNodePermissionInfo[0]);
            
            AssertNodeActions(120, changePermissions, false, new[]
            {
                new TreeNodePermissionInfo {NodeId = 120, UserId = 100, PermissionId = Perm_Change}, 
                new TreeNodePermissionInfo {NodeId = 120, UserId = 3, PermissionId = Perm_Read, DenyPermission = true},

            });
            feed = new InMemoryEffectivePermissionQueue();
            await tc.ChangePermissions(feed, tr, 100,
                new[]
                {
                    new TreeNodePermissionInfo {NodeId = 100, UserId = 100, PermissionId = Perm_Change}
                },
                new[]
                {
                    new TreeNodePermissionInfo {NodeId = 100, UserId = 2, PermissionId = Perm_Read}
                }
            );
            // now we check starting node 120, should be the same
            changePermissions = feed.GetAll();
            
            AssertNodeActions(120, changePermissions, false, new[]
            {
                new TreeNodePermissionInfo {NodeId = 120, UserId = 100, PermissionId = Perm_Change}, 
            });

        }

        

        [Test]
        public async Task PermissionDenyAtUserAndGroupLevel()
        {
            var tc = new TreePermissionCalculator();
            var permissions = MakePermSet1();
            var tr = MakeCtx1(permissions);
            var feed = new InMemoryEffectivePermissionQueue();
            await tc.ChangePermissions(feed, tr, 100,
                new[]
                {
                    new TreeNodePermissionInfo
                        {NodeId = 100, UserId = 1, PermissionId = Perm_Read, DenyPermission = true}
                }, null
            );
            var changePermissions = feed.GetAll();

            AssertNodeActions(100, changePermissions, false, new TreeNodePermissionInfo[]
            {
                new TreeNodePermissionInfo {NodeId = 100, UserId = 1, PermissionId = Perm_Read, DenyPermission = true}
            });
            AssertNodeActions(120, changePermissions, false, new TreeNodePermissionInfo[]
            {
                new TreeNodePermissionInfo {NodeId = 120, UserId = 1, PermissionId = Perm_Read, DenyPermission = true}
            });
            feed = new InMemoryEffectivePermissionQueue();
            await tc.ChangePermissions(feed, tr, 120,
                new[]
                {
                    new TreeNodePermissionInfo
                        {NodeId = 120, UserId = 1, PermissionId = Perm_Read, DenyPermission = true}
                }, null
            );
            changePermissions = feed.GetAll();
            
            AssertNodeActions(120, changePermissions, false, new TreeNodePermissionInfo[]
            {
                new TreeNodePermissionInfo {NodeId = 120, UserId = 1, PermissionId = Perm_Read, DenyPermission = true}
            });
            
            // remove user group
            feed = new InMemoryEffectivePermissionQueue();
            await tc.ChangePermissions(feed, tr, 120,
                new[]
                {
                    new TreeNodePermissionInfo
                        {NodeId = 120, UserGroupId = 1, PermissionId = Perm_Read, DenyPermission = true}
                }, null
            );
            changePermissions = feed.GetAll();
            
            // user2 was also allowed by group2 rule, but deny at same level has priority
            AssertNodeActions(120, changePermissions, false, new TreeNodePermissionInfo[]
            {
                new TreeNodePermissionInfo {NodeId = 120, UserId = 1, PermissionId = Perm_Read, DenyPermission = true}, 
                new TreeNodePermissionInfo {NodeId = 120, UserId = 2, PermissionId = Perm_Read, DenyPermission = true} 
            });
            
        }
        
        [Test]
        public async Task PermissionDenyNotExisting()
        {
            var tc = new TreePermissionCalculator();
            var permissions = MakePermSet1();
            var tr = MakeCtx1(permissions);
            var feed = new InMemoryEffectivePermissionQueue();

            // same as before but with user-based rule override
            await tc.ChangePermissions(feed, tr, 100,
                new[]
                {
                    new TreeNodePermissionInfo
                        {NodeId = 100, UserId = 999, PermissionId = Perm_Read, DenyPermission = true}
                }, null
            );
            var changePermissions = feed.GetAll();
            
            AssertNodeActions(100, changePermissions, false, new TreeNodePermissionInfo[]
            {
            });

            
            AssertNodeActions(120, changePermissions, false, new TreeNodePermissionInfo[]
            {
            });
        }


        [Test]
        public async Task PermissionDenyForGroupsButAllowOnUsersLevel()
        {
            var tc = new TreePermissionCalculator();
            var permissions = MakePermSet1();
            var tr = MakeCtx1(permissions);
            
            var feed = new InMemoryEffectivePermissionQueue();
            // same as before but with user-based rule override
            await tc.ChangePermissions(feed, tr, 100,
                new[]
                {
                    new TreeNodePermissionInfo
                        {NodeId = 100, UserGroupId = 1, PermissionId = Perm_Read, DenyPermission = true},
                    new TreeNodePermissionInfo {NodeId = 100, UserId = 2, PermissionId = Perm_Read}
                },
                new[]
                {
                    new TreeNodePermissionInfo {NodeId = 100, UserGroupId = 1, PermissionId = Perm_Read},
                }
            );
            var changePermissions = feed.GetAll();
            
            AssertNodeActions(100, changePermissions, false, new TreeNodePermissionInfo[]
            {
                new TreeNodePermissionInfo {NodeId = 100, UserId = 1, PermissionId = Perm_Read, DenyPermission = true} 
            });

            AssertNodeActions(120, changePermissions, false, new TreeNodePermissionInfo[]
            {
                new TreeNodePermissionInfo {NodeId = 120, UserId = 1, PermissionId = Perm_Read, DenyPermission = true} 
            });
        }

        [Test]
        public async Task IncrementalAddDeny1()
        {
             var tc = new TreePermissionCalculator();

            var permissions = MakePermSet1();


            // 100 (actual UG:1,2:R, U:4:R, eff U:1,2,3,4:R)
            // 110 [x] (actual U:5:D eff U:5:D)
            // 111 (actual U:10:R,C eff U:10:R,C U:5:D) 
            
            var tr = MakeCtx1(permissions);
            var feed = new InMemoryEffectivePermissionQueue();
            await tc.ChangePermissions(feed, tr, 100,
                new[]
                {
                    new TreeNodePermissionInfo
                    {
                        NodeId = 100, UserId = 1, PermissionId = Perm_Read, DenyPermission = true
                    }
                }, null);
            var changePermissions = feed.GetAll();

            
            AssertNodeActions(100, changePermissions, false, new TreeNodePermissionInfo[]
            {
                new TreeNodePermissionInfo {NodeId = 100, UserId = 1, PermissionId = Perm_Read, DenyPermission = true},
            });
            
            AssertNodeActions(110, changePermissions, false, new TreeNodePermissionInfo[0]);
            AssertNodeActions(111, changePermissions, false, new TreeNodePermissionInfo[0]);
            
            AssertNodeActions(120, changePermissions, false, new TreeNodePermissionInfo[]
            {
                new TreeNodePermissionInfo {NodeId = 120, UserId = 1, PermissionId = Perm_Read, DenyPermission = true},
            });
            
        }
        
        
        [Test]
        public async Task PermissionChangeInheritanceFlag()
        {
            var tc = new TreePermissionCalculator();

            var permissions = MakePermSet1();


            // 100 (actual UG:1,2:R, U:4:R, eff U:1,2,3,4:R)
            // 110 [x] (actual U:5:D eff U:5:D)
            // 111 (actual U:10:R,C eff U:10:R,C U:5:D) 
            
            var tr = MakeCtx1(permissions);
            var feed = new InMemoryEffectivePermissionQueue();
            await tc.ChangePermissions(feed,
                tr, 110, null, null
            );
            var changePermissions = feed.GetAll();
            
            Assert.IsFalse(changePermissions.Any()); // nothing has changed
          
            feed = new InMemoryEffectivePermissionQueue();
            await tc.ChangePermissions(feed,
                tr, 110, null, null, NodeParentPermissionInheritanceActionEnum.SetDoNotInherit
            );
            changePermissions = feed.GetAll();
            
            Assert.IsFalse(changePermissions.Any()); // nothing has changed

             feed = new InMemoryEffectivePermissionQueue();
             await tc.ChangePermissions(feed,
                 tr, 110, null, null
                 , NodeParentPermissionInheritanceActionEnum.SetInherit
             );
             changePermissions = feed.GetAll();
            
            
            AssertNodeActions(110, changePermissions, false, new TreeNodePermissionInfo[]
            {
                new TreeNodePermissionInfo {NodeId = 110, UserId = 1, PermissionId = Perm_Read},
                new TreeNodePermissionInfo {NodeId = 110, UserId =  2, PermissionId = Perm_Read},
                new TreeNodePermissionInfo {NodeId = 110,  UserId = 3, PermissionId = Perm_Read},
                new TreeNodePermissionInfo {NodeId = 110,  UserId = 4, PermissionId = Perm_Read}
                
            });
            
            AssertNodeActions(111, changePermissions, false, new TreeNodePermissionInfo[]
            {
                new TreeNodePermissionInfo {NodeId = 111, UserId = 1, PermissionId = Perm_Read},
                new TreeNodePermissionInfo {NodeId = 111, UserId =  2, PermissionId = Perm_Read},
                new TreeNodePermissionInfo {NodeId = 111,  UserId = 3, PermissionId = Perm_Read},
                new TreeNodePermissionInfo {NodeId = 111,  UserId = 4, PermissionId = Perm_Read}
                
            });
            feed = new InMemoryEffectivePermissionQueue();
            await tc.ChangePermissions(feed,
                tr, 110, null,
                new[]
                {
                    new TreeNodePermissionInfo {NodeId = 110, UserId = 5, PermissionId = Perm_Delete},
                }
                , NodeParentPermissionInheritanceActionEnum.SetInherit
            );
            changePermissions = feed.GetAll();
            
            AssertNodeActions(110, changePermissions, false, new TreeNodePermissionInfo[]
            {
                new TreeNodePermissionInfo {NodeId = 110, UserId = 1, PermissionId = Perm_Read},
                new TreeNodePermissionInfo {NodeId = 110, UserId =  2, PermissionId = Perm_Read},
                new TreeNodePermissionInfo {NodeId = 110,  UserId = 3, PermissionId = Perm_Read},
                new TreeNodePermissionInfo {NodeId = 110,  UserId = 4, PermissionId = Perm_Read},
                new TreeNodePermissionInfo {NodeId = 110, UserId = 5, PermissionId = Perm_Delete, DenyPermission = true},

            });
            
            AssertNodeActions(111, changePermissions, false, new TreeNodePermissionInfo[]
            {
                new TreeNodePermissionInfo {NodeId = 111, UserId = 1, PermissionId = Perm_Read},
                new TreeNodePermissionInfo {NodeId = 111, UserId =  2, PermissionId = Perm_Read},
                new TreeNodePermissionInfo {NodeId = 111,  UserId = 3, PermissionId = Perm_Read},
                new TreeNodePermissionInfo {NodeId = 111,  UserId = 4, PermissionId = Perm_Read},
                new TreeNodePermissionInfo {NodeId = 111, UserId = 5, PermissionId = Perm_Delete, DenyPermission = true},

            });


        }
        
        [Test]
        public async Task PermissionChangeNodeRemoved()
        {
            var tc = new TreePermissionCalculator();

            var permissions = MakePermSet1();
            
            // 100 (actual UG:1,2:R, U:4:R, eff U:1,2,3,4:R)
            // 110 [x] (actual U:5:D eff U:5:D)
            // 111 (actual U:10:R,C eff U:10:R,C U:5:D) 
            
            var tr = MakeCtx1(permissions);
            var feed = new InMemoryEffectivePermissionQueue();
            await tc.BeforeNodeRemoved(feed, tr, 110);
            var changedPermissions = feed.GetAll();
            
            AssertNodeActions(110, changedPermissions, true, Array.Empty<TreeNodePermissionInfo>());
            AssertNodeActions(111, changedPermissions, true, Array.Empty<TreeNodePermissionInfo>());
        }
        
        [Test]
        public async Task PermissionChangeNodeAdded()
        {
            var tc = new TreePermissionCalculator();

            var permissions = MakePermSet1();
            
            // 100 (actual UG:1,2:R, U:4:R, eff U:1,2,3,4:R)
            // 110 [x] (actual U:5:D eff U:5:D)
            // 111 (actual U:10:R,C eff U:10:R,C U:5:D) 
            
            var tr = MakeCtx1(permissions);
            var feed = new InMemoryEffectivePermissionQueue();
            await tc.AfterNodeInserted(feed, tr, 110); // entire 110'th subtree will refresh
            var changedPermissions = feed.GetAll();
            AssertNodeActions(110, changedPermissions, true, new[]
            {
                new TreeNodePermissionInfo {NodeId = 110, UserId = 5, PermissionId = Perm_Delete}
            });
            
            AssertNodeActions(111, changedPermissions, true, new[]
            {
                new TreeNodePermissionInfo {NodeId = 111, UserId =5, PermissionId = Perm_Delete},
                new TreeNodePermissionInfo {NodeId = 111, UserId =10, PermissionId = Perm_Read},
                new TreeNodePermissionInfo {NodeId = 111, UserId =10, PermissionId = Perm_Change},
            });
        }
        
        
        // **** utility methods ****
        
        private List<TreeNodePermissionInfo> MakePermSet1()
        {
            var permissions = new List<TreeNodePermissionInfo>
            {
                new TreeNodePermissionInfo {NodeId = 100, UserGroupId = 1, PermissionId = Perm_Read},
                new TreeNodePermissionInfo {NodeId = 100, UserGroupId = 2, PermissionId = Perm_Read},
                new TreeNodePermissionInfo {NodeId = 100, UserId = 4, PermissionId = Perm_Read},
                new TreeNodePermissionInfo {NodeId = 110, UserId = 5, PermissionId = Perm_Delete},
                new TreeNodePermissionInfo {NodeId = 120, UserId = 2, PermissionId = Perm_Change},
                new TreeNodePermissionInfo {NodeId = 111, UserId = 10, PermissionId = Perm_Read},
                new TreeNodePermissionInfo {NodeId = 111, UserId = 10, PermissionId = Perm_Change}
            };
            return permissions;
        }

        
        private void AssertNodeActions(int nodeId, 
            PermissionActionInfo[] actionFull, bool checkInitialRemoveAction,
            TreeNodePermissionInfo[] treeNodePermissionInfos)
        {
            var actionsByNode = actionFull.Where(a => a.ObjectId == nodeId).ToArray();
            var actions = actionsByNode.ToList();
            if (checkInitialRemoveAction)
            {
                var firstAction = actions.First();
                Assert.IsTrue( firstAction.Action == PermissionActionEnum.RemoveAllObjectsDirectPermission);
                actions.Remove(firstAction);
            }
            Assert.AreEqual(treeNodePermissionInfos.Length, actions.Count);
            foreach (var p in treeNodePermissionInfos)
            {
                PermissionActionInfo toRemove = null;
                foreach (var a in actions)
                {
                    if (a.PermissionId == p.PermissionId
                        && nodeId == p.NodeId
                        && a.Action == (p.DenyPermission ?
                            PermissionActionEnum.RemoveDirectPermission:
                            PermissionActionEnum.AddDirectPermission)
                        && p.UserId.Value == a.UserId 
                        && !p.UserGroupId.HasValue)
                    {
                        toRemove = a;
                        break;
                    }
                }

                if (toRemove != null)
                    actions.Remove(toRemove);
            }

            if (actions.Any())
            {
                var acStr = JsonConvert.SerializeObject(actions, Formatting.Indented);
                throw new AssertionException($"action list is wrong, was {actionsByNode.Length}, expected {treeNodePermissionInfos.Length}, unmatched: {acStr}");
            }
        }
        
        private TreeActionContext MakeCtx1(List<TreeNodePermissionInfo> permList) => new TreeActionContext
        {
            TreeId = FakeTreeId,
            GetTreeNode = async (treeId, nodeId) => MakeTree1().FindNode(nodeId, true),
            GetUsersInGroups = async groupId => groupId switch
            {
                1=>new []{1,2}, // group 1 => users 1,2
                2=>new []{2,3}, // group 2 => users 2,3
                10=>new []{4,5,6}, // group 10 => users 4 5 6
                _=> new int[0]
            },
            GetTreeNodePermissions = async (treeId, nodeId) =>
                permList.Where(a=>a.NodeId==nodeId).ToArray(),
            
            GetTreeNodePermissionList = async (treeId, nodeIds, permIds) =>
            {
                var nodeSet = nodeIds.ToHashSet();
                var permSet = permIds.ToHashSet();
                return permList.Where(a =>
                    nodeSet.Contains(a.NodeId) 
                    && permSet.Contains(a.PermissionId))
                    .ToArray();
            },
            
            GetNodeEffectivePermissions = async (treeId, nodeId) =>
            {
                if (treeId != FakeTreeId) throw new ArgumentException("treeId");
                // in-memory pseudo-db method
                var mirrorCtx = MakeCtx1(permList);
                var tc = new TreePermissionCalculator();
                var feed = new InMemoryEffectivePermissionQueue();
                await tc.RepairNodePermissions(feed, mirrorCtx, nodeId);
                var fullPermissions = feed.GetAll();
                return fullPermissions.Where(
                    p => p.ObjectId == nodeId && p.Action == PermissionActionEnum.AddDirectPermission
                )
                    .Select(a=>new TreeNodePermissionInfo
                    { 
                        NodeId = a.ObjectId,
                        UserId = a.UserId,
                        PermissionId = a.PermissionId
                    })
                    .ToArray();
            }

        };

        /*
         Node1 (inherit perms)
         - Node1-1 (NOT inherit perms)
         -- Node1-1-1 (inherit perms)
         - Node1-2 (inherit perms)
         Node2 (inherit perms)
         - Node2-1 (inherit perms) 
         -- Node2-1-1 (NOT inherit perms)
         -- Node2-1-2 (NOT inherit perms)
         Node3 (inherit perms)
         */
        private TreeNodeItem MakeTree1() =>

            new TreeNodeItem
            {
                NodeId = 0,
                Subnodes =
                {
                    new TreeNodeItem
                    {
                        NodeId = 100, InheritParentPermissions  = true, Subnodes =
                        {
                            new TreeNodeItem {NodeId = 110, ParentNodeId = 100,InheritParentPermissions  = false,  Subnodes =
                            {
                                new TreeNodeItem {NodeId = 111, ParentNodeId = 110, InheritParentPermissions  = true, Subnodes = { }}
                            }},
                            new TreeNodeItem {NodeId = 120, ParentNodeId = 100, InheritParentPermissions  = true, Subnodes = { }}
                        }
                    },
                    new TreeNodeItem
                    {
                        NodeId = 200, InheritParentPermissions  = true, Subnodes =
                        {
                            new TreeNodeItem
                            {
                                NodeId = 210, ParentNodeId = 100, InheritParentPermissions  = true, Subnodes =
                                {
                                    new TreeNodeItem {NodeId = 211, InheritParentPermissions  = false, ParentNodeId = 210},
                                    new TreeNodeItem {NodeId = 212, InheritParentPermissions  = false, ParentNodeId = 210}
                                }
                            },
                        }
                    },
                    new TreeNodeItem {NodeId = 300, InheritParentPermissions  = true, Subnodes = { }},
                }
            };
    };

    
}