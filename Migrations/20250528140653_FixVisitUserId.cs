using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PunktWeterynaryjny.Migrations
{
    /// <inheritdoc />
    public partial class FixVisitUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Skuteczny lek na b�l", "Lek przeciwb�lowy" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Ochrona przed pch�ami", "Preparat na pch�y" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Skuteczny lek na ból", "Lek przeciwbólowy" });

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Description", "Name" },
                values: new object[] { "Ochrona przed pchłami", "Preparat na pchły" });
        }
    }
}
