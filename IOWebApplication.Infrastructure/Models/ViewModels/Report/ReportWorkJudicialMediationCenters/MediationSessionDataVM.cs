// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Report.ReportWorkJudicialMediationCenters
{
    /// <summary>
    /// Модел за извличане на данни за срещи за отчет за работата на съдебните центрове по медиация
    /// </summary>
    public class MediationSessionDataVM
    {
        /// <summary>
        /// Идентификатор на дело
        /// </summary>
        public int CaseId { get; set; }

        /// <summary>
        /// Идентификатор на вид среща: Информационна среща; Процедура по медиация.
        /// </summary>
        public int MediationTypeId { get; set; }

        /// <summary>
        /// Начало: дата и час
        /// </summary>
        public DateTime DateFrom { get; set; }

        /// <summary>
        /// Край: дата и час
        /// </summary>
        public DateTime? DateTo { get; set; }

        /// <summary>
        /// Идентификатор на вид избор на медиатора в делото
        /// </summary>
        public int? MediationTypeChoiceMediatorId { get; set; }

        /// <summary>
        /// Флаг дали има резултат в срещата с прекратителна група 
        /// </summary>
        public bool TerminationMediationResultGroup {  get; set; }

        /// <summary>
        /// Флаг дали има резултат в срещата по делото с прекратителна група 
        /// </summary>
        public bool CaseTerminationMediationResultGroup { get; set; }

        /// <summary>
        /// Прекратяване на дело със спогодба
        /// </summary>
        public bool TerminationCaseSettlement { get; set; }

        /// <summary>
        /// Прекратяване на дело поради оттегляне или отказ
        /// </summary>
        public bool TerminationCaseTerminationWithdrawalRefusal { get; set; }

        /// <summary>
        /// Частично прекратяване на дело със спогодба
        /// </summary>
        public bool PartiallyTerminationCaseSettlement { get; set; }

        /// <summary>
        /// Частично прекратяване на дело поради оттегляне или отказ
        /// </summary>
        public bool PartiallyTerminationCaseTerminationWithdrawalRefusal { get; set; }

        /// <summary>
        /// Оценки в срещата
        /// </summary>
        public List<AppraisalDataVM> Appraisals { get; set; }
    }

    /// <summary>
    /// Обект за оценки
    /// </summary>
    public class AppraisalDataVM
    {
        /// <summary>
        /// Сбор от оценките
        /// </summary>
        public int SumAppraisal { get; set; }

        /// <summary>
        /// Обща оценка
        /// </summary>
        public int SumAppraisalTotal
        {
            get
            {
                if (SumAppraisal <= 14)
                    return 1;
                if (SumAppraisal >= 15 && SumAppraisal <= 28)
                    return 2;
                if (SumAppraisal >= 29 && SumAppraisal <= 42)
                    return 3;
                if (SumAppraisal >= 43 && SumAppraisal <= 56)
                    return 4;
                if (SumAppraisal >= 57)
                    return 5;

                return 1;
            }
        }

        /// <summary>
        /// Обща оценка текст
        /// </summary>
        public string SumAppraisalTotalText
        {
            get
            {
                if (SumAppraisal <= 14)
                    return "Лоша";
                if (SumAppraisal >= 15 && SumAppraisal <= 28)
                    return "Задоволителна";
                if (SumAppraisal >= 29 && SumAppraisal <= 42)
                    return "Добра";
                if (SumAppraisal >= 43 && SumAppraisal <= 56)
                    return "Много добра";
                if (SumAppraisal >= 57)
                    return "Отлична";

                return "Лоша";
            }
        }
    }
}
