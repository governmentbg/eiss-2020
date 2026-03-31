using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace IOWebApplicationService.Infrastructure.Data.Models.Base
{
    public class DWUserDateWRT : DWCourt
    {
        [Column("user_id")]
        public string UserId { get; set; }

        [Column("user_name")]
        public string UserName { get; set; }
        [Column("date_wrt")]
        public DateTime DateWrt { get; set; }

        [Column("date_transfered_dw")]
        public DateTime? DateTransferedDW { get; set; }



    }
}
