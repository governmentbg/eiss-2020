using Newtonsoft.Json;
using System;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseLawyerHelpVM
    {
        public int Id { get; set; }

        public int? CourtId { get; set; }

        public int CaseId { get; set; }
        public string LawyerHelpBaseText { get; set; }
        public string LawyerHelpTypeText { get; set; }
        public string CaseSessionActText { get; set; }

        [JsonIgnore]
        public DateTime DocumentDateFromDb { get; set; }
        public DateTime? DocumentDate
        {
            get
            {
                if (DocumentDateFromDb == DateTime.MinValue || DocumentDateFromDb.Year < 2000)
                {
                    return null;
                }
                return DocumentDateFromDb;
            }
        }
    }
}
