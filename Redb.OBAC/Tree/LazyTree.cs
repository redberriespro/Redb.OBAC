using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Redb.OBAC.Core.Models;
using Redb.OBAC.Exceptions;
using Redb.OBAC.Models;

namespace Redb.OBAC.Tree
{
    public class TreeNodeItem
    {
        public Guid NodeId;
        public Guid? ParentNodeId;
        public bool InheritParentPermissions;
        public int OwnerUserid { get; set; }


        public HashSet<TreeNodeItem> Subnodes = new HashSet<TreeNodeItem>();

        public List<Guid> GetNodeIds(bool deep, bool visitChildrenWhoDoesNotHaveRightsInheritance=true)
        {
            var res = new List<Guid> {NodeId};
            foreach (var subnode in Subnodes)
            {
                res.Add(subnode.NodeId);
                if (!deep) continue;
                if (!visitChildrenWhoDoesNotHaveRightsInheritance && !subnode.InheritParentPermissions) continue;
                res.AddRange(subnode.GetNodeIds(true, visitChildrenWhoDoesNotHaveRightsInheritance));
            }

            return res;
        }
        public TreeNodeItem FindNode(Guid nodeId, bool deep)
        {
            if (this.NodeId == nodeId) return this;
            
            foreach (var subnode in Subnodes)
            {
                if (subnode.NodeId == nodeId) return subnode;
                if (!deep) continue;
                
                var n = subnode.FindNode(nodeId, true);
                if (n != null) return n;
            }

            return null;
        }
    }


    public class LazyTree
    {
        // todo add synchronization primitives, rwlocks etc
        
        private Dictionary<Guid, TreeNodeItem> _allNodes = new Dictionary<Guid, TreeNodeItem>();
        private TreeNodeItem _rootNode;

        private Guid _treeId;
        private ILazyTreeDataProvider _dataProvider;

        public LazyTree(Guid treeId, ILazyTreeDataProvider dataProvider)
        {
            _treeId = treeId;
            _dataProvider = dataProvider;
        }

        public void Clear()
        {
            _rootNode = null;
            _allNodes = new Dictionary<Guid, TreeNodeItem>();
        }

        public async Task<TreeNodeItem> GetRootNode()
        {
            if (_rootNode == null)
            {
                await EnsureEntireTree();
                return _rootNode;
            }
            
            // todo ensure all top-level nodes are here
            var rootNodes = await _dataProvider.GetTreeSubnodesShallow(_treeId);
            foreach (var n in rootNodes)
            {
                var needLoadSegment = true;

                if (_allNodes.TryGetValue(n.NodeId, out var nd))
                {
                    if (_rootNode.Subnodes.Contains(nd))
                    {
                        needLoadSegment = false;
                    }
                }

                if (needLoadSegment)
                {
                    await EnsureTreeSegment(n.NodeId);
                }
                    
            }

            return _rootNode;
        }

        public async Task<TreeNodeItem> GetNode(Guid nodeId)
        {
            if (!_allNodes.ContainsKey(nodeId))
                await EnsureTreeSegment(nodeId);
            return _allNodes[nodeId];
        }
        
        public List<Guid> GetUpperNodeIds()
        {
            var res = new List<Guid>();
            foreach (var subnode in _allNodes.Values)
            {
                if (subnode.ParentNodeId.HasValue && _allNodes.ContainsKey(subnode.ParentNodeId.Value))
                    continue;
                res.Add(subnode.NodeId);
            }

            return res;
        }


        public async Task EnsureTreeSegment(Guid nodeId)
        {
            var node = await _dataProvider.GetTreeNode(_treeId, nodeId);
            if (node == null) throw new ObacException("node not found " + nodeId);
            SettleNodes(new []{node});

            var childNodes =
                (await _dataProvider.GetTreeSubnodesDeep(_treeId, nodeId))
                .ToArray();
            SettleNodes(childNodes);
        }

        public async Task EnsureEntireTree()
        {
            if (_rootNode != null) return;

            var allNodes =
                (await _dataProvider.GetTreeSubnodesDeep(_treeId, null))
                .ToArray();
            EnsureRootNodeItem();
            SettleNodes(allNodes);
        }

        private void SettleNodes(TreeNodeInfo[] subnodeInfos)
        {
            var nodesToAdd = new Dictionary<Guid, TreeNodeItem>();

            foreach (var n in subnodeInfos)
            {
                var ni = new TreeNodeItem
                {
                    NodeId = n.NodeId,
                    ParentNodeId = n.ParentNodeId,
                    InheritParentPermissions = n.InheritParentPermissions,
                    
                    OwnerUserid = n.OwnerUserid
                };
                nodesToAdd[n.NodeId] = ni;
            }
            
            foreach (var n in nodesToAdd)
            {
                _allNodes.Remove(n.Key);
                _allNodes.Add(n.Key, n.Value);
            }
            // and finally, set up links

            foreach (var n in subnodeInfos)
            {
                if (n.ParentNodeId == null)
                {
                    EnsureRootNodeItem();
                    _rootNode.Subnodes.Add(_allNodes[n.NodeId]);
                }
                else
                {
                    if (!_allNodes.TryGetValue(n.ParentNodeId.Value, out var pn)) continue;

                    if (pn
                        .Subnodes
                        .SingleOrDefault(n2 => n2.NodeId == n.NodeId) == null)
                    {
                        pn.Subnodes.Add(_allNodes[n.NodeId]);
                    }
                }
            }
        }

        private void EnsureRootNodeItem()
        {
            if (_rootNode != null) return;
            _rootNode = new TreeNodeItem // dummy node
            {
                NodeId = Guid.Empty, Subnodes = new HashSet<TreeNodeItem>()
            };
        }

        public int Count => _allNodes.Count;

        public void InvalidateNode(Guid nodeId)
        {
            if (!_allNodes.TryGetValue(nodeId, out var node)) return; // no node found
            var toRemove = node.GetNodeIds(true);

            foreach (var ndid in toRemove)
            {
                if (!_allNodes.TryGetValue(ndid, out var nd))
                    continue;
                
                TreeNodeItem parent;
                if (!nd.ParentNodeId.HasValue)
                {
                    parent = _rootNode;
                }
                else
                {
                    _allNodes.TryGetValue(nd.ParentNodeId.Value, out parent);
                }

                if (parent?.Subnodes?.Contains(nd) != null)
                {
                    parent.Subnodes.Remove(nd);
                }

                _allNodes.Remove(ndid);
            }
            
        }

        public void InvalidateRootNode()
        {
            _rootNode = null;
            _allNodes.Clear();
        }

       
    }
}