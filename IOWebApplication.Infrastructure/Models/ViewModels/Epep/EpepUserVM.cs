using IOWebApplication.Infrastructure.Extensions;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Epep
{
    public class EpepUserVM
    {
        public int Id { get; set; }
        public string UserTypeName { get; set; }
        public int EpepUserTypeId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        [JsonIgnore]
        public string EGN { get; set; }
        [JsonIgnore]
        public string LawyerNumber { get; set; }

        public string PersonNumber
        {
            get
            {
                if (!string.IsNullOrEmpty(LawyerNumber))
                {
                    return LawyerNumber;
                }
                return EGN;
            }
        }

        public bool CanSummon { get; set; }
        public string RoleType { get; set; }
    }

    public class EpepUserFilterVM
    {
        [Display(Name = "Вид потребител")]
        public int? EpepUserTypeId { get; set; }
        [Display(Name = "Имена")]
        public string FullName { get; set; }
        [Display(Name = "ЕГН/Номер")]
        public string PersonNumber { get; set; }
        [Display(Name = "Електронна поща")]
        public string Email { get; set; }

        public void UpdateNullables()
        {
            EpepUserTypeId = EpepUserTypeId.EmptyToNull();
            PersonNumber = PersonNumber.EmptyToNull();
            FullName = FullName.EmptyToNull();
            Email = Email.EmptyToNull();
        }
    }
}
