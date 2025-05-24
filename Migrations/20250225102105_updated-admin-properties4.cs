using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EMS.Migrations
{
    /// <inheritdoc />
    public partial class updatedadminproperties4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ismobileVerified",
                table: "Admins",
                newName: "isMobileVerified");

            migrationBuilder.AddColumn<string>(
                name: "otp",
                table: "Admins",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "otp",
                table: "Admins");

            migrationBuilder.RenameColumn(
                name: "isMobileVerified",
                table: "Admins",
                newName: "ismobileVerified");
        }
    }
}
