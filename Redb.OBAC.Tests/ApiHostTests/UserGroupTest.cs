using System.Threading.Tasks;
using NUnit.Framework;
using Redb.OBAC.Tests.Utils;
using Redberries.OBAC.Api;

namespace Redb.OBAC.Tests.ApiHostTests
{
    [TestFixture]
    public class UserGroupTest: TestBase
    {
        [Test]
        public async Task Users()
        {
            var api = GetApiHost(TestBase.CONFIG_POSTGRES);
            var res = await api.EnsureUser(new EnsureUserParams
            {
                UserId = 10, Description = "user10", ExternalIntId = 1010, ExternalStrId = "1010"
            },null);
            Assert.IsNotNull(res);

            var res2 = await api.GetUserById(new GetUserParams{UserId = 10}, null);
            Assert.AreEqual(res2.UserId,10);
            Assert.AreEqual(res2.Description,"user10");

            res2 = await api.GetUserById(new GetUserParams{UserId = 101010}, null);
            Assert.AreEqual(res2.UserId,0);

            await api.DeleteUser(new DeleteUserParams {UserId = 10},null);
            res2 = await api.GetUserById(new GetUserParams{UserId = 10}, null);
            Assert.AreEqual(res2.UserId,0);
        }
        
        [Test]
        public async Task Groups()
        {
            var api = GetApiHost(TestBase.CONFIG_POSTGRES);
            var res = await api.EnsureUserGroup(new EnsureUserGroupParams
            {
                UserGroupId = 10, Description = "group10", ExternalIntId = 1010, ExternalStrId = "1010"
            },null);
            Assert.IsNotNull(res);
            
            var res3 = await api.EnsureUser(new EnsureUserParams
            {
                UserId = 11, Description = "user11", ExternalIntId = 1111, ExternalStrId = "1111"
            },null);
            res3 = await api.EnsureUser(new EnsureUserParams
            {
                UserId = 12, Description = "user12", ExternalIntId = 1212, ExternalStrId = "1212"
            },null);

            var res2 = await api.GetUserGroupById(new GetUserGroupParams{UserGroupId = 10}, null);
            Assert.AreEqual(res2.UserGroupId,10);
            Assert.AreEqual(res2.Description,"group10");

           

            await api.AddUserToGroupById(new AddUserToGroupParams {UserGroupId = 10, UserId = {11}}, null);
            await api.AddUserToGroupById(new AddUserToGroupParams {UserGroupId = 10, UserId = {11,12}}, null);

            var res4 = await api.GetUserGroupUsers(new GetUserGroupParams {UserGroupId = 10},null);

            Assert.AreEqual(res4.MemberUserId.Count, 2);
            Assert.IsTrue(res4.MemberUserId.Contains(11));
            Assert.IsTrue(res4.MemberUserId.Contains(12));
            
            await api.RemoveUserFromGroupById(new RemoveUserFromGroupParams {UserGroupId = 10, UserId = {11}}, null);
            res4 = await api.GetUserGroupUsers(new GetUserGroupParams {UserGroupId = 10},null);

            Assert.AreEqual(res4.MemberUserId.Count, 1);
            Assert.IsTrue(res4.MemberUserId.Contains(12));

                
            res2 = await api.GetUserGroupById(new GetUserGroupParams{UserGroupId = 101010}, null);
            Assert.AreEqual(res2.UserGroupId,0);
            
            await api.DeleteUserGroup(new DeleteUserGroupParams {UserGroupId = 10},null);
            res2 = await api.GetUserGroupById(new GetUserGroupParams{UserGroupId = 10}, null);
            Assert.AreEqual(res2.UserGroupId,0);
        }
    }
}