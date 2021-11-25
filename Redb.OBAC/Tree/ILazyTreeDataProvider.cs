using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Redb.OBAC.Core.Models;

namespace Redb.OBAC.Tree
{
    public interface ILazyTreeDataProvider
    {
        public Task<TreeNodeInfo> GetTreeNode(Guid treeId, int nodeId);

        public Task<IEnumerable<TreeNodeInfo>> GetTreeSubnodesDeep(Guid treeId, int? startingNodeId = null);
        public Task<IEnumerable<TreeNodeInfo>> GetTreeSubnodesShallow(Guid treeId, int? startingNodeId = null);
    }
}