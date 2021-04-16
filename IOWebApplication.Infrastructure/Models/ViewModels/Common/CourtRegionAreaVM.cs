using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class CourtRegionAreaVM
    {
        public int Id { get; set; }
        public string CourtRegionLabel { get; set; }
        public string DistrictCode { get; set; }
        public string DistrictName { get; set; }
        public string MunicipalityCode { get; set; }
        public string MunicipalityName { get; set; }
    }
}
