using Integration.Epep;
using System;

namespace IOWebApplication.Infrastructure.Models.Integrations.EpepRest
{
    public class ExecProcessDetailsVM
    {
        public Guid Gid { get; set; }
        public int CaseNumber { get; set; }
        public int CaseYear { get; set; }
        public string CaseKindName { get; set; }
        public string CourtName { get; set; }
        public int? ActNumber { get; set; }
        public DateTime ActDate { get; set; }

        public int? OrderNumber { get; set; }
        public DateTime? OrderDate { get; set; }

        public Guid CaseGid { get; set; }
        public Guid ActGid { get; set; }
        public Guid? ActFileId { get; set; }
        public Guid? OrderFileId { get; set; }
        public bool HasCorrectedAct { get; set; }
        public bool JointDistribution { get; set; }

        public bool IsExpired { get; set; }
        public string InfoExpired { get; set; }


        public ExecProcessSideDetailsVM[] SideList { get; set; }
        public ExecProcessObligationDetailsVM[] ObligationList { get; set; }
        public ExecCaseDetailsVM[] ExecCases { get; set; }
        public ExecProcessAccessVM[] AccessList { get; set; }
        public CorrectedActVM[] CorrectionActList { get; set; }

        public SummaryFile[] ClaimDocumentFiles { get; set; }
    }

    public class ExecProcessObligationDetailsVM
    {
        /// <summary>
        /// Идентификатор на задължение
        /// </summary>
        public Guid Gid { get; set; }

        /// <summary>
        /// Код на вид задължение
        /// </summary>
        public string ObligationTypeCode { get; set; }

        /// <summary>
        /// Наименование на вид задължение
        /// </summary>
        public string ObligationTypeName { get; set; }

        /// <summary>
        /// Данни за взискател
        /// </summary>
        public Guid BeneficiaryGid { get; set; }

        /// <summary>
        /// Размер на задължението
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Код на валута
        /// </summary>
        public string CurrencyCode { get; set; }

        /// <summary>
        /// Дата на законна лихва
        /// </summary>
        public DateTime? StatutoryInterestDate { get; set; }

        /// <summary>
        /// Описание
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Списък длъжници по задължението
        /// </summary>
        public Guid[] Debtors { get; set; }
    }

    public class ExecProcessSideDetailsVM
    {
        /// <summary>
        /// Идентификатор на страна
        /// </summary>
        public Guid Gid { get; set; }

        /// <summary>
        /// Длъжник вид:1-ФЛ,2-ЮЛ
        /// </summary>
        public int SubjectKind { get; set; }

        /// <summary>
        /// Длъжник идентификатор
        /// </summary>
        public string SideUic { get; set; }

        /// <summary>
        /// Длъжник имена
        /// </summary>
        public string SideName { get; set; }

        /// <summary>
        /// Длъжник роля в делото
        /// </summary>
        public string SideRoleCode { get; set; }

        /// <summary>
        /// Наименование на роля
        /// </summary>
        public string SideRoleName { get; set; }

        /// <summary>
        /// Вид страна: 1-Лява страна,2-Дясна страна
        /// </summary>
        public int SideType { get; set; }

        public string AddressTypeCode { get; set; }
        public string AddressTypeName { get; set; }
        public string CountryCode { get; set; }
        public string Address { get; set; }
    }

    public class ExecProcessAccessVM
    {
        public Guid Gid { get; set; }
        public DateTime CreateDate { get; set; }
        public string AccessKey { get; set; }
        public string Creator { get; set; }
        public string UserName { get; set; }
        public bool IsClaimed { get; set; }
        public bool IsActive { get; set; }
        public DateTime? DateClaimed { get; set; }
    }

    public class ExecCaseVM
    {
        public Guid Gid { get; set; }

        public string ExecCaseNumber { get; set; }
        public DateTime ExecCaseDate { get; set; }
        public string ExecUserName { get; set; }
        public string ExecCaseState { get; set; }
    }

    public class ExecCaseDetailsVM
    {
        public Guid Gid { get; set; }

        public string Number { get; set; }
        public DateTime DateRegister { get; set; }
        public string UserName { get; set; }
        public string StateCode { get; set; }
        public string StateName { get; set; }

        /// <summary>
        /// Дата на връчване на Изпълнителен лист
        /// </summary>
        public DateTime? ListDeliveryDate { get; set; }

        /// <summary>
        /// Дата на пренасочване по чл.427
        /// </summary>
        public DateTime? Redirect427Date { get; set; }

        /// <summary>
        /// Дата на прекратяване на Изпълнителен лист
        /// </summary>
        public DateTime? ListTerminateDate { get; set; }

        /// <summary>
        /// Дата на прекратяване на Изпълнителното дело
        /// </summary>
        public DateTime? CaseTerminateDate { get; set; }

        public ExecCasePaymentDetailsVM[] Payments { get; set; }

        public SummaryFile[] Files { get; set; }
    }

    /// <summary>
    /// ServiceModel - Плащане по изпълнително дело
    /// </summary>
    public class ExecCasePaymentDetailsVM
    {
        /// <summary>
        /// Идентификатор на плащането
        /// </summary>
        public Guid Gid { get; set; }

        /// <summary>
        /// Идентификатор на задължението
        /// </summary>
        public Guid ObligationGid { get; set; }

        /// <summary>
        /// Идентификатор на страна-длъжник
        /// </summary>
        public Guid DebtorGid { get; set; }

        /// <summary>
        /// Дата на плащане
        /// </summary>
        public DateTime PaymentDate { get; set; }

        /// <summary>
        /// Размер на плащане
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Валута
        /// </summary>
        public string CurrencyCode { get; set; }

        /// <summary>
        /// Забележка
        /// </summary>
        public string Description { get; set; }

    }

    public class ExecProcessAccessChange
    {
        /// <summary>
        /// Идентификатор на партида
        /// </summary>
        public Guid ExecProcessGid { get; set; }

        /// <summary>
        /// Идентификатор на достъп
        /// </summary>
        public Guid? Gid { get; set; }

        /// <summary>
        /// Лице, издало/премахнало кода за достъп. Съдия, постановил акта
        /// </summary>
        public string UserName { get; set; }
    }

    public class CorrectedActVM
    {
        public Guid Gid { get; set; }
        public string ActKind { get; set; }
        public int? ActNumber { get; set; }
        public DateTime ActDate { get; set; }
        public Guid? ActFileId { get; set; }
    }

}
