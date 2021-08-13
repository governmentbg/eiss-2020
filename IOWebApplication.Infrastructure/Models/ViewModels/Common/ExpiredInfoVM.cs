using IOWebApplication.Infrastructure.Attributes;
using IOWebApplication.Infrastructure.Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Common
{
    public class ExpiredInfoVM : IExpiredInfo
    {
        public int Id { get; set; }
        public long LongId { get; set; }
        public string StringId { get; set; }
        public string FileContainerName { get; set; }
        public DateTime? DateExpired { get; set; }
        public string UserExpiredId { get; set; }
        [Display(Name = "Причина за премахването")]
        [Required(ErrorMessage = "Въведете {0}.")]
        //[MaxWordLength(5, ErrorMessage = "Моля, използвайте разделители на отделните думи.")]
        public string DescriptionExpired { get; set; }

        public string ExpireSubmitUrl { get; set; }
        public string DialogTitle { get; set; }
        public string ReturnUrl { get; set; }
        public bool OtherBool { get; set; }
        public int? OtherId { get; set; }
    }
}
