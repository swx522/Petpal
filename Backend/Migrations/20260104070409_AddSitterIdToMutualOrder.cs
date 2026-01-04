using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace petpal.Migrations
{
    /// <inheritdoc />
    public partial class AddSitterIdToMutualOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MutualOrders_Users_UserId",
                table: "MutualOrders");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "MutualOrders",
                newName: "SitterId");

            migrationBuilder.RenameIndex(
                name: "IX_MutualOrders_UserId",
                table: "MutualOrders",
                newName: "IX_MutualOrders_SitterId");

            migrationBuilder.AddForeignKey(
                name: "FK_MutualOrders_Users_SitterId",
                table: "MutualOrders",
                column: "SitterId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MutualOrders_Users_SitterId",
                table: "MutualOrders");

            migrationBuilder.RenameColumn(
                name: "SitterId",
                table: "MutualOrders",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_MutualOrders_SitterId",
                table: "MutualOrders",
                newName: "IX_MutualOrders_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_MutualOrders_Users_UserId",
                table: "MutualOrders",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
