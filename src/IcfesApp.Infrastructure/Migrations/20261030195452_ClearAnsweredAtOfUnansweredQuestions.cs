using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IcfesApp.Infrastructure.Migrations
{
    /// <summary>
    /// Data-only migration (refactor phase 6). Unanswered questions used to get the session start date as
    /// "answered at" because the column was NOT NULL. Now that it is nullable, those fake dates are cleared:
    /// no selected option = not answered = null date.
    ///
    /// It is separate from RenameSchoolToInstitutionAndNullableAnsweredAt because, on SQLite, EF rebuilds
    /// the table at the end of that migration, so the UPDATE must run in a later one.
    /// </summary>
    public partial class ClearAnsweredAtOfUnansweredQuestions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "UPDATE \"StudentAnswers\" SET \"AnsweredAtUtc\" = NULL WHERE \"SelectedOptionId\" IS NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restores the previous behaviour (unanswered rows carry their session start date), which the
            // previous migration needs before making the column NOT NULL again.
            migrationBuilder.Sql(
                "UPDATE \"StudentAnswers\" SET \"AnsweredAtUtc\" = " +
                "(SELECT \"StartedAtUtc\" FROM \"PracticeSessions\" WHERE \"PracticeSessions\".\"Id\" = \"StudentAnswers\".\"PracticeSessionId\") " +
                "WHERE \"AnsweredAtUtc\" IS NULL;");
        }
    }
}
