using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Models.Integrations.DW
{
  public class UpdateDateTransferedVM
  {
    public const string ProcedureName = "public.update_date_transfered_dw";
    [Column("rows_affected")]
    public int RowsAffected { get; set; }

    public class Tables
    {
      public const string Case = "case";
      public const string CasePerson = "case_person";
      public const string CaseLawUnit = "case_lawunit";
      public const string CaseLifecycle = "case_lifecycle";
      public const string CaseSelectionProtocol = "case_selection_protokol";
      public const string CaseSelectionProtocolCompartment = "case_selection_protokol_compartment";
      public const string CaseSelectionProtocolLawunit = "case_selection_protokol_lawunit";
      public const string CaseSession = "case_session";
      public const string CaseSessionAct = "case_session_act";
      public const string CaseSessionActComplain = "case_session_act_complain";
      public const string CaseSessionActComplainPerson = "case_session_act_complain_person";
      public const string CaseSessionActComplainResult = "case_session_act_complain_result";
      public const string CaseSessionActCoordination = "case_session_act_coordination";
      public const string CaseSessionActDivorce = "case_session_act_divorce";

      public const string Document = "document";
      
      public const string DocumentDecision = "document_decision";

     

    }
  }
}
