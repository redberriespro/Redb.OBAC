using Microsoft.EntityFrameworkCore.Migrations;

namespace Redb.OBAC.PgSql.Migrations
{
    public partial class TreeNullParentId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_obac_tree_nodes_obac_tree_nodes_tree_id_parent_id",
                table: "obac_tree_nodes");

            migrationBuilder.AlterColumn<int>(
                name: "parent_id",
                table: "obac_tree_nodes",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_obac_tree_nodes_obac_tree_nodes_tree_id_parent_id",
                table: "obac_tree_nodes",
                columns: new[] { "tree_id", "parent_id" },
                principalTable: "obac_tree_nodes",
                principalColumns: new[] { "tree_id", "id" },
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_obac_tree_nodes_obac_tree_nodes_tree_id_parent_id",
                table: "obac_tree_nodes");

            migrationBuilder.AlterColumn<int>(
                name: "parent_id",
                table: "obac_tree_nodes",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_obac_tree_nodes_obac_tree_nodes_tree_id_parent_id",
                table: "obac_tree_nodes",
                columns: new[] { "tree_id", "parent_id" },
                principalTable: "obac_tree_nodes",
                principalColumns: new[] { "tree_id", "id" },
                onDelete: ReferentialAction.Cascade);
        }
    }
}
