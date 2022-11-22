using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Redb.OBAC;
using Redb.OBAC.Client.EffectivePermissionsReceiver;
using Redb.OBAC.Core;
using Redb.OBAC.Core.Models;
using Redb.OBAC.EF;
using Redb.OBAC.PgSql;

namespace HelloObac
{
    public class Program
    {
        private static IObacObjectManager obacManager;
        private static HelloDbContext ctx;
        private static Guid docTypeId;

        
        // two postgres databases will be created for the example:
        
        // internal OBAC database
        public const string OBAC_CONNECTION =
            "Host=localhost;Port=5432;Database=obac;Username=postgres;Password=12345678";
        
        // 'user-level' database containing domain entities and effective permission cache
        public const string TEST_CONNECTION =
            "Host=localhost;Port=5432;Database=obac_hello_ef;Username=postgres;Password=12345678";

        public static async Task Main()
        {
            // initialize local DB
            ctx = new HelloDbContext(); // the context must inherit ObacEpContextBase to be able to receive EP messages.
            await ctx.Database.EnsureCreatedAsync();
            
            // configure OBAC
            var pgStorage = new PgSqlObacStorageProvider(OBAC_CONNECTION);
            await pgStorage.EnsureDatabaseExists();
            
            // NOTE the context instance passed to the receiver could not be used across other program when in production code
            var epHouseReceiver = new EffectivePermissionsEfReceiver(ctx); 
            
            // initialize OBAC with out effective permission's receiver
            var obacConfiguration = ObacManager.CreateConfiguration(pgStorage, epHouseReceiver);

            obacManager = obacConfiguration.GetObjectManager();
            
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
            await EnsureDocument( 1, "doc1");
            await obacManager.SetTreeNodeAcl(docTypeId, 1, new AclInfo
            {
                AclItems = new[]
                {
                    new AclItemInfo {UserId = 1, PermissionId = readPermission}
                }
            });
            
            
            await EnsureDocument(2, "doc2");
            await obacManager.SetTreeNodeAcl(docTypeId, 2, new AclInfo
            {
                AclItems = new[]
                {
                    new AclItemInfo { UserId = 1, PermissionId = readPermission },
                    new AclItemInfo { UserId = 1, PermissionId = writePermission },
                }
            });
            
            await EnsureDocument(3, "doc3");
            await obacManager.SetTreeNodeAcl(docTypeId, 3, new AclInfo
            {
                InheritParentPermissions = false,
                AclItems = new[]
                {
                    new AclItemInfo { UserId = 2, PermissionId = readPermission, Kind = PermissionKindEnum.Allow},
                }
            });
            
            // get the docs available for read for user 1
            await DumpDocs(1, readPermission, "read");

            // get the docs available for write for user 1
            await DumpDocs(1, writePermission, "write");
            
            // get the docs available for read for user 2
            await DumpDocs(2, readPermission, "read");

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

        private static async Task DumpDocs(int userId, Guid permissionId, string permissionName)
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

        private static async Task EnsureDocument(int id, string name)
        {
            // check if object already exists
            if (await ctx.Documents.AnyAsync(a => a.Id == id)) return;
            
            // create object in main database
            await ctx.Documents.AddAsync(new DocumentEntity {Id = id, Name = name});
            // register object's replica in OBAC
            await obacManager.EnsureTreeNode(docTypeId, id, null, 1);
        }
    }
}
