using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Eispp
{
    public class EisppEventItemVM
    {
        public int Id { get; set; }

        /// <summary>
        /// Дата на събитие
        /// </summary>
        [Display(Name = "От дата")]
        public DateTime? EventDate { get; set; }

        /// <summary>
        /// Събитие 
        /// </summary>
        [Display(Name = "Събитие")]
        public string EventTypeName { get; set; }

        /// <summary>
        /// Дата на събитие
        /// </summary>
        [Display(Name = "Връзка")]
        public string EventLink { get; set; }

        /// <summary>
        /// Акт/Протокол
        /// </summary>
        [Display(Name = "Акт/Протокол")]
        public string SessionAct { get; set; }

        /// <summary>
        /// Лице за което се отнася събитието
        /// само когато събитието е за едно лице
        /// </summary>
        [Display(Name = "Лице")]
        public string PersonName { get; set; }

        /// <summary>
        /// Описание на грешка
        /// </summary>
        [Display(Name = "Описание на грешка")]
        public string ErrorDescription { get; set; }

        /// <summary>
        /// Статус трансфер
        /// </summary>
        [Display(Name = "Статус трансфер")]
        public string StatusTransfer { get; set; }

        /// <summary>
        /// Към дело номер
        /// </summary>
        public string CaseRegNum { get; set; }

        /// <summary>
        /// Към дело номер
        /// </summary>
        public DateTime? CaseRegDate { get; set; }


        [Display(Name = "Дата на анулиране")]
        public DateTime? DateExpired { get; set; }

        public long? MqEpepId { get; set; }

        public int IntegrationStateId { get; set; }
        public int? EventFromId { get; set; }
        public bool CanExpireError { get; set; }
    }
}
