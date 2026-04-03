using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace movieRecom.Migrations
{
    /// <inheritdoc />
    public partial class af : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsMadeUp",
                table: "StudentLessons",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsMadeUp",
                table: "StudentLessons");
        }
    }
}
