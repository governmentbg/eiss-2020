namespace IOWebApplication.Infrastructure.Models.ViewModels.Case
{
    public class CaseLawyerHelpPersonVM
    {
        public int Id { get; set; }
        public string CasePersonText { get; set; }
        public string AssignedLawyerText { get; set; }
        public string SpecifiedLawyerLawUnitLabel { get; set; }
        public string CaseName { get; set; }

        public string DescriptionExpired { get; set; }
    }
}
