using Microsoft.EntityFrameworkCore.Migrations;

namespace Redb.OBAC.MsSql.Migrations
{
    public partial class Acl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "acl",
                table: "obac_tree_nodes",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "acl",
                table: "obac_tree_nodes");
        }
    }
}
