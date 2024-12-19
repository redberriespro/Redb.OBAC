using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Redb.OBAC.Core.Models;

namespace Redb.OBAC.Tree
{
    public interface ILazyTreeDataProvider
    {
        public Task<TreeNodeInfo> GetTreeNode(Guid treeId, Guid nodeId);

        public Task<IEnumerable<TreeNodeInfo>> GetTreeSubnodesDeep(Guid treeId, Guid? startingNodeId = null);
        public Task<IEnumerable<TreeNodeInfo>> GetTreeSubnodesShallow(Guid treeId, Guid? startingNodeId = null);
    }
}