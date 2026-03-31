using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class CourtJuryFeeListVM
    {
        public int Id { get; set; }

        public decimal HourFee { get; set; }

        public decimal HourFeeEUR { get; set; }

        public decimal MinDayFee { get; set; }

        public decimal MinDayFeeEUR { get; set; }

        public DateTime DateFrom { get; set; }

        public DateTime? DateTo { get; set; }

    }
}
