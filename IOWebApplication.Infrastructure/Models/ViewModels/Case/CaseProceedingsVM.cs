// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using IOWebApplication.Infrastructure.Data.Models.Cases;
using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseProceedingsVM
    {
        public int Id { get; set; }

        // Номер Дело
        public string RegNumber { get; set; }

        // Дата дело
        public DateTime RegDate { get; set; }

        // Основен вид дело
        public string CaseGroupLabel { get; set; }

        // Точен вид дело
        public string CaseTypeLabel { get; set; }

        // Шифър
        public string CaseCodeLabel { get; set; }

        // Статус
        public string CaseStateLabel { get; set; }

        // Основание за образуване
        public string CaseReasonLabel { get; set; }

        // Основание
        public string CaseStateDescription { get; set; }

        // Съдия докладчик
        public string JudgeRapporteur { get; set; }

        // Архивиране
        public string ArchRegNumber { get; set; }

        // Дата на архивиране
        public DateTime? ArchRegDate { get; set; }

        // Иницииращ документ
        public string DocumentLabel { get; set; }

        // Индикатори
        public virtual ICollection<CaseClassification> CaseClassifications { get; set; }

        // Страни
        public virtual ICollection<CasePersonListVM> CasePersons { get; set; }

        // Състав
        public virtual ICollection<CaseLawUnitVM> CaseLawUnits { get; set; }

        // Заседания, актове и документи
        public virtual ICollection<CaseProceedingsObjectsVM> CaseProceedingsObjects { get; set; }
    }
}
