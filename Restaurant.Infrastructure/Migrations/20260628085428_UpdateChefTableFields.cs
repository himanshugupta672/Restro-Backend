using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Restaurant.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateChefTableFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Chefs",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "Chefs",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Chefs",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Chefs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Chefs",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedDate",
                table: "Chefs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chefs_Email",
                table: "Chefs",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chefs_PhoneNumber",
                table: "Chefs",
                column: "PhoneNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chefs_Email",
                table: "Chefs");

            migrationBuilder.DropIndex(
                name: "IX_Chefs_PhoneNumber",
                table: "Chefs");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "Chefs");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Chefs");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Chefs");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Chefs");

            migrationBuilder.DropColumn(
                name: "UpdatedDate",
                table: "Chefs");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Chefs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);
        }
    }
}
