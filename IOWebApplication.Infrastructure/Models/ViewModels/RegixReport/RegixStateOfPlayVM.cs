using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.RegixReport
{
    public static class RegixStateDefinitionConstants
    {
        public const string LegalBase = "1";
        public const string LegalStatut = "2";
        public const string SubjectGroup = "3";
        public const string LegalForm = "4";
        public const string EventType = "7";
        public const string SubjectState = "8";
        public const string OwnershipForm = "9";
        public const string FundingSource = "10";
        public const string AccountingRecordForm = "11";
        public const string SubordinateLevel = "12";
        public const string Position = "16";
        public const string CollectiveBody = "17";
        public const string Partner = "18";
        public const string Belonging = "20";
        public const string Assignee = "21";
        public const string AddressType = "22";
        public const string CommunicationType = "23";
        public const string DocumentType = "27";
        public const string IdentificationDocType = "28";
        public const string SubjectType = "53";
        public const string Profession = "2001";
        public const string RepresentationType = "2002";
        public const string ActivityType = "2003";
        public const string MainActivity2003 = "2006";
        public const string MainActivity2008 = "2007";
        public const string EKATTE = "2008";
        public const string EKATTERegion = "2009";
    }

    public class RegixStateOfPlayVM
    {
        public RegixReportVM Report { get; set; }

        public RegixStateOfPlayFilterVM StateOfPlayFilter { get; set; }

        public RegixStateOfPlayResponseVM StateOfPlayResponse { get; set; }

        public RegixStateOfPlayVM()
        {
            Report = new RegixReportVM();
            StateOfPlayFilter = new RegixStateOfPlayFilterVM();
            StateOfPlayResponse = new RegixStateOfPlayResponseVM();
        }
    }

    public class RegixStateOfPlayFilterVM
    {
        [Display(Name = "БУЛСТАТ")]
        [Required(ErrorMessage = "Полето {0} е задължително")]
        public string UIC { get; set; }
    }

    public class RegixStateOfPlayResponseVM
    {
        [Display(Name = "Начин на представляване вид:")]
        public string RepresentationType { get; set; }

        [Display(Name = "Начин на представляване забележка:")]
        public string RepresentationText { get; set; }

        [Display(Name = "Предмет на дейност:")]
        public string ScopeOfActivity { get; set; }

        [Display(Name = "Основна дейност(КИД2008):")]
        public string MainActivity2008 { get; set; }

        [Display(Name = "Основна дейност (НКИД2003):")]
        public string MainActivity2003 { get; set; }

        [Display(Name = "Срок на същестуване дата:")]
        public string LifeTimeDate { get; set; }

        [Display(Name = "Срок на същестуване забележка:")]
        public string LifeTimeDescription { get; set; }

        [Display(Name = "Форма на счетоводно записване:")]
        public string AccountingRecordForm { get; set; }

        [Display(Name = "Състояние на субект:")]
        public string State { get; set; }

        [Display(Name = "Тип дейност:")]
        public string ActivityType { get; set; }

        [Display(Name = "Дата на тип дейност:")]
        public string ActivityDate { get; set; }

        public RegixStateSubjectVM StateSubject { get; set; }

        public RegixStateEventVM StateEvent { get; set; }

        public List<RegixStateInstallmentVM> StateInstallment { get; set; }

        public List<RegixStateOwnershipFormVM> StateOwnershipForm { get; set; }

        public List<RegixStateFundingSourceVM> StateFundingSource { get; set; }

        public List<RegixStateManagerVM> StateManager { get; set; }

        public List<RegixStatePartnerVM> StatePartner { get; set; }

        public RegixStateAssigneeVM StateAssignee { get; set; }
        
        public RegixStateBelongingVM StateBelonging { get; set; }

        public List<RegixCollectiveBodiesVM> CollectiveBodies { get; set; }

        public List<string> AdditionalActivities2008 { get; set; }

        public List<string> Professions { get; set; }

        public RegixStateOfPlayResponseVM()
        {
            StateSubject = new RegixStateSubjectVM();
            StateEvent = new RegixStateEventVM();
            StateInstallment = new List<RegixStateInstallmentVM>();
            StateOwnershipForm = new List<RegixStateOwnershipFormVM>();
            StateFundingSource = new List<RegixStateFundingSourceVM>();
            StateManager = new List<RegixStateManagerVM>();
            StatePartner = new List<RegixStatePartnerVM>();
            StateAssignee = new RegixStateAssigneeVM();
            StateBelonging = new RegixStateBelongingVM();
            CollectiveBodies = new List<RegixCollectiveBodiesVM>();
            AdditionalActivities2008 = new List<string>();
            Professions = new List<string>();
        }
    }

    public class RegixStateSubjectVM
    {
        [Display(Name = "Код по БУЛСТАТ:")]
        public string UIC { get; set; }

        [Display(Name = "Вид субект:")]
        public string SubjectType { get; set; }

        [Display(Name = "Забележки:")]
        public string Remark { get; set; }


        public RegixStateLegalEntitySubjectVM StateLegalEntitySubject { get; set; }

        public RegixStateNaturalPersonSubjectVM StateNaturalPersonSubject { get; set; }

        public List<RegixStateAddressVM> StateAddress { get; set; }

        public List<RegixStateCommunicationsVM> StateCommunications { get; set; }

        public RegixStateSubjectVM()
        {
            StateAddress = new List<RegixStateAddressVM>();
            StateCommunications = new List<RegixStateCommunicationsVM>();
        }
    }


    public class RegixStateAddressVM
    {
        public string AddressType { get; set; }

        public string AddressText { get; set; }
    }

    public class RegixStateCommunicationsVM
    {
        public string CommunicationType { get; set; }

        public string CommunicationText { get; set; }
    }

    public class RegixStateLegalEntitySubjectVM 
    {
        [Display(Name = "Държава:")]
        public string Country { get; set; }

        [Display(Name = "Правна форма:")]
        public string LegalForm { get; set; }

        [Display(Name = "Юридически статут:")]
        public string LegalStatute { get; set; }

        [Display(Name = "Група субекти:")]
        public string SubjectGroup { get; set; }

        [Display(Name = "Пълно наименование на кирилица:")]
        public string CyrillicFullName { get; set; }

        [Display(Name = "Кратко наименование на кирилица:")]
        public string CyrillicShortName { get; set; }

        [Display(Name = "Пълно наименование, изписано на латиница:")]
        public string LatinFullName { get; set; }

        [Display(Name = "Ниво на подчиненост:")]
        public string SubordinateLevel { get; set; }

        [Display(Name = "Статус от Търговския регистър:")]
        public string TRStatus { get; set; }
    }

    public class RegixStateNaturalPersonSubjectVM
    {
        [Display(Name = "Държава:")]
        public string Country { get; set; }

        [Display(Name = "ЕГН:")]
        public string EGN { get; set; }

        [Display(Name = "ЛНЧ:")]
        public string LNC { get; set; }

        [Display(Name = "Име на кирилица:")]
        public string CyrillicName { get; set; }

        [Display(Name = "Име на латиница:")]
        public string LatinName { get; set; }

        [Display(Name = "Дата на раждане:")]
        public string BirthDate { get; set; }

        [Display(Name = "Документ за самоличност:")]
        public string IdentificationDoc { get; set; }
    }

    public class RegixStateEventVM
    {
        [Display(Name = "Вид събитие:")]
        public string EventType { get; set; }

        [Display(Name = "Дата на събитие:")]
        public string EventDate { get; set; }

        [Display(Name = "Вид основание:")]
        public string LegalBase { get; set; }

        [Display(Name = "Дело:")]
        public string Case { get; set; }

        [Display(Name = "Вид документ:")]
        public string DocumentType { get; set; }

        [Display(Name = "Номер на документ:")]
        public string DocumentNumber { get; set; }

        [Display(Name = "Дата на документа:")]
        public string DocumentDate { get; set; }

        [Display(Name = "Дата, на която влиза в сила:")]
        public string ValidFromDate { get; set; }

        [Display(Name = "Име на документа:")]
        public string DocumentName { get; set; }

        [Display(Name = "Автор:")]
        public string AuthorName { get; set; }
    }

    public class RegixStateInstallmentVM
    {
        public string Count { get; set; }

        public string Amount { get; set; }
    }

    public class RegixStateOwnershipFormVM
    {
        public string Form { get; set; }

        public string Percent { get; set; }
    }

    public class RegixStateFundingSourceVM
    {
        public string Source { get; set; }

        public string Percent { get; set; }
    }

    public class RegixStateManagerVM
    {
        [Display(Name = "Длъжност:")]
        public string Position { get; set; }

        public RegixStateSubjectVM RelatedSubject { get; set; }

        public List<RegixStateSubjectVM> RepresentedSubjects { get; set; }

        public RegixStateManagerVM()
        {
            RelatedSubject = new RegixStateSubjectVM();
            RepresentedSubjects = new List<RegixStateSubjectVM>();
        }
    }

    public class RegixStatePartnerVM
    {
        [Display(Name = "Роля:")]
        public string Role { get; set; }

        [Display(Name = "Процент:")]
        public string Percent { get; set; }

        [Display(Name = "Сума:")]
        public string Amount { get; set; }

        public RegixStateSubjectVM RelatedSubject { get; set; }

        public RegixStatePartnerVM()
        {
            RelatedSubject = new RegixStateSubjectVM();
        }
    }

    public class RegixStateAssigneeVM
    {
        [Display(Name = "Вид:")]
        public string Type { get; set; }

        public List<RegixStateSubjectVM> RelatedSubject { get; set; }

        public RegixStateAssigneeVM()
        {
            RelatedSubject = new List<RegixStateSubjectVM>();
        }
    }

    public class RegixStateBelongingVM
    {
        [Display(Name = "Вид:")]
        public string Type { get; set; }

        public RegixStateSubjectVM RelatedSubject { get; set; }

        public RegixStateBelongingVM()
        {
            RelatedSubject = new RegixStateSubjectVM();
        }
    }

    public class RegixCollectiveBodiesVM
    {
        [Display(Name = "Вид:")]
        public string Type { get; set; }

        public List<RegixStateManagerVM> StateMembers { get; set; }

        public RegixCollectiveBodiesVM()
        {
            StateMembers = new List<RegixStateManagerVM>();
        }
    }

}
