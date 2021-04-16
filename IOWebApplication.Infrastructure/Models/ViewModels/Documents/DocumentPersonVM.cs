using IOWebApplication.Infrastructure.Data.Models.Base;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class DocumentPersonVM : PersonNamesBase
    {
        public bool CanChange { get; set; }
        public long Id { get; set; }
        public int Index { get; set; }
        [Display(Name = "Вид лице")]
        [Range(1, int.MaxValue, ErrorMessage = "Изберете {0}.")]
        public int PersonRoleId { get; set; }

        [Display(Name = "Характер на лицето")]
        public int? PersonMaturityId { get; set; }

        [Display(Name = "Военно звание")]
        public int? MilitaryRangId { get; set; }
        public IList<DocumentPersonAddressVM> Addresses { get; set; }

        public DocumentPersonVM()
        {
            Addresses = new List<DocumentPersonAddressVM>();
            CanChange = true;
        }

        public string GetPrefix
        {
            get
            {
                return nameof(DocumentVM.DocumentPersons);
            }
        }
        public string GetPath
        {
            get
            {
                return string.Format("{0}[{1}]", this.GetPrefix, Index);
            }
        }

    }

}
