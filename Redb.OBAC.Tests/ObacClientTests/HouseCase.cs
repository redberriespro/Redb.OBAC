// using System;
// using System.Collections.Generic;
// using System.ComponentModel;
// using System.Threading.Tasks;
// using NUnit.Framework;
// using Redb.OBAC.Core.Builders;
// using Redb.OBAC.Tests.Utils;
//
// namespace Redb.OBAC.Tests.RoleBasedTests
// {
//     [TestFixture]
//     public class HouseCase: TestBase
//     {
//         private IObacConfiguration _configuration;
//         
//         Guid OBJECT_HOUSE => new Guid("1a2faead-cafa-4e75-83e1-2beca1f8A001");
//         Guid OBJECT_ROOM => new Guid("1a2faead-cafa-4e75-83e1-2beca1f8A002");
//
//         Guid ROLE_OWN_HOUSE => new Guid("1a2faead-cafa-4e75-83e1-2beca1f80001");
//         Guid ROLE_RESIDENT => new Guid("1a2faead-cafa-4e75-83e1-2beca1f80002");
//         Guid ROLE_BABYSITTER => new Guid("1a2faead-cafa-4e75-83e1-2beca1f80003");
//
//         Guid PERM_CHANGE_RIGHTS => new Guid("1a2faead-cafa-4e75-83e1-2beca1f80004");
//         Guid PERM_REBUILD_HOUSE => new Guid("1a2faead-cafa-4e75-83e1-2beca1f80005");
//         Guid PERM_GET_IN => new Guid("1a2faead-cafa-4e75-83e1-2beca1f80006");
//         Guid PERM_COOK => new Guid("1a2faead-cafa-4e75-83e1-2beca1f80007");
//         Guid PERM_SLEEP => new Guid("1a2faead-cafa-4e75-83e1-2beca1f80008");
//
//         public const int USER_JOHN = 11001;
//         public const int USER_SARAH= 11002;
//         public const int USER_JANE= 11003;
//         public const int USER_NICK= 11004;
//         public const int USER_DORA= 11005;
//
//         [OneTimeSetUp]
//         public async Task InitModel()
//         {
//             await using var ctx = MakeContext();
//             await ctx.Database.EnsureCreatedAsync();
//
//             await InitCity(ctx);
//             
//
//             var cb = new ObacConfigurationBuilder();
//             cb.DeclareRole(ROLE_OWN_HOUSE, nameof(ROLE_OWN_HOUSE))
//                 .HasPermission(PERM_CHANGE_RIGHTS, nameof(PERM_CHANGE_RIGHTS))
//                 .HasPermission(PERM_REBUILD_HOUSE, nameof(PERM_REBUILD_HOUSE));
//
//             cb.DeclareRole(ROLE_RESIDENT, nameof(ROLE_RESIDENT))
//                 .HasPermission(PERM_GET_IN, nameof(PERM_GET_IN))
//                 .HasPermission(PERM_COOK, nameof(PERM_COOK))
//                 .HasPermission(PERM_SLEEP, nameof(PERM_SLEEP));
//
//             cb.DeclareRole(ROLE_BABYSITTER, nameof(ROLE_RESIDENT))
//                 .HasPermission(PERM_GET_IN, nameof(PERM_GET_IN))
//                 .HasPermission(PERM_COOK, nameof(PERM_COOK));
//
//             cb.DeclareProtectedObject<HouseEntity>(OBJECT_HOUSE, nameof(OBJECT_HOUSE))
//                 .WithIdProperty(h => (int)h.Id)
//                 // special case: if object ha
//                 .HasContextualDynamicPermission((a,context)=>
//                         context.GetValueOrDefault("SLEEPING_BAG") != null,
//                 PERM_SLEEP);
//             
//             cb.DeclareDependedProtectedObject<RoomEntity>(OBJECT_ROOM, nameof(OBJECT_ROOM))
//                 .WithMasterObjectType(OBJECT_HOUSE)
//                 .WithMasterIdProperty(r => (int) r.HouseId);
//
//             var conf = GetConfiguration(CONFIG_POSTGRES);
//             var _configuration = conf.GetObjectManager();
//             await cb.BuildAsync(_configuration);
//
//             await _configuration.EnsureUser(USER_JOHN, "John");
//             await _configuration.EnsureUser(USER_SARAH, "Sarah");
//             await _configuration.EnsureUser(USER_JANE, "Jane");
//             await _configuration.EnsureUser(USER_NICK, "Nick");
//             await _configuration.EnsureUser(USER_DORA, "Dora");
//             
//             var c = _configuration;
//
//             var permsRoleOwner = (await c.GetRoleById(ROLE_OWN_HOUSE)).PermissionIds;
//             var permsRoleResident = (await c.GetRoleById(ROLE_RESIDENT)).PermissionIds;
//             var permsRoleBabysitter = (await c.GetRoleById(ROLE_BABYSITTER)).PermissionIds;
//             
//             /// Houses: John's house #1 (John: OWN, Sarah: RESIDENT, Jane: Babysitter)
//             /// Houses: Nick's house #2 (Nick: OWN+RESIDENT, Dora: RESIDENT, Jane: Babysitter)
//
//             await c.GivePermissionsToUser(USER_JOHN, OBJECT_HOUSE, permsRoleOwner, 1);
//             await c.GivePermissionsToUser(USER_SARAH, OBJECT_HOUSE, permsRoleResident, 1);
//             await c.GivePermissionsToUser(USER_JANE, OBJECT_HOUSE, permsRoleBabysitter, 1);
//             
//             await c.GivePermissionsToUser(USER_NICK, OBJECT_HOUSE, permsRoleOwner, 2);
//             await c.GivePermissionsToUser(USER_NICK, OBJECT_HOUSE, permsRoleResident, 2);
//             await c.GivePermissionsToUser(USER_DORA, OBJECT_HOUSE, permsRoleResident, 2);
//             await c.GivePermissionsToUser(USER_JANE, OBJECT_HOUSE, permsRoleBabysitter, 2);
//         }
//
//         private static async Task InitCity(HouseCaseContext ctx)
//         {
//             ctx.Rooms.RemoveRange(ctx.Rooms.ToList());
//             ctx.Houses.RemoveRange(ctx.Houses.ToList());
//             await ctx.SaveChangesAsync();
//             
//             await ctx.Houses.AddAsync(new HouseEntity
//             {
//                 Id = 1, Name = "John's House"
//             });
//             await ctx.Rooms.AddAsync(new RoomEntity
//             {
//                 HouseId = 1, Name = "JH Room 1"
//             });
//             await ctx.Rooms.AddAsync(new RoomEntity
//             {
//                 HouseId = 1, Name = "JH Room 2"
//             });
//
//
//             await ctx.Houses.AddAsync(new HouseEntity
//             {
//                 Id = 2, Name = "Nicks's House"
//             });
//             await ctx.Rooms.AddAsync(new RoomEntity
//             {
//                 HouseId = 2, Name = "NH Room 1"
//             });
//             await ctx.SaveChangesAsync();
//         }
//
//         [Test]
//         public async Task ContextPolicePermission()
//         {
//             var house2 = await MakeContext()
//                 .Houses
//                 .Include(h => h.Rooms)
//                 .FirstAsync(h => h.Id == 2);
//             
//             var permJane = await _configuration.GetUserPermissionStore(USER_JANE);
//             Assert.IsFalse(await permJane.CheckActualPermissions(OBJECT_HOUSE, 2, PERM_SLEEP));
//
//             var ctx = new Dictionary<string, object>();
//             Assert.IsFalse(await permJane.CheckActualPermissions(house2, PERM_SLEEP, ctx));
//
//             // but If we have a secret sleeping bag...
//             ctx["SLEEPING_BAG"] = true;
//             Assert.IsTrue(await permJane.CheckActualPermissions(house2, PERM_SLEEP, ctx));
//         }
//
//         [Test]
//         public async Task CheckModelOk()
//         {
//             var c = _configuration;
//           
//
//             // check all permissions (id versions)
//             var permJohn = await c.GetUserPermissionStore(USER_JOHN);
//             
//             Assert.IsTrue(await permJohn.CheckActualPermissions(OBJECT_HOUSE, 1, PERM_REBUILD_HOUSE));
//             Assert.IsTrue(await permJohn.CheckActualPermissions(OBJECT_HOUSE, 1, PERM_CHANGE_RIGHTS));
//             Assert.IsFalse(await permJohn.CheckActualPermissions(OBJECT_HOUSE, 1, PERM_COOK));
//
//             Assert.IsFalse(await permJohn.CheckActualPermissions(OBJECT_HOUSE, 2, PERM_REBUILD_HOUSE));
//             Assert.IsFalse(await permJohn.CheckActualPermissions(OBJECT_HOUSE, 2, PERM_CHANGE_RIGHTS));
//             Assert.IsFalse(await permJohn.CheckActualPermissions(OBJECT_HOUSE, 2, PERM_COOK));
//             
//             var permSarah = await c.GetUserPermissionStore(USER_SARAH);
//             Assert.IsFalse(await permSarah.CheckActualPermissions(OBJECT_HOUSE, 1, PERM_REBUILD_HOUSE));
//             Assert.IsFalse(await permSarah.CheckActualPermissions(OBJECT_HOUSE, 1, PERM_CHANGE_RIGHTS));
//             Assert.IsTrue(await permSarah.CheckActualPermissions(OBJECT_HOUSE, 1, PERM_SLEEP));
//             Assert.IsTrue(await permSarah.CheckActualPermissions(OBJECT_HOUSE, 1, PERM_GET_IN));
//             Assert.IsFalse(await permSarah.CheckActualPermissions(OBJECT_HOUSE, 2, PERM_GET_IN));
//
//             var permJane = await c.GetUserPermissionStore(USER_JANE);
//             Assert.IsTrue(await permJane.CheckActualPermissions(OBJECT_HOUSE, 1, PERM_COOK));
//             Assert.IsTrue(await permJane.CheckActualPermissions(OBJECT_HOUSE, 1, PERM_GET_IN));
//             Assert.IsTrue(await permJane.CheckActualPermissions(OBJECT_HOUSE, 2, PERM_COOK));
//             Assert.IsTrue(await permJane.CheckActualPermissions(OBJECT_HOUSE, 2, PERM_GET_IN));
//             Assert.IsFalse(await permJane.CheckActualPermissions(OBJECT_HOUSE, 1, PERM_SLEEP));
//             Assert.IsFalse(await permJane.CheckActualPermissions(OBJECT_HOUSE, 2, PERM_SLEEP));
//
//             var permNick = await c.GetUserPermissionStore(USER_NICK);
//             Assert.IsTrue(await permNick.CheckActualPermissions(OBJECT_HOUSE, 2, PERM_REBUILD_HOUSE));
//             Assert.IsTrue(await permNick.CheckActualPermissions(OBJECT_HOUSE, 2, PERM_CHANGE_RIGHTS));
//             Assert.IsTrue(await permNick.CheckActualPermissions(OBJECT_HOUSE, 2, PERM_GET_IN));
//             Assert.IsTrue(await permNick.CheckActualPermissions(OBJECT_HOUSE, 2, PERM_SLEEP));
//
//             Assert.IsFalse(await permNick.CheckActualPermissions(OBJECT_HOUSE, 1, PERM_REBUILD_HOUSE));
//             Assert.IsFalse(await permNick.CheckActualPermissions(OBJECT_HOUSE, 1, PERM_CHANGE_RIGHTS));
//             Assert.IsFalse(await permNick.CheckActualPermissions(OBJECT_HOUSE, 1, PERM_COOK));
//
//             var permDora = await c.GetUserPermissionStore(USER_DORA);
//             Assert.IsFalse(await permDora.CheckActualPermissions(OBJECT_HOUSE, 2, PERM_REBUILD_HOUSE));
//             Assert.IsFalse(await permDora.CheckActualPermissions(OBJECT_HOUSE, 2, PERM_CHANGE_RIGHTS));
//             Assert.IsTrue(await permDora.CheckActualPermissions(OBJECT_HOUSE, 2, PERM_SLEEP));
//             Assert.IsTrue(await permDora.CheckActualPermissions(OBJECT_HOUSE, 2, PERM_GET_IN));
//             Assert.IsFalse(await permDora.CheckActualPermissions(OBJECT_HOUSE, 1, PERM_GET_IN));
//             
//             // per object ids
//             await using var ctx = MakeContext();
//             var house1 = await ctx
//                 .Houses
//                 .Include(h => h.Rooms)
//                 .FirstAsync(h => h.Id == 1);
//
//             Assert.IsTrue(await permJohn.CheckActualPermissions(house1, PERM_REBUILD_HOUSE));
//             Assert.IsTrue(await permJohn.CheckActualPermissions(house1.Rooms.First(), PERM_REBUILD_HOUSE));
//             Assert.IsTrue(await permJohn.CheckActualPermissions(house1.Rooms.Last(), PERM_REBUILD_HOUSE));
//
//             Assert.IsTrue(await permJohn.CheckActualPermissions(house1, PERM_CHANGE_RIGHTS));
//             Assert.IsFalse(await permJohn.CheckActualPermissions(house1, PERM_COOK));
//
//             
//             var house2 = await ctx
//                 .Houses
//                 .Include(h => h.Rooms)
//                 .FirstAsync(h => h.Id == 2);
//             
//             Assert.IsFalse(await permJohn.CheckActualPermissions(house2, PERM_REBUILD_HOUSE));
//             Assert.IsFalse(await permJohn.CheckActualPermissions(house2.Rooms.First(), PERM_REBUILD_HOUSE));
//
//             Assert.IsTrue(await permJane.CheckActualPermissions(house1, PERM_COOK));
//             Assert.IsTrue(await permJane.CheckActualPermissions(house2, PERM_COOK));
//             Assert.IsTrue(await permJane.CheckActualPermissions(house1.Rooms.First(), PERM_COOK));
//             Assert.IsTrue(await permJane.CheckActualPermissions(house1.Rooms.Last(), PERM_COOK));
//             Assert.IsTrue(await permJane.CheckActualPermissions(house2.Rooms.Last(), PERM_COOK));
//
//             
//         }
//
//         protected string HouseConnectionString =>
//             Container.Resolve<ObacConfig>().Configuration.Store.Connection.Replace("obac_test", "obac_test_house");
//
//         protected abstract HouseCaseContext MakeContext();
//     }
// }