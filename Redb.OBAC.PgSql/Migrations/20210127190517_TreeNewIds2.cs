using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Redb.OBAC.PgSql.Migrations
{
    public partial class TreeNewIds2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "obac_trees",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false),
                    description = table.Column<string>(nullable: true),
                    external_id_int = table.Column<int>(nullable: true),
                    external_id_str = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_obac_trees", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "obac_tree_nodes",
                columns: table => new
                {
                    tree_id = table.Column<Guid>(nullable: false),
                    id = table.Column<int>(nullable: false),
                    parent_id = table.Column<int>(nullable: false),
                    external_id_int = table.Column<int>(nullable: true),
                    external_id_str = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_obac_tree_nodes", x => new { x.tree_id, x.id });
                    table.ForeignKey(
                        name: "FK_obac_tree_nodes_obac_trees_tree_id",
                        column: x => x.tree_id,
                        principalTable: "obac_trees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_obac_tree_nodes_obac_tree_nodes_tree_id_parent_id",
                        columns: x => new { x.tree_id, x.parent_id },
                        principalTable: "obac_tree_nodes",
                        principalColumns: new[] { "tree_id", "id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_obac_tree_nodes_tree_id",
                table: "obac_tree_nodes",
                column: "tree_id");

            migrationBuilder.CreateIndex(
                name: "IX_obac_tree_nodes_tree_id_parent_id",
                table: "obac_tree_nodes",
                columns: new[] { "tree_id", "parent_id" });

            migrationBuilder.CreateIndex(
                name: "IX_obac_tree_nodes_tree_id_id_parent_id",
                table: "obac_tree_nodes",
                columns: new[] { "tree_id", "id", "parent_id" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "obac_tree_nodes");

            migrationBuilder.DropTable(
                name: "obac_trees");
        }
    }
}
