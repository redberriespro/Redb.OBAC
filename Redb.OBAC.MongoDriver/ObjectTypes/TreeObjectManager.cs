using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Redb.OBAC.Backends;
using Redb.OBAC.MongoDriver.BL;
using Redb.OBAC.BL;
using Redb.OBAC.Core;
using Redb.OBAC.Core.Models;
using Redb.OBAC.MongoDriver.DB;
using Redb.OBAC.MongoDriver.DB.Entities;
using Redb.OBAC.Exceptions;
using Redb.OBAC.MongoDriver.ModelsPrivate;
using Redb.OBAC.ModelsPrivate;
using Redb.OBAC.Tree;

namespace Redb.OBAC.MongoDriver.ObjectTypes
{
    /// <summary>
    /// todo merge with EF.TreeObjectManager, introduce IObjectStorage interface, pull up the manager to OBAC.Core
    /// </summary>
    public class TreeObjectManager
    {
        private readonly ObjectStorage _storage;
        // todo add cache
        private readonly IObacCacheBackend _cacheBackend;
        private readonly IEffectivePermissionFeed[] _extraFeeds;
        private IEffectivePermissionFeed _mainFeed;

        public TreeObjectManager(ObjectStorage storage, IObacCacheBackend cacheBackend,
            IEffectivePermissionFeed[] extraFeeds)
        {
            _storage = storage;
            _cacheBackend = cacheBackend;
            _extraFeeds = extraFeeds;
        }

        public async Task<TreeObjectTypeInfo> GetTreeObjectById(Guid treeObjectTypeId)
        {
            var res = await _storage.GetObjectTreeById(treeObjectTypeId);
            return res;
        }
        
        public async Task<TreeObjectTypeInfo> GetTreeObjectByExternalIntId(int externalId)
        {
            var res = await _storage.GetTreeObjectByExternalIntId(externalId);
            return res;
        }

        public async Task<TreeObjectTypeInfo> GetTreeObjectByExternalStringId(string externalId)
        {
            var res = await _storage.GetTreeObjectByExternalStringId(externalId);
            return res;
        }

        public async Task DeleteTreeObjectType(Guid treeObjectTypeId, bool force)
        {
            if (!force)
            {
                var res = await _storage.GetTreeNodeCount(treeObjectTypeId);
                if (res > 0)
                    throw new ObacException("the tree does contain elements");
            }

            // todo remove all node's permissions
            await _storage.DeleteObjectTree(treeObjectTypeId);
            await _storage.DeleteObjectType(treeObjectTypeId);
        }

        public async Task<TreeObjectTypeInfo> EnsureTreeObject(Guid treeObjectTypeId, string description, int? intId,
            string stringId)
        {
            var tr = await _storage.GetObjectTreeById(treeObjectTypeId);
            if (tr == null)
            {
                await _storage.AddObjectType(treeObjectTypeId, description, ObjectTypeEnum.TreeObject);
                await _storage.CreateObjectTree(treeObjectTypeId, description, intId, stringId);
            }
            else
            {
                await _storage.UpdateObjectTree(treeObjectTypeId, description, intId, stringId);
            }

            return new TreeObjectTypeInfo
            {
                Description = description,
                TreeObjectTypeId = treeObjectTypeId
            };
        }


        public async Task DeleteTreeNode(Guid treeId, int nodeId)
        {
            var nd = await _storage.GetTreeNode(treeId, nodeId);
            if (nd == null)
                return; // nothing to delete
            var tc = new TreePermissionCalculator();
            var tr = MakeTreeActionContext(treeId);

            await tc.AfterNodeDeleted(
                GetEffectivePermissionsFeed(),
                tr, nodeId);

            // todo move this before AfterNodeDeleted call and make sure it works (at this moment, it does not remove EP for deleted node
            await _storage.DeleteTreeNode(treeId, nodeId, nd.ParentNodeId);

        }
        
        public async Task EnsureTreeNode(Guid treeId, int nodeId, int? parentId, int ownerUserId, int? intId = null, string stringId=null)
        {
            var nd = await _storage.GetTreeNode(treeId, nodeId);
            if (nd == null)
            {
                await CreateTreeNode(treeId, nodeId, parentId, ownerUserId, intId, stringId);
            }
            else
            {
                if (nd.ParentNodeId != parentId)
                {
                    await ChangeTreeNodeParent(treeId, nodeId, parentId);
                }
                // else do Nothing
            }
        }

        private async Task ChangeTreeNodeParent(Guid treeId, int nodeId, int? parentId)
        {
            // switch parent Id
            var oldParent= await _storage.ReplaceTreeNode(treeId, nodeId, parentId);
            
            if (oldParent==parentId) return;

            var tc = new TreePermissionCalculator();
            var tr = MakeTreeActionContext(treeId);

            await tc.RepairNodePermissions(GetEffectivePermissionsFeed(), tr, nodeId);
            if (oldParent.HasValue)
                await tc.RepairNodePermissions(GetEffectivePermissionsFeed(), tr, oldParent.Value);
        }

        private async Task CreateTreeNode(Guid treeId, int nodeId, int? parentId, int ownerUserId,int? intId = null, string stringId=null)
        {
            // create new Node
            var tc = new TreePermissionCalculator();
            var tr = MakeTreeActionContext(treeId);
            
            await _storage.CreateTreeNode(treeId, nodeId, parentId, ownerUserId, intId, stringId);

            await tc.AfterNodeInserted(
                GetEffectivePermissionsFeed(),
                tr, nodeId);
        }

        public async Task<List<TreeNodeInfo>> GetTreeNodes(Guid treeId, int? startingNodeId, bool deep=true)
        {
            var res = new List<TreeNodeInfo>();

            if (startingNodeId.HasValue)
            {
                res.Add(new TreeNodeInfo
                {
                    TreeObjectTypeId = treeId, 
                    NodeId = startingNodeId.Value
                });
            }

            var subNodes = deep switch
            {
                true => await _storage.GetTreeSubnodesDeep(treeId, startingNodeId),
                false => await _storage.GetTreeSubnodesShallow(treeId, startingNodeId)
            };
            res.AddRange(subNodes);

            return res;
        }
        
        public async Task<TreeNodeInfo> GetTreeNode(Guid treeId, int treeNodeId)
        {
            return await _storage.GetTreeNode(treeId, treeNodeId);
        }

        public async Task<TreeNodeInfo> GetTreeNodeByExternalIntId(Guid treeId, int externalId)
        {
            return await _storage.GetTreeNodeByExternalIntId(treeId, externalId);
        }

        public async Task<TreeNodeInfo> GetTreeNodeByExternalStringId(Guid treeId, string externalId)
        {
            return await _storage.GetTreeNodeByExternalStringId(treeId, externalId);
        }

        
        public async Task RepairTreeNodeEffectivePermissions(Guid treeId, int treeNodeId)
        {
            var tc = new TreePermissionCalculator();
            var tr = MakeTreeActionContext(treeId);
            
            await tc.RepairNodePermissions(
                GetEffectivePermissionsFeed(),
                tr, treeNodeId);
        }

        public async Task<AclInfo> GetTreeNodeAcl(Guid treeId, int treeNodeId)
        {
            var tn = await _storage.GetTreeNode(treeId, treeNodeId);
            return tn.Acl;
        }
        
        public async Task SetTreeNodeAcl(Guid treeId, int treeNodeId, AclInfo acl)
        {
                    // that's how we deal with role-based ACL items:
            // internally, TreePermissioinCalculator doesn't know about roles at all
            // we explode toles to a set of individual permissions
            // on the other hand, we store information about roles within db (see) _storage.SetTreeNodePermissions. In addition,
            
            // TODO - store in the DB a list of individual permission has been set at the moment of SetACL call
            // just in case role's permission set will change in the future
            
            try
            {
                EnsureAclValid(acl);

                var oldAcl = await GetTreeNodeAcl(treeId, treeNodeId);

                var oldAclExploded = await ExplodeAcl(oldAcl);
                var newAclExploded = await ExplodeAcl(acl);
                
                var diff = AclComparer.CompareAcls(oldAclExploded, newAclExploded);


                if (!diff.AclItemsToBeAdded.Any()
                    && !diff.AclItemsToBeRemoved.Any()
                    && diff.InheritParentPermissionsAction == NodeParentPermissionInheritanceActionEnum.KeepSame)
                    return;

                var tc = new TreePermissionCalculator();
                var tr = MakeTreeActionContext(treeId);

                var ptoadd = diff.AclItemsToBeAdded.Select(a => TreeObjectMapper.AclToPermissionInfo(treeNodeId, a))
                    .ToArray();
                var ptodel = diff.AclItemsToBeRemoved.Select(a => TreeObjectMapper.AclToPermissionInfo(treeNodeId, a))
                    .ToArray();

                await tc.ChangePermissions(
                    GetEffectivePermissionsFeed(),
                    tr, treeNodeId,
                    ptoadd,
                    ptodel,
                    diff.InheritParentPermissionsAction);

                await _storage.SetTreeNodePermissions(treeId,
                    treeNodeId, 
                    acl,
                    ptoadd,
                    ptodel);
            }
            catch (Exception ex)
            {
                await RepairTreeNodeEffectivePermissions(treeId, treeNodeId);
                throw new ObacException("Error when ACL set", ex);
            }
        }

        /// <summary>
        /// Replace role-based acl with individual permissions exploded for roles
        /// </summary>
        private async Task<AclInfo> ExplodeAcl(AclInfo acl)
        {
            var itemsExploded = new List<AclItemInfo>();
            
            foreach (var aclItem in acl.AclItems)
            {
                if (aclItem.PermissionType == AclItemInfoPermissionType.Permission)
                {
                    itemsExploded.Add(aclItem);
                    continue;
                }

                var role = await _storage.GetRoleById(aclItem.PermissionId);
                foreach (var permId in role.PermissionIds)
                {
                    itemsExploded.Add(new AclItemInfo
                    {
                        Kind = aclItem.Kind,
                        PermissionType = AclItemInfoPermissionType.Permission,
                        PermissionId = permId, UserId = aclItem.UserId, UserGroupId = aclItem.UserGroupId
                    });
                }
            }

            return new AclInfo
            {
                InheritParentPermissions = acl.InheritParentPermissions,
                AclItems = itemsExploded.ToArray()
            };
        }

        private IEffectivePermissionFeed GetEffectivePermissionsFeed()
        {
            // todo aggregation, workflow etc
            if (_mainFeed != null) return _mainFeed;
            
            var feeds = new List<IEffectivePermissionFeed>();
            feeds.Add(_storage);
            feeds.Add(new EPCacheInvalidator(_cacheBackend));
            feeds.AddRange(_extraFeeds);
            _mainFeed = new BroadcastEffectivePermissionFeedProxy(feeds.ToArray());
            return _mainFeed;
        }


        private void EnsureAclValid(AclInfo acl)
        {
            if (acl == null) throw new ArgumentNullException(nameof(acl));
            if (acl.AclItems == null) throw new ObacException("ACL item list not defined");
            foreach (var item in acl.AclItems)
            {
                if (!item.UserId.HasValue && !item.UserGroupId.HasValue) 
                    throw new ObacException($"{item.ToString()}: neither user id not user group has set");

                if (item.UserId.HasValue && item.UserGroupId.HasValue) 
                    throw new ObacException($"{item.ToString()}: both user id and user group has set");
            }
        }
        
        private TreeActionContext MakeTreeActionContext(Guid treeId)
        {
            var treeCache = new LazyTree(treeId, _storage);
            
            return new TreeActionContext
            {
                TreeId = treeId,
                GetTreeNode = async (treeObjectId, nodeId) => await treeCache.GetNode(nodeId),

                GetUsersInGroups = async groupId => await _storage.GetGroupMembers(groupId),

                GetTreeNodePermissions = async (treeObjectId, nodeId) =>
                {
                    var npe = await _storage.GetTreeNodePermissions(treeObjectId, nodeId);
                    return npe.Select(TreeObjectMapper.EntityToPermissionInfo).ToArray();
                },

                GetTreeNodePermissionList = async (treeObjectId, nodeIds, permIds) =>
                {
                    var npe = await _storage.GetTreeNodePermissionList(treeObjectId,
                        nodeIds.ToArray(), permIds.ToArray());
                    return npe.Select(TreeObjectMapper.EntityToPermissionInfo).ToArray();
                },

                GetNodeEffectivePermissions = async (treeObjectId, nodeId) =>
                {
                    var effpe = await _storage.GetEffectivePermissionsForAllUsers(treeObjectId, nodeId);
                    return effpe.ToArray();
                }
            };
        }

        public async Task UpdatePermissionsUsersGroupChanged(int userGroupId)
        {
            var treeNodesFromDb = await _storage.GetTreeNodePermissionsForGroup(userGroupId);
            var treeNodesGroupedByTreeId = treeNodesFromDb.GroupBy(t => t.TreeId);
            foreach (var tnGroup in treeNodesGroupedByTreeId)
            {
                var treeId = tnGroup.Key;
                var treeNodes = tnGroup.ToArray();
                await UpdatePermissionsUsersGroupChangedInternal(treeId, treeNodes);
            }
        }

        private async Task UpdatePermissionsUsersGroupChangedInternal(Guid treeId, ObacTreeNodePermissionEntity[] treeNodes)
        {
            var lt = new LazyTree(treeId, _storage);
            
            // 1 - load tree segment containing all the nodes affected
            foreach (var tnode in treeNodes)
            {
                await lt.EnsureTreeSegment(tnode.NodeId);
            }
            
            // 2 - obtain upper nodes
            var nodesToRebuild = lt.GetUpperNodeIds();
            foreach (var n in nodesToRebuild)
            {
                await RepairTreeNodeEffectivePermissions(treeId, n);
            }
        }

     
    }


}