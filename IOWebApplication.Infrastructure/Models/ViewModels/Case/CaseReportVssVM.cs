using IOWebApplication.Infrastructure.Attributes;
using IOWebApplication.Infrastructure.Data.Models.Cases;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class CaseReportVss:BaseInfo_Case
    {

        //public int Id { get; set; }


        //public int CourtId { get; set; }

        //public long DocumentId { get; set; }


        //public int? ProcessPriorityId { get; set; }


        //public string EISSPNumber { get; set; }

        ///// <summary>
        ///// Кратък 5 цифрен номер на делото
        ///// </summary>

        //public string ShortNumber { get; set; }


        //public int? ShortNumberValue { get; set; }

        ///// <summary>
        /////Пълен 14 цифрен номер на делото
        ///// </summary>

        //public string RegNumber { get; set; }

        //public DateTime RegDate { get; set; }


        //public bool? IsOldNumber { get; set; }



        //public int CaseGroupId { get; set; }


        //public int CaseCharacterId { get; set; }


        //public int CaseTypeId { get; set; }


        //public int? CaseCodeId { get; set; }

        ///// <summary>
        ///// Съдебна група за разпределяне
        ///// </summary>

        //public int? CourtGroupId { get; set; }

        ///// <summary>
        ///// Група по натовареност, за всички без ВКС
        ///// </summary>

        //public int? LoadGroupLinkId { get; set; }

        ///// <summary>
        ///// Само за ВКС, ръчна, фактическа сложност на делото

        //public decimal ComplexIndex { get; set; }


        //public int? CaseReasonId { get; set; }



        //public int? CaseTypeUnitId { get; set; }



        //public decimal LoadIndex { get; set; }



        //public decimal? CorrectionLoadIndex { get; set; }


        //public bool IsRestictedAccess { get; set; }

        //public string Description { get; set; }


        //public int CaseStateId { get; set; }

        ///// <summary>
        ///// Дата на влизане в законна сила

        //public DateTime? CaseInforcedDate { get; set; }

        ///// <summary>
        ///// Делото е по несъстоятелност

        //public bool? IsISPNcase { get; set; }

        //public string CaseStateDescription { get; set; }

        ///// <summary>
        ///// Върнато за ново разглеждане под нов номер
        ///// </summary>

        //public bool? IsNewCaseNewNumber { get; set; }
        /// ////////////////////////////////////////////////////////
        /// \\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\

        [Display(Name = "Съдия докладчик")]
    public int? JudgeReporterId { get; set; }

    [Display(Name = "Несвършени дела в началото на отчетния период")]
    public bool NotFinishedPreviousPeriod { get; set; }
    [Display(Name = "Постъпили дела през отчетния период")]
    public bool StartedInPeriod { get; set; }

    [Display(Name = "Решени по същество")]
    public bool FinishedByDecision { get; set; }
    [Display(Name = "Прекратени")]
    public bool FinishedByCanceling { get; set; }

    [Display(Name = "Свършени в тримесечен срок")]
    public bool FinishedInThreeMonths { get; set; }

    [Display(Name = "Продължителност в месеци")]
    public int ? CaseDurationMonths { get; set; }

    [Display(Name = "Тип на документа")]
    public int ? DocumentTypeId { get; set; }
    [Display(Name = "Група по натоварване")]
    public int ? LoadGrouId { get; set; }
    
  }
}

