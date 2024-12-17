using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Redb.OBAC.Client.EffectivePermissionsReceiver;
using Redb.OBAC.Core;
using Redb.OBAC.Core.Models;
using Redb.OBAC.EF;
using Redb.OBAC.PgSql;

namespace HelloObac
{
    public class Program
    {
        private static Guid docTypeId;
        
        // internal OBAC database
        public const string OBAC_CONNECTION =
            "Host=localhost;Port=5432;Database=obac;Username=postgres;Password=12345678";
        
        // 'user-level' database containing domain entities and effective permission cache
        public const string TEST_CONNECTION =
            "Host=localhost;Port=5432;Database=obac_hello_ef;Username=postgres;Password=12345678";

        // 'user-level' database containing domain entities and effective permission cache and identity tables
        public const string TEST_CONNECTION_IDENTITY =
            "Host=localhost;Port=5432;Database=obac_hello_identity_ef;Username=postgres;Password=12345678";

        public static async Task Main()
        {
            // EXAMPLE #1 - OBAC tables in separate schema + obac_EP (cache) in app's schema. App's schema has no identity.

            // initialize local DB
            var ctx = new HelloDbContext(); // the context must inherit ObacEpContextBase to be able to receive EP messages.
            await ctx.Database.EnsureCreatedAsync();

            // obac in own schema while cache in app's schema
            await MainInternal(ctx, OBAC_CONNECTION);



            // EXAMPLE #2 - OBAC tables and obac_ep (cache) in app's schema. App's schema has identity.

            // initialize local DB
            var identityCtx = new HelloIdentityDbContext(); // the context must inherit ObacEpIdentityContextBase to be able to receive EP messages and have identity tables
            await identityCtx.Database.EnsureCreatedAsync();

            // obac and cache in app's schema
            await MainInternal(identityCtx, TEST_CONNECTION_IDENTITY);
        }

        private static async Task MainInternal(IHelloDbContext ctx, string obacConnectionString)
        {
            // configure OBAC
            var pgStorage = new PgSqlObacStorageProvider(obacConnectionString);
            await pgStorage.EnsureDatabaseExists();
            
            // NOTE the context instance passed to the receiver could not be used across other program when in production code
            var epHouseReceiver = new EffectivePermissionsEfReceiver(() => ctx); 
            
            // initialize OBAC with out effective permission's receiver
            var obacConfiguration = ObacManager.CreateConfiguration(pgStorage, epHouseReceiver);

            var obacManager = obacConfiguration.GetObjectManager();
            
            // initialize object types and permissions
            docTypeId = new Guid("5B068FE3-A694-4489-B396-37CE1AE13A23");
            await obacManager.EnsureTree(docTypeId, "Documents");

            var readPermission = new Guid("1B2214E4-ADB1-40F0-AFDE-ECCC787B2F78");
            var writePermission = new Guid("C3A14624-02DA-43B9-AC32-D3FC5707FA3A");
            await obacManager.EnsurePermission(readPermission, "read");
            await obacManager.EnsurePermission(writePermission, "write");
            
            // create example users
            await obacManager.EnsureUser(1, "user 1");
            await obacManager.EnsureUser(2, "user 2");

            // now we create the documents and sets the permissions
            // doc 1 available to read for user 1 
            // doc 2 available to read and write for user 1
            // doc 3 available to read for user 2
            // in this example, we don't use threes and complex data structures, just flat list of
            // documents reduced with user's effective permissions
            await EnsureDocument(ctx, obacManager, 1, "doc1");
            await obacManager.SetTreeNodeAcl(docTypeId, 1, new AclInfo
            {
                AclItems = new[]
                {
                    new AclItemInfo {UserId = 1, PermissionId = readPermission}
                }
            });
            
            
            await EnsureDocument(ctx, obacManager, 2, "doc2");
            await obacManager.SetTreeNodeAcl(docTypeId, 2, new AclInfo
            {
                AclItems = new[]
                {
                    new AclItemInfo { UserId = 1, PermissionId = readPermission },
                    new AclItemInfo { UserId = 1, PermissionId = writePermission },
                }
            });
            
            await EnsureDocument(ctx, obacManager, 3, "doc3");
            await obacManager.SetTreeNodeAcl(docTypeId, 3, new AclInfo
            {
                InheritParentPermissions = false,
                AclItems = new[]
                {
                    new AclItemInfo { UserId = 2, PermissionId = readPermission, Kind = PermissionKindEnum.Allow},
                }
            });
            
            // get the docs available for read for user 1
            await DumpDocs(ctx, 1, readPermission, "read");

            // get the docs available for write for user 1
            await DumpDocs(ctx, 1, writePermission, "write");
            
            // get the docs available for read for user 2
            await DumpDocs(ctx,2, readPermission, "read");

            // alternate way - don't use local DB
            var user1checker = obacConfiguration.GetPermissionChecker(1);
            var permissionsToDoc1 = await user1checker.GetObjectPermissions(docTypeId, 1);
            Console.WriteLine("Alternative approach: user 1 has the following permissions to Doc 1:");
            foreach (var p in permissionsToDoc1)
            {
                Console.WriteLine($"- {p}");
            }
            Console.WriteLine("User 1 has write permission to doc1: " + (await user1checker.CheckObjectPermissions(docTypeId, 1, writePermission)));
        }

        private static async Task DumpDocs(IHelloDbContext ctx, int userId, Guid permissionId, string permissionName)
        {
            var docs = from d in ctx.Documents
                join p in ctx.EffectivePermissions // join document's table with effective permissions cache
                    on d.Id equals p.ObjectId
                where // specifying..
                    p.ObjectTypeId == docTypeId // object's type id 
                    && p.UserId == userId  // current user's id
                    && p.PermissionId == permissionId // permission to check
                select d;

            Console.WriteLine($"Documents available for {permissionName} to user #{userId}:");
            
            foreach (var d in await docs.ToListAsync())
            {
                Console.WriteLine($" - {d.Id}, {d.Name}");
            }

        }

        private static async Task EnsureDocument(IHelloDbContext crx, IObacObjectManager obacManager, int id, string name)
        {
            // check if object already exists
            if (await crx.Documents.AnyAsync(a => a.Id == id)) return;
            
            // create object in main database
            await crx.Documents.AddAsync(new DocumentEntity {Id = id, Name = name});
            // register object's replica in OBAC
            await obacManager.EnsureTreeNode(docTypeId, id, null, 1);
        }
    }
}
