using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DEKODE.OptiFlow.DAL.Repository.Migrations
{
    /// <inheritdoc />
    public partial class Add_Roles_And_Users_Tables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "planograms",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "allocations",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConcurrencyToken = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NormalizedUsername = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    NormalizedEmail = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    PasswordSalt = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    FailedLoginAttempts = table.Column<int>(type: "integer", nullable: false),
                    LockoutEndUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastLoginAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConcurrencyToken = table.Column<byte[]>(type: "bytea", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_users_roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "Id", "Code", "ConcurrencyToken", "CreatedAtUtc", "Description", "IsActive", "Name", "UpdatedAtUtc" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "Admin", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Full administrative access", true, "Administrator", null },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "Planner", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Can run allocations and review store gap reports", true, "Merchandise Planner", null },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "StoreManager", null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Store dispatch views", true, "Store Manager", null }
                });

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "Id", "ConcurrencyToken", "CreatedAtUtc", "DisplayName", "Email", "FailedLoginAttempts", "IsActive", "LastLoginAtUtc", "LockoutEndUtc", "NormalizedEmail", "NormalizedUsername", "PasswordHash", "PasswordSalt", "RoleId", "UpdatedAtUtc", "Username" },
                values: new object[] { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "OptiFlow Administrator", "admin@dekode.io", 0, true, null, null, "ADMIN@DEKODE.IO", "ADMIN", "J0YZgWm8TIvP6h9+atZ6M07yoblgeRLhhH5Egfutbr8=", "V2Zsb3dTYWx0MjAyNg==", new Guid("11111111-1111-1111-1111-111111111111"), null, "admin" });

            migrationBuilder.CreateIndex(
                name: "IX_planograms_CreatedByUserId",
                table: "planograms",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_allocations_UserId",
                table: "allocations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_roles_Code",
                table: "roles",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_NormalizedEmail",
                table: "users",
                column: "NormalizedEmail",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_NormalizedUsername",
                table: "users",
                column: "NormalizedUsername",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_users_RoleId",
                table: "users",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_allocations_users_UserId",
                table: "allocations",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_planograms_users_CreatedByUserId",
                table: "planograms",
                column: "CreatedByUserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_allocations_users_UserId",
                table: "allocations");

            migrationBuilder.DropForeignKey(
                name: "FK_planograms_users_CreatedByUserId",
                table: "planograms");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropIndex(
                name: "IX_planograms_CreatedByUserId",
                table: "planograms");

            migrationBuilder.DropIndex(
                name: "IX_allocations_UserId",
                table: "allocations");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "planograms");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "allocations");
        }
    }
}
