using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace plt.Migrations
{
    /// <inheritdoc />
    public partial class start : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_users_roles_role_id",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "fk_clients_service_dates_last_date_id",
                table: "Clients");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "ServiceDates");

            migrationBuilder.DropTable(
                name: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Users_role_id",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "BasePrice",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "role_id",
                table: "Users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BasePrice",
                table: "Users",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "role_id",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    role_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("roles_pkey", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LastDateId = table.Column<int>(type: "integer", nullable: true),
                    ProfiId = table.Column<int>(type: "integer", nullable: false),
                    Balance = table.Column<double>(type: "double precision", nullable: false),
                    CountServicesDone = table.Column<int>(type: "integer", nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    PriceServices = table.Column<double>(type: "double precision", nullable: false),
                    SecondName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("clients_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "fk_clients_users_profi_id",
                        column: x => x.ProfiId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceDates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClientId = table.Column<int>(type: "integer", nullable: false),
                    ProfiId = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Price = table.Column<double>(type: "numeric(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("service_dates_pkey", x => x.Id);
                    table.ForeignKey(
                        name: "fk_service_dates_clients_client_id",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_service_dates_users_profi_id",
                        column: x => x.ProfiId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_role_id",
                table: "Users",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_LastDateId",
                table: "Clients",
                column: "LastDateId");

            migrationBuilder.CreateIndex(
                name: "IX_Clients_ProfiId",
                table: "Clients",
                column: "ProfiId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceDates_ClientId",
                table: "ServiceDates",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceDates_ProfiId",
                table: "ServiceDates",
                column: "ProfiId");

            migrationBuilder.AddForeignKey(
                name: "fk_users_roles_role_id",
                table: "Users",
                column: "role_id",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_clients_service_dates_last_date_id",
                table: "Clients",
                column: "LastDateId",
                principalTable: "ServiceDates",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
