using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EMS.Migrations
{
    /// <inheritdoc />
    public partial class Changedmodelnames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PhotoFile",
                table: "Employees",
                newName: "photoFile");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Employees",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "JoinedDate",
                table: "Employees",
                newName: "joinedDate");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Employees",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "Department",
                table: "Employees",
                newName: "department");

            migrationBuilder.RenameColumn(
                name: "Age",
                table: "Employees",
                newName: "age");

            migrationBuilder.RenameColumn(
                name: "Address",
                table: "Employees",
                newName: "address");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Employees",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Departments",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Departments",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "Password",
                table: "Admins",
                newName: "password");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Admins",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Admins",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Admins",
                newName: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "photoFile",
                table: "Employees",
                newName: "PhotoFile");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Employees",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "joinedDate",
                table: "Employees",
                newName: "JoinedDate");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "Employees",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "department",
                table: "Employees",
                newName: "Department");

            migrationBuilder.RenameColumn(
                name: "age",
                table: "Employees",
                newName: "Age");

            migrationBuilder.RenameColumn(
                name: "address",
                table: "Employees",
                newName: "Address");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Employees",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Departments",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Departments",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "password",
                table: "Admins",
                newName: "Password");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Admins",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "email",
                table: "Admins",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Admins",
                newName: "Id");
        }
    }
}
