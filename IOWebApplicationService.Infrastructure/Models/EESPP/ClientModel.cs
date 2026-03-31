using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IOWebApplicationService.Infrastructure.Models.EESPP
{
    public class ClientModel
    {
        /// <summary>
        /// Идентификатор на лицето
        /// </summary>
        public string ID { get; set; }

        [NotMapped]
        public int CaseLawyerHelpPersonId { get; set; }

        /// <summary>
        /// Имена на лицето
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// ЕГН
        /// </summary>
        public string EIN { get; set; }

        /// <summary>
        /// Адрес на лицето
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Процесуално качество
        /// </summary>
        public string SystemNumber { get; set; }

        /// <summary>
        /// Избран адвокат
        /// </summary>
        public string Lawyer { get; set; }
    }
}
