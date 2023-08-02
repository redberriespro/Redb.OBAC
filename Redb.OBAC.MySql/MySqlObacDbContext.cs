using Microsoft.EntityFrameworkCore;
using Redb.OBAC.Core.Models;
using Redb.OBAC.EF.DB;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Redb.OBAC.MySql
{
    public class MySqlObacDbContext : ObacDbContext
    {
        // uncomment base(...) when doing migrations  
        public MySqlObacDbContext()//: base("Host=192.168.2.12;Port=3306;Database=obac_test_user;Username=root;Password=12345678")
        { }

        public MySqlObacDbContext(string connectionString) : base(connectionString) { }


        public override string GetTreeSubnodesDeepQueryRoot()
        {
            return @"with recursive nodes(id, parent_id, external_id_int, external_id_str, inherit_parent_perms, owner_user_id, acl) as (
            select id, parent_id, external_id_int, external_id_str, inherit_parent_perms, owner_user_id, acl
            from obac_tree_nodes
            where parent_id = {0} and tree_id={1}
            union all
            select o.id, o.parent_id, o.external_id_int, o.external_id_str, o.inherit_parent_perms, o.owner_user_id, o.acl
                from obac_tree_nodes o
            join nodes n on n.id = o.parent_id and o.tree_id={1}
                )
            select *, {1} as tree_id
                from nodes
                order by id desc";
        }

        public override string GetTreeSubnodesDeepQueryGivenNode()
        {
            return @"with recursive nodes(id, parent_id, external_id_int, external_id_str, inherit_parent_perms, owner_user_id, acl) as (
            select id, parent_id, external_id_int, external_id_str, inherit_parent_perms, owner_user_id, acl
            from obac_tree_nodes
            where parent_id is null and tree_id={0}
            union all
            select o.id, o.parent_id, o.external_id_int, o.external_id_str, o.inherit_parent_perms, o.owner_user_id, o.acl
                from obac_tree_nodes o
            join nodes n on n.id = o.parent_id and o.tree_id={0}
                )
            select *, {0} as tree_id
                from nodes
                order by id desc";
        }



        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            //! uuid var(16)
            //optionsBuilder.UseMySQL(
            //    string.IsNullOrEmpty(ConnectionString)
            //        ? GetDefaultConnectionString()
            //        : ConnectionString

            var connectionString = string.IsNullOrEmpty(ConnectionString)
                    ? GetDefaultConnectionString()
                    : ConnectionString;
            optionsBuilder.UseMySql(
                connectionString
                , ServerVersion.AutoDetect(connectionString)
                );
        }

        private string GetDefaultConnectionString()
        {
            return string.Format(
                "Host={0};Port={1};database={2};user={3};password={4}",
                Environment.GetEnvironmentVariable("OBAC_HOST") ?? "localhost",
                Environment.GetEnvironmentVariable("OBAC_PORT") ?? "3306",
                Environment.GetEnvironmentVariable("OBAC_DB") ?? "obac",
                Environment.GetEnvironmentVariable("OBAC_USER") ?? "myname",
                Environment.GetEnvironmentVariable("OBAC_PASSWORD") ?? "12345678"
                );
        }
    }
}
