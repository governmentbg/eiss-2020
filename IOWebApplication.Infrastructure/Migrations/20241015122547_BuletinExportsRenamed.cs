using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IOWebApplication.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class BuletinExportsRenamed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_person_sentence_bulletin_export_case_person_sentence_b~",
                table: "case_person_sentence_bulletin_export");

            migrationBuilder.DropForeignKey(
                name: "FK_case_person_sentence_bulletin_export_identity_users_reg_use~",
                table: "case_person_sentence_bulletin_export");

            migrationBuilder.DropForeignKey(
                name: "FK_case_person_sentence_bulletin_export_identity_users_user_id",
                table: "case_person_sentence_bulletin_export");

            migrationBuilder.DropPrimaryKey(
                name: "PK_case_person_sentence_bulletin_export",
                table: "case_person_sentence_bulletin_export");

            migrationBuilder.RenameTable(
                name: "case_person_sentence_bulletin_export",
                newName: "case_person_sentence_bulletin_file");

            migrationBuilder.RenameIndex(
                name: "IX_case_person_sentence_bulletin_export_user_id",
                table: "case_person_sentence_bulletin_file",
                newName: "IX_case_person_sentence_bulletin_file_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_case_person_sentence_bulletin_export_reg_user_id",
                table: "case_person_sentence_bulletin_file",
                newName: "IX_case_person_sentence_bulletin_file_reg_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_case_person_sentence_bulletin_export_case_person_sentence_b~",
                table: "case_person_sentence_bulletin_file",
                newName: "IX_case_person_sentence_bulletin_file_case_person_sentence_bul~");

            migrationBuilder.AddPrimaryKey(
                name: "PK_case_person_sentence_bulletin_file",
                table: "case_person_sentence_bulletin_file",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_person_sentence_bulletin_file_case_person_sentence_bul~",
                table: "case_person_sentence_bulletin_file",
                column: "case_person_sentence_bulletin__id",
                principalTable: "case_person_sentence_bulletin",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_case_person_sentence_bulletin_file_identity_users_reg_user_~",
                table: "case_person_sentence_bulletin_file",
                column: "reg_user_id",
                principalTable: "identity_users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_person_sentence_bulletin_file_identity_users_user_id",
                table: "case_person_sentence_bulletin_file",
                column: "user_id",
                principalTable: "identity_users",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_case_person_sentence_bulletin_file_case_person_sentence_bul~",
                table: "case_person_sentence_bulletin_file");

            migrationBuilder.DropForeignKey(
                name: "FK_case_person_sentence_bulletin_file_identity_users_reg_user_~",
                table: "case_person_sentence_bulletin_file");

            migrationBuilder.DropForeignKey(
                name: "FK_case_person_sentence_bulletin_file_identity_users_user_id",
                table: "case_person_sentence_bulletin_file");

            migrationBuilder.DropPrimaryKey(
                name: "PK_case_person_sentence_bulletin_file",
                table: "case_person_sentence_bulletin_file");

            migrationBuilder.RenameTable(
                name: "case_person_sentence_bulletin_file",
                newName: "case_person_sentence_bulletin_export");

            migrationBuilder.RenameIndex(
                name: "IX_case_person_sentence_bulletin_file_user_id",
                table: "case_person_sentence_bulletin_export",
                newName: "IX_case_person_sentence_bulletin_export_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_case_person_sentence_bulletin_file_reg_user_id",
                table: "case_person_sentence_bulletin_export",
                newName: "IX_case_person_sentence_bulletin_export_reg_user_id");

            migrationBuilder.RenameIndex(
                name: "IX_case_person_sentence_bulletin_file_case_person_sentence_bul~",
                table: "case_person_sentence_bulletin_export",
                newName: "IX_case_person_sentence_bulletin_export_case_person_sentence_b~");

            migrationBuilder.AddPrimaryKey(
                name: "PK_case_person_sentence_bulletin_export",
                table: "case_person_sentence_bulletin_export",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_person_sentence_bulletin_export_case_person_sentence_b~",
                table: "case_person_sentence_bulletin_export",
                column: "case_person_sentence_bulletin__id",
                principalTable: "case_person_sentence_bulletin",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_case_person_sentence_bulletin_export_identity_users_reg_use~",
                table: "case_person_sentence_bulletin_export",
                column: "reg_user_id",
                principalTable: "identity_users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_case_person_sentence_bulletin_export_identity_users_user_id",
                table: "case_person_sentence_bulletin_export",
                column: "user_id",
                principalTable: "identity_users",
                principalColumn: "id");
        }
    }
}
