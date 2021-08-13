using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplicationService.Infrastructure.Migrations
{
    public partial class DW_DocumentPerson20200413 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "dw_document_person",
                columns: table => new
                {
                    dw_Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    court_id = table.Column<int>(nullable: true),
                    court_name = table.Column<string>(nullable: true),
                    ecli_code = table.Column<string>(nullable: true),
                    court_type_id = table.Column<int>(nullable: true),
                    court_type_name = table.Column<string>(nullable: true),
                    parent_court_id = table.Column<int>(nullable: true),
                    parent_court_name = table.Column<string>(nullable: true),
                    court_region_id = table.Column<int>(nullable: true),
                    court_region_name = table.Column<string>(nullable: true),
                    eispp_code = table.Column<string>(nullable: true),
                    city_name = table.Column<string>(nullable: true),
                    city_code = table.Column<string>(nullable: true),
                    case_instance_id = table.Column<int>(nullable: true),
                    case_instance_name = table.Column<string>(nullable: true),
                    case_instance_code = table.Column<string>(nullable: true),
                    user_id = table.Column<string>(nullable: true),
                    user_name = table.Column<string>(nullable: true),
                    date_wrt = table.Column<DateTime>(nullable: false),
                    date_transfered_dw = table.Column<DateTime>(nullable: true),
                    id = table.Column<long>(nullable: false),
                    document_id = table.Column<long>(nullable: false),
                    person_id = table.Column<int>(nullable: true),
                    uic = table.Column<string>(nullable: true),
                    uic_type_id = table.Column<int>(nullable: false),
                    uic_type_name = table.Column<string>(nullable: true),
                    first_name = table.Column<string>(nullable: true),
                    middle_name = table.Column<string>(nullable: true),
                    family_name = table.Column<string>(nullable: true),
                    family_2_name = table.Column<string>(nullable: true),
                    full_name = table.Column<string>(nullable: true),
                    department_name = table.Column<string>(nullable: true),
                    latin_name = table.Column<string>(nullable: true),
                    person_source_type = table.Column<int>(nullable: true),
                    person_source_type_name = table.Column<string>(nullable: true),
                    person_source_id = table.Column<long>(nullable: true),
                    person_source_name = table.Column<long>(nullable: true),
                    person_role_id = table.Column<int>(nullable: false),
                    person_role_name = table.Column<string>(nullable: true),
                    military_rang_id = table.Column<int>(nullable: true),
                    military_rang_name = table.Column<string>(nullable: true),
                    person_maturity_id = table.Column<int>(nullable: true),
                    person_maturity_name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dw_document_person", x => x.dw_Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "dw_document_person");
        }
    }
}
