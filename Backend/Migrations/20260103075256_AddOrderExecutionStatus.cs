using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace petpal.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderExecutionStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ExecutionStatus",
                table: "MutualOrders",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExecutionStatus",
                table: "MutualOrders");
        }
    }
}
