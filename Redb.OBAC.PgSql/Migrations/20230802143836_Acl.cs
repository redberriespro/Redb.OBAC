using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Redb.OBAC.PgSql.Migrations
{
    public partial class Acl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "obac_users",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "obac_user_groups",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "acl",
                table: "obac_tree_nodes",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_obac_userpermissions_objtypeid_objid",
                table: "obac_userpermissions",
                columns: new[] { "objtypeid", "objid" });

            migrationBuilder.CreateIndex(
                name: "IX_obac_tree_node_permissions_tree_id_tree_node_id_perm_id",
                table: "obac_tree_node_permissions",
                columns: new[] { "tree_id", "tree_node_id", "perm_id" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_obac_userpermissions_objtypeid_objid",
                table: "obac_userpermissions");

            migrationBuilder.DropIndex(
                name: "IX_obac_tree_node_permissions_tree_id_tree_node_id_perm_id",
                table: "obac_tree_node_permissions");

            migrationBuilder.DropColumn(
                name: "acl",
                table: "obac_tree_nodes");

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "obac_users",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "id",
                table: "obac_user_groups",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        }
    }
}
