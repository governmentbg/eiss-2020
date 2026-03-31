using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class CaseSessionActSelectionProtocolVM
    {

            public int Id { get; set; }
            public int SelectedLawUnitId { get; set; }

            [Display(Name = "Съдия-докладчик")]
            public string SelectedLawUnitName { get; set; }

            [Display(Name = "От дата")]
            public DateTime DateFrom { get; set; }

            [Display(Name = "До дата")]
            public DateTime DateTo { get; set; }

            [Display(Name = "Дата на протокола")]
            public DateTime SelectionDate { get; set; }

            [Display(Name = "Финализиращ акт")]

            public bool IsFinal { get; set; }

            public string IsFinalStr { get; set; }



             [Display(Name = "За прекратяване")]
            public bool IsCanceling { get; set; }

            public String IsCancelingstr { get; set; }


            [Display(Name = "Влязъл в сила")]
            public bool IsBeacameFinal { get; set; }

             public string IsBeacameFinalStr { get; set; }


        [Display(Name = "Резултат/степен на уважаване на иска")]
            public int? ActResultId { get; set; }

        public int SelectionProtocolStateId { get; set; }
        public string SelectionProtocolStateName { get; set; }
        public string UserName { get; set; }
        public string UserUIK { get; set; }

        public IList<CaseSessionActSelectionActVM> SelectionActs { get; set; }

        public CaseSessionActSelectionProtocolVM()
        {
            SelectionActs = new List<CaseSessionActSelectionActVM>();
        }
    }


}

