// using System;
// using System.Collections.Generic;
// using System.ComponentModel;
// using System.IO;
// using System.Linq;
// using System.Reflection;
// using System.Threading.Tasks;
// using Microsoft.EntityFrameworkCore;
// using Newtonsoft.Json;
// using NUnit.Framework;
// using Redb.OBAC.ApiHost;
// using Redb.OBAC.BL;
// using Redb.OBAC.Client.EffectivePermissionsReceiver;
// using Redb.OBAC.DB;
// using Redb.OBAC.MySql;
// using Redb.OBAC.PgSql;
// using Redb.OBAC.Tests.ObacClientTests;
//
// namespace Redb.OBAC.Tests.Utils
// {
//     [TestFixtureSource(nameof(GetNameDbProviders))]
//     public abstract class TestBaseV0
//     {
//         protected const string CONFIG_POSTGRES = "postgres";
//         protected const string CONFIG_MYSQL = "mysql";
//
//         private static Dictionary<string, HouseTestDbContext> TestDbContexts = new();
//         private static Dictionary<string, IObacConfiguration> _configurations =new();
//         private static readonly Dictionary<string, ObjectStorage> _storageProviders = new();
//         private static readonly Dictionary<string, ApiHostImpl> _apiHosts = new();
//         private string _dbName;
//
//         public TestBaseV0(string dbName)
//         {
//             _dbName = dbName;
//         }
//
//         public static IEnumerable<string> GetNameDbProviders()
//         {
//             yield return CONFIG_POSTGRES;
//             yield return CONFIG_MYSQL;
//         }
//
//         protected IObacConfiguration GetConfiguration() => _configurations[_dbName];
//         
//         protected ApiHostImpl GetApiHost() => _apiHosts[_dbName];
//
//         protected ObjectStorage GetObjectStorage() => _storageProviders[_dbName];
//
//         protected HouseTestDbContext TestDbContext { get => TestDbContexts[_dbName]; }
//
//         //protected ApiServerImpl ApiServer() => Container.Resolve<ApiServerImpl>();
//         [OneTimeSetUp]
//         public async Task RunBeforeAnyTests()
//         {
//             try
//             {
//                 switch (_dbName)
//                 {
//                     case CONFIG_POSTGRES:
//                         await InitTestPostgreFramework();
//                         break;
//                     case CONFIG_MYSQL:
//                         await InitTestMysqlFramework();
//                         break;
//
//                     default:
//                         break;
//                 }
//             }
//             catch (Exception ex)
//             {
//                 // todo logging
//                 throw;
//             }
//         }
//
//         private async Task<TestAppSettings> ReadSettings()
//         {
//             var appBinPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
//             var configFile = Path.Combine(appBinPath, "appsettings.json");
//             var configText = await File.ReadAllTextAsync(configFile);
//             return JsonConvert.DeserializeObject<TestAppSettings>(configText);
//         }
//
//         private async Task InitTestMysqlFramework()
//         {
//             if (_storageProviders.ContainsKey(CONFIG_MYSQL)) return;
//
//             var settings = await ReadSettings();
//
//             // create config for db
//             var pgStorage = new MySqlObacStorageProvider(settings.Mysql.Config.Connection);
//             _storageProviders[CONFIG_MYSQL] = new ObjectStorage(pgStorage);
//
//             // drop DBs before running any tests
//             var pgDbCleaner = new MySqlDbCleaner(settings.Mysql.Config.Connection);
//             pgDbCleaner.CleanDb();
//             await pgStorage.EnsureDatabaseExists();
//
//             pgDbCleaner = new MySqlDbCleaner(settings.Mysql.ConnectionTest);
//             pgDbCleaner.CleanDb();
//             TestDbContexts.Add(CONFIG_MYSQL, new HouseTestMySqlDbContext(settings.Mysql.ConnectionTest));
//             await Task.Run(() => TestDbContexts[CONFIG_MYSQL].Database.EnsureCreated());
//
//             var epHouseReceiver = new EffectivePermissionsEfReceiver(new HouseTestMySqlDbContext(settings.Mysql.ConnectionTest));
//
//             // register OBAC configuration
//             var pgConfig = ObacManager.CreateConfiguration(pgStorage, epHouseReceiver);
//             _configurations[CONFIG_MYSQL] = pgConfig;
//
//             // initialize in-process obac api based on db config
//             var pgApiHost = new ApiHostImpl(pgConfig);
//             _apiHosts[CONFIG_MYSQL] = pgApiHost;
//         }
//
//         private async Task InitTestPostgreFramework()
//         {
//             if (_storageProviders.ContainsKey(CONFIG_POSTGRES)) return;
//             
//             var settings = await ReadSettings();
//             
//             // create config for PG
//             var pgStorage = new PgSqlObacStorageProvider(settings.Postgres.Config.Connection);
//             _storageProviders[CONFIG_POSTGRES] = new ObjectStorage(pgStorage);
//
//                 // drop DBs before running any tests
//             var pgDbCleaner = new PgSqlDbCleaner(settings.Postgres.Config.Connection);
//             pgDbCleaner.CleanDb();
//             await pgStorage.EnsureDatabaseExists();
//
//             pgDbCleaner = new PgSqlDbCleaner(settings.Postgres.ConnectionTest);
//             pgDbCleaner.CleanDb();
//             TestDbContexts.Add(CONFIG_POSTGRES, new HouseTestPgDbContext(settings.Postgres.ConnectionTest));
//             await TestDbContexts[CONFIG_POSTGRES].Database.EnsureCreatedAsync();
//             
//             var epHouseReceiver = new EffectivePermissionsEfReceiver(new HouseTestPgDbContext(settings.Postgres.ConnectionTest));
//             
//             // register OBAC configuration
//             var pgConfig = ObacManager.CreateConfiguration(pgStorage, epHouseReceiver);
//             _configurations[CONFIG_POSTGRES] = pgConfig;
//             
//             // initialize in-process obac api based on Postgres config
//             var pgApiHost = new ApiHostImpl(pgConfig);
//             _apiHosts[CONFIG_POSTGRES] = pgApiHost;
//         }
//         
//         
//     }
// }