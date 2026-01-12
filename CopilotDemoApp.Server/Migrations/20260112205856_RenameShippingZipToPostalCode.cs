using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CopilotDemoApp.Server.Migrations
{
    /// <inheritdoc />
    public partial class RenameShippingZipToPostalCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ShippingZip",
                table: "Orders",
                newName: "ShippingPostalCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ShippingPostalCode",
                table: "Orders",
                newName: "ShippingZip");
        }
    }
}
