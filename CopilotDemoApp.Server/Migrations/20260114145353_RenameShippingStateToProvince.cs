using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CopilotDemoApp.Server.Migrations
{
    /// <inheritdoc />
    public partial class RenameShippingStateToProvince : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ShippingState",
                table: "Orders",
                newName: "ShippingProvince");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ShippingProvince",
                table: "Orders",
                newName: "ShippingState");
        }
    }
}
