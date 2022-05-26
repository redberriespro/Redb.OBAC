using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Redb.OBAC.ApiHost;
using Redb.OBAC.Tests.Utils;
using Redb.OBAC.Tree;
using Redberries.OBAC.Api;

namespace Redb.OBAC.Tests.ApiHostTests
{
    public class TreeManageApiTests: TestBase
    {
        private Guid Tree1Id = new Guid("333356ea-8d6c-11ea-bc55-0242ac110001");
        private Guid Tree2Id = new Guid("333356ea-8d6c-11ea-bc55-0242ac110002");
        private Guid Tree3Id = new Guid("333356ea-8d6c-11ea-bc55-0242ac110003");
        private Guid Tree4Id = new Guid("333356ea-8d6c-11ea-bc55-0242ac110004");
        private Guid Tree5Id = new Guid("333356ea-8d6c-11ea-bc55-0242ac110005");
        
        private Guid Perm_Read = new Guid("7C74B8CF-FDFE-404E-972E-78C3E5145101");
        private Guid Perm_Change = new Guid("7C74B8CF-FDFE-404E-972E-78C3E5145102");
        private Guid Perm_Delete = new Guid("7C74B8CF-FDFE-404E-972E-78C3E5145103");
        
        private Guid Role_Viewer = new Guid("7C74B8CF-FDFE-404E-972E-78C3E5145111"); // read 
        private Guid Role_Editor = new Guid("7C74B8CF-FDFE-404E-972E-78C3E5145112"); // read+change
        private Guid Role_Admin = new Guid("7C74B8CF-FDFE-404E-972E-78C3E5145113"); // read+change+delete

        private int User1Id = 100;
        private int User2Id = 101;
        private int User3Id = 102;
        private int Group1Id = 100; // User2 and User3



        private Guid Perm1Id = new Guid("7C74B8CF-FDFE-404E-972E-78C3E51451D6");
        private Guid Perm2Id = new Guid("7C74B8CF-FDFE-404E-972E-78C3E51451D7");
        private Guid Role1Id = new Guid("7C74B8CF-FDFE-404E-972E-78C3E51451D8");

        public TreeManageApiTests(string dbName) : base(dbName) { }
        
        /*
         Node1
         - Node1-1
         - Node1-2
         Node2
         - Node2-1
         -- Node2-1-1
         -- Node2-1-2
         Node3
         */
        private const int Node1_id = 100;
        private const int Node1_1_id = 110;
        private const int Node1_2_id = 120;
        private const int Node2_id = 200;
        private const int Node2_1_id = 210;
        private const int Node2_1_1_id = 211;
        private const int Node2_1_2_id = 212;
        private const int Node3_id = 300;

        private async Task MakeSimpleTree(ApiHostImpl api, Guid treeId)
        {
            var nodes = new EnsureTreeNodeParams {TreeId = treeId.ToGrpcUuid()};
            nodes.Nodes.AddRange(new []
            {
                new TreeNodeItemInfo  {
                    Id = Node1_id,
                    OwnerUserId = User1Id
                },
                new TreeNodeItemInfo {
                    Id = Node1_1_id, ParentId = Node1_id,
                    OwnerUserId = User1Id
                },
                new TreeNodeItemInfo  {
                    Id = Node1_2_id, ParentId = Node1_id,
                    OwnerUserId = User1Id
                },
                new TreeNodeItemInfo  {
                    Id = Node2_id,
                    OwnerUserId = User1Id
                },
                new TreeNodeItemInfo {
                    Id = Node2_1_id, ParentId = Node2_id,                     OwnerUserId = User1Id},

                new TreeNodeItemInfo  {
                    Id = Node2_1_1_id, ParentId = Node2_1_id,                     OwnerUserId = User1Id},
                new TreeNodeItemInfo {
                    Id = Node2_1_2_id, ParentId = Node2_1_id,                     OwnerUserId = User1Id},
                
                new TreeNodeItemInfo  {
                    Id = Node3_id,                    OwnerUserId = User1Id },
            });
            await api.EnsureTreeNodes(nodes, null);
        }

        
        [Test]
        public void GrpcUuid()
        {
            var u = Tree1Id.ToGrpcUuid();
            var u2 = u.ToGuid();
            Assert.AreEqual(Tree1Id,u2);
            var apiS =             GetApiHost();
        }

        [Test]
        public async Task TreeManipulation()
        {
            var api = GetApiHost();
            
            await api.EnsureTree(new EnsureTreeParams
            {
                TreeId = Tree2Id.ToGrpcUuid(),
                Description = "test2",
                ExternalIntId = 22,
                ExternalStrId = "2-1"
            }, null);

            await SetupUsersAndGroups(api);
            
            await MakeSimpleTree(api, Tree2Id);

            var allNodes = await api.GetTreeById(new GetTreeParams
            {
                TreeId = Tree2Id.ToGrpcUuid(), IncludeNodes = true
            }, null);
            
            Assert.AreEqual(8, allNodes.Nodes.Count);

            var allUnderNode2 = await api.GetTreeById(new GetTreeParams
            {
                TreeId = Tree2Id.ToGrpcUuid(), IncludeNodes = true, StartingNodeId = Node2_id
            }, null);
            Assert.AreEqual(allUnderNode2.Nodes.Count, 4);

            
            // todo get it back
            //await api.DeleteTree(new DeleteTreeParams {TreeId = Tree2Id.ToGrpcUuid(), ForceDeleteIfNotEmpty=true}, null);
        }

        [Test]
        public async Task TreePermissions1()
        {
            var api = GetApiHost();

            await api.EnsureTree(new EnsureTreeParams
            {
                TreeId = Tree3Id.ToGrpcUuid(),
                Description = "test3",
                ExternalIntId = 33,
                ExternalStrId = "3-1"
            }, null);

            await SetupUsersAndGroups(api);
            await MakeSimpleTree(api, Tree3Id);
            await SetupSecurityModel(api);

            var gp = new SetAclParams()
            {
                ObjectType = Tree3Id.ToGrpcUuid(),
                ObjectId = Node1_id
            };
            gp.Acl.Add(new AclItemParams
            {
                UserId = User3Id,
                Permission = Perm_Read.ToGrpcUuid() 
            });
            await api.SetAcl(gp, null);
            

            gp = new SetAclParams()
            {
                ObjectType = Tree3Id.ToGrpcUuid(),
                ObjectId = Node1_2_id
            };
            gp.Acl.Add(new AclItemParams
            {
                UserId = User3Id,
                Permission = Perm_Read.ToGrpcUuid() 
            });
            gp.Acl.Add(new AclItemParams
            {
                UserId = User3Id,
                Permission = Perm_Change.ToGrpcUuid() 
            });
            await api.SetAcl(gp, null);
            
            // user 1 must have reading right to entire node1 subtree plus editor rights to node 1-2
        }

        private async Task SetupSecurityModel(ApiHostImpl api)
        {
            await api.EnsurePermission(new EnsurePermissionParams
            {
                PermissionId = Perm_Read.ToGrpcUuid(),
                Description = "read"
            }, null);


            await api.EnsurePermission(new EnsurePermissionParams
            {
                PermissionId = Perm_Change.ToGrpcUuid(),
                Description = "change"
            }, null);

            await api.EnsurePermission(new EnsurePermissionParams
            {
                PermissionId = Perm_Delete.ToGrpcUuid(),
                Description = "delete"
            }, null);

            var rp = new EnsureRoleParams
            {
                RoleId = Role_Viewer.ToGrpcUuid(),
                Description = "role_view",
            };
            rp.PermissionId.Add(Perm_Read.ToGrpcUuid());
            await api.EnsureRole(rp, null);

            rp = new EnsureRoleParams
            {
                RoleId = Role_Editor.ToGrpcUuid(),
                Description = "role_edit",
            };
            rp.PermissionId.Add(Perm_Read.ToGrpcUuid());
            rp.PermissionId.Add(Perm_Change.ToGrpcUuid());
            await api.EnsureRole(rp, null);

            rp = new EnsureRoleParams
            {
                RoleId = Role_Admin.ToGrpcUuid(),
                Description = "role_admin",
            };
            rp.PermissionId.Add(Perm_Read.ToGrpcUuid());
            rp.PermissionId.Add(Perm_Change.ToGrpcUuid());
            rp.PermissionId.Add(Perm_Delete.ToGrpcUuid());
            await api.EnsureRole(rp, null);
        }
        
     
        
        private async Task SetupUsersAndGroups(ApiHostImpl api)
        {
            await api.EnsureUser(new EnsureUserParams
            {
                UserId = User1Id, Description = "user1",ExternalIntId = User1Id,  ExternalStrId = "user1"
            },null);
            await api.EnsureUser(new EnsureUserParams
            {
                UserId = User2Id, Description = "user2", ExternalIntId = User2Id, ExternalStrId = "user2"
            }, null);
            await api.EnsureUser(new EnsureUserParams
            {
                UserId = User3Id, Description = "user3", ExternalIntId = User3Id,  ExternalStrId = "user3"
            }, null);
            
            await api.EnsureUserGroup(new EnsureUserGroupParams
            {
                UserGroupId = Group1Id, Description = "users1+2"
            }, null);
            var ug = new AddUserToGroupParams {UserGroupId = Group1Id};
            ug.UserId.Add(User2Id);
            ug.UserId.Add(User3Id);
            await api.AddUserToGroupById(ug, null);
        }


        [Test]
        public async Task TreeSimple()
        {
            var api = GetApiHost();
            try
            {
                var ti = await api.GetTreeById(new GetTreeParams
                {
                    TreeId = Tree1Id.ToGrpcUuid()
                }, null);
            }
            catch (Exception ex)
            {
                
                await api.DeleteTree(new DeleteTreeParams {TreeId = Tree1Id.ToGrpcUuid(), ForceDeleteIfNotEmpty=true}, null);
            }

            await api.EnsureTree(new EnsureTreeParams
            {
                TreeId = Tree1Id.ToGrpcUuid(),
                Description = "test1",
                ExternalIntId = 11,
                ExternalStrId = "1-1"
            },null);
            
            var ti2 = await api.GetTreeById(new GetTreeParams
            {
                TreeId = Tree1Id.ToGrpcUuid()
            }, null);
            
            Assert.AreEqual(ti2.Description,"test1");
            
            await api.EnsureTree(new EnsureTreeParams
            {
                TreeId = Tree1Id.ToGrpcUuid(),
                Description = "test2",
                ExternalIntId = 11,
                ExternalStrId = "1-2"
            },null);
            
            ti2 = await api.GetTreeById(new GetTreeParams
            {
                TreeId = Tree1Id.ToGrpcUuid()
            }, null);
            
            Assert.AreEqual(ti2.Description,"test2");
            
        }
        
        [Test]
        public async Task PermRoleSimple()
        {
            var api = GetApiHost();
            try
            {
                var ti = await api.GetPermissionById(new GetPermissionParams()
                {
                    PermissionId = Perm1Id.ToGrpcUuid()
                }, null);
            }
            catch (Exception ex)
            {
                await api.DeletePermission(new DeletePermissionParams()
                {
                    PermissionId = Perm1Id.ToGrpcUuid(), ForceDelete=true
                }, null);
            }

            await api.EnsurePermission(new EnsurePermissionParams
            {
                PermissionId = Perm1Id.ToGrpcUuid(),
                Description = "prm1"
            },null);
            
            var ti2 = await api.GetPermissionById(new GetPermissionParams
            {
                PermissionId = Perm1Id.ToGrpcUuid()
            }, null);
            
            Assert.AreEqual(ti2.Description,"prm1");
            
            await api.EnsurePermission(new EnsurePermissionParams
            {
                PermissionId = Perm1Id.ToGrpcUuid(),
                Description = "prm1x"
            },null);
            
            ti2 = await api.GetPermissionById(new GetPermissionParams
            {
                PermissionId = Perm1Id.ToGrpcUuid()
            }, null);
            
            Assert.AreEqual(ti2.Description,"prm1x");
            
            // roles
            await api.EnsurePermission(new EnsurePermissionParams
            {
                PermissionId = Perm2Id.ToGrpcUuid(),
                Description = "prm2"
            },null);

            var rp = new EnsureRoleParams
            {
                RoleId = Role1Id.ToGrpcUuid(),
                Description = "role1",
            };
            rp.PermissionId.Add(Perm1Id.ToGrpcUuid());
            rp.PermissionId.Add(Perm2Id.ToGrpcUuid());
            await api.EnsureRole(rp,null);

            var roleinfo = await api.GetRoleById(
                new GetRoleParams {RoleId = Role1Id.ToGrpcUuid()}, null);
            
            Assert.AreEqual(roleinfo.Description, "role1");
            Assert.AreEqual(2, roleinfo.PermissionId.Count);
            Assert.IsTrue(roleinfo.PermissionId.Any(x=>x.Equals(Perm1Id.ToGrpcUuid())));
            Assert.IsTrue(roleinfo.PermissionId.Any(x=>x.Equals(Perm2Id.ToGrpcUuid())));

            await api.DeleteRole(new DeleteRoleParams{RoleId = Role1Id.ToGrpcUuid()},null);
            roleinfo = await api.GetRoleById(
                new GetRoleParams {RoleId = Role1Id.ToGrpcUuid()}, null);
            Assert.IsNull(roleinfo.RoleId);
            
            await api.DeletePermission(new DeletePermissionParams{PermissionId = Perm1Id.ToGrpcUuid()},null);
            var permInfo = await api.GetPermissionById(
                new GetPermissionParams() {PermissionId = Perm1Id.ToGrpcUuid()}, null);
            Assert.IsNull(permInfo.PermissionId);
        }

        [Test]
        public async Task LazyTreeRestructureTest()
        {
            var api = GetApiHost();
            var lzProvider = GetObjectStorage() as ILazyTreeDataProvider;
            
            await api.EnsureTree(new EnsureTreeParams
            {
                TreeId = Tree5Id.ToGrpcUuid(),
                Description = "test5",
                ExternalIntId = 55,
                ExternalStrId = "5"
            }, null);

            await SetupUsersAndGroups(api);
            
            await MakeSimpleTree(api, Tree5Id);
            
            var lzTree1 = new LazyTree(Tree5Id, lzProvider);
            await lzTree1.GetRootNode();
            await AssertTreeValid(lzTree1);
            
            // change 211 node parent to root
            var nodes = new EnsureTreeNodeParams {TreeId = Tree5Id.ToGrpcUuid()};
            nodes.Nodes.AddRange(new []
            {
                new TreeNodeItemInfo {Id = Node2_1_1_id, OwnerUserId = User1Id},
            });
            await api.EnsureTreeNodes(nodes, null);
            lzTree1.InvalidateRootNode();
            lzTree1.InvalidateNode(211);
            lzTree1.InvalidateNode(210);
            
            // check if three is ok
            var lzTree = new LazyTree(Tree5Id, lzProvider);
            await lzTree.GetRootNode();
            // todo check
            
            // change 210 node parent to node1
            nodes = new EnsureTreeNodeParams {TreeId = Tree5Id.ToGrpcUuid()};
            nodes.Nodes.AddRange(new []
            {
                new TreeNodeItemInfo {Id = Node2_1_id, ParentId = Node1_id, OwnerUserId = User1Id},
            });
            await api.EnsureTreeNodes(nodes, null);
            await lzTree1.GetRootNode();
            lzTree1.InvalidateNode(210);
            lzTree1.InvalidateNode(100);
            // check if three is ok

            lzTree = new LazyTree(Tree5Id, lzProvider);
            await lzTree.GetRootNode();
            await AssertReorderedTreeValid(lzTree);
            await AssertReorderedTreeValid(lzTree1);
            
            // move 300 node from parent to under 100
            nodes = new EnsureTreeNodeParams {TreeId = Tree5Id.ToGrpcUuid()};
            nodes.Nodes.AddRange(new []
            {
                new TreeNodeItemInfo {Id = Node3_id, ParentId = Node1_id,OwnerUserId = User1Id},
            });
            await api.EnsureTreeNodes(nodes, null);

            
            lzTree = new LazyTree(Tree5Id, lzProvider);
            var nd3 = await lzTree.GetNode(Node3_id);
            Assert.AreEqual(0, nd3.Subnodes.Count);
            Assert.AreEqual(Node1_id, nd3.ParentNodeId);
            
            var root = await lzTree.GetRootNode();
            Assert.AreEqual(3,root.Subnodes.Count);
            var nd1 = await lzTree.GetNode(Node1_id);
            Assert.IsNotNull(nd1.Subnodes.Single(n=>n.NodeId==Node3_id));
        }
        
        private async  Task AssertReorderedTreeValid(LazyTree lzTree)
        {
            var rn = await lzTree.GetRootNode();

            var expectedNodeCount = 8;
            Assert.AreEqual(expectedNodeCount, lzTree.Count);

            /* expected
             Node1
             - Node1-1
             - Node1-2
             - Node2-1
             -- Node2-1-2
             Node2-1-1
             Node2
             Node3
            */
            
            Assert.AreEqual(4, rn.Subnodes.Count);
            Assert.IsNotNull(rn.Subnodes.Single(n=>n.NodeId==100));
            Assert.IsNotNull(rn.Subnodes.Single(n=>n.NodeId==200));
            Assert.IsNotNull(rn.Subnodes.Single(n=>n.NodeId==211));
            Assert.IsNotNull(rn.Subnodes.Single(n=>n.NodeId==300));
            
            Assert.IsNotNull((await lzTree.GetNode(100)).Subnodes.Single(a => a.NodeId == 110));
            Assert.IsNotNull((await lzTree.GetNode(100)).Subnodes.Single(a => a.NodeId == 120));
            Assert.IsNotNull((await lzTree.GetNode(100)).Subnodes.Single(a => a.NodeId == 210));
            
            Assert.IsNotNull((await lzTree.GetNode(210)).Subnodes.Single(a => a.NodeId == 212));
            Assert.AreEqual(1, (await lzTree.GetNode(210)).Subnodes.Count);
            
            Assert.IsFalse((await lzTree.GetNode(110)).Subnodes.Any());
            Assert.IsFalse((await lzTree.GetNode(120)).Subnodes.Any());
            Assert.IsFalse((await lzTree.GetNode(212)).Subnodes.Any());
            Assert.IsFalse((await lzTree.GetNode(200)).Subnodes.Any());
            Assert.IsFalse((await lzTree.GetNode(300)).Subnodes.Any());
        }
        
         [Test]
        public async Task LazyTreeRestructureTest2()
        {
            var api = GetApiHost();
            
            await api.EnsureTree(new EnsureTreeParams
            {
                TreeId = Tree5Id.ToGrpcUuid(),
                Description = "test5",
                ExternalIntId = 55,
                ExternalStrId = "5"
            }, null);

            await SetupUsersAndGroups(api);
            
            await MakeSimpleTree(api, Tree5Id);
            
            var lzProvider = GetObjectStorage() as ILazyTreeDataProvider;
            var lzTree = new LazyTree(Tree5Id, lzProvider);
            await lzTree.GetRootNode();
            await AssertTreeValid(lzTree);
         
            
            // move 211 node to node 300
            var nodes = new EnsureTreeNodeParams {TreeId = Tree5Id.ToGrpcUuid()};
            nodes.Nodes.AddRange(new []
            {
                new TreeNodeItemInfo {Id = Node2_1_1_id, ParentId = Node3_id, OwnerUserId = User1Id},
            });
            await api.EnsureTreeNodes(nodes, null);

            // check if three is ok
            
            /* Expected
           ....
           Node2
            - Node2-1
            -- Node2-1-2
            Node3
            - Node2-1-1
           */
            
            lzTree.InvalidateNode(210);
            lzTree.InvalidateNode(211);
            lzTree.InvalidateNode(300);
            // check if the three is ok
            
            var nd3 = await lzTree.GetNode(Node3_id);
            Assert.IsNotNull(nd3.Subnodes.Single(a=>a.NodeId==Node2_1_1_id));
            Assert.AreEqual(1, nd3.Subnodes.Count);

            var nd21 = await lzTree.GetNode(Node2_1_id);
            Assert.AreEqual(1, nd21.Subnodes.Count);
            var nd211 = await lzTree.GetNode(Node2_1_1_id);
            Assert.AreEqual(Node3_id, nd211.ParentNodeId);
            Assert.AreEqual(0, nd211.Subnodes.Count);

            // reload from scratch, make sure it works
            lzTree.Clear();
            await lzTree.GetNode(300);
            await lzTree.GetRootNode();
        }

        [Test]
        public async Task LazyTreeTest()
        {
            // todo check for cache hit count as well
            
            var api = GetApiHost();
            
            await api.EnsureTree(new EnsureTreeParams
            {
                TreeId = Tree4Id.ToGrpcUuid(),
                Description = "test4",
                ExternalIntId = 44,
                ExternalStrId = "4"
            }, null);

            await SetupUsersAndGroups(api);
            
            await MakeSimpleTree(api, Tree4Id);

            var lzProvider = GetObjectStorage() as ILazyTreeDataProvider;
            var lzTree = new LazyTree(Tree4Id, lzProvider);
            await lzTree.GetNode(211);
            Assert.AreEqual(1, lzTree.Count);
            await lzTree.GetNode(212);
            Assert.AreEqual(2, lzTree.Count);
            await lzTree.GetNode(210);
            Assert.AreEqual(3, lzTree.Count);

            await lzTree.GetNode(Node2_id);
            await AssertTreeValid(lzTree, false, true, false);
            
            
            await lzTree.GetNode(Node1_id);
            Assert.AreEqual(7, lzTree.Count);

            await lzTree.GetNode(Node1_1_id); // should not add more
            Assert.AreEqual(7, lzTree.Count);

            var rn = await lzTree.GetRootNode();
            Assert.AreEqual(8, lzTree.Count); // root node itself does not counts
            Assert.AreEqual(3, rn.Subnodes.Count);

            
            var lzTree2 = new LazyTree(Tree4Id, lzProvider);
            await lzTree2.GetRootNode();

            
            // ensure GetRootNode if working ok for partial read scenarios
            var lzTree3 = new LazyTree(Tree4Id, lzProvider);
            await lzTree3.GetNode(200);
            var root = await lzTree3.GetRootNode();
            Assert.AreEqual(3, root.Subnodes.Count);
            await AssertTreeValid(lzTree3);

        }

        private async  Task AssertTreeValid(LazyTree lzTree, bool checkNode1=true, bool checkNode2=true, bool checkNode3=true)
        {
            var expectedNodeCount = (checkNode1 ? 3 : 0) + (checkNode2 ? 4 : 0) + (checkNode3 ? 1 : 0);
            Assert.AreEqual(expectedNodeCount, lzTree.Count);

            if (checkNode1)
            {
                Assert.IsNotNull((await lzTree.GetNode(100)).Subnodes.Single(a => a.NodeId == 110));
                Assert.IsNotNull((await lzTree.GetNode(100)).Subnodes.Single(a => a.NodeId == 120));
                Assert.IsFalse((await lzTree.GetNode(110)).Subnodes.Any());
                Assert.IsFalse((await lzTree.GetNode(120)).Subnodes.Any());
            }
            
            if (checkNode2)
            {
                Assert.IsNotNull((await lzTree.GetNode(200)).Subnodes.Single(a => a.NodeId == 210));
                Assert.IsNotNull((await lzTree.GetNode(210)).Subnodes.Single(a => a.NodeId == 211));
                Assert.IsNotNull((await lzTree.GetNode(210)).Subnodes.Single(a => a.NodeId == 212));
                Assert.IsFalse((await lzTree.GetNode(211)).Subnodes.Any());
                Assert.IsFalse((await lzTree.GetNode(212)).Subnodes.Any());
            }
            
            if (checkNode1)
            {
                Assert.IsNotNull((await lzTree.GetNode(300)));
                Assert.IsFalse((await lzTree.GetNode(300)).Subnodes.Any());
            }

        }
    }
}