using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Redb.OBAC.ApiHost;
using Redb.OBAC.BL;
using Redb.OBAC.Tests.ObacClientTests;

namespace Redb.OBAC.Tests.Utils
{
    [TestFixtureSource(nameof(GetNameDbProviders))]
    public abstract class TestBase//V2
    {
        private static Dictionary<string, HouseTestDbContext> TestDbContexts = new();
        private static Dictionary<string, IObacConfiguration> _configurations = new();
        private static readonly Dictionary<string, ObjectStorage> _storageProviders = new();
        private static readonly Dictionary<string, ApiHostImpl> _apiHosts = new();
        private readonly DbAggregator dbAggregator;

        public TestBase(string dbName)
        {
            switch (dbName)
            {
                case PgDbAggregator.NAME:
                    dbAggregator = PgDbAggregator.GetInstance();
                    break;
                case MySqlDbAggregator.NAME:
                    dbAggregator = MySqlDbAggregator.GetInstance();
                    break;

                default:
                    break;
            }
        }

        public static IEnumerable<string> GetNameDbProviders()
        {
            yield return PgDbAggregator.NAME;
            yield return MySqlDbAggregator.NAME;
        }

        protected IObacConfiguration GetConfiguration() => dbAggregator.Configuration;

        protected ApiHostImpl GetApiHost() => dbAggregator.ApiHost;

        protected ObjectStorage GetObjectStorage() => dbAggregator.DbStorage;

        protected HouseTestDbContext TestDbContext => dbAggregator.HouseDbContext;

        //protected ApiServerImpl ApiServer() => Container.Resolve<ApiServerImpl>();
        [OneTimeSetUp]
        public async Task RunBeforeAnyTests()
        {
            try
            {
                await dbAggregator.InitTestFramework();
            }
            catch (Exception ex)
            {
                // todo logging
                throw;
            }
        }

    }
}
