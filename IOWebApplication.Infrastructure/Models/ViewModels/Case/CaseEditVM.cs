using IOWebApplication.Infrastructure.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class CaseEditVM
    {
        public int Id { get; set; }
        public DateTime DateWrt { get; set; }

        [Display(Name = "Основен вид")]
        public int CaseGroupId { get; set; }

        [Display(Name = "Основен вид")]
        public string CaseGroupName { get; set; }

        [Display(Name = "ЕИСПП номер")]
        public string EISSPNumber { get; set; }

        [Display(Name = "Точен вид")]
        [IORequired]
        public int CaseTypeId { get; set; }

        [Display(Name = "Шифър")]
        [IORequired]
        public int CaseCodeId { get; set; }

        [Display(Name = "Шифър")]
        public string CaseCodeName { get; set; }

        [Display(Name = "Съдебна група за разпределяне")]
        [IORequired]
        public int? CourtGroupId { get; set; }

        [Display(Name = "Съдебна група за разпределяне")]
        [IORequired]
        public string CourtGroupName { get; set; }

        [Display(Name = "Група по натовареност")]
        [IORequired]
        public int? LoadGroupLinkId { get; set; }

        [Display(Name = "Сложност на делото")]
        public decimal ComplexIndex { get; set; }

        [Display(Name = "Фактическа сложност")]
        public int? ComplexIndexActual { get; set; }

        [Display(Name = "Правна сложност")]
        public int? ComplexIndexLegal { get; set; }

        public int CourtTypeId { get; set; }

        [Column("ispn_case_competence_id")]
        [Display(Name = "Компетентност за образуване на дело")]
        public int? IspnCaseCompetenceId { get; set; }


        [Display(Name = "Характер на делото")]
        [IORequired]
        public int CaseCharacterId { get; set; }

        [Display(Name = "Статус")]
        [IORequired]
        public int CaseStateId { get; set; }

        public int CourtId { get; set; }

        public long DocumentId { get; set; }

        [Display(Name = "Документ: ")]
        public string DocumentName { get; set; }

        public int DocumentTypeId { get; set; }

        public string DocumentTypeName { get; set; }

        public string RegNumber { get; set; }
        public string RegNumberText { get; set; }

        public DateTime RegDate { get; set; }

        [Display(Name = "Въвеждане на стар номер от друга система")]
        public bool IsOldNumber { get; set; }
        [Display(Name = "Стар кратък номер на дело")]
        [RegularExpression("[0-9]*$", ErrorMessage = "Невалиден {0}.")]
        public string OldNumber { get; set; }
        [Display(Name = "Дата дело")]
        public DateTime? OldDate { get; set; }

        [Display(Name = "Основание за образуване")]
        public int? CaseReasonId { get; set; }

        [Display(Name = "Описание")]
        public string Description { get; set; }


        //------------- Състав по делото --------------
        [Display(Name = "Състав по делото")]
        public int? CaseTypeUnitId { get; set; }

        [Display(Name = "Резервни участници")]
        public int? CaseTypeUnitReserves { get; set; }


        //[Display(Name = "Съдийски състав")]
        //public int JudgesCount { get; set; }
        //[Display(Name = "Заседатели")]
        //public int JuryCount { get; set; }
        //[Display(Name = "Резервен съдия")]
        //public int ReserveJudgesCount { get; set; }
        //[Display(Name = "Резервен заседател")]
        //public int ReserveJuryCount { get; set; }

        [Display(Name = "Влизане в законна сила")]
        public DateTime? CaseInforcedDate { get; set; }

        [Display(Name = "Вид производство")]
        [IORequired]
        public int? ProcessPriorityId { get; set; }

        [Display(Name = "Основание")]
        public string CaseStateDescription { get; set; }

        [Display(Name = "Върнато за ново разглеждане под нов номер")]
        public bool? IsNewCaseNewNumber { get; set; }

        [Display(Name = "Дело за възобновяване")]
        public bool? IsRenewCase { get; set; }

        [Display(Name = "Брой длъжници")]
        public int? DebtorsCount { get; set; }

        /// <summary>
        /// Изключва транзакцията при регистриране на електронен документ от Случайно разпределение, default = false
        /// </summary>
        public bool DisableTransaction { get; set; } = false;

        /// <summary>
        /// id на изходящо движение за преразпределение
        /// </summary>
        public int? RandomAssignmentOutCaseMigrationId { get; set; }

        public bool IsFastProcess { get; set; }

        /// <summary>
        /// РНФЛ - Вид производство, по подразбиране - Главно
        /// </summary>
        [Display(Name = "Вид РНФЛ производство")]
        public int? RnflProcessTypeId { get; set; }

        public int? IspnKind { get; set; }

        public int RowVersion { get; set; }
    }
}
