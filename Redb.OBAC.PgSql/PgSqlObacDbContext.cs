using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Redb.OBAC.Core.Models;
using Redb.OBAC.EF.DB;

namespace Redb.OBAC.PgSql
{
    public class PgSqlObacDbContext : ObacDbContext
    {
        public PgSqlObacDbContext(string connectionString) : base(connectionString)
        {
        }

        public PgSqlObacDbContext(DbContextOptions<ObacDbContext> options) : base(options)
        {
            
        }

        // uncomment base(...) when doing migrations  
        public PgSqlObacDbContext() //: base("Host=localhost;Port=5432;Database=obac_test;Username=postgres;Password=12345678")
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = ConnectionString;
                if (connectionString == null)
                {
                    var host = Environment.GetEnvironmentVariable("OBAC_HOST") ?? "localhost";
                    var port = Environment.GetEnvironmentVariable("OBAC_PORT") ?? "5432";
                    var db = Environment.GetEnvironmentVariable("OBAC_DB") ?? "obac";
                    var user = Environment.GetEnvironmentVariable("OBAC_USER") ?? "postgres";
                    var password = Environment.GetEnvironmentVariable("OBAC_PASSWORD") ?? "12345678";
                    connectionString = $"Host={host};Port={port};Database={db};Username={user};Password={password}";
                }
                optionsBuilder.UseNpgsql(connectionString);
            }
            
            if (false)
            {
                optionsBuilder.EnableDetailedErrors();
                optionsBuilder.EnableSensitiveDataLogging(); // show parameter values in sql log
                var lp = LoggerFactory.Create(builder =>
                    {
                        builder.SetMinimumLevel(LogLevel.Debug)
                            //.AddFilter("Microsoft", LogLevel.Debug)
                            // .AddFilter("System", LogLevel.Warning)
                            // .AddFilter("SampleApp.Program", LogLevel.Debug)
                            .AddConsole();
                    }
                );
                
                optionsBuilder.UseLoggerFactory(lp);
            }
        }

        public override string GetTreeSubnodesDeepQueryRoot()
        {
            return @"with recursive nodes(id, parent_id, inherit_parent_perms, owner_user_id, acl) as (
            select id, parent_id, inherit_parent_perms, owner_user_id, acl
            from obac_tree_nodes
            where parent_id = {0} and tree_id={1}
            union all
            select o.id, o.parent_id, o.inherit_parent_perms, o.owner_user_id, o.acl
                from obac_tree_nodes o
            join nodes n on n.id = o.parent_id and o.tree_id={1}
                )
            select *, {1} as tree_id
                from nodes
                order by id desc";
        }

        public override string GetTreeSubnodesDeepQueryGivenNode()
        {
            return @"with recursive nodes(id, parent_id, inherit_parent_perms, owner_user_id, acl) as (
            select id, parent_id, inherit_parent_perms, owner_user_id, acl
            from obac_tree_nodes
            where parent_id is null and tree_id={0}
            union all
            select o.id, o.parent_id, o.inherit_parent_perms, o.owner_user_id, o.acl
                from obac_tree_nodes o
            join nodes n on n.id = o.parent_id and o.tree_id={0}
                )
            select *, {0} as tree_id
                from nodes
                order by id desc";
        }
    }
}
