using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseLawUnitManualJudgeVM
    {
        public int Id { get; set; }
        public int CaseId { get; set; }

        [Display(Name = "Съдебно дело")]
        public string CaseNumber { get; set; }

        [Display(Name = "Дата на промяната")]
        public DateTime ChangeDate { get; set; }

        [Display(Name = "Съдия")]
        public string LawUnitName { get; set; }

        [Display(Name = "Роля")]
        public string JudgeRoleName { get; set; }

        [Display(Name = "Основание")]
        public string Description { get; set; }

        [Display(Name = "Промяната извършена от")]
        public string ChangeUserName { get; set; }
    }
}
