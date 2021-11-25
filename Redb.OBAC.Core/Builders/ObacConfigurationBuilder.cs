using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Redb.OBAC.Core.Models;

namespace Redb.OBAC.Core.Builders
{

    public class ObacConfigurationBuilder
    {
        private readonly List<PermissionInfo> _permissions = new List<PermissionInfo>();
        private readonly List<RoleInfo> _roles = new List<RoleInfo>();


        public class PermInfo
        {
            public Guid Id;
            public string Name;
        }

        public class RoleInfo
        {
            public Guid Id;
            public string Name;
            public readonly List<PermInfo> Permissions = new List<PermInfo>();
        }


        public class ProtectedDependedObjectBuilder<TEntity>
        {
            private readonly ObacConfigurationBuilder _cb;

            public ProtectedDependedObjectBuilder(ObacConfigurationBuilder cb)
            {
                _cb = cb;
            }

        }

        public class RoleBuilder
        {
            private readonly ObacConfigurationBuilder _cb;
            private readonly RoleInfo _role;


            public RoleBuilder(ObacConfigurationBuilder cb, RoleInfo role)
            {
                _cb = cb;
                _role = role;
            }

            public RoleBuilder HasPermission(Guid permId, string permName = null)
            {
                _role.Permissions.Add(new PermInfo
                    {Id = permId, Name = permName});

                return this;
            }
        }

        public RoleBuilder DeclareRole(Guid roleId, string roleName = null)
        {
            var role = new RoleInfo
            {
                Id = roleId, Name = roleName
            };
            _roles.Add(role);
            return new RoleBuilder(this, role);
        }

        public ObacConfigurationBuilder DeclarePermission(Guid permId, string permName = null)
        {
            var perm = new PermissionInfo
            {
                PermissionId = permId, Description = permName
            };
            _permissions.Add(perm);
            return this;
        }



        public async Task BuildAsync(IObacObjectManager configuration)
        {
            var allRolePermissions = _roles.SelectMany(r => r.Permissions);
            foreach (var p in allRolePermissions)
            {
                await configuration.EnsurePermission(p.Id, p.Name, force: true);
            }

            foreach (var p in _permissions)
            {
                await configuration.EnsurePermission(p.PermissionId, p.Description, force: true);
            }

            foreach (var ri in _roles)
            {
                await configuration.EnsureRole(ri.Id, ri.Name, ri.Permissions.Select(p => p.Id).ToArray(), force: true);
            }

            //  consider removing that
            // var rtInfo = configuration as IConfigurationStoreInternal;
            //
            // foreach (var o in _objects.Where(o => o.InfoType == ObjectInfo.ObjectInfoType.Simple))
            // {
            //     await configuration.EnsureSimpleObjectType(o.Id, o.Name, force:true);
            //     break;
            // }
            //
            // foreach (var o in _objects)
            // { 
            //     switch (o.InfoType)
            //     {
            //         case ObjectInfo.ObjectInfoType.Simple: break;
            //
            //         case ObjectInfo.ObjectInfoType.Depended:
            //         {
            //             var otMaster = await configuration.GetSimpleObjectTypeById(o.MasterObjectId);
            //             if (otMaster == null) 
            //                 throw new ObacException($"Master object type not found: {o.MasterObjectId}");
            //             break;
            //         } 
            //
            //         default: throw new OverflowException($"Object Type Unknown: {o.InfoType}");
            //     }
            //
            //     if (o.RuntimeEntityType != null)
            //         rtInfo?.RegisterRuntimeObjectType(o);
        }
    }
}