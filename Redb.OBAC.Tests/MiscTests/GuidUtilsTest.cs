using System;
using NUnit.Framework;
using Redb.OBAC.Utils;

namespace Redb.OBAC.Tests.MiscTests
{
    [TestFixture]
    public class GuidUtilsTest
    {
        [Test]
        public void GuidDiff1()
        {
            var g1 = "A8BC8F9D-5E4D-474F-AF39-0CBD044F4414";
            var g2 = "2AA001BB-C681-4D41-B472-1C4E04DDD0C7";

            var l1 = new Guid[]
            {
                Guid.Parse(g1),
                Guid.Parse(g2),
            };
            
            var l2 = new Guid[]
            {
                Guid.Parse(g1),
                Guid.Parse(g2),
            };
            
            Assert.IsTrue(GuidUtils.ListsEquals(l1,l2));
            Assert.IsFalse(GuidUtils.ListsEquals(l1,new Guid[]
            {
                Guid.Parse(g1)}
            ));

        }
    }
}