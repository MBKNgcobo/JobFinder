using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobFinder.Migrations
{
    /// <inheritdoc />
    public partial class AddJobMatchUniqueConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JobMatches_UserId",
                table: "JobMatches");

            migrationBuilder.CreateIndex(
                name: "IX_JobMatches_UserId_JobId",
                table: "JobMatches",
                columns: new[] { "UserId", "JobId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JobMatches_UserId_JobId",
                table: "JobMatches");

            migrationBuilder.CreateIndex(
                name: "IX_JobMatches_UserId",
                table: "JobMatches",
                column: "UserId");
        }
    }
}
