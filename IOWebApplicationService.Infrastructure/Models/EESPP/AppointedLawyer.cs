using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplicationService.Infrastructure.Models.EESPP
{
    public class AppointedLawyer
    {
        /// <summary>
        /// Личен номер на адвоката
        /// </summary>
        public long LawyerID { get; set; }

        /// <summary>
        /// Имена на адвоката
        /// </summary>
        public string LawyerName { get; set; }

        /// <summary>
        /// ID на уведомително писмо
        /// </summary>
        public string NotificationID { get; set; }

        /// <summary>
        /// Уведомително писмо файл - име на файла
        /// </summary>
        public string NotifyFileName1 { get; set; }

        /// <summary>
        /// Уведомително писмо файл {Base64}
        /// </summary>
        public string NotifyFile1 { get; set; }

        /// <summary>
        /// Списък на идентификатори на лица
        /// </summary>
        public string[] Clients { get; set; }
    }
}
