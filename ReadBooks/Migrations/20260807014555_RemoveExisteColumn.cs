using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReadBooks.Migrations
{
    /// <inheritdoc />
    public partial class RemoveExisteColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Existe",
                table: "Libros");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Existe",
                table: "Libros",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
