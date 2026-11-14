using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Vitalita.API.Shared.Infrastructure.Persistence.EFC.Migrations
{
    /// <inheritdoc />
    public partial class AddProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "caregiver_profiles",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    first_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    phone_number = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    professional_title = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    bio = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    photo_url = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    certifications = table.Column<string>(type: "json", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_caregiver_profiles", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "family_member_profiles",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    first_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    phone_number = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_family_member_profiles", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "older_adults",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    caregiver_id = table.Column<int>(type: "int", nullable: false),
                    first_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    birth_date = table.Column<DateOnly>(type: "date", nullable: false),
                    document_number = table.Column<string>(type: "varchar(8)", maxLength: 8, nullable: false),
                    blood_type = table.Column<string>(type: "varchar(3)", maxLength: 3, nullable: false),
                    allergies = table.Column<string>(type: "json", nullable: false),
                    chronic_conditions = table.Column<string>(type: "json", nullable: false),
                    observations = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false),
                    status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    emergency_contact_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    emergency_contact_phone_number = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    emergency_contact_relationship = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_older_adults", x => x.id);
                    table.ForeignKey(
                        name: "fk_older_adults_caregiver_profiles_caregiver_id",
                        column: x => x.caregiver_id,
                        principalTable: "caregiver_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "family_accesses",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    older_adult_id = table.Column<int>(type: "int", nullable: false),
                    family_member_id = table.Column<int>(type: "int", nullable: true),
                    invited_email = table.Column<string>(type: "varchar(254)", maxLength: 254, nullable: false),
                    invitation_code = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    relationship = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    invited_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    accepted_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    expires_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_family_accesses", x => x.id);
                    table.ForeignKey(
                        name: "fk_family_accesses_family_member_profile_family_member_id",
                        column: x => x.family_member_id,
                        principalTable: "family_member_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_family_accesses_older_adult_older_adult_id",
                        column: x => x.older_adult_id,
                        principalTable: "older_adults",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "ix_caregiver_profiles_user_id",
                table: "caregiver_profiles",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_family_accesses_family_member_id",
                table: "family_accesses",
                column: "family_member_id");

            migrationBuilder.CreateIndex(
                name: "ix_family_accesses_invitation_code",
                table: "family_accesses",
                column: "invitation_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_family_accesses_invited_email",
                table: "family_accesses",
                column: "invited_email");

            migrationBuilder.CreateIndex(
                name: "ix_family_accesses_older_adult_id",
                table: "family_accesses",
                column: "older_adult_id");

            migrationBuilder.CreateIndex(
                name: "ix_family_member_profiles_user_id",
                table: "family_member_profiles",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_older_adults_caregiver_id",
                table: "older_adults",
                column: "caregiver_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "family_accesses");

            migrationBuilder.DropTable(
                name: "family_member_profiles");

            migrationBuilder.DropTable(
                name: "older_adults");

            migrationBuilder.DropTable(
                name: "caregiver_profiles");
        }
    }
}
