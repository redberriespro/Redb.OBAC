using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Redb.OBAC.MsSql.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "obac_objecttypes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_obac_objecttypes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "obac_permissions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_obac_permissions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "obac_roles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_obac_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "obac_trees",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    external_id_int = table.Column<int>(type: "int", nullable: true),
                    external_id_str = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_obac_trees", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "obac_user_groups",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false),
                    external_id_int = table.Column<int>(type: "int", nullable: true),
                    external_id_str = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_obac_user_groups", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "obac_userpermissions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    userid = table.Column<int>(type: "int", nullable: false),
                    permid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    objtypeid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    objid = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_obac_userpermissions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "obac_users",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false),
                    external_id_int = table.Column<int>(type: "int", nullable: true),
                    external_id_str = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_obac_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "obac_permissions_in_roles",
                columns: table => new
                {
                    perm_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    role_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_obac_permissions_in_roles", x => new { x.perm_id, x.role_id });
                    table.ForeignKey(
                        name: "FK_obac_permissions_in_roles_obac_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "obac_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "obac_tree_nodes",
                columns: table => new
                {
                    tree_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    id = table.Column<int>(type: "int", nullable: false),
                    parent_id = table.Column<int>(type: "int", nullable: true),
                    owner_user_id = table.Column<int>(type: "int", nullable: false),
                    inherit_parent_perms = table.Column<bool>(type: "bit", nullable: false),
                    external_id_int = table.Column<int>(type: "int", nullable: true),
                    external_id_str = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_obac_tree_nodes", x => new { x.tree_id, x.id });
                    table.ForeignKey(
                        name: "FK_obac_tree_nodes_obac_tree_nodes_tree_id_parent_id",
                        columns: x => new { x.tree_id, x.parent_id },
                        principalTable: "obac_tree_nodes",
                        principalColumns: new[] { "tree_id", "id" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_obac_tree_nodes_obac_trees_tree_id",
                        column: x => x.tree_id,
                        principalTable: "obac_trees",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_obac_tree_nodes_obac_users_owner_user_id",
                        column: x => x.owner_user_id,
                        principalTable: "obac_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "obac_users_in_groups",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "int", nullable: false),
                    group_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_obac_users_in_groups", x => new { x.user_id, x.group_id });
                    table.ForeignKey(
                        name: "FK_obac_users_in_groups_obac_user_groups_group_id",
                        column: x => x.group_id,
                        principalTable: "obac_user_groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_obac_users_in_groups_obac_users_user_id",
                        column: x => x.user_id,
                        principalTable: "obac_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "obac_tree_node_permissions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    user_id = table.Column<int>(type: "int", nullable: true),
                    user_group_id = table.Column<int>(type: "int", nullable: true),
                    tree_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    tree_node_id = table.Column<int>(type: "int", nullable: false),
                    perm_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    is_deny = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_obac_tree_node_permissions", x => x.id);
                    //table.ForeignKey(
                    //    name: "FK_obac_tree_node_permissions_obac_tree_nodes_tree_id_tree_node_id",
                    //    columns: x => new { x.tree_id, x.tree_node_id },
                    //    principalTable: "obac_tree_nodes",
                    //    principalColumns: new[] { "tree_id", "id" },
                    //    onDelete: ReferentialAction.Cascade);
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
                });

            migrationBuilder.CreateIndex(
                name: "IX_obac_permissions_in_roles_role_id",
                table: "obac_permissions_in_roles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_obac_tree_node_permissions_tree_id_tree_node_id",
                table: "obac_tree_node_permissions",
                columns: new[] { "tree_id", "tree_node_id" });

            migrationBuilder.CreateIndex(
                name: "IX_obac_tree_node_permissions_tree_id_tree_node_id_perm_id",
                table: "obac_tree_node_permissions",
                columns: new[] { "tree_id", "tree_node_id", "perm_id" });

            migrationBuilder.CreateIndex(
                name: "IX_obac_tree_node_permissions_user_group_id",
                table: "obac_tree_node_permissions",
                column: "user_group_id");

            migrationBuilder.CreateIndex(
                name: "IX_obac_tree_node_permissions_user_id_user_group_id_tree_id_tree_node_id_perm_id",
                table: "obac_tree_node_permissions",
                columns: new[] { "user_id", "user_group_id", "tree_id", "tree_node_id", "perm_id" },
                unique: true,
                filter: "[user_id] IS NOT NULL AND [user_group_id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_obac_tree_nodes_owner_user_id",
                table: "obac_tree_nodes",
                column: "owner_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_obac_tree_nodes_tree_id",
                table: "obac_tree_nodes",
                column: "tree_id");

            migrationBuilder.CreateIndex(
                name: "IX_obac_tree_nodes_tree_id_id_parent_id",
                table: "obac_tree_nodes",
                columns: new[] { "tree_id", "id", "parent_id" });

            migrationBuilder.CreateIndex(
                name: "IX_obac_tree_nodes_tree_id_parent_id",
                table: "obac_tree_nodes",
                columns: new[] { "tree_id", "parent_id" });

            migrationBuilder.CreateIndex(
                name: "IX_obac_userpermissions_objtypeid_objid",
                table: "obac_userpermissions",
                columns: new[] { "objtypeid", "objid" });

            migrationBuilder.CreateIndex(
                name: "IX_obac_userpermissions_userid_objtypeid_objid",
                table: "obac_userpermissions",
                columns: new[] { "userid", "objtypeid", "objid" });

            migrationBuilder.CreateIndex(
                name: "IX_obac_userpermissions_userid_permid_objtypeid_objid",
                table: "obac_userpermissions",
                columns: new[] { "userid", "permid", "objtypeid", "objid" },
                unique: true,
                filter: "[objid] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_obac_users_external_id_int",
                table: "obac_users",
                column: "external_id_int");

            migrationBuilder.CreateIndex(
                name: "IX_obac_users_external_id_str",
                table: "obac_users",
                column: "external_id_str");

            migrationBuilder.CreateIndex(
                name: "IX_obac_users_in_groups_group_id",
                table: "obac_users_in_groups",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "IX_obac_users_in_groups_user_id",
                table: "obac_users_in_groups",
                column: "user_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "obac_objecttypes");

            migrationBuilder.DropTable(
                name: "obac_permissions");

            migrationBuilder.DropTable(
                name: "obac_permissions_in_roles");

            migrationBuilder.DropTable(
                name: "obac_tree_node_permissions");

            migrationBuilder.DropTable(
                name: "obac_userpermissions");

            migrationBuilder.DropTable(
                name: "obac_users_in_groups");

            migrationBuilder.DropTable(
                name: "obac_roles");

            migrationBuilder.DropTable(
                name: "obac_tree_nodes");

            migrationBuilder.DropTable(
                name: "obac_user_groups");

            migrationBuilder.DropTable(
                name: "obac_trees");

            migrationBuilder.DropTable(
                name: "obac_users");
        }
    }
}
