using System;
using NUnit.Framework;
using Redb.OBAC.Core.Models;
using Redb.OBAC.Models;

namespace Redb.OBAC.Tests.ThreeTests
{
    [TestFixture]
    public class TnpiTest
    {
        [Test]
        public void ParseTest()
        {
            var g = Guid.NewGuid();
            var tnpi = new TreeNodePermissionInfo
            {
                UserId = 666, UserGroupId = null, DenyPermission = true, NodeId = 10, PermissionId = g
            };

            var tnpi2 = TreeNodePermissionInfo.Parse(tnpi.ToString());
            
            Assert.AreEqual(tnpi.UserId, tnpi2.UserId);
            Assert.AreEqual(tnpi.UserGroupId, tnpi2.UserGroupId);
            Assert.AreEqual(tnpi.NodeId, tnpi2.NodeId);
            Assert.AreEqual(tnpi.PermissionId, tnpi2.PermissionId);
            Assert.AreEqual(tnpi.DenyPermission, tnpi2.DenyPermission);
            Assert.AreEqual(tnpi.GetHashCode(), tnpi2.GetHashCode());
        }
    }
}