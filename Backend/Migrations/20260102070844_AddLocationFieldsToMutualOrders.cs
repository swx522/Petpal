using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace petpal.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationFieldsToMutualOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Latitude",
                table: "MutualOrders",
                type: "decimal(65,30)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Longitude",
                table: "MutualOrders",
                type: "decimal(65,30)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "MutualOrders");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "MutualOrders");
        }
    }
}
