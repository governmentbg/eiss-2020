using IOWebApplication.Infrastructure.Models.Regix.GetStateOfPlay;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.RegixReport
{
    public class RegixReportListVM
    {
        public int Id { get; set; }

        [Display(Name = "Вид справка")]
        public string RegixTypeName { get; set; }

        [Display(Name = "Потребител")]
        public string UserName { get; set; }

        [Display(Name = "Дело")]
        public string CaseRegNumber { get; set; }

        [Display(Name = "Документ")]
        public string DocumentNumber { get; set; }

        [Display(Name = "Критерии")]
        public string RequestData {
            get
            {
                return (Request.UIC ?? "") + (Request.IdentityFilter ?? "") + (Request.EgnFilter ?? "") +
                       (Request.IdentifierFilter ?? "") + (Request.EGN ?? "");
            }
        }

        [Display(Name = "Дата и час")]
        public DateTime DateWrt { get; set; }

        public string RequestRemark { get; set; }

        public string RegixRequestTypeName { get; set; }

        public RegixReportListRequestVM Request { get; set; }
    }

    public class RegixReportListFilterVM
    {
        [Display(Name = "От дата")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "Изберете потребител")]
        public string UserId { get; set; }

        [Display(Name = "Вид справка")]
        public int RegixTypeId { get; set; }

        [Display(Name = "Търсене от")]
        public int RegixRequestTypeId { get; set; }
    }

    public class RegixReportListRequestVM
    {
        public string UIC { get; set; }
        public string IdentityFilter { get; set; }
        public string EgnFilter { get; set; }
        public string IdentifierFilter { get; set; }
        public string EGN { get; set; }
    }
}
