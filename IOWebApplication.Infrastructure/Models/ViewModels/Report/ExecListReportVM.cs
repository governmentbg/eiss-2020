using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Report
{
    public class ExecListReportVM
    {
        public string ExecListNumber { get; set; }

        public string CaseRegNumber { get { return string.Join(Environment.NewLine, CaseRegNumbers.Distinct()); } }

        public string[] CaseRegNumbers { get; set; }

        public string CaseSessionActData { get { return string.Join(Environment.NewLine, CaseSessionActDatas.Distinct()); } }

        public string[] CaseSessionActDatas { get; set; }

        public string PersonName { get { return string.Join(Environment.NewLine, PersonNames.Distinct()); } }

        public string[] PersonNames { get; set; }

        public DateTime ExecListDate { get; set; }

        public string SendData { get; set; }

        public string Receiver { get; set; }

        public string ExecListCaseNumber { get; set; }

        public IEnumerable<ExecListObligationReportVM> obligations { get; set; }

        public ExecListReportVM()
        {
            obligations = new HashSet<ExecListObligationReportVM>();
        }

        public int ExecListStateId { get; set; }
    }

    public class ExecListFilterReportVM
    {
        [Display(Name = "От дата")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "До дата")]
        public DateTime? DateTo { get; set; }

        [Display(Name = "Основен вид дело")]
        public int CaseGroupId { get; set; }
    }

    public class ExecListObligationReportVM
    {
        public string MoneyTypeName { get; set; }

        public decimal Amount { get; set; }

        public decimal AmountBGN { get; set; }

        public string PaymentData { get; set; }

        public string PersonNameReceive { get; set; }
    }
}
