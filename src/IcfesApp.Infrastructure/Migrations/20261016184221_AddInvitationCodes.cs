using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IcfesApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInvitationCodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InvitationCodes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    InstitutionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MaxUses = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: -1),
                    Uses = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    ExpiresAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    CreatedByUserId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvitationCodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvitationCodes_Schools_InstitutionId",
                        column: x => x.InstitutionId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InvitationCodes_Code",
                table: "InvitationCodes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvitationCodes_InstitutionId",
                table: "InvitationCodes",
                column: "InstitutionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvitationCodes");
        }
    }
}
