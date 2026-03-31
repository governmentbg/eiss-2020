// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplication.Infrastructure.Models.Integrations.EpepRest
{
    public class ExecProcess
    {
        /// <summary>
        /// Тип процес: 1-Legacy Изпълнителни листа,2-ИЛ ЗП
        /// </summary>
        public int ProcessKind { get; set; }

        /// <summary>
        /// Идентификатор на Изпълнителен лист
        /// </summary>
        public Guid ListActId { get; set; }

        /// <summary>
        /// Идентификатор на Заповед за изпълнение
        /// </summary>
        public Guid? OrderActId { get; set; }

        /// <summary>
        /// Солидарно разпределение
        /// </summary>
        public bool JointDistribution { get; set; }

        /// <summary>
        /// Лице, издало кода за достъп. Съдия, постановил акта
        /// </summary>
        public string CodeCreator { get; set; }

        /// <summary>
        /// Списък на страни по партидата
        /// </summary>
        public ExecProcessSideModel[] Sides { get; set; }

        /// <summary>
        /// Списък на задълженията, предмет на изпълнителния лист
        /// </summary>
        public ExecProcessObligationModel[] Obligations { get; set; }
    }


    public class ExecProcessObligationModel
    {
        /// <summary>
        /// Идентификатор на лице
        /// </summary>
        public Guid Gid { get; set; }

        /// <summary>
        /// Данни за взискател, получател на парите
        /// </summary>
        public Guid BeneficiaryGid { get; set; }

        /// <summary>
        /// Вид задължение
        /// </summary>
        public string ObligationTypeCode { get; set; }

        /// <summary>
        /// Сума
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Валута
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
        /// Списък на длъжници
        /// </summary>
        public Guid[] Debtors { get; set; }
    }

    public class ExecProcessSideModel
    {
        [NotMapped]
        public int EissId { get; set; }

        [NotMapped]
        public bool IsDebtor { get; set; }

        /// <summary>
        /// Идентификатор на лице
        /// </summary>
        public Guid Gid { get; set; }

        /// <summary>
        /// Вид лице:1-ФЛ, 2-ЮЛ
        /// </summary>
        public int SubjectKind { get; set; }

        /// <summary>
        /// Код на роля на страна
        /// </summary>
        public string SideInvolvementKindCode { get; set; }

        /// <summary>
        /// Имена на лице, когато не е участник в дело
        /// </summary>
        public string SideUic { get; set; }

        /// <summary>
        /// Имена на лице, когато не е участник в дело
        /// </summary>
        public string SideName { get; set; }

        /// <summary>
        /// Вид адрес
        /// </summary>
        public string AddressTypeCode { get; set; }

        /// <summary>
        /// Код на държава, 2буквен
        /// </summary>
        public string CountryCode { get; set; }

        /// <summary>
        /// Адрес на длъжника
        /// </summary>
        public string Address { get; set; }
    }

    public class ExecProcessDeliveredListVM
    {
        public Guid ExecProcessGid { get; set; }
        public Guid ExecCaseGid { get; set; }
        public DateTime ListDeliveryDate { get; set; }
        public string CaseNumber { get; set; }
        public string CaseUser { get; set; }
    }
}
