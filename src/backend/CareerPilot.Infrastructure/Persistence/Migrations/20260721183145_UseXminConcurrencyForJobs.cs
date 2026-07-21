using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareerPilot.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UseXminConcurrencyForJobs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "row_version",
                table: "jobs");

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "jobs",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "xmin",
                table: "jobs");

            migrationBuilder.AddColumn<byte[]>(
                name: "row_version",
                table: "jobs",
                type: "bytea",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);
        }
    }
}
