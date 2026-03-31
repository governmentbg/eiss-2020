using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels
{
    public class FastProcessSelectionCourtReportVM
    {

   
        public List<FastProcessSelectionCourtReportRowVM> rows { get; set; }
        public List<int> listOfDates { get; set; }


        public FastProcessSelectionCourtReportVM()
        {
            rows = new List<FastProcessSelectionCourtReportRowVM>();
            listOfDates = new List<int>();
        }


    }


}
