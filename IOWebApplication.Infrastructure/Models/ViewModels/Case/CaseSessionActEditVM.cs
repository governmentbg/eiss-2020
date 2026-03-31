// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Base;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseSessionActEditVM : UserDateWRT
    {

        public int Id { get; set; }

        public int? CourtId { get; set; }

        public int? CaseId { get; set; }

        public int CaseSessionId { get; set; }

        /// <summary>
        /// Вид на документ
        /// </summary>
        [Display(Name = "Тип")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете {0}.")]
        public int ActTypeId { get; set; }

        /// <summary>
        /// Под-вид на документ
        /// </summary>
        [Display(Name = "Вид")]
        public int? ActKindId { get; set; }

        /// <summary>
        /// Вид на резултат
        /// </summary>
        [Display(Name = "Резултат от обжалване")]
        public int? ActResultId { get; set; }

        /// <summary>
        /// По съд, основен вид дело, вид акт
        /// </summary>
        [Display(Name = "Рег. номер")]
        public string RegNumber { get; set; }

        [Display(Name = "Дата на регистрация")]
        public DateTime? RegDate { get; set; }

        /// <summary>
        /// Дали е финализиращ документ, да генерира ЕКЛИ код
        /// </summary>
        [Display(Name = "Финализиращ акт")]
        public bool IsFinalDoc { get; set; }

        /// <summary>
        /// Дали е готов за публикуване
        /// </summary>
        [Display(Name = "Готово за публикуване")]
        public bool IsReadyForPublish { get; set; }

        [Display(Name = "ECLI код")]
        public string EcliCode { get; set; }

        /// <summary>
        /// Дата на постановяване
        /// </summary>
        [Display(Name = "Дата")]
        public DateTime? ActDate { get; set; }

        /// <summary>
        /// Дата на обявяване на акта: подписване от последния съдия
        /// </summary>
        [Display(Name = "Дата на постановяване")]
        public DateTime? ActDeclaredDate { get; set; }

        /// <summary>
        /// Срок в който е постановен акта
        /// </summary>
        [Display(Name = "Срок в който е постановен акта")]
        public int? DeclaredMonthCount { get; set; }

        /// <summary>
        /// Дата на обявяване на мотивите: подписване от последния съдия
        /// </summary>
        [Display(Name = "Дата на обявяване на мотивите")]
        public DateTime? ActMotivesDeclaredDate { get; set; }

        /// <summary>
        /// Дата на влизане в сила на акта: след приключване на обжалването
        /// </summary>
        [Display(Name = "Дата на влизане в сила на акта")]
        public DateTime? ActInforcedDate { get; set; }

        /// <summary>
        /// Диспозитив
        /// </summary>
        [Display(Name = "Диспозитив")]
        public string Description { get; set; }

        /// <summary>
        /// В процес на изготвяне,изготвен, обявен
        /// </summary>
        [Display(Name = "Статус")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете {0}.")]
        public int ActStateId { get; set; }

        [Display(Name = "Секретар")]
        public string SecretaryUserId { get; set; }
        public string SecretaryUserId_list { get; set; }
        [Display(Name = "Секретар")]
        public string SecretaryUserNamesList { get; set; }

        /// <summary>
        /// Потребител, създал бланката на акта
        /// </summary>
        public string ActCreatorUserId { get; set; }

        /// <summary>
        /// Потребител, създал бланката на мотивите
        /// </summary>
        public string MotiveCreatorUserId { get; set; }

        /// <summary>
        /// Потребител обезличил акта
        /// </summary>
        public string DepersonalizeUserId { get; set; }

        [Display(Name = "Подлежи на обжалване")]
        public bool? CanAppeal { get; set; }

        [Display(Name = "Резултат/степен на уважаване на иска")]
        public int? ActComplainResultId { get; set; }

        [Display(Name = "Индекс на резултат от обжалване")]
        public int? ActComplainIndexId { get; set; }

        [Display(Name = "За срок")]
        public string ActTerm { get; set; }

        [Display(Name = "Правно основание")]
        public int? ActISPNReasonId { get; set; }

        [Display(Name = "Статус на длъжник")]
        public int? ActISPNDebtorStateId { get; set; }

        [Display(Name = "Свързан съдебен акт")]
        public int? RelatedActId { get; set; }

        /// <summary>
        /// Дата на връщане
        /// </summary>
        [Display(Name = "Дата на връщане")]
        public DateTime? ActReturnDate { get; set; }

        /// <summary>
        /// Дата на начало на обезличаването
        /// </summary>
        public DateTime? DepersonalizeStartDate { get; set; }

        /// <summary>
        /// Дата на финализиране на обезличаването
        /// </summary>
        public DateTime? DepersonalizeEndDate { get; set; }

        /// <summary>
        /// Дата на финализиране на обезличаването на мотивите
        /// </summary>
        public DateTime? DepersonalizeMotiveEndDate { get; set; }

        /// <summary>
        /// Потребител обезличил мотивите
        /// </summary>
        public string DepersonalizeMotiveUserId { get; set; }


        /// <summary>
        /// Съдия, подписващ изпълнителен лист
        /// </summary>
        [Display(Name = "Съдия")]
        public int? SignJudgeLawUnitId { get; set; }

        /// <summary>
        /// Страни в бланката
        /// </summary>
        [Display(Name = "Страни в бланката")]
        public int? ActDirectionId { get; set; }

        /// <summary>
        /// Вписване/обявяване в АИСТН
        /// </summary>
        [Display(Name = "Вписване/обявяване в АИСТН")]
        public bool? TDActForRegistration { get; set; }

        [Display(Name = "Дата на анулиране")]
        public DateTime? DateExpired { get; set; }

        [Display(Name = "Генериране на партида")]
        public bool GenerateExecProcess { get; set; }

        /// <summary>
        /// Срок за обжалване
        /// </summary>
        [Display(Name = "Срок за обжалване")]
        public DateTime? AppealNotificationStartFastProcess { get; set; }

        /// <summary>
        /// Нотификация след дни - обжалване
        /// </summary>
        [Display(Name = "Нотификация след дни - обжалване")]
        public int? AppealNotificationDaysFastProcess { get; set; }

        /// <summary>
        /// Нотификация след седмици - обжалване
        /// </summary>
        [Display(Name = "Нотификация след седмици - обжалване")]
        public int? AppealNotificationWeeksFastProcess { get; set; }

        /// <summary>
        /// Нотификация след месеци - обжалване
        /// </summary>
        [Display(Name = "Нотификация след месеци - обжалване")]
        public int? AppealNotificationMontsFastProcess { get; set; }

        /// <summary>
        /// Да се създаде нотификация
        /// </summary>
        [Display(Name = "Нотификация след връчване")]
        public bool? NotificationOn { get; set; }

        /// <summary>
        /// Нотификация след дни
        /// </summary>
        [Display(Name = "Нотификация след дни")]
        public int? NotificationDays { get; set; }

        /// <summary>
        /// Нотификация след седмици - обжалване
        /// </summary>
        [Display(Name = "Нотификация след седмици")]
        public int? NotificationWeeks { get; set; }

        /// <summary>
        /// Нотификация след месеци
        /// </summary>
        [Display(Name = "Нотификация след месеци")]
        public int? NotificationMonts { get; set; }

        /// <summary>
        /// Да се създаде нотификация изтекъл, определения от потребителя, срок от постановяване на акт
        /// </summary>
        [Display(Name = "Нотификация от постановяване на акт")]
        public bool? ActNotificationOn { get; set; }

        /// <summary>
        /// Нотификация за изтекъл, определения от потребителя, срок от постановяване на акт след дни
        /// </summary>
        [Display(Name = "След дни")]
        public int? ActNotificationDays { get; set; }

        /// <summary>
        /// Нотификация за изтекъл, определения от потребителя, срок от постановяване на акт след седмици - обжалване
        /// </summary>
        [Display(Name = "След седмици")]
        public int? ActNotificationWeeks { get; set; }

        /// <summary>
        /// Нотификация за изтекъл, определения от потребителя, срок от постановяване на акт след месеци
        /// </summary>
        [Display(Name = "След месеци")]
        public int? ActNotificationMonts { get; set; }

        /// <summary>
        /// Описани към нотификация
        /// </summary>
        [Display(Name = "Описание")]
        public string ActNotificationDescription { get; set; }

        [Display(Name = "Коригиране на акт (ОФГ)")]
        public bool HasCorrectedAct { get; set; }

        [Display(Name = "Коригирани съдебни актове")]
        public string[] CorrectedActsIds { get; set; }

        [Display(Name = "Незабавно изпълнение на акта")]
        public bool? RnflEffectiveImmediately { get; set; }
    }
}
