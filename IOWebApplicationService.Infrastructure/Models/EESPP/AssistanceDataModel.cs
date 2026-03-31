using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.Converters.EESPP;
using Newtonsoft.Json;
using System;

namespace IOWebApplicationService.Infrastructure.Models.EESPP
{
    public class AssistanceDataModel
    {
        /// <summary>
        /// Уникален идентификатор на искането
        /// </summary>
        public string RequestId { get; set; }

        /// <summary>
        /// Адвокатска колегия
        /// </summary>
        public string LawyersAssociation { get; set; }

        /// <summary>
        /// Дата на искането
        /// </summary>
        [JsonConverter(typeof(JsonDateTimeConverter))]
        public DateTime RequestDate { get; set; }

        /// <summary>
        /// Изходящ номер на искането
        /// </summary>
        public string DocNo { get; set; }

        /// <summary>
        /// Дата на извеждане на искането с изходящ номер
        /// </summary>
        [JsonConverter(typeof(JsonDateTimeConverter))]
        public DateTime DocDate { get; set; }

        /// <summary>
        /// Основание за изпращане на искането
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// Вид правна помощ
        /// </summary>
        public string LawServiceType { get; set; }

        /// <summary>
        /// Съд, който подава искането
        /// </summary>
        public string JusticeCourtId { get; set; }

        /// <summary>
        /// Структурна единица на съда
        /// </summary>
        public string JusticeCourtDescr { get; set; }

        /// <summary>
        /// Вид дело
        /// </summary>
        public string CaseType { get; set; }

        /// <summary>
        /// Номер на дело
        /// </summary>
        public string CaseID { get; set; }

        /// <summary>
        /// Година на дело
        /// </summary>
        public int CaseYear { get; set; }

        /// <summary>
        /// Предмет на дело
        /// </summary>
        public string CaseDescr { get; set; }

        /// <summary>
        /// Съдия-докладчик
        /// </summary>
        public string JudgeName { get; set; }

        /// <summary>
        /// Вид на съдебния акт
        /// </summary>
        public string JudDocType { get; set; }

        /// <summary>
        /// Номер на съдебния акт
        /// </summary>
        public string JudDocNo { get; set; }

        /// <summary>
        /// Дата на съдебния акт
        /// </summary>
        [JsonConverter(typeof(JsonDateConverter))]
        public DateTime JudDocDate { get; set; }

        /// <summary>
        /// Дата и час на о.с.з
        /// </summary>
        [JsonConverter(typeof(JsonDateTimeConverter))]
        public DateTime? JudgementDate { get; set; }

        /// <summary>
        /// Място на провеждане на о.с.з.
        /// </summary>
        public string JudgementAddress { get; set; }

        /// <summary>
        /// Допълнителна информация
        /// </summary>
        public string JudgementDescr { get; set; }

        /// <summary>
        /// Противоречиви интереси
        /// </summary>
        public int ConflictInterests { get; set; }

        /// <summary>
        /// Адвокати на другата страна
        /// </summary>
        public string ОppositeLawyers { get; set; }

        /// <summary>
        /// Служебен защитник на предходна инстанция
        /// </summary>
        public string PrevLawyers { get; set; }

        /// <summary>
        /// Служител, изготвил искането
        /// </summary>
        public string AgentName { get; set; }

        /// <summary>
        /// Длъжност на служителя, изготвил искането
        /// </summary>
        public string AgentType { get; set; }

        public string FileName1 { get; set; }

        public string FileData1 { get; set; }

        public string FileName2 { get; set; }

        public string FileData2 { get; set; }

        public ClientModel[] Clients { get; set; }

        public void Sanitize()
        {
            Reason = Reason.TrimLength(1000);
            JusticeCourtDescr = JusticeCourtDescr.TrimLength(1000);
            CaseDescr = CaseDescr.TrimLength(1000);
            JudgementAddress = JudgementAddress.TrimLength(1000);
            JudgementDescr = JudgementDescr.TrimLength(1000);
            ОppositeLawyers = ОppositeLawyers.TrimLength(1000);
            PrevLawyers = PrevLawyers.TrimLength(1000);

            JudgeName = JudgeName.TrimLength(255);
            AgentName = AgentName.TrimLength(100);
            AgentType = AgentType.TrimLength(50);
            for (int i = 0; i < Clients.Length; i++)
            {
                Clients[i].Name = Clients[i].Name.TrimLength(100);
                Clients[i].Address = Clients[i].Address.TrimLength(250);
                Clients[i].Lawyer = Clients[i].Lawyer.TrimLength(255);
            }           
        }
    }
}
