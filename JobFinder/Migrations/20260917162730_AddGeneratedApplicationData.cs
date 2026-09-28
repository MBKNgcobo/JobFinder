using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobFinder.Migrations
{
    /// <inheritdoc />
    public partial class AddGeneratedApplicationData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "GeneratedAt",
                table: "Applications",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GeneratedCoverLetter",
                table: "Applications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GeneratedCvChanges",
                table: "Applications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GeneratedMatchedSkills",
                table: "Applications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GeneratedProjects",
                table: "Applications",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GeneratedAt",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "GeneratedCoverLetter",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "GeneratedCvChanges",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "GeneratedMatchedSkills",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "GeneratedProjects",
                table: "Applications");
        }
    }
}
