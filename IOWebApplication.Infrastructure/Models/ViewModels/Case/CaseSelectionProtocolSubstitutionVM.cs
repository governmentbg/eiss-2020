using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class CaseSelectionProtocolSubstitutionVM
    {

            public int Id { get; set; }
            public int ? SelectedLawUnitId { get; set; }

            [Display(Name = "Избран заместващ съдия:")]
            public string SelectedLawUnitName { get; set; }
       
         public string CourtName { get; set; }
        [Display(Name = "Група за разпределние:")]
        public string CourtGroupName { get; set; }
        [Display(Name = "Начин на разпределение:")]
        public string SelectionTypeName { get; set; }

        [Display(Name = "Причина за заместване:")]
        public string Description { get; set; }
        public int? SubstitudedLawUnitId { get; set; }
  
            [Display(Name = "Отсъстващ съдия:")]
           public string SubstitutedLawUnitName { get; set; }


            [Display(Name = "От дата")]
            public DateTime SubstitutionDateFrom { get; set; }

            [Display(Name = "До дата")]
            public DateTime SubstitutionDateTo { get; set; }

            [Display(Name = "Дата на разпределението:")]
            public DateTime SelectionDate { get; set; }
        [Display(Name = "Дата на посписване:")]
        public DateTime DeclareDate { get; set; }

        public int SelectionProtocolStateId { get; set; }
        public string SelectionProtocolStateName { get; set; }

        [Display(Name = "Извършил разпределението:")]
        public string UserName { get; set; }
        public string UserUIK { get; set; }

        public IList<CaseSelectionProtokolLawUnitVM> ProtokolLawUnit { get; set; }

        public CaseSelectionProtocolSubstitutionVM()
        {
            ProtokolLawUnit = new List<CaseSelectionProtokolLawUnitVM>();
        }
    }


}

