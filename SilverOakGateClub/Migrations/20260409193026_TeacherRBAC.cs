using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SilverOakGateClub.Migrations
{
    /// <inheritdoc />
    public partial class TeacherRBAC : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "MockTests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedByUserId",
                table: "Lectures",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TeacherDepartments",
                columns: table => new
                {
                    TeacherId = table.Column<int>(type: "int", nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherDepartments", x => new { x.TeacherId, x.DepartmentId });
                    table.ForeignKey(
                        name: "FK_TeacherDepartments_Branches_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeacherDepartments_Users_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MockTests_CreatedByUserId",
                table: "MockTests",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Lectures_CreatedByUserId",
                table: "Lectures",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherDepartments_DepartmentId",
                table: "TeacherDepartments",
                column: "DepartmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Lectures_Users_CreatedByUserId",
                table: "Lectures",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_MockTests_Users_CreatedByUserId",
                table: "MockTests",
                column: "CreatedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lectures_Users_CreatedByUserId",
                table: "Lectures");

            migrationBuilder.DropForeignKey(
                name: "FK_MockTests_Users_CreatedByUserId",
                table: "MockTests");

            migrationBuilder.DropTable(
                name: "TeacherDepartments");

            migrationBuilder.DropIndex(
                name: "IX_MockTests_CreatedByUserId",
                table: "MockTests");

            migrationBuilder.DropIndex(
                name: "IX_Lectures_CreatedByUserId",
                table: "Lectures");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "MockTests");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Lectures");
        }
    }
}
