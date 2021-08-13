using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace IOWebApplication.Infrastructure.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_identity_users_common_court_court_id",
                table: "identity_users");

            migrationBuilder.DropForeignKey(
                name: "FK_common_law_unit_identity_users_UserId",
                table: "common_law_unit");

            migrationBuilder.DropTable(
                name: "case_archive");

            migrationBuilder.DropTable(
                name: "case_bank_account");

            migrationBuilder.DropTable(
                name: "case_classification");

            migrationBuilder.DropTable(
                name: "case_depersonalization_value");

            migrationBuilder.DropTable(
                name: "case_evidence_movement");

            migrationBuilder.DropTable(
                name: "case_fast_process");

            migrationBuilder.DropTable(
                name: "case_h");

            migrationBuilder.DropTable(
                name: "case_lawunit_count");

            migrationBuilder.DropTable(
                name: "case_lawunit_replace");

            migrationBuilder.DropTable(
                name: "case_lifecycle");

            migrationBuilder.DropTable(
                name: "case_load_correction");

            migrationBuilder.DropTable(
                name: "case_load_index");

            migrationBuilder.DropTable(
                name: "case_migration");

            migrationBuilder.DropTable(
                name: "case_money");

            migrationBuilder.DropTable(
                name: "case_money_collection_person");

            migrationBuilder.DropTable(
                name: "case_money_expense");

            migrationBuilder.DropTable(
                name: "case_movement");

            migrationBuilder.DropTable(
                name: "case_notification_h");

            migrationBuilder.DropTable(
                name: "case_notification_mlink");

            migrationBuilder.DropTable(
                name: "case_person_address_h");

            migrationBuilder.DropTable(
                name: "case_person_crimes");

            migrationBuilder.DropTable(
                name: "case_person_documents");

            migrationBuilder.DropTable(
                name: "case_person_h");

            migrationBuilder.DropTable(
                name: "case_person_inheritance");

            migrationBuilder.DropTable(
                name: "case_person_measures");

            migrationBuilder.DropTable(
                name: "case_person_sentence_bulletin");

            migrationBuilder.DropTable(
                name: "case_person_sentence_lawbase");

            migrationBuilder.DropTable(
                name: "case_person_sentence_punishment_crime");

            migrationBuilder.DropTable(
                name: "case_selection_protokol_compartment");

            migrationBuilder.DropTable(
                name: "case_selection_protokol_lawunit");

            migrationBuilder.DropTable(
                name: "case_selection_protokol_lock");

            migrationBuilder.DropTable(
                name: "case_session_act_company");

            migrationBuilder.DropTable(
                name: "case_session_act_complain_person");

            migrationBuilder.DropTable(
                name: "case_session_act_complain_result");

            migrationBuilder.DropTable(
                name: "case_session_act_coordination");

            migrationBuilder.DropTable(
                name: "case_session_act_divorce");

            migrationBuilder.DropTable(
                name: "case_session_act_h");

            migrationBuilder.DropTable(
                name: "case_session_act_law_base");

            migrationBuilder.DropTable(
                name: "case_session_doc");

            migrationBuilder.DropTable(
                name: "case_session_fast_document_h");

            migrationBuilder.DropTable(
                name: "case_session_h");

            migrationBuilder.DropTable(
                name: "case_session_meeting_user");

            migrationBuilder.DropTable(
                name: "case_session_notification_list");

            migrationBuilder.DropTable(
                name: "common_compartment_lawunit");

            migrationBuilder.DropTable(
                name: "common_counter_cases");

            migrationBuilder.DropTable(
                name: "common_counter_document");

            migrationBuilder.DropTable(
                name: "common_counter_session_act");

            migrationBuilder.DropTable(
                name: "common_court_archive_committee_lawunit");

            migrationBuilder.DropTable(
                name: "common_court_archive_index_codes");

            migrationBuilder.DropTable(
                name: "common_court_department_group");

            migrationBuilder.DropTable(
                name: "common_court_department_lawunit");

            migrationBuilder.DropTable(
                name: "common_court_duty_lawunit");

            migrationBuilder.DropTable(
                name: "common_court_group_code");

            migrationBuilder.DropTable(
                name: "common_court_lawunit");

            migrationBuilder.DropTable(
                name: "common_court_lawunit_activity");

            migrationBuilder.DropTable(
                name: "common_court_lawunit_group");

            migrationBuilder.DropTable(
                name: "common_court_load_period_lawunit");

            migrationBuilder.DropTable(
                name: "common_court_organization_case_group");

            migrationBuilder.DropTable(
                name: "common_court_pos_device");

            migrationBuilder.DropTable(
                name: "common_court_region_area");

            migrationBuilder.DropTable(
                name: "common_email_file");

            migrationBuilder.DropTable(
                name: "common_excel_report_case_filter");

            migrationBuilder.DropTable(
                name: "common_excel_report_data");

            migrationBuilder.DropTable(
                name: "common_external_data");

            migrationBuilder.DropTable(
                name: "common_html_template_link");

            migrationBuilder.DropTable(
                name: "common_html_template_param_link");

            migrationBuilder.DropTable(
                name: "common_institution_address");

            migrationBuilder.DropTable(
                name: "common_integration_keys");

            migrationBuilder.DropTable(
                name: "common_jury_fee");

            migrationBuilder.DropTable(
                name: "common_law_unit_h");

            migrationBuilder.DropTable(
                name: "common_lawunit_address");

            migrationBuilder.DropTable(
                name: "common_lawunit_speciality");

            migrationBuilder.DropTable(
                name: "common_mq_epep");

            migrationBuilder.DropTable(
                name: "common_person_address");

            migrationBuilder.DropTable(
                name: "common_priceval");

            migrationBuilder.DropTable(
                name: "common_report_request");

            migrationBuilder.DropTable(
                name: "common_work_notification");

            migrationBuilder.DropTable(
                name: "common_working_day");

            migrationBuilder.DropTable(
                name: "common_worktask");

            migrationBuilder.DropTable(
                name: "delivery_account");

            migrationBuilder.DropTable(
                name: "delivery_area_address");

            migrationBuilder.DropTable(
                name: "delivery_item_oper");

            migrationBuilder.DropTable(
                name: "delivery_item_visit_mobile");

            migrationBuilder.DropTable(
                name: "delivery_mobile_file");

            migrationBuilder.DropTable(
                name: "document_case_info");

            migrationBuilder.DropTable(
                name: "document_decision_case");

            migrationBuilder.DropTable(
                name: "document_institution_case_info");

            migrationBuilder.DropTable(
                name: "document_link");

            migrationBuilder.DropTable(
                name: "document_person_address");

            migrationBuilder.DropTable(
                name: "document_template");

            migrationBuilder.DropTable(
                name: "ek_areas");

            migrationBuilder.DropTable(
                name: "ek_regions");

            migrationBuilder.DropTable(
                name: "ek_sobr");

            migrationBuilder.DropTable(
                name: "ek_streets");

            migrationBuilder.DropTable(
                name: "epep_user_assignment");

            migrationBuilder.DropTable(
                name: "identity_role_claims");

            migrationBuilder.DropTable(
                name: "identity_user_claims");

            migrationBuilder.DropTable(
                name: "identity_user_logins");

            migrationBuilder.DropTable(
                name: "identity_user_roles");

            migrationBuilder.DropTable(
                name: "identity_user_tokens");

            migrationBuilder.DropTable(
                name: "log_operations");

            migrationBuilder.DropTable(
                name: "money_exchange_doc_exec_list");

            migrationBuilder.DropTable(
                name: "money_exec_list_obligation");

            migrationBuilder.DropTable(
                name: "money_expense_order_obligation");

            migrationBuilder.DropTable(
                name: "money_obligation_payment");

            migrationBuilder.DropTable(
                name: "money_obligation_receive");

            migrationBuilder.DropTable(
                name: "money_pos_payment_result");

            migrationBuilder.DropTable(
                name: "nom_act_complain_index_court_type_case_group");

            migrationBuilder.DropTable(
                name: "nom_act_complain_result_case_type");

            migrationBuilder.DropTable(
                name: "nom_act_complain_result_grouping");

            migrationBuilder.DropTable(
                name: "nom_act_motive_state");

            migrationBuilder.DropTable(
                name: "nom_act_result_group");

            migrationBuilder.DropTable(
                name: "nom_act_result_grouping");

            migrationBuilder.DropTable(
                name: "nom_act_type_court_link");

            migrationBuilder.DropTable(
                name: "nom_bank");

            migrationBuilder.DropTable(
                name: "nom_case_code_group");

            migrationBuilder.DropTable(
                name: "nom_case_code_grouping");

            migrationBuilder.DropTable(
                name: "nom_case_load_add_activity_index");

            migrationBuilder.DropTable(
                name: "nom_case_load_correction_activity_index");

            migrationBuilder.DropTable(
                name: "nom_case_type_character");

            migrationBuilder.DropTable(
                name: "nom_case_type_code");

            migrationBuilder.DropTable(
                name: "nom_case_type_unit_count");

            migrationBuilder.DropTable(
                name: "nom_code_mapping");

            migrationBuilder.DropTable(
                name: "nom_complain_type");

            migrationBuilder.DropTable(
                name: "nom_court_type_case_type");

            migrationBuilder.DropTable(
                name: "nom_court_type_session_type");

            migrationBuilder.DropTable(
                name: "nom_delivery_direction_group");

            migrationBuilder.DropTable(
                name: "nom_delivery_number_type");

            migrationBuilder.DropTable(
                name: "nom_delivery_oper_state");

            migrationBuilder.DropTable(
                name: "nom_delivery_state_reason");

            migrationBuilder.DropTable(
                name: "nom_document_register_links");

            migrationBuilder.DropTable(
                name: "nom_document_register_types");

            migrationBuilder.DropTable(
                name: "nom_document_type_case_type");

            migrationBuilder.DropTable(
                name: "nom_document_type_court_type");

            migrationBuilder.DropTable(
                name: "nom_document_type_decision_type");

            migrationBuilder.DropTable(
                name: "nom_document_type_grouping");

            migrationBuilder.DropTable(
                name: "nom_eispp_case_code");

            migrationBuilder.DropTable(
                name: "nom_eispp_rules");

            migrationBuilder.DropTable(
                name: "nom_eispp_tbl_element");

            migrationBuilder.DropTable(
                name: "nom_exec_list_law_base_case_group");

            migrationBuilder.DropTable(
                name: "nom_execlist_moneytype");

            migrationBuilder.DropTable(
                name: "nom_expence_moneytype");

            migrationBuilder.DropTable(
                name: "nom_judge_load_activity_index");

            migrationBuilder.DropTable(
                name: "nom_law_unit_type_position");

            migrationBuilder.DropTable(
                name: "nom_load_group_link_code");

            migrationBuilder.DropTable(
                name: "nom_money_fee_document_group");

            migrationBuilder.DropTable(
                name: "nom_money_fine_case_group");

            migrationBuilder.DropTable(
                name: "nom_notification_delivery_group_state");

            migrationBuilder.DropTable(
                name: "nom_person_role_case_type");

            migrationBuilder.DropTable(
                name: "nom_person_role_link_direction");

            migrationBuilder.DropTable(
                name: "nom_session_act_type");

            migrationBuilder.DropTable(
                name: "nom_session_duration");

            migrationBuilder.DropTable(
                name: "nom_session_result_base_grouping");

            migrationBuilder.DropTable(
                name: "nom_session_result_filter_rule");

            migrationBuilder.DropTable(
                name: "nom_session_result_grouping");

            migrationBuilder.DropTable(
                name: "nom_session_type_state");

            migrationBuilder.DropTable(
                name: "nom_task_type_source_type");

            migrationBuilder.DropTable(
                name: "regix_map_actual_state");

            migrationBuilder.DropTable(
                name: "regix_report");

            migrationBuilder.DropTable(
                name: "audit_log",
                schema: "audit_log");

            migrationBuilder.DropTable(
                name: "nom_case_bank_account_type");

            migrationBuilder.DropTable(
                name: "nom_classification");

            migrationBuilder.DropTable(
                name: "case_evidence");

            migrationBuilder.DropTable(
                name: "nom_evidence_movement_type");

            migrationBuilder.DropTable(
                name: "nom_lifecycle_type");

            migrationBuilder.DropTable(
                name: "nom_case_load_element_type");

            migrationBuilder.DropTable(
                name: "nom_case_migration_type");

            migrationBuilder.DropTable(
                name: "case_money_collection");

            migrationBuilder.DropTable(
                name: "nom_case_money_expense_type");

            migrationBuilder.DropTable(
                name: "nom_movement_type");

            migrationBuilder.DropTable(
                name: "nom_case_person_inheritance_result");

            migrationBuilder.DropTable(
                name: "nom_sentence_lawbase");

            migrationBuilder.DropTable(
                name: "case_crimes");

            migrationBuilder.DropTable(
                name: "case_person_sentence_punishment");

            migrationBuilder.DropTable(
                name: "nom_person_role_in_crime");

            migrationBuilder.DropTable(
                name: "nom_recidive_type");

            migrationBuilder.DropTable(
                name: "nom_selection_lawunit_state");

            migrationBuilder.DropTable(
                name: "nom_act_coordination_type");

            migrationBuilder.DropTable(
                name: "nom_law_base");

            migrationBuilder.DropTable(
                name: "case_session_fast_document");

            migrationBuilder.DropTable(
                name: "common_counter");

            migrationBuilder.DropTable(
                name: "common_court_archive_index");

            migrationBuilder.DropTable(
                name: "nom_period_type");

            migrationBuilder.DropTable(
                name: "common_court_load_period");

            migrationBuilder.DropTable(
                name: "common_email_message");

            migrationBuilder.DropTable(
                name: "common_mongo_file");

            migrationBuilder.DropTable(
                name: "common_excel_report_template");

            migrationBuilder.DropTable(
                name: "nom_html_template_param");

            migrationBuilder.DropTable(
                name: "nom_integration_state");

            migrationBuilder.DropTable(
                name: "nom_integration_type");

            migrationBuilder.DropTable(
                name: "common_pricecol");

            migrationBuilder.DropTable(
                name: "common_report");

            migrationBuilder.DropTable(
                name: "case_deadline");

            migrationBuilder.DropTable(
                name: "nom_work_notification_type");

            migrationBuilder.DropTable(
                name: "nom_day_type");

            migrationBuilder.DropTable(
                name: "nom_task_action");

            migrationBuilder.DropTable(
                name: "nom_task_execution");

            migrationBuilder.DropTable(
                name: "nom_task_state");

            migrationBuilder.DropTable(
                name: "delivery_item");

            migrationBuilder.DropTable(
                name: "document_decision");

            migrationBuilder.DropTable(
                name: "nom_institution_case_type");

            migrationBuilder.DropTable(
                name: "document_person");

            migrationBuilder.DropTable(
                name: "nom_document_template_state");

            migrationBuilder.DropTable(
                name: "ek_ekatte");

            migrationBuilder.DropTable(
                name: "epep_user");

            migrationBuilder.DropTable(
                name: "identity_roles");

            migrationBuilder.DropTable(
                name: "money_exchange_doc");

            migrationBuilder.DropTable(
                name: "money_exec_list");

            migrationBuilder.DropTable(
                name: "money_expense_order");

            migrationBuilder.DropTable(
                name: "money_obligation");

            migrationBuilder.DropTable(
                name: "money_payment");

            migrationBuilder.DropTable(
                name: "nom_case_load_add_activity");

            migrationBuilder.DropTable(
                name: "nom_case_load_correction_activity");

            migrationBuilder.DropTable(
                name: "nom_delivery_oper");

            migrationBuilder.DropTable(
                name: "nom_delivery_reason");

            migrationBuilder.DropTable(
                name: "nom_document_registers");

            migrationBuilder.DropTable(
                name: "nom_eispp_tbl");

            migrationBuilder.DropTable(
                name: "nom_judge_load_activity");

            migrationBuilder.DropTable(
                name: "nom_law_unit_position");

            migrationBuilder.DropTable(
                name: "nom_session_act_group");

            migrationBuilder.DropTable(
                name: "nom_regix_type");

            migrationBuilder.DropTable(
                name: "nom_evidence_state");

            migrationBuilder.DropTable(
                name: "nom_evidence_type");

            migrationBuilder.DropTable(
                name: "nom_case_load_element_group");

            migrationBuilder.DropTable(
                name: "case_money_claim");

            migrationBuilder.DropTable(
                name: "nom_case_money_collection_kind");

            migrationBuilder.DropTable(
                name: "nom_case_money_collection_type");

            migrationBuilder.DropTable(
                name: "nom_currency");

            migrationBuilder.DropTable(
                name: "case_person_sentence");

            migrationBuilder.DropTable(
                name: "nom_sentence_regime_type");

            migrationBuilder.DropTable(
                name: "nom_sentence_type");

            migrationBuilder.DropTable(
                name: "nom_session_doc_state");

            migrationBuilder.DropTable(
                name: "nom_session_doc_type");

            migrationBuilder.DropTable(
                name: "nom_counter_type");

            migrationBuilder.DropTable(
                name: "nom_counter_reset_type");

            migrationBuilder.DropTable(
                name: "common_court_archive_committee");

            migrationBuilder.DropTable(
                name: "common_court_load_reset_period");

            migrationBuilder.DropTable(
                name: "nom_email_message_state");

            migrationBuilder.DropTable(
                name: "common_pricedesc");

            migrationBuilder.DropTable(
                name: "case_session_result");

            migrationBuilder.DropTable(
                name: "nom_deadline_type");

            migrationBuilder.DropTable(
                name: "nom_task_type");

            migrationBuilder.DropTable(
                name: "case_notification");

            migrationBuilder.DropTable(
                name: "delivery_area");

            migrationBuilder.DropTable(
                name: "nom_decision_type");

            migrationBuilder.DropTable(
                name: "nom_document_decision_state");

            migrationBuilder.DropTable(
                name: "ek_munincipalities");

            migrationBuilder.DropTable(
                name: "nom_epep_user_type");

            migrationBuilder.DropTable(
                name: "nom_exec_list_law_base");

            migrationBuilder.DropTable(
                name: "nom_exec_list_type");

            migrationBuilder.DropTable(
                name: "nom_expense_order_state");

            migrationBuilder.DropTable(
                name: "case_session_meeting");

            migrationBuilder.DropTable(
                name: "nom_money_fee_type");

            migrationBuilder.DropTable(
                name: "nom_money_fine_type");

            migrationBuilder.DropTable(
                name: "nom_money_type");

            migrationBuilder.DropTable(
                name: "common_court_bank_account");

            migrationBuilder.DropTable(
                name: "nom_payment_type");

            migrationBuilder.DropTable(
                name: "nom_case_money_claim_type");

            migrationBuilder.DropTable(
                name: "nom_case_money_collection_group");

            migrationBuilder.DropTable(
                name: "common_institution");

            migrationBuilder.DropTable(
                name: "nom_punishement_activity");

            migrationBuilder.DropTable(
                name: "nom_sentence_exec_period");

            migrationBuilder.DropTable(
                name: "nom_sentence_result_type");

            migrationBuilder.DropTable(
                name: "nom_session_result_base");

            migrationBuilder.DropTable(
                name: "nom_session_result");

            migrationBuilder.DropTable(
                name: "nom_deadline_group");

            migrationBuilder.DropTable(
                name: "case_person_address");

            migrationBuilder.DropTable(
                name: "case_person_link");

            migrationBuilder.DropTable(
                name: "case_session_act_complain");

            migrationBuilder.DropTable(
                name: "common_html_template");

            migrationBuilder.DropTable(
                name: "nom_notification_delivery_type");

            migrationBuilder.DropTable(
                name: "nom_notification_state");

            migrationBuilder.DropTable(
                name: "nom_notification_type");

            migrationBuilder.DropTable(
                name: "ek_districts");

            migrationBuilder.DropTable(
                name: "nom_session_meeting_type");

            migrationBuilder.DropTable(
                name: "nom_money_group");

            migrationBuilder.DropTable(
                name: "nom_case_money_claim_group");

            migrationBuilder.DropTable(
                name: "nom_institution_type");

            migrationBuilder.DropTable(
                name: "nom_session_result_group");

            migrationBuilder.DropTable(
                name: "case_person");

            migrationBuilder.DropTable(
                name: "nom_link_direction");

            migrationBuilder.DropTable(
                name: "nom_complain_state");

            migrationBuilder.DropTable(
                name: "nom_notification_delivery_group");

            migrationBuilder.DropTable(
                name: "nom_notification_mode");

            migrationBuilder.DropTable(
                name: "ek_countries");

            migrationBuilder.DropTable(
                name: "case_selection_protokol");

            migrationBuilder.DropTable(
                name: "nom_company_type");

            migrationBuilder.DropTable(
                name: "nom_military_rang");

            migrationBuilder.DropTable(
                name: "nom_person_maturity");

            migrationBuilder.DropTable(
                name: "nom_person_role");

            migrationBuilder.DropTable(
                name: "case_lawunit_dismisal");

            migrationBuilder.DropTable(
                name: "nom_selection_mode");

            migrationBuilder.DropTable(
                name: "nom_selection_protokol_state");

            migrationBuilder.DropTable(
                name: "nom_speciality");

            migrationBuilder.DropTable(
                name: "nom_role_kind");

            migrationBuilder.DropTable(
                name: "case_lawunit");

            migrationBuilder.DropTable(
                name: "case_session_act");

            migrationBuilder.DropTable(
                name: "nom_dismisal_type");

            migrationBuilder.DropTable(
                name: "common_court_department");

            migrationBuilder.DropTable(
                name: "common_court_duty");

            migrationBuilder.DropTable(
                name: "nom_judge_department_role");

            migrationBuilder.DropTable(
                name: "nom_judge_role");

            migrationBuilder.DropTable(
                name: "nom_act_complain_index");

            migrationBuilder.DropTable(
                name: "nom_act_complain_result");

            migrationBuilder.DropTable(
                name: "nom_act_ispn_debtor_state");

            migrationBuilder.DropTable(
                name: "nom_act_ispn_reason");

            migrationBuilder.DropTable(
                name: "nom_act_kind");

            migrationBuilder.DropTable(
                name: "nom_act_result");

            migrationBuilder.DropTable(
                name: "nom_act_state");

            migrationBuilder.DropTable(
                name: "case_session");

            migrationBuilder.DropTable(
                name: "nom_department_type");

            migrationBuilder.DropTable(
                name: "nom_act_type");

            migrationBuilder.DropTable(
                name: "case");

            migrationBuilder.DropTable(
                name: "common_compartment");

            migrationBuilder.DropTable(
                name: "common_court_hall");

            migrationBuilder.DropTable(
                name: "nom_session_state");

            migrationBuilder.DropTable(
                name: "nom_session_type");

            migrationBuilder.DropTable(
                name: "nom_case_character");

            migrationBuilder.DropTable(
                name: "nom_case_code");

            migrationBuilder.DropTable(
                name: "nom_case_reason");

            migrationBuilder.DropTable(
                name: "nom_case_state");

            migrationBuilder.DropTable(
                name: "nom_case_type_unit");

            migrationBuilder.DropTable(
                name: "common_court_group");

            migrationBuilder.DropTable(
                name: "document");

            migrationBuilder.DropTable(
                name: "nom_load_group_link");

            migrationBuilder.DropTable(
                name: "nom_process_priority");

            migrationBuilder.DropTable(
                name: "nom_case_type");

            migrationBuilder.DropTable(
                name: "common_court_organization");

            migrationBuilder.DropTable(
                name: "nom_delivery_type");

            migrationBuilder.DropTable(
                name: "nom_document_type");

            migrationBuilder.DropTable(
                name: "nom_load_group");

            migrationBuilder.DropTable(
                name: "nom_case_group");

            migrationBuilder.DropTable(
                name: "nom_case_instance");

            migrationBuilder.DropTable(
                name: "nom_organization_level");

            migrationBuilder.DropTable(
                name: "nom_delivery_group");

            migrationBuilder.DropTable(
                name: "nom_document_group");

            migrationBuilder.DropTable(
                name: "nom_html_template_type");

            migrationBuilder.DropTable(
                name: "nom_document_kind");

            migrationBuilder.DropTable(
                name: "nom_document_direction");

            migrationBuilder.DropTable(
                name: "common_court");

            migrationBuilder.DropTable(
                name: "common_address");

            migrationBuilder.DropTable(
                name: "common_court_region");

            migrationBuilder.DropTable(
                name: "nom_court_type");

            migrationBuilder.DropTable(
                name: "nom_address_type");

            migrationBuilder.DropTable(
                name: "identity_users");

            migrationBuilder.DropTable(
                name: "common_law_unit");

            migrationBuilder.DropTable(
                name: "nom_judge_seniority");

            migrationBuilder.DropTable(
                name: "nom_law_unit_type");

            migrationBuilder.DropTable(
                name: "common_person");

            migrationBuilder.DropTable(
                name: "nom_uic_type");
        }
    }
}
