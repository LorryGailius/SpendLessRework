using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpendLess.Server.Migrations
{
    /// <inheritdoc />
    public partial class addedSupportId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SupportId",
                table: "Tickets",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SupportId",
                table: "Tickets");
        }
    }
}
