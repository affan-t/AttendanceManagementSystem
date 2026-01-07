using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AttendanceManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseAllocationsFixed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseAllocation_Batches_BatchId",
                table: "CourseAllocation");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseAllocation_Courses_CourseId",
                table: "CourseAllocation");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseAllocation_Sections_SectionId",
                table: "CourseAllocation");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseAllocation_Semesters_SemesterId",
                table: "CourseAllocation");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseAllocation_Teachers_TeacherId",
                table: "CourseAllocation");

            migrationBuilder.DropForeignKey(
                name: "FK_Enrollments_CourseAllocation_CourseAllocationId",
                table: "Enrollments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CourseAllocation",
                table: "CourseAllocation");

            migrationBuilder.RenameTable(
                name: "CourseAllocation",
                newName: "CourseAllocations");

            migrationBuilder.RenameIndex(
                name: "IX_CourseAllocation_TeacherId",
                table: "CourseAllocations",
                newName: "IX_CourseAllocations_TeacherId");

            migrationBuilder.RenameIndex(
                name: "IX_CourseAllocation_SemesterId",
                table: "CourseAllocations",
                newName: "IX_CourseAllocations_SemesterId");

            migrationBuilder.RenameIndex(
                name: "IX_CourseAllocation_SectionId",
                table: "CourseAllocations",
                newName: "IX_CourseAllocations_SectionId");

            migrationBuilder.RenameIndex(
                name: "IX_CourseAllocation_CourseId",
                table: "CourseAllocations",
                newName: "IX_CourseAllocations_CourseId");

            migrationBuilder.RenameIndex(
                name: "IX_CourseAllocation_BatchId",
                table: "CourseAllocations",
                newName: "IX_CourseAllocations_BatchId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CourseAllocations",
                table: "CourseAllocations",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseAllocations_Batches_BatchId",
                table: "CourseAllocations",
                column: "BatchId",
                principalTable: "Batches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseAllocations_Courses_CourseId",
                table: "CourseAllocations",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseAllocations_Sections_SectionId",
                table: "CourseAllocations",
                column: "SectionId",
                principalTable: "Sections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseAllocations_Semesters_SemesterId",
                table: "CourseAllocations",
                column: "SemesterId",
                principalTable: "Semesters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseAllocations_Teachers_TeacherId",
                table: "CourseAllocations",
                column: "TeacherId",
                principalTable: "Teachers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollments_CourseAllocations_CourseAllocationId",
                table: "Enrollments",
                column: "CourseAllocationId",
                principalTable: "CourseAllocations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseAllocations_Batches_BatchId",
                table: "CourseAllocations");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseAllocations_Courses_CourseId",
                table: "CourseAllocations");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseAllocations_Sections_SectionId",
                table: "CourseAllocations");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseAllocations_Semesters_SemesterId",
                table: "CourseAllocations");

            migrationBuilder.DropForeignKey(
                name: "FK_CourseAllocations_Teachers_TeacherId",
                table: "CourseAllocations");

            migrationBuilder.DropForeignKey(
                name: "FK_Enrollments_CourseAllocations_CourseAllocationId",
                table: "Enrollments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CourseAllocations",
                table: "CourseAllocations");

            migrationBuilder.RenameTable(
                name: "CourseAllocations",
                newName: "CourseAllocation");

            migrationBuilder.RenameIndex(
                name: "IX_CourseAllocations_TeacherId",
                table: "CourseAllocation",
                newName: "IX_CourseAllocation_TeacherId");

            migrationBuilder.RenameIndex(
                name: "IX_CourseAllocations_SemesterId",
                table: "CourseAllocation",
                newName: "IX_CourseAllocation_SemesterId");

            migrationBuilder.RenameIndex(
                name: "IX_CourseAllocations_SectionId",
                table: "CourseAllocation",
                newName: "IX_CourseAllocation_SectionId");

            migrationBuilder.RenameIndex(
                name: "IX_CourseAllocations_CourseId",
                table: "CourseAllocation",
                newName: "IX_CourseAllocation_CourseId");

            migrationBuilder.RenameIndex(
                name: "IX_CourseAllocations_BatchId",
                table: "CourseAllocation",
                newName: "IX_CourseAllocation_BatchId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CourseAllocation",
                table: "CourseAllocation",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseAllocation_Batches_BatchId",
                table: "CourseAllocation",
                column: "BatchId",
                principalTable: "Batches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseAllocation_Courses_CourseId",
                table: "CourseAllocation",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseAllocation_Sections_SectionId",
                table: "CourseAllocation",
                column: "SectionId",
                principalTable: "Sections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseAllocation_Semesters_SemesterId",
                table: "CourseAllocation",
                column: "SemesterId",
                principalTable: "Semesters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseAllocation_Teachers_TeacherId",
                table: "CourseAllocation",
                column: "TeacherId",
                principalTable: "Teachers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollments_CourseAllocation_CourseAllocationId",
                table: "Enrollments",
                column: "CourseAllocationId",
                principalTable: "CourseAllocation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
