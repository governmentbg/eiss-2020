using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace IOWebApplicationService.Infrastructure.Migrations
{
    public partial class DW_CaseSessionAct_20200513 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "act_return_date",
                table: "dw_case_session_act",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "act_return_date_str",
                table: "dw_case_session_act",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "act_return_date",
                table: "dw_case_session_act");

            migrationBuilder.DropColumn(
                name: "act_return_date_str",
                table: "dw_case_session_act");
        }
    }
}
