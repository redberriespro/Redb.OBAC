// using System;
// using System.Collections.Generic;
// using System.Threading.Tasks;
// using NUnit.Framework;
// using Redb.OBAC.Models;
// using Redb.OBAC.Tests.Utils;
//
// namespace Redb.OBAC.Tests.ThreeTests
// {
//     public class HierarchyPermissionsTest : TestBase
//     {
//         public static Guid PERMISSION_DEVICE_OPERATOR => new Guid("d35b64d0-e7df-5000-8fe3-f7ccf2100001");
//         public static Guid PERMISSION_DEVICE_ADMIN => new Guid("d35b64d0-e7df-5001-8fe3-f7ccf2100002");
//
//         public static Guid OBJECT_TYPE_HOST => new Guid("d35b64d0-e7df-5002-8fe3-f7ccf2100004");
//         public static Guid OBJECT_TYPE_CATALOG => new Guid("d35b64d0-e7df-5003-8fe3-f7ccf2100004");
//
//         public const int USER_DOZOR_1 = 11004;
//         public const int USER_DOZOR_2 = 11005;
//
//         public const int DOZOR_ROOT_CATALOG = 20000;
//         public const int DOZOR_CATALOG_1 = 20001;
//         public const int DOZOR_CATALOG_2 = 20002;
//
//         public const int DOZOR_HOST_1_ID = 20003;
//
//         [Test]
//         public async Task CheckHierarchyPermissionsOnHosts()
//         {
//             var rootCatalog = CreateRootCatalog();
//             var configuration = await CreateObacConfiguration(rootCatalog);
//
//             //host to be checked in OBAC
//             var host = rootCatalog.Children.Single().Children.Single().Objects.Single();
//
//             //HOST1(OPERATOR) ---INHERIT--->CATALOG2(OPERATOR)---INHERIT--->CATALOG1(OPERATOR)---INHERIT--->ROOT(ADMIN)
//             bool userHasAdminPermissionOnHost = await configuration.HasPermission(USER_DOZOR_1, host, PERMISSION_DEVICE_ADMIN);
//             Assert.That(userHasAdminPermissionOnHost, Is.True);
//         }
//
//         [Test]
//         public async Task CheckHierarchyPermissionsOnCatalog()
//         {
//             var rootCatalog = CreateRootCatalog();
//             var configuration = await CreateObacConfiguration(rootCatalog);
//
//             //catalog to be checked in OBAC
//             var catalog = rootCatalog.Children.Single().Children.Single();
//
//             //HOST1(OPERATOR) ---INHERIT--->CATALOG2(OPERATOR)---INHERIT--->CATALOG1(OPERATOR)---INHERIT--->ROOT(ADMIN)
//             bool userHasAdminPermissionOnCatalog = await configuration.HasPermission(USER_DOZOR_1, catalog, PERMISSION_DEVICE_ADMIN);
//             Assert.That(userHasAdminPermissionOnCatalog, Is.True);
//         }
//
//         [Test]
//         public async Task CheckDirectPermissionsOnHost()
//         {
//             var rootCatalog = CreateRootCatalog();
//             var configuration = await CreateObacConfiguration(rootCatalog);
//
//             //host to be checked in OBAC
//             var host = rootCatalog.Children.Single().Children.Single().Objects.Single();
//
//             //HOST1(OPERATOR) ---INHERIT--->CATALOG2(OPERATOR)---INHERIT--->CATALOG1(OPERATOR)---INHERIT--->ROOT(ADMIN)
//             var permissions = await configuration.GetDirectPermissions(host);
//             Assert.That(permissions.Count(), Is.EqualTo(1));
//             Assert.That(permissions.Single().Permissions.Count(), Is.EqualTo(1));
//             Assert.That(permissions.Single().Permissions.Single(), Is.EqualTo(PERMISSION_DEVICE_OPERATOR));
//         }
//
//         [Test]
//         public async Task CheckInDirectPermissionsOnHost()
//         {
//             var rootCatalog = CreateRootCatalog();
//             var configuration = await CreateObacConfiguration(rootCatalog);
//
//             //host to be checked in OBAC
//             var host = rootCatalog.Children.Single().Children.Single().Objects.Single();
//
//             //HOST1(OPERATOR) ---INHERIT--->CATALOG2(OPERATOR)---INHERIT--->CATALOG1(OPERATOR)---INHERIT--->ROOT(ADMIN)
//             var permissions = await configuration.GetInDirectPermissions(host);
//             Assert.That(permissions.Count(), Is.EqualTo(1));
//             Assert.That(permissions.Single().Permissions.Count(), Is.EqualTo(2));
//             Assert.That(permissions.Single().Permissions.Contains(PERMISSION_DEVICE_ADMIN), Is.True);
//             Assert.That(permissions.Single().Permissions.Contains(PERMISSION_DEVICE_OPERATOR), Is.True);
//         }
//
//         [Test]
//         public async Task CheckDirectPermissionsOnCatalog()
//         {
//             var rootCatalog = CreateRootCatalog();
//             var configuration = await CreateObacConfiguration(rootCatalog);
//
//             //catalog to be checked in OBAC
//             var catalog = rootCatalog.Children.Single().Children.Single();
//
//             //HOST1(OPERATOR) ---INHERIT--->CATALOG2(OPERATOR)---INHERIT--->CATALOG1(OPERATOR)---INHERIT--->ROOT(ADMIN)
//             var permissions = await configuration.GetDirectPermissions(catalog);
//             Assert.That(permissions.Count(), Is.EqualTo(1));
//             Assert.That(permissions.Single().Permissions.Count(), Is.EqualTo(1));
//             Assert.That(permissions.Single().Permissions.Single(), Is.EqualTo(PERMISSION_DEVICE_OPERATOR));
//         }
//
//         [Test]
//         public async Task CheckInDirectPermissionsOnCatalog()
//         {
//             var rootCatalog = CreateRootCatalog();
//             var configuration = await CreateObacConfiguration(rootCatalog);
//
//             //catalog to be checked in OBAC
//             var catalog = rootCatalog.Children.Single().Children.Single();
//
//             //HOST1(OPERATOR) ---INHERIT--->CATALOG2(OPERATOR)---INHERIT--->CATALOG1(OPERATOR)---INHERIT--->ROOT(ADMIN)
//             var permissions = await configuration.GetInDirectPermissions(catalog);
//             Assert.That(permissions.Count(), Is.EqualTo(1));
//             Assert.That(permissions.Single().Permissions.Count(), Is.EqualTo(2));
//             Assert.That(permissions.Single().Permissions.Contains(PERMISSION_DEVICE_ADMIN), Is.True);
//             Assert.That(permissions.Single().Permissions.Contains(PERMISSION_DEVICE_OPERATOR), Is.True);
//         }
//
//         [Test]
//         public async Task RemovePermissionTest()
//         {
//             var rootCatalog = CreateRootCatalog();
//             var configuration = await CreateObacConfiguration(rootCatalog);
//
//             var host = rootCatalog.Children.Single().Children.Single().Objects.Single();
//
//             var permissions = await configuration.GetDirectPermissions(host);
//             Assert.That(permissions.Count(), Is.EqualTo(1));
//
//             //HOST1(OPERATOR) ---INHERIT--->CATALOG2(OPERATOR)---INHERIT--->CATALOG1(OPERATOR)---INHERIT--->ROOT(ADMIN)
//             await configuration.RevokePermission(SubjectTypeEnum.User, USER_DOZOR_1, host.Type, PERMISSION_DEVICE_OPERATOR, host.Id);
//
//             permissions = await configuration.GetDirectPermissions(host);
//             Assert.That(permissions.Count(), Is.EqualTo(0));
//         }
//
//         private async Task<IObacConfiguration> CreateObacConfiguration(Catalog rootCatalog)
//         {
//             var configurationBuilder = new ObacConfigurationBuilder();
//             configurationBuilder
//                 .DeclarePermission(PERMISSION_DEVICE_OPERATOR, nameof(PERMISSION_DEVICE_OPERATOR))
//                 .DeclarePermission(PERMISSION_DEVICE_ADMIN, nameof(PERMISSION_DEVICE_ADMIN));
//             var configuration = Configuration;
//             await configurationBuilder.BuildAsync(configuration);
//
//             await configuration.EnsureUser(USER_DOZOR_1, "USER_1");
//             await configuration.EnsureUser(USER_DOZOR_2, "USER_1");
//
//             //USER 1 is OPERATOR for HOST 1
//             await configuration.GivePermissionToUser(USER_DOZOR_1, rootCatalog.Children.Single().Children.Single().Objects.Single(), PERMISSION_DEVICE_OPERATOR);
//
//             //USER 1 is ADMIN for ROOT catalog
//             await configuration.GivePermissionToUser(USER_DOZOR_1, rootCatalog, PERMISSION_DEVICE_ADMIN);
//
//             //USER 1 is OPERATOR for CATALOG1 catalog
//             await configuration.GivePermissionToUser(USER_DOZOR_1, rootCatalog.Children.Single(), PERMISSION_DEVICE_OPERATOR);
//
//             //USER 1 is OPERATOR for CATALOG2 catalog
//             await configuration.GivePermissionToUser(USER_DOZOR_1, rootCatalog.Children.Single().Children.Single(), PERMISSION_DEVICE_OPERATOR);
//             return configuration;
//         }
//
//         private static Catalog CreateRootCatalog()
//         {
//             var rootCatalog = new Catalog()
//             {
//                 Id = DOZOR_ROOT_CATALOG,
//                 Parent = null,
//                 Objects = new List<IObject>(),
//                 Children = new List<ICatalog>()
//             };
//
//             var catalog1 = new Catalog()
//             {
//                 Id = DOZOR_CATALOG_1,
//                 Parent = rootCatalog,
//                 Children = new List<ICatalog>(),
//                 Objects = new List<IObject>(),
//                 InheritParentPermissions = true
//             };
//             rootCatalog.Children.Add(catalog1);
//
//             var catalog2 = new Catalog()
//             {
//                 Id = DOZOR_CATALOG_2,
//                 Parent = catalog1,
//                 Children = new List<ICatalog>(),
//                 Objects = new List<IObject>(),
//                 InheritParentPermissions = true
//             };
//             catalog2.Objects.Add(new Host() { Id = DOZOR_HOST_1_ID, Catalog = catalog2, InheritCatalogPermissions = true });
//             catalog1.Children.Add(catalog2);
//
//             return rootCatalog;
//         }
//     }
// }
