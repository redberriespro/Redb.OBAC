using System;
using System.Threading.Tasks;
using Redb.OBAC.Core.Hierarchy;
using Redb.OBAC.Core.Models;
using Redb.OBAC.Models;

namespace Redb.OBAC.Tree
{
   
    public class TreePathNode
    {
        public TreePathNode Parent { get; set; } 
        public TreeNodeItem Node { get; set; }
        public TreeNodePermissionInfo[] CurrentEffectivePermissions { get; set; }
        public TreeNodePermissionInfo[] UpdatedEffectivePermissions { get; set; }

    }

    public class VisitOptions
    {
        public bool VisitChildrenWhoDoesNotHaveRightsInheritance = true;
    };
    public class SimpleTreeVisitor
    {
        public enum WhatToDoNext
        {
            Continue,
            Break
        };
        public async Task VisitNodes(
            TreeActionContext ctx,
            int nodeId,
            Func<TreePathNode, Task<WhatToDoNext>> action,
            VisitOptions options = null)
        {
            var rootNode = await ctx.GetTreeNode(ctx.TreeId, nodeId);

            var path = new TreePathNode {Node = rootNode};
            
            var whatToDo = await action(path);
            if (whatToDo == WhatToDoNext.Break) return;
            await VisitNodesImpl(action, path, options??new VisitOptions());
        }
        
        public async Task VisitNodes(
            TreeNodeItem rootNode,
            Func<TreePathNode, Task<WhatToDoNext>> action,
            VisitOptions options = null)
        {
            var path = new TreePathNode {Node = rootNode};
            
            var whatToDo = await action(path);
            if (whatToDo == WhatToDoNext.Break) return;
            await VisitNodesImpl(action, path, options??new VisitOptions());
            // todo cleanup parent effective permissions for every node which cannot be accessed during further visiting (i.e., presceeding siblings)
        }


        private static async Task<WhatToDoNext> VisitNodesImpl(Func<TreePathNode, Task<WhatToDoNext>> action,
            TreePathNode parentPath, VisitOptions options)
        {
            var nd = parentPath.Node;
            if (nd.Subnodes == null) return WhatToDoNext.Continue;

            foreach (var subnode in nd.Subnodes)
            {
                if (!subnode.InheritParentPermissions && !options.VisitChildrenWhoDoesNotHaveRightsInheritance)
                    continue;
                
                
                var path = new TreePathNode {Node = subnode, Parent = parentPath};
     
                var whatToDo = await action(path);
                if (whatToDo == WhatToDoNext.Break) return WhatToDoNext.Break;

                whatToDo = await VisitNodesImpl(action, path, options);
                if (whatToDo == WhatToDoNext.Break) return WhatToDoNext.Break;
            }

            return WhatToDoNext.Continue;
        }
    }
}