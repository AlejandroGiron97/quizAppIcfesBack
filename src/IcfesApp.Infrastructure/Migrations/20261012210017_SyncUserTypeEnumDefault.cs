using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IcfesApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncUserTypeEnumDefault : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "Schools",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Domain",
                table: "Schools",
                type: "TEXT",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Schools",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "Domain",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Schools");
        }
    }
}
