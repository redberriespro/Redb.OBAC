using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Redb.OBAC.PgSql.Migrations
{
    public partial class InitialMigrations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "obac_objecttypes",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false),
                    description = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_obac_objecttypes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "obac_permissions",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false),
                    description = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_obac_permissions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "obac_roles",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false),
                    description = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_obac_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "obac_userpermissions",
                columns: table => new
                {
                    id = table.Column<Guid>(nullable: false),
                    userid = table.Column<int>(nullable: false),
                    permid = table.Column<Guid>(nullable: false),
                    objtypeid = table.Column<Guid>(nullable: false),
                    objid = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_obac_userpermissions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "obac_users",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    external_id_int = table.Column<int>(nullable: true),
                    external_id_str = table.Column<string>(nullable: true),
                    description = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_obac_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "obac_permissions_in_roles",
                columns: table => new
                {
                    perm_id = table.Column<Guid>(nullable: false),
                    role_id = table.Column<Guid>(nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_obac_permissions_in_roles_role_id",
                table: "obac_permissions_in_roles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_obac_userpermissions_userid_objtypeid_objid",
                table: "obac_userpermissions",
                columns: new[] { "userid", "objtypeid", "objid" });

            migrationBuilder.CreateIndex(
                name: "IX_obac_userpermissions_userid_permid_objtypeid_objid",
                table: "obac_userpermissions",
                columns: new[] { "userid", "permid", "objtypeid", "objid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_obac_users_external_id_int",
                table: "obac_users",
                column: "external_id_int");

            migrationBuilder.CreateIndex(
                name: "IX_obac_users_external_id_str",
                table: "obac_users",
                column: "external_id_str");
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
                name: "obac_userpermissions");

            migrationBuilder.DropTable(
                name: "obac_users");

            migrationBuilder.DropTable(
                name: "obac_roles");
        }
    }
}
