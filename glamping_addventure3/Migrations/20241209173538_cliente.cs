using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace glamping_addventure3.Migrations
{
    /// <inheritdoc />
    public partial class cliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Pais",
                table: "Clientes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoDocumento",
                table: "Clientes",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Pais",
                table: "Clientes");

            migrationBuilder.DropColumn(
                name: "TipoDocumento",
                table: "Clientes");
        }
    }
}
