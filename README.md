# Redb.OBAC - Object-Based Access Control Library
*Access Control and effective rights calculation for hierarchical data structures*

The library proposes an alternative approach to declarative ABAC libraries.
Instead of writing complex rules for declaring permissions and access rights to resources, Redb.OBAC allows us to apply ACL lists to nodes of a hierchical structure.

Discussions: [Telegram Chat](https://t.me/+InGrdq8qXeYyOTNi)

## Current State
It's ALPHA version of the second generation code. "Alpha" means core APIs are more or less stable but *MIGHT* be changed before it becomes beta.

Technologies Supported
- NetCore 3.1/Net5.0
- PostgreSQL as DB engine (Supporting MySQL is in the roadmap)
- gRPC (for accessing OBAC API from outside .Net)

## Features
- Users and UserGroups support
- Multiple Object Types
- Multiple Permissions and Roles
- Set ACL to an object or a node (object tree structures are supported)
- Allow or Deny Permission to an Object for User ot User Group. 
- Inherit-permissions-from-parent flag
- Can be used by any language by calling OBAC's API via gRPC protocol (API Host process is included)

## Code Examples
Library initialization (generic):
```c#
var pgStorage = new PgSqlObacStorageProvider(OBAC_CONNECTION);
await pgStorage.EnsureDatabaseExists();
var obacConfiguration = ObacManager.CreateConfiguration(pgStorage);
obacManager = obacConfiguration.GetObjectManager();
```

Local effective permission cache (can be used to apply permissions at DB level):
```c#
ctx = new HelloDbContext();
var pgStorage = new PgSqlObacStorageProvider(OBAC_CONNECTION);
await pgStorage.EnsureDatabaseExists();
            
var epLocalReceiver = new EffectivePermissionsEfReceiver(ctx);
var obacConfiguration = ObacManager.CreateConfiguration(pgStorage, epLocalReceiver);
```

Set up security model:
```c#
var readPermission = Guid.NewGuid();
await obacManager.EnsurePermission(readPermission, "read");
var writePermission = Guid.NewGuid();
await obacManager.EnsurePermission(writePermission, "write");

await obacManager.EnsureUser(1, "user 1");
await obacManager.EnsureUser(2, "user 2");
await obacManager.EnsureUserGroup(10, "group1");
await obacManager.AddUserToUserGroup(10,1);
await obacManager.AddUserToUserGroup(10,2);
```

Set up Object Types and object hierarchy
```c#
var docType = Guid.NewGuid();
await obacManager.EnsureTree(docType, "Documents");

await obacManager.EnsureTreeNode(docType, 100, null, 1);
await obacManager.EnsureTreeNode(docType, 110, 100, 1);
await obacManager.EnsureTreeNode(docType, 200, null, 1);
await obacManager.EnsureTreeNode(docType, 210, 200, 1);
```

Setting up ACL lists:
```c#
await obacManager.SetTreeNodeAcl(docType, 100, new AclInfo
{ InheritParentPermissions = false,
  AclItems = new[] {
     new AclItemInfo { UserGroupId = 10, PermissionId = readPermission, Kind = PermissionKindEnum.Allow },
     new AclItemInfo { UserId = 2, PermissionId = writePermission, Kind = PermissionKindEnum.Allow }
  }
});
```

Checking user's rights to objects (via API):
```c#
var checker = obacConfiguration.GetPermissionChecker(1);
Guid[] effectivePermissions = await checker.GetObjectPermissions(docType, 110);
var hasReadAccessToDocument110 = await checker.CheckObjectPermissions(docType, 110, readPermission);
```

Checking user's rights to objects (on DB level via Entity Framework):
```c#
var docsUser1CanRead = from d in (new HelloDbContext()).Documents
    join p in ctx.EffectivePermissions
    on d.Id equals p.ObjectId
  where
    p.ObjectTypeId == docType 
    && p.UserId == 1  
    && p.PermissionId == readPermission
  select d;
```

For more details please discover code Examples (https://github.com/redberriespro/Redb.OBAC/tree/main/Examples) and Unit Tests (https://github.com/redberriespro/Redb.OBAC/tree/main/Redb.OBAC.Tests)

## Example Apps
- Simple app with effective permissions cache stored in local DB (C#, EF) - https://github.com/redberriespro/Redb.OBAC/blob/main/Examples/HelloObacEf/Program.cs

## Credits
Initially Created by
- Yury Skaletskiy (yury@redberries.pro)
- Dmitry Koval (dkoval@redberries.pro)

Great thanks to our dearest contributors, including
- [PapaCarloSap](https://github.com/PapaCarloSap) (MySql backend)
- [kaiser113-ru](https://github.com/kaiser113-ru) (MongoDB and MSSQL backends)

(c) 2021-... Redberries.pro
