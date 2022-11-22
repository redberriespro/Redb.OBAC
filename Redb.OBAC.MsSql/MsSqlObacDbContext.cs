using System;
using Microsoft.EntityFrameworkCore;
using Redb.OBAC.EF.DB;

namespace Redb.OBAC.MsSql
{
    public class MsSqlObacDbContext : ObacDbContext
    {
        public MsSqlObacDbContext():base()
        {
        }

        public MsSqlObacDbContext(string connectionString) : base(connectionString)
        {
        }

        public MsSqlObacDbContext(DbContextOptions<ObacDbContext> options) : base(options)
        {
        }

        public override string GetTreeSubnodesDeepQueryRoot()
        {
            return @"with nodes(id, parent_id, external_id_int, external_id_str, inherit_parent_perms, owner_user_id) as (
            select id, parent_id, external_id_int, external_id_str, inherit_parent_perms, owner_user_id
            from obac_tree_nodes
            where parent_id = {0} and tree_id={1}
            union all
            select o.id, o.parent_id, o.external_id_int, o.external_id_str, o.inherit_parent_perms, o.owner_user_id
                from obac_tree_nodes o
            join nodes n on n.id = o.parent_id and o.tree_id={1}
                )
            select *, {1} as tree_id
                from nodes
                order by id desc";
        }

        public override string GetTreeSubnodesDeepQueryGivenNode()
        {
            return @"with nodes(id, parent_id, external_id_int, external_id_str, inherit_parent_perms, owner_user_id) as (
            select id, parent_id, external_id_int, external_id_str, inherit_parent_perms, owner_user_id
            from obac_tree_nodes
            where parent_id is null and tree_id={0}
            union all
            select o.id, o.parent_id, o.external_id_int, o.external_id_str, o.inherit_parent_perms, o.owner_user_id
                from obac_tree_nodes o
            join nodes n on n.id = o.parent_id and o.tree_id={0}
                )
            select *, {0} as tree_id
                from nodes
                order by id desc";
            
        }        

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = string.IsNullOrEmpty(ConnectionString) ?
                    $"Server={Environment.GetEnvironmentVariable("OBAC_HOST") ?? "localhost"},{Environment.GetEnvironmentVariable("OBAC_PORT") ?? "1433"};" +
                    $"Database={Environment.GetEnvironmentVariable("OBAC_DB") ?? "obac"};" +
                    $"User Id={Environment.GetEnvironmentVariable("OBAC_USER") ?? "sa"};" +
                    $"Password={Environment.GetEnvironmentVariable("OBAC_PASSWORD") ?? "12345678"};" :
                    ConnectionString;
                optionsBuilder.UseSqlServer(connectionString);
            }

        }
    }
}
