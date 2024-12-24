using Microsoft.EntityFrameworkCore.Migrations;

namespace Redb.OBAC.PgSql.Migrations
{
    public partial class AddExternalStrId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "external_string_id",
                table: "obac_userpermissions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_string_id",
                table: "obac_tree_node_permissions",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "external_string_id",
                table: "obac_userpermissions");

            migrationBuilder.DropColumn(
                name: "external_string_id",
                table: "obac_tree_node_permissions");
        }
    }
}
