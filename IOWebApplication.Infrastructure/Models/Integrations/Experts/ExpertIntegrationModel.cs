using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.Integrations.Experts
{
    public class ExpertIntegrationModel
    {
        public string FullName { get; set; }

        public List<CompetenceIntegrationModel> Competences { get; set; }

        public List<string> CourtRegions { get; set; }
    }
}
