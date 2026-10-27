using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IcfesApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddImagesToOptionsAndQuestions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Questions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JustificationImageUrl",
                table: "Questions",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "QuestionOptions",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "JustificationImageUrl",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "QuestionOptions");
        }
    }
}
