using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NUnit.Framework;
using Redb.OBAC.ApiHost;
using Redb.OBAC.BL;
using Redb.OBAC.Client.EffectivePermissionsReceiver;
using Redb.OBAC.DB;
using Redb.OBAC.PgSql;
using Redb.OBAC.Tests.ObacClientTests;

namespace Redb.OBAC.Tests.Utils
{
    public abstract class TestBase
    {
        protected const string CONFIG_POSTGRES = "postgres";
        
        private static Dictionary<string, IObacConfiguration> _configurations =
            new Dictionary<string, IObacConfiguration>();

        protected static HouseTestPgDbContext TestPgContext;
        
        protected IObacConfiguration GetConfiguration(string type) => _configurations[type];
        
        private static Dictionary<string, ApiHostImpl> _apiHosts =
            new Dictionary<string, ApiHostImpl>(); 
        
        protected ApiHostImpl GetApiHost(string type) => _apiHosts[type];
        
        private static Dictionary<string, ObjectStorage> _storageProviders  =
            new Dictionary<string, ObjectStorage>();

        protected ObjectStorage GetObjectStorage(string type) => _storageProviders[type];

        
        
        //protected ApiServerImpl ApiServer() => Container.Resolve<ApiServerImpl>();
        [OneTimeSetUp]
        public async Task RunBeforeAnyTests()
        {
            try
            {
                await InitTestFramework();
            }
            catch (Exception ex)
            {
                // todo logging
                throw;
            }
        }


        private async Task<TestAppSettings> ReadSettings()
        {
            var appBinPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var configFile = Path.Combine(appBinPath, "appsettings.json");
            var configText = await File.ReadAllTextAsync(configFile);
            return JsonConvert.DeserializeObject<TestAppSettings>(configText);
        }
        private async Task InitTestFramework()
        {
            if (_storageProviders.Any()) return;
            
            var settings = await ReadSettings();
            
            // create config for PG
            var pgStorage = new PgSqlObacStorageProvider(settings.Postgres);
            _storageProviders[CONFIG_POSTGRES] = new ObjectStorage(pgStorage);

                // drop DBs before running any tests
            var pgDbCleaner = new PgSqlDbCleaner(settings.Postgres.Connection);
            pgDbCleaner.CleanDb();
            await pgStorage.EnsureDatabaseExists();

            pgDbCleaner = new PgSqlDbCleaner(settings.ConnectionTest);
            pgDbCleaner.CleanDb();
            TestPgContext = new HouseTestPgDbContext(settings.ConnectionTest);
            await TestPgContext.Database.EnsureCreatedAsync();
            
            var epHouseReceiver = new EffectivePermissionsEfReceiver(new HouseTestPgDbContext(settings.ConnectionTest));
            
            // register OBAC configuration
            var pgConfig = ObacManager.CreateConfiguration(pgStorage, epHouseReceiver);
            _configurations[CONFIG_POSTGRES] = pgConfig;
            
            // initialize in-process obac api based on Postgres config
            var pgApiHost = new ApiHostImpl(pgConfig);
            _apiHosts[CONFIG_POSTGRES] = pgApiHost;
        }
        
        
    }
}