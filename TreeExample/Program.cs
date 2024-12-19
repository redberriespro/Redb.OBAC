using Microsoft.EntityFrameworkCore;
using Redb.OBAC.Client.EffectivePermissionsReceiver;
using Redb.OBAC.Core;
using Redb.OBAC.Core.Models;
using Redb.OBAC.EF;
using Redb.OBAC.PgSql;

namespace TreeExample
{
    public class Program
    {
        // 'user-level' database containing domain entities and effective permission cache
        public const string TEST_CONNECTION =
            "Host=localhost;Port=5432;Database=obac_tree_ef;Username=postgres;Password=12345678";

        public static async Task Main(string[] args)
        {
            // initialize local DB
            var ctx = new TreeDbContext(); // the context must inherit ObacEpContextBase to be able to receive EP messages.
            await ctx.Database.EnsureCreatedAsync();

            // configure OBAC
            var pgStorage = new PgSqlObacStorageProvider(TEST_CONNECTION);
            await pgStorage.EnsureDatabaseExists();

            // NOTE the context instance passed to the receiver could not be used across other program when in production code
            var epHouseReceiver = new EffectivePermissionsEfReceiver(() => ctx);

            // initialize OBAC with out effective permission's receiver
            var obacConfiguration = ObacManager.CreateConfiguration(pgStorage, epHouseReceiver);

            var obacManager = obacConfiguration.GetObjectManager();


            // initialize object types and permissions
            var treeId = new Guid("5B068FE3-A694-4489-B396-37CE1AE13A23");
            await obacManager.EnsureTree(treeId, "Catalogs");

            var accessPermission = new Guid("2B2214E4-ADB1-40F0-AFDE-ECCC787B2F78");
            await obacManager.EnsurePermission(accessPermission, "access");

            // create example users
            await obacManager.EnsureUser(1, "user 1");
            await obacManager.EnsureUser(2, "user 2");


            // catalog 1 with file 1 and file2
            // catalog 2 with file 3
            // file4 with no catalog

            var catalog1 = await EnsureCatalog(ctx, obacManager, treeId);
            var file1 = await EnsureFile(ctx, obacManager, treeId, catalog1);
            var file2 = await EnsureFile(ctx, obacManager, treeId, catalog1);

            var catalog2 = await EnsureCatalog(ctx, obacManager, treeId);
            var file3 = await EnsureFile(ctx, obacManager, treeId, catalog2);

            var file4 = await EnsureFile(ctx, obacManager, treeId);


            // user 1 have perm to catalog1
            await obacManager.SetTreeNodeAcl(treeId, catalog1.Guid, new AclInfo
            {
                AclItems = new[]
                {
                    new AclItemInfo {UserId = 1, PermissionId = accessPermission}
                }
            });

            // get the files available for read for user 1
            await DumpDocs(ctx, 1, accessPermission, "access", treeId);

            var permChecker = obacConfiguration.GetPermissionChecker(1);
            var file1Perm  = await permChecker.GetObjectPermissions(treeId, file1.Guid).ConfigureAwait(false);
            var file2Perm = await permChecker.GetObjectPermissions(treeId, file2.Guid).ConfigureAwait(false);
        }

        private static async Task<CatalogEntity> EnsureCatalog(TreeDbContext ctx, IObacObjectManager obacManager, Guid treeId)
        {
            // create object in main database
            var result = await ctx.Catalogs.AddAsync(new CatalogEntity { Guid = Guid.NewGuid() });

            // register object's replica in OBAC
            await obacManager.EnsureTreeNode(treeId, result.Entity.Guid, null, 1);

            return result.Entity;
        }

        private static async Task<FileEntity> EnsureFile(TreeDbContext ctx, IObacObjectManager obacManager, Guid treeId, CatalogEntity? catalog = null)
        {
            // create object in main database
            var result = await ctx.Files.AddAsync(new FileEntity() { Version = 1 });

            // register object's replica in OBAC
            await obacManager.EnsureTreeNode(treeId, result.Entity.Guid, catalog?.Guid, 1);

            return result.Entity;
        }

        private static async Task DumpDocs(TreeDbContext ctx, int userId, Guid permissionId, string permissionName, Guid treeId)
        {
            var docs = from d in ctx.Files
                join p in ctx.EffectivePermissions // join document's table with effective permissions cache
                    on d.Guid equals p.ObjectId
                where // specifying..
                    p.ObjectTypeId == treeId // object's type id 
                    && p.UserId == userId  // current user's id
                    && p.PermissionId == permissionId // permission to check
                select d;

            Console.WriteLine($"Documents available for {permissionName} to user #{userId}:");

            foreach (var d in await docs.ToListAsync())
            {
                Console.WriteLine($" - {d.Id}, {d.Guid}");
            }

        }
    }
}
