using System;

namespace IOWebApplication.Infrastructure.Data.Models.Delivery
{
    /// <summary>
    /// Посещение отразено през мобилния призовкар
    /// </summary>
    //[JsonConverter(typeof(SaveVisitConvertor))]
    public class DeliveryItemVisitMobileModel
    {
        public int Id { get; set; }

        public int DeliveryItemId { get; set; }

        public int CourtId { get; set; }

        public int VisitCount { get; set; }

        public int DeliveryOperId { get; set; }

        public int DeliveryReasonId { get; set; }

        public DateTime DateOper { get; set; }

        public int NotificationStateId { get; set; }

        public decimal Long { get; set; }

        public decimal Lat { get; set; }

        //public int LawUnitId { get; set; }

        public string UserId { get; set; }

        //public bool IsOK { get; set; }

        //public string Error { get; set; }

        //public string DeliveryInfo { get; set; }

        public string DeliveryUUID { get; set; }

        //public string DateTimeOffset { get; set; }

    }
}
