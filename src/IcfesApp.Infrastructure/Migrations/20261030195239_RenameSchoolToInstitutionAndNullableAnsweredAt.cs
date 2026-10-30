using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IcfesApp.Infrastructure.Migrations
{
    /// <summary>
    /// Refactor phase 6:
    ///   1. "Schools" table → "Institutions", and AspNetUsers.SchoolId → AspNetUsers.InstitutionId.
    ///   2. StudentAnswers.AnsweredAtUtc becomes nullable (null = not answered yet). The existing fake dates
    ///      are cleared by the next migration, ClearAnsweredAtOfUnansweredQuestions.
    ///
    /// EDITED BY HAND. EF Core cannot detect renames: the scaffolded version dropped the "Schools" table
    /// and created an empty "Institutions" one, which would have deleted every institution (and, by cascade,
    /// their invitation codes and subscriptions). This version renames the table and the column instead,
    /// so all existing data is kept.
    /// </summary>
    public partial class RenameSchoolToInstitutionAndNullableAnsweredAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Schools_SchoolId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_InvitationCodes_Schools_InstitutionId",
                table: "InvitationCodes");

            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_Schools_InstitutionId",
                table: "Subscriptions");

            migrationBuilder.RenameTable(
                name: "Schools",
                newName: "Institutions");

            migrationBuilder.RenameColumn(
                name: "SchoolId",
                table: "AspNetUsers",
                newName: "InstitutionId");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUsers_SchoolId",
                table: "AspNetUsers",
                newName: "IX_AspNetUsers_InstitutionId");

            migrationBuilder.AlterColumn<DateTime>(
                name: "AnsweredAtUtc",
                table: "StudentAnswers",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "TEXT");

            // The fake dates of unanswered questions are cleared in the NEXT migration
            // (ClearAnsweredAtOfUnansweredQuestions): on SQLite, EF rebuilds the table at the end of this
            // migration, so an UPDATE here would still run against the old NOT NULL column.

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Institutions_InstitutionId",
                table: "AspNetUsers",
                column: "InstitutionId",
                principalTable: "Institutions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_InvitationCodes_Institutions_InstitutionId",
                table: "InvitationCodes",
                column: "InstitutionId",
                principalTable: "Institutions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_Institutions_InstitutionId",
                table: "Subscriptions",
                column: "InstitutionId",
                principalTable: "Institutions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Institutions_InstitutionId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_InvitationCodes_Institutions_InstitutionId",
                table: "InvitationCodes");

            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_Institutions_InstitutionId",
                table: "Subscriptions");

            // Null dates were already filled back by the Down of ClearAnsweredAtOfUnansweredQuestions.
            migrationBuilder.AlterColumn<DateTime>(
                name: "AnsweredAtUtc",
                table: "StudentAnswers",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "TEXT",
                oldNullable: true);

            // Same order as Up (column first, then its index): on SQLite the AspNetUsers rebuild is pending
            // here, and renaming the index before the column makes the rebuild look for "SchoolId" too early.
            migrationBuilder.RenameColumn(
                name: "InstitutionId",
                table: "AspNetUsers",
                newName: "SchoolId");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUsers_InstitutionId",
                table: "AspNetUsers",
                newName: "IX_AspNetUsers_SchoolId");

            migrationBuilder.RenameTable(
                name: "Institutions",
                newName: "Schools");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Schools_SchoolId",
                table: "AspNetUsers",
                column: "SchoolId",
                principalTable: "Schools",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_InvitationCodes_Schools_InstitutionId",
                table: "InvitationCodes",
                column: "InstitutionId",
                principalTable: "Schools",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_Schools_InstitutionId",
                table: "Subscriptions",
                column: "InstitutionId",
                principalTable: "Schools",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
