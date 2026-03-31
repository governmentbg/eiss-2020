using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.Helper.GlobalConstants;
using IOWebApplication.Core.Services;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Delivery;
using IOWebApplicationApi.Contracts;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IOWebApplicationApi.Services
{
    public class MobileFileService : BaseService, IMobileFileService
    {
        private readonly IDeliveryItemService deliveryItemService;
        public MobileFileService(
            ILogger<MobileFileService> _logger,
            IRepository _repo,
            IDeliveryItemService _deliveryItemService)
        {
            logger = _logger;
            repo = _repo;
            deliveryItemService = _deliveryItemService;
        }
        public async Task<bool> SaveMobileFile(string deliveryAccountId, int courtId, string Content)
        {
            var mobileFile = new DeliveryMobileFile()
            {
                DeliveryAccountId = deliveryAccountId,
                CourtId = courtId,
                Content = Encoding.UTF8.GetBytes(Content)
            };
            repo.Add(mobileFile);
            repo.SaveChanges();
            await CheckMobileFile(mobileFile, false);
            return true;
        }
        public async Task<bool> CheckMobileFile(DeliveryMobileFile mobileFile, bool saveNotFound)
        {
            var stream = new MemoryStream(mobileFile.Content);
            string content = "";
            using (StreamReader sr = new StreamReader(stream))
               content = sr.ReadToEnd();
            List<DeliveryItemVisitMobile> visits;  
            try
            {
                var dateTimeConverter = new IsoDateTimeConverter() { DateTimeFormat = FormattingConstant.NormalDateFormat };
                 visits = JsonConvert.DeserializeObject<List<DeliveryItemVisitMobile>>("["+content+"]", dateTimeConverter);
            } 
            catch(Exception ex)
            {
                mobileFile.IsChecked = true;
                mobileFile.ErrorMessage = ex.Message;
                repo.Update(mobileFile);
                repo.SaveChanges();
                return true;
            }
            string error = "";
            foreach (var visit in visits)
            {
                string visitID = "delivery_itemId = " + visit.DeliveryItemId + " and delivery_uuid = " + visit.DeliveryUUID;
                if (visit.UserId == "TEST" || visit.UserId == "DELETE")
                    continue;
                var saved = repo.AllReadonly<DeliveryItemVisitMobile>()
                                .Where(x => x.DeliveryItemId == visit.DeliveryItemId &&
                                            x.DeliveryUUID == visit.DeliveryUUID)
                                .FirstOrDefault();
                if (saved == null)
                {
                    if (saveNotFound)
                    {
                        bool isOK = await deliveryItemService.DeliveryItemSaveOperMobile(visit);
                        if (!isOK)
                            error += visitID + " не е записан правилно" + Environment.NewLine;
                    }
                    else
                    {
                        error += visitID + " не е намерен" + Environment.NewLine;
                    }
                } else
                {
                    if (!saved.IsOK)
                       error += visitID + " не е записан правилно" + Environment.NewLine;
                }
            }
            mobileFile.IsChecked = true;
            mobileFile.ErrorMessage = error;
            repo.Update(mobileFile);
            repo.SaveChanges();
            return true;
        }
    }
}
