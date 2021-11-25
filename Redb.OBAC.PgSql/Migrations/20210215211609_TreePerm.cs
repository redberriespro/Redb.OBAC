using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Redb.OBAC.PgSql.Migrations
{
    public partial class TreePerm : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "inherit_parent_perms",
                table: "obac_tree_nodes",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "owner_user_id",
                table: "obac_tree_nodes",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "obac_tree_node_permissions",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false),
                    user_id = table.Column<int>(nullable: true),
                    user_group_id = table.Column<int>(nullable: true),
                    tree_id = table.Column<Guid>(nullable: false),
                    tree_node_id = table.Column<int>(nullable: false),
                    perm_id = table.Column<Guid>(nullable: false),
                    is_deny = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_obac_tree_node_permissions", x => x.id);
                    table.ForeignKey(
                        name: "FK_obac_tree_node_permissions_obac_user_groups_user_group_id",
                        column: x => x.user_group_id,
                        principalTable: "obac_user_groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_obac_tree_node_permissions_obac_users_user_id",
                        column: x => x.user_id,
                        principalTable: "obac_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_obac_tree_node_permissions_obac_tree_nodes_tree_id_tree_nod~",
                        columns: x => new { x.tree_id, x.tree_node_id },
                        principalTable: "obac_tree_nodes",
                        principalColumns: new[] { "tree_id", "id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_obac_tree_nodes_owner_user_id",
                table: "obac_tree_nodes",
                column: "owner_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_obac_tree_node_permissions_user_group_id",
                table: "obac_tree_node_permissions",
                column: "user_group_id");

            migrationBuilder.CreateIndex(
                name: "IX_obac_tree_node_permissions_tree_id_tree_node_id",
                table: "obac_tree_node_permissions",
                columns: new[] { "tree_id", "tree_node_id" });

            migrationBuilder.CreateIndex(
                name: "IX_obac_tree_node_permissions_user_id_user_group_id_tree_id_tr~",
                table: "obac_tree_node_permissions",
                columns: new[] { "user_id", "user_group_id", "tree_id", "tree_node_id", "perm_id" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_obac_tree_nodes_obac_users_owner_user_id",
                table: "obac_tree_nodes",
                column: "owner_user_id",
                principalTable: "obac_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_obac_tree_nodes_obac_users_owner_user_id",
                table: "obac_tree_nodes");

            migrationBuilder.DropTable(
                name: "obac_tree_node_permissions");

            migrationBuilder.DropIndex(
                name: "IX_obac_tree_nodes_owner_user_id",
                table: "obac_tree_nodes");

            migrationBuilder.DropColumn(
                name: "inherit_parent_perms",
                table: "obac_tree_nodes");

            migrationBuilder.DropColumn(
                name: "owner_user_id",
                table: "obac_tree_nodes");
        }
    }
}
