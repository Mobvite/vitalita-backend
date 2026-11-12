using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Vitalita.API.Shared.Infrastructure.Persistence.EFC.Migrations
{
    /// <inheritdoc />
    public partial class AddMonitoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "daily_reports",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    older_adult_id = table.Column<int>(type: "int", nullable: false),
                    recorded_by_user_id = table.Column<int>(type: "int", nullable: false),
                    report_date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    mood = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    general_condition = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    observations = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_daily_reports", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "medications",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    older_adult_id = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    dosage = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    frequency = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    schedule_times = table.Column<string>(type: "json", nullable: false),
                    instructions = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_medications", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "vital_signs",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    older_adult_id = table.Column<int>(type: "int", nullable: false),
                    type = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    value = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: false),
                    secondary_value = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    unit = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false),
                    measured_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vital_signs", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "ix_daily_reports_older_adult_id",
                table: "daily_reports",
                column: "older_adult_id");

            migrationBuilder.CreateIndex(
                name: "ix_medications_older_adult_id",
                table: "medications",
                column: "older_adult_id");

            migrationBuilder.CreateIndex(
                name: "ix_vital_signs_older_adult_id",
                table: "vital_signs",
                column: "older_adult_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "daily_reports");

            migrationBuilder.DropTable(
                name: "medications");

            migrationBuilder.DropTable(
                name: "vital_signs");
        }
    }
}
