using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareerPilot.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCareerProfileData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "desired_salary_amount",
                table: "user_profiles",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "desired_salary_currency",
                table: "user_profiles",
                type: "character(3)",
                fixedLength: true,
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "preferred_employment_type",
                table: "user_profiles",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "preferred_remote_type",
                table: "user_profiles",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "years_of_experience",
                table: "user_profiles",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "profile_skill",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    profile_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    years_of_experience = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_profile_skill", x => x.id);
                    table.ForeignKey(
                        name: "fk_profile_skill_user_profiles_profile_id",
                        column: x => x.profile_id,
                        principalTable: "user_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_profile_skill_profile_name",
                table: "profile_skill",
                columns: new[] { "profile_id", "name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "profile_skill");

            migrationBuilder.DropColumn(
                name: "desired_salary_amount",
                table: "user_profiles");

            migrationBuilder.DropColumn(
                name: "desired_salary_currency",
                table: "user_profiles");

            migrationBuilder.DropColumn(
                name: "preferred_employment_type",
                table: "user_profiles");

            migrationBuilder.DropColumn(
                name: "preferred_remote_type",
                table: "user_profiles");

            migrationBuilder.DropColumn(
                name: "years_of_experience",
                table: "user_profiles");
        }
    }
}
