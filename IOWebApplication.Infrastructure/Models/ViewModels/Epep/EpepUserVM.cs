using IOWebApplication.Infrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Epep
{
    public class EpepUserVM
    {
        public int Id { get; set; }
        public string UserTypeName { get; set; }
        public int EpepUserTypeId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string LawyerNumber { get; set; }
    }

    public class EpepUserFilterVM
    {
        [Display(Name = "Вид потребител")]
        public int? EpepUserTypeId { get; set; }
        [Display(Name = "Имена")]
        public string FullName { get; set; }
        [Display(Name = "Електронна поща")]
        public string Email { get; set; }

        public void UpdateNullables()
        {
            EpepUserTypeId = EpepUserTypeId.EmptyToNull();
            FullName = FullName.EmptyToNull();
            Email = Email.EmptyToNull();
        }
    }
}
