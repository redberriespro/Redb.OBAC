using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Redb.OBAC.BL;
using Redb.OBAC.Core;
using Redb.OBAC.Core.Ep;
using Redb.OBAC.Core.Models;
using Redb.OBAC.Models;
using Redb.OBAC.ModelsPrivate;

namespace Redb.OBAC.Tree
{
   

    public enum NodeParentPermissionInheritanceActionEnum
    {
        KeepSame,
        SetInherit,
        SetDoNotInherit
    }

   
    
    /// <summary>
    /// context for most of tree
    /// node explode/refresh calculations
    /// </summary>
    public class TreeActionContext
    {
        /// <summary>
        /// id of a tree being manipulated
        /// </summary>
        public Guid TreeId { get; set; }
        
        /// <summary>
        /// provide subnode list for a given node of a current tree
        /// </summary>
        public Func<Guid, Guid, Task<TreeNodeItem>> GetTreeNode { get; set; }
        
        /// <summary>
        /// provide original permission set for a given tree node
        /// </summary>
        public Func<Guid, Guid, Task<TreeNodePermissionInfo[]>> GetTreeNodePermissions { get; set; }
        
        /// <summary>
        /// get requested list of permissions values for a given set of nodes
        /// </summary>
        public Func<Guid, IEnumerable<Guid>, IEnumerable<Guid>, Task<TreeNodePermissionInfo[]>> GetTreeNodePermissionList { get; set; }
        
        /// <summary>
        /// unwind user groups
        /// </summary>
        public Func<int, Task<int[]>> GetUsersInGroups { get; set; }
        
        /// <summary>
        /// load or calculate effective permissions for a certain node 
        /// </summary>
        public Func<Guid, Guid, Task<TreeNodePermissionInfo[]>> GetNodeEffectivePermissions { get; set; }

        
    }
    
    public class TreePermissionCalculator
    {
        private readonly EffectivePermissionCalculator _effectivePermissionCalculator = new EffectivePermissionCalculator();
        
        /// <summary>
        /// produce list of actions sufficient to fully restore actual permissions for
        /// a given node and all their children 
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        public async Task RepairNodePermissions(
            IEffectivePermissionFeed feed, 
            TreeActionContext ctx, Guid nodeId
        )
        {
            var visitor = new SimpleTreeVisitor();

            await visitor.VisitNodes( ctx, nodeId,
                async path =>
                {
                    if (path.CurrentEffectivePermissions != null) return SimpleTreeVisitor.WhatToDoNext.Continue;
                    
                    var nodePermissions = await ctx
                            .GetTreeNodePermissions(ctx.TreeId, path.Node.NodeId);
                    
                    var parentEffectivePermissions = await LoadParentsCurrentEffectivePermissionsFromPermanentStorage(ctx, path);

                    var ngp = await UnwindGroupBasedPermissions(ctx, nodePermissions);
                    path.CurrentEffectivePermissions = _effectivePermissionCalculator
                        .CalculateEffectivePermissions(path.Node.NodeId, path.Node.InheritParentPermissions,
                            nodePermissions,
                            ngp,
                            parentEffectivePermissions);

                    await feed.FeedWithActionList(new[]
                    {
                        new PermissionActionInfo
                        {
                            Action = PermissionActionEnum.RemoveAllObjectsDirectPermission,
                            ObjectId = path.Node.NodeId,
                            ObjectTypeId = ctx.TreeId
                        }
                    });
                    
                    await feed.FeedWithActionList(path
                        .CurrentEffectivePermissions
                        .Where(ep => ep.UserId.HasValue && !ep.DenyPermission)
                        .Select(ep => new PermissionActionInfo
                        {
                            Action = PermissionActionEnum.AddDirectPermission,
                            ObjectId = path.Node.NodeId,
                            ObjectTypeId = ctx.TreeId,
                            PermissionId = ep.PermissionId,
                            UserId = ep.UserId.Value
                        }));

                    return SimpleTreeVisitor.WhatToDoNext.Continue;
                });
        }

        /// <summary>
        /// incremental action set will be generated.
        /// the result will contain actions required to insert a node or subtree to a new location
        /// </summary>
        public async Task AfterNodeInserted(IEffectivePermissionFeed feed, TreeActionContext ctx, Guid nodeId)
        {
            await RepairNodePermissions(feed, ctx, nodeId);
        }
        
        public async Task AfterNodeDeleted(IEffectivePermissionFeed feed, TreeActionContext ctx, Guid nodeId)
        {
            await RepairNodePermissions(feed, ctx, nodeId);
        }

        /// <summary>
        /// incremental action set will be generated.
        /// the result will contain actions required to subtree removal
        /// </summary>
        public async Task BeforeNodeRemoved(IEffectivePermissionFeed feed, TreeActionContext ctx, Guid nodeId)
        {
            var visitor = new SimpleTreeVisitor();
            
            await visitor.VisitNodes( ctx, nodeId,
                async path =>
                {
                    if (path.CurrentEffectivePermissions != null) return SimpleTreeVisitor.WhatToDoNext.Continue;
                    
                    await feed.FeedWithActionList(
                        new []{
                        new PermissionActionInfo
                    {
                        Action = PermissionActionEnum.RemoveAllObjectsDirectPermission,
                        ObjectId = path.Node.NodeId,
                        ObjectTypeId = ctx.TreeId
                    }});
                    return SimpleTreeVisitor.WhatToDoNext.Continue;
                });
        }

        /// <summary>
        /// incremental action set will be generated.
        /// the result will contain permissionToAdd/remove properly Added 
        /// </summary>
        public async Task ChangePermissions(IEffectivePermissionFeed feed, TreeActionContext ctx, Guid nodeId, 
            TreeNodePermissionInfo[] permissionsToAdd, 
            TreeNodePermissionInfo[] permissionsToRemove,
            NodeParentPermissionInheritanceActionEnum inheritParentPermissions = NodeParentPermissionInheritanceActionEnum.KeepSame)
        {
            var visitor = new SimpleTreeVisitor();
            var startingNode = await ctx.GetTreeNode(ctx.TreeId, nodeId);

            var nodeIds = startingNode.GetNodeIds(true);
            
            var permIds = new List<Guid>();
            if (permissionsToAdd!=null)
                permIds.AddRange(permissionsToAdd.Select(a=>a.PermissionId));
            if (permissionsToRemove!=null)
                permIds.AddRange(permissionsToRemove.Select(a=>a.PermissionId));
            
            // key if node id, value is set of permissions set for a node - only those ones who get affected by a change 
            var permissionBag = await GetPermissionsForNodeSet(ctx, nodeIds, permIds);
            
            // todo - get effective permissions for a parent node (if exists)
            
            // visit subtree and set up list of effective permissions for each node, calculate list of actions required
            await visitor.VisitNodes(startingNode,
                async path =>
                {
                    // the node has already been processed, skipping
                    if (path.CurrentEffectivePermissions != null) return SimpleTreeVisitor.WhatToDoNext.Continue;

                    // STEP 1: restore current permissions on all three levels (user<group<parent)
                    
                    // get effective permissions for it's parent
                    var currentParentEffectivePermissions = await LoadParentsCurrentEffectivePermissionsFromPermanentStorage(ctx, path);
                    
                    // get old permissions for the current node
                    var currentNodePermissions = permissionBag.ContainsKey(path.Node.NodeId)
                        ? permissionBag[path.Node.NodeId].ToArray()
                        :Array.Empty<TreeNodePermissionInfo>();
                    
                    var currentNodeGroupPermissions = await UnwindGroupBasedPermissions(ctx, currentNodePermissions);
                    
                    var updatedInheritanceFlag = inheritParentPermissions switch
                    {
                        NodeParentPermissionInheritanceActionEnum.KeepSame => path.Node.InheritParentPermissions,
                        NodeParentPermissionInheritanceActionEnum.SetInherit => true,
                        NodeParentPermissionInheritanceActionEnum.SetDoNotInherit => false,
                        _ => throw new ArgumentOutOfRangeException(nameof(inheritParentPermissions), inheritParentPermissions, null)
                    };
                    
                    // calculate current effective permissions based on current' everything (and a parents' E.P.) 
                    var currentEffectivePermissions = _effectivePermissionCalculator
                        .CalculateEffectivePermissions(path.Node.NodeId, path.Node.InheritParentPermissions,
                            currentNodePermissions.ToArray(),
                            currentNodeGroupPermissions,
                            currentParentEffectivePermissions);

                    path.CurrentEffectivePermissions = currentEffectivePermissions;
                    
                    // STEP 2: calculate updated permissions based on old and new states
                    
                    // for the current node we're setting the permissions, update effective permissions with new provided ones
                    var updatedNodePermissions = path.Node.NodeId == nodeId 
                        ? MergePermissionList(currentNodePermissions, permissionsToAdd, permissionsToRemove)
                        : currentNodePermissions;

                    // (step 3 preps) unwind updated group permissions (if it has ones)
                    var updatedNodeGroupPermissions = await UnwindGroupBasedPermissions(ctx, updatedNodePermissions);

                    // STEP3: calculate updated permissions against updated node permissions
                    
                    // for starting node, we assume updated and current parent permissions as the same, 'cos we haven't
                    // anything updated for parent node
                    
                    var updatedParentEffectivePermissions =
                        path.Parent?.UpdatedEffectivePermissions ?? currentParentEffectivePermissions;

                   

                    // for nodeId and deeper nodes, effective permission list will be calculated, 
                    // while it is loaded from db for nodeId's parent's node
                    path.UpdatedEffectivePermissions = _effectivePermissionCalculator
                        .CalculateEffectivePermissions(path.Node.NodeId, updatedInheritanceFlag,
                            updatedNodePermissions,
                            updatedNodeGroupPermissions,
                            updatedParentEffectivePermissions);

                    var actionsDiff = AclComparer.CompareEffectivePermissions(
                        path.CurrentEffectivePermissions,
                        path.UpdatedEffectivePermissions
                        , ctx.TreeId);
                    await feed.FeedWithActionList(actionsDiff);

                    return SimpleTreeVisitor.WhatToDoNext.Continue;
                }, 
                new VisitOptions {VisitChildrenWhoDoesNotHaveRightsInheritance = false}); // we're not interesting in those ones during top-down  
        }

        private async Task<TreeNodePermissionInfo[]> LoadParentsCurrentEffectivePermissionsFromPermanentStorage(TreeActionContext ctx, TreePathNode path)
        {
            if (path.Parent?.CurrentEffectivePermissions != null)
                return path.Parent.CurrentEffectivePermissions;

            if (!path.Node.ParentNodeId.HasValue)
                return null;
            
            var effectivePerms = await ctx.GetNodeEffectivePermissions(ctx.TreeId, path.Node.ParentNodeId.Value);
            if (path.Parent != null)
            {
                path.Parent.CurrentEffectivePermissions = effectivePerms;
            }
            
            return effectivePerms; 
        }

        private TreeNodePermissionInfo[] MergePermissionList(IEnumerable<TreeNodePermissionInfo> nodePermissions,
            TreeNodePermissionInfo[] permissionsToAdd, 
            TreeNodePermissionInfo[] permissionsToRemove)
        {
            var newList = nodePermissions.ToList();
            var toRemove = new List<TreeNodePermissionInfo>();
            foreach (var p in permissionsToRemove ?? Array.Empty<TreeNodePermissionInfo>())
            {
                toRemove.AddRange(newList.Where(p2 => p.Equals(p2)));
            }

            foreach (var r in toRemove)
            {
                newList.Remove(r);
            }
            
            newList.AddRange(permissionsToAdd??Array.Empty<TreeNodePermissionInfo>());

            return newList.ToArray();
        }

        

       
        private async Task<Dictionary<Guid, List<TreeNodePermissionInfo>>> GetPermissionsForNodeSet(TreeActionContext ctx, IEnumerable<Guid> nodeIds, IEnumerable<Guid> permissionIds)
        {
            // todo split onto chunks (4instance, split by 200) during retreival
            var perms = await ctx.GetTreeNodePermissionList(ctx.TreeId, nodeIds, permissionIds);

            var res = new Dictionary<Guid, List<TreeNodePermissionInfo>>();
            foreach (var p in perms)
            {
                List<TreeNodePermissionInfo> nest = null;
                if (res.TryGetValue(p.NodeId, out var nest2))
                {
                    nest = nest2;
                } else {
                    nest = new List<TreeNodePermissionInfo>();
                    res[p.NodeId] = nest;
                }
                
                nest.Add(p);
            }

            return res;
        }

        private async Task<TreeNodePermissionInfo[]> UnwindGroupBasedPermissions(TreeActionContext ctx, 
            IEnumerable<TreeNodePermissionInfo> plist)
        {
            var ngp =  new List<TreeNodePermissionInfo>();
            foreach (var p in plist.Where(p=>p.UserGroupId.HasValue))
            {
                var groupIds = await ctx.GetUsersInGroups(p.UserGroupId.Value);
                foreach (var uid in groupIds)
                {
                    ngp.Add(new TreeNodePermissionInfo
                    {
                        DenyPermission = p.DenyPermission,
                        PermissionId = p.PermissionId,
                        UserId = uid
                    });
                }
            }

            return ngp.ToArray();
        }


     
    }
}