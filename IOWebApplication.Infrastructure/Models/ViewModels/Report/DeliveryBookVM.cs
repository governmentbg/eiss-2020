using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Report
{
    public class DeliveryBookVM
    {
        public long Id { get; set; }

        public string DocumentNumber { get; set; }

        public DateTime DocumentDate { get; set; }

        [Display(Name = "Лицето/институцията, на която се връчва делото/документа")]
        public string DocumentPersonName { get; set; }

        public string Description { get; set; }

        [Display(Name = "Вид на документа")]
        public string DocumentGroupName { get; set; }

        [Display(Name = "Дата на връчване")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? DocumentLinkDate { get; set; }

        [Display(Name = "Лице, упълномощено да получи документ")]
        public string DocumentLinkUser { get; set; }

        public int DocumentNumberValue { get; set; }

        [Display(Name = "Номер и дата на документ")]
        public string DocumentNumberDate { get { return this.DocumentNumber + "/" + this.DocumentDate.ToString("dd.MM.yyyy"); } }

        [Display(Name = "Основен вид дело")]
        public string CaseGroupName { get; set; }

        [Display(Name = "Номер дело")]
        public string CaseNumber { get; set; }

        [Display(Name = "Дата на дело")]
        [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy}", ApplyFormatInEditMode = true)]
        public DateTime? CaseDate { get; set; }
    }

    public class DeliveryBookFilterVM
    {
        [Display(Name = "От дата")]
        public DateTime DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime DateTo { get; set; }

        [Display(Name = "Основен вид дело")]
        public int CaseGroupId { get; set; }

        [Display(Name = "Точен вид дело")]
        public int CaseTypeId { get; set; }

        [Display(Name = "Лице/Институция")]
        public string CasePersonName { get; set; }

    }
}
