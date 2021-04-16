using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Delivery;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using System.Text;

namespace IOWebApplication.Infrastructure.Models.ViewModels.Delivery
{
    public class DeliveryAreaFindVM
    {
        public List<DeliveryAreaAddress> DeliveryAreaAddressList { get; set; }
        public List<DeliveryArea> DeliveryAreaList { get; set; }
         public int DeliveryAreaId { get; set; }
        public int LawUnitId { get; set; }
        public int ToCourtId { get; set; }

        public bool IsFoundArea()
        {
            return DeliveryAreaList.Count > 0;
        }
    }
}
