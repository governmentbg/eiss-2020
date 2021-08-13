using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplicationService.Infrastructure.Migrations
{
    public partial class DW_DocumentLinkt20200414 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "dw_document_institution_case_info",
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
                    institution_id = table.Column<int>(nullable: false),
                    institution_name = table.Column<string>(nullable: true),
                    institution_case_type_id = table.Column<int>(nullable: true),
                    institution_case_type_name = table.Column<string>(nullable: true),
                    case_number = table.Column<string>(nullable: true),
                    case_year = table.Column<int>(nullable: false),
                    description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dw_document_institution_case_info", x => x.dw_Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "dw_document_institution_case_info");
        }
    }
}
