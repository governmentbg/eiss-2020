// Copyright (C) Information Services. All Rights Reserved.
// Licensed under the Apache License, Version 2.0

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseEvidenceSprVM
    {
        public int Id { get; set; }
        public int CaseId { get; set; }
        [Display(Name = "Дата и час на регистриране")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy HH:mm}", ApplyFormatInEditMode = true)]
        public DateTime DateAccept { get; set; }
        [Display(Name = "Номер на служебно дело")]
        public string FileNumber { get; set; }
        [Display(Name = "№ по ред")]
        public string RegNumber { get; set; }
        [Display(Name = "Номер на наказателно дело")]
        public string CaseNumber { get; set; }
        [Display(Name = "Вид НД")]
        public string CaseGroupLabel { get; set; }
        [Display(Name = "Трите имена на обвиняемия/подсъдимия")]
        public string NamePodsydim { get; set; }
        [Display(Name = "Описание на вещественото доказателство")]
        public string Description { get; set; }
        public DateTime DateSendOtherInstance { get; set; }
        public DateTime DateReceiveOtherInstance { get; set; }
        [Display(Name = "Разпоредителни действия(дата и основание за унищожаване, предаване на друга институция, връщане на физически лица)")]
        public string Movements { get; set; }
        public string EvidenceStateLabel { get; set; }
        public string EvidenceTypeLabel { get; set; }

        [Display(Name = "Дата на изпращане на друга инстанция")]
        public string MovementsDateSend { get; set; }
        [Display(Name = "Дата на получаване от друга инстанция")]
        public string MovementsDateReceive { get; set; }
    }
}
