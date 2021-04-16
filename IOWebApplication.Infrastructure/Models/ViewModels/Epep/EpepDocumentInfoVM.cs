using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Epep
{
    public class EpepDocumentInfoVM
    {
        [Display(Name ="Регистрирано в")]
        public string CourtName { get; set; }
        public int CourtId { get; set; }
        [Display(Name ="Документ")]
        public string DocumentInfo { get; set; }
        public long DocumentId { get; set; }
    }
}
