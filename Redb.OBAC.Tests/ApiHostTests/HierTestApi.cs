using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Redb.OBAC.ApiClient;
using Redb.OBAC.ApiHost;
using Redb.OBAC.Tests.Utils;
using Redberries.OBAC.Api;

namespace Redb.OBAC.Tests.ApiHostTests
{
    public class HierTestApi:TestBase
    {
        private Guid OrgUnitTreeId = new Guid("A5FAEDB2-F149-4781-B32E-7F81993AD39F");
        private Guid DeletePermId = new Guid("2EDB5F72-8D70-41E5-A70A-922899A076EF");
        private Guid UpdatePermId = new Guid("F1C2BACE-52C8-4DD4-97C0-41A0837C8642");
        private Guid ViewPermId = new Guid("D3869541-AE42-42D8-B482-CAACC1556F66");

        private Guid ManageRoleId = new Guid("1DD6D727-F1C1-45D7-94CC-5D6D72012733");
        private Guid ViewRoleId = new Guid("F7F0E312-9BA9-43A3-9A62-84D6B2101D05");

        public HierTestApi(string dbName) : base(dbName) { }
        
        private async Task EnsureObjects()
        {
            var api = GetApiHost();
            
            await api.EnsureTree(new EnsureTreeParams
            {
                TreeId = OrgUnitTreeId.ToGrpcUuid(),
                Description = "organization units"
            },null);
            
            await api.EnsurePermission(new EnsurePermissionParams
            {
                PermissionId = DeletePermId.ToGrpcUuid(),
                Description = "delete"
            },null);
            
            await api.EnsurePermission(new EnsurePermissionParams
            {
                PermissionId = UpdatePermId.ToGrpcUuid(),
                Description = "update"
            },null);
            
            await api.EnsurePermission(new EnsurePermissionParams
            {
                PermissionId = ViewPermId.ToGrpcUuid(),
                Description = "view"
            },null);
            
            var rp = new EnsureRoleParams
            {
                RoleId = ManageRoleId.ToGrpcUuid(),
                Description = "role_manage",
            };
            rp.PermissionId.Add(UpdatePermId.ToGrpcUuid());
            rp.PermissionId.Add(DeletePermId.ToGrpcUuid());
            await api.EnsureRole(rp,null);
            
            rp = new EnsureRoleParams
            {
                RoleId = ViewRoleId.ToGrpcUuid(),
                Description = "role_view",
            };
            rp.PermissionId.Add(ViewPermId.ToGrpcUuid());
            await api.EnsureRole(rp,null);
        }


        [Test]
        public async Task Test1()
        {
            await EnsureObjects();
        }
        
    }
}