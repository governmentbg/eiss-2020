using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class ElectronicDocumentMainTransaction : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "assignment_role",
                table: "epep_user_assignment",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "electronic_document_id",
                table: "document",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "epep_has_electronic_documents",
                table: "common_court",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "electronic_document",
                columns: table => new
                {
                    id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    epep_id = table.Column<Guid>(nullable: false),
                    epep_user_id = table.Column<int>(nullable: false),
                    court_id = table.Column<int>(nullable: false),
                    document_kind_id = table.Column<int>(nullable: false),
                    document_group_id = table.Column<int>(nullable: false),
                    case_id = table.Column<int>(nullable: true),
                    case_person_id = table.Column<int>(nullable: true),
                    apply_number = table.Column<string>(nullable: true),
                    apply_date = table.Column<DateTime>(nullable: false),
                    currency_code = table.Column<string>(nullable: true),
                    base_amount = table.Column<decimal>(nullable: true),
                    tax_amount = table.Column<decimal>(nullable: true),
                    paid_date = table.Column<DateTime>(nullable: false),
                    date_court_accept = table.Column<DateTime>(nullable: true),
                    document_date = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_electronic_document", x => x.id);
                    table.ForeignKey(
                        name: "FK_electronic_document_case_case_id",
                        column: x => x.case_id,
                        principalTable: "case",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_electronic_document_case_person_case_person_id",
                        column: x => x.case_person_id,
                        principalTable: "case_person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_electronic_document_common_court_court_id",
                        column: x => x.court_id,
                        principalTable: "common_court",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_electronic_document_nom_document_group_document_group_id",
                        column: x => x.document_group_id,
                        principalTable: "nom_document_group",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_electronic_document_nom_document_kind_document_kind_id",
                        column: x => x.document_kind_id,
                        principalTable: "nom_document_kind",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_electronic_document_epep_user_epep_user_id",
                        column: x => x.epep_user_id,
                        principalTable: "epep_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "nom_transaction_operation_type",
                columns: table => new
                {
                    id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    order_number = table.Column<int>(nullable: false),
                    code = table.Column<string>(nullable: true),
                    label = table.Column<string>(nullable: false),
                    description = table.Column<string>(nullable: true),
                    is_active = table.Column<bool>(nullable: false),
                    date_start = table.Column<DateTime>(nullable: false),
                    date_end = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nom_transaction_operation_type", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "electronic_document_person",
                columns: table => new
                {
                    id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    uic = table.Column<string>(nullable: true),
                    uic_type_id = table.Column<int>(nullable: false),
                    first_name = table.Column<string>(nullable: true),
                    middle_name = table.Column<string>(nullable: true),
                    family_name = table.Column<string>(nullable: true),
                    family_2_name = table.Column<string>(nullable: true),
                    full_name = table.Column<string>(nullable: true),
                    department_name = table.Column<string>(nullable: true),
                    latin_name = table.Column<string>(nullable: true),
                    is_deceased = table.Column<bool>(nullable: true),
                    date_deceased = table.Column<DateTime>(nullable: true),
                    person_source_type = table.Column<int>(nullable: true),
                    person_source_id = table.Column<long>(nullable: true),
                    person_source_code = table.Column<string>(nullable: true),
                    person_id = table.Column<int>(nullable: true),
                    electronic_document_id = table.Column<long>(nullable: false),
                    person_role_id = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_electronic_document_person", x => x.id);
                    table.ForeignKey(
                        name: "FK_electronic_document_person_electronic_document_electronic_d~",
                        column: x => x.electronic_document_id,
                        principalTable: "electronic_document",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_electronic_document_person_common_person_person_id",
                        column: x => x.person_id,
                        principalTable: "common_person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_electronic_document_person_nom_person_role_person_role_id",
                        column: x => x.person_role_id,
                        principalTable: "nom_person_role",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_electronic_document_person_nom_uic_type_uic_type_id",
                        column: x => x.uic_type_id,
                        principalTable: "nom_uic_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "common_main_transaction",
                columns: table => new
                {
                    id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    user_id = table.Column<string>(nullable: true),
                    date_wrt = table.Column<DateTime>(nullable: false),
                    date_transfered_dw = table.Column<DateTime>(nullable: true),
                    main_group_id = table.Column<long>(nullable: false),
                    prior_operation_type_id = table.Column<int>(nullable: false),
                    operation_type_id = table.Column<int>(nullable: false),
                    message = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_common_main_transaction", x => x.id);
                    table.ForeignKey(
                        name: "FK_common_main_transaction_nom_transaction_operation_type_oper~",
                        column: x => x.operation_type_id,
                        principalTable: "nom_transaction_operation_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_common_main_transaction_nom_transaction_operation_type_prio~",
                        column: x => x.prior_operation_type_id,
                        principalTable: "nom_transaction_operation_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_common_main_transaction_identity_users_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "common_main_group",
                columns: table => new
                {
                    id = table.Column<long>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    user_id = table.Column<string>(nullable: true),
                    date_wrt = table.Column<DateTime>(nullable: false),
                    date_transfered_dw = table.Column<DateTime>(nullable: true),
                    source_type = table.Column<int>(nullable: false),
                    source_id = table.Column<long>(nullable: false),
                    parent_source_id = table.Column<long>(nullable: true),
                    message = table.Column<string>(nullable: true),
                    last_transaction_id = table.Column<long>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_common_main_group", x => x.id);
                    table.ForeignKey(
                        name: "FK_common_main_group_common_main_transaction_last_transaction_~",
                        column: x => x.last_transaction_id,
                        principalTable: "common_main_transaction",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_common_main_group_identity_users_user_id",
                        column: x => x.user_id,
                        principalTable: "identity_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_document_electronic_document_id",
                table: "document",
                column: "electronic_document_id");

            migrationBuilder.CreateIndex(
                name: "IX_common_main_group_last_transaction_id",
                table: "common_main_group",
                column: "last_transaction_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_common_main_group_user_id",
                table: "common_main_group",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_common_main_transaction_main_group_id",
                table: "common_main_transaction",
                column: "main_group_id");

            migrationBuilder.CreateIndex(
                name: "IX_common_main_transaction_operation_type_id",
                table: "common_main_transaction",
                column: "operation_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_common_main_transaction_prior_operation_type_id",
                table: "common_main_transaction",
                column: "prior_operation_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_common_main_transaction_user_id",
                table: "common_main_transaction",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_electronic_document_case_id",
                table: "electronic_document",
                column: "case_id");

            migrationBuilder.CreateIndex(
                name: "IX_electronic_document_case_person_id",
                table: "electronic_document",
                column: "case_person_id");

            migrationBuilder.CreateIndex(
                name: "IX_electronic_document_court_id",
                table: "electronic_document",
                column: "court_id");

            migrationBuilder.CreateIndex(
                name: "IX_electronic_document_document_group_id",
                table: "electronic_document",
                column: "document_group_id");

            migrationBuilder.CreateIndex(
                name: "IX_electronic_document_document_kind_id",
                table: "electronic_document",
                column: "document_kind_id");

            migrationBuilder.CreateIndex(
                name: "IX_electronic_document_epep_user_id",
                table: "electronic_document",
                column: "epep_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_electronic_document_person_electronic_document_id",
                table: "electronic_document_person",
                column: "electronic_document_id");

            migrationBuilder.CreateIndex(
                name: "IX_electronic_document_person_person_id",
                table: "electronic_document_person",
                column: "person_id");

            migrationBuilder.CreateIndex(
                name: "IX_electronic_document_person_person_role_id",
                table: "electronic_document_person",
                column: "person_role_id");

            migrationBuilder.CreateIndex(
                name: "IX_electronic_document_person_uic_type_id",
                table: "electronic_document_person",
                column: "uic_type_id");

            migrationBuilder.AddForeignKey(
                name: "FK_document_electronic_document_electronic_document_id",
                table: "document",
                column: "electronic_document_id",
                principalTable: "electronic_document",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_common_main_transaction_common_main_group_main_group_id",
                table: "common_main_transaction",
                column: "main_group_id",
                principalTable: "common_main_group",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_document_electronic_document_electronic_document_id",
                table: "document");

            migrationBuilder.DropForeignKey(
                name: "FK_common_main_group_common_main_transaction_last_transaction_~",
                table: "common_main_group");

            migrationBuilder.DropTable(
                name: "electronic_document_person");

            migrationBuilder.DropTable(
                name: "electronic_document");

            migrationBuilder.DropTable(
                name: "common_main_transaction");

            migrationBuilder.DropTable(
                name: "common_main_group");

            migrationBuilder.DropTable(
                name: "nom_transaction_operation_type");

            migrationBuilder.DropIndex(
                name: "IX_document_electronic_document_id",
                table: "document");

            migrationBuilder.DropColumn(
                name: "assignment_role",
                table: "epep_user_assignment");

            migrationBuilder.DropColumn(
                name: "electronic_document_id",
                table: "document");

            migrationBuilder.DropColumn(
                name: "epep_has_electronic_documents",
                table: "common_court");
        }
    }
}
