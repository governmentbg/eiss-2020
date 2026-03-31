using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class CaseSessionActSelectionActVM
    {



        public int Id { get; set; }

        public int SelectedLawUnitId { get; set; }

        [Display(Name = "Съдия-докладчик")]
        public string SelectedLawUnitName { get; set; }

        public int ActTypeId { get; set; }
        public string ActTypeLabel { get; set; }

        public string Vid_RegNumber { get; set; }

        public DateTime? RegDate { get; set; }


        public string CaseType_CaseLabel { get; set; }

        public int CaseSessionId { get; set; }
      
 

        public DateTime? ActInforcedDate { get; set; }

        public string Result { get; set; }
        public int? ResultId { get; set; }

        public string JudgeReport { get; set; }
  
        public bool IsFinal { get; set; }
        public string IsFinalStr { get; set; }
        public bool IsCanceling { get; set; }
        public String IsCancelingstr { get; set; }
        public bool IsBeacameFinal { get; set; }
        public string IsBeacameFinalStr { get; set; }

        public bool IsSelected { get; set; }




    }
}
