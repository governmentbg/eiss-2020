using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Data.Models.Delivery;
using IOWebApplication.Infrastructure.Data.Models.Identity;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using IOWebApplication.Infrastructure.Models.ViewModels.Delivery;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZXing;
using ZXing.QrCode;

namespace IOWebApplication.Core.Services
{
    public class DeliveryAccountService : BaseService, IDeliveryAccountService
    {
        private readonly IConfiguration configuration;
        public DeliveryAccountService(
            ILogger<DeliveryAccountService> _logger,
            IRepository _repo,
            IUserContext _userContext,
            IConfiguration _configuration)
        {
            logger = _logger;
            repo = _repo;
            userContext = _userContext;
            configuration = _configuration;
        }
        public (string, string) GenerateBarcodeTying(string userId)
        {
            string mobileApiAddr = configuration["MobileApiURI"];
            var user = repo.All<ApplicationUser>()
                       .Where(x => x.Id == userId)
                       .FirstOrDefault();
            if (user == null)
                return (null, null);
            string newId = Guid.NewGuid().ToString();
            var account = new DeliveryAccount()
            {
                Id = newId,
                ApiAddress = mobileApiAddr + newId,
                CourtId = user.CourtId,
                LawUnitId = user.LawUnitId,
                IsActive = true,
                MobileUserId = user.Id,
                MobileToken = "",
                PinHash = "",
                UserId = userContext.UserId,
                DateWrt = DateTime.Now
            };
            repo.Add(account);
            repo.SaveChanges();

            return (account.ApiAddress, GenerateBarcode(account.ApiAddress));
        }
        public (string, string) GetBarcodeTying(string Id)
        {
            string mobileApiAddr = configuration["MobileApiURI"];
            var account = repo.All<DeliveryAccount>()
                              .Where(x => x.Id == Id &&
                                          x.IsActive &&
                                          string.IsNullOrEmpty(x.MobileToken))
                              .FirstOrDefault();
            if (account == null)
                return ("", "");
            return (account.ApiAddress, GenerateBarcode(account.ApiAddress));
        }

        private string GenerateBarcode(string apiAddr)
        {
            var writer = new BarcodeWriterPixelData
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new QrCodeEncodingOptions { Width = 400, Height = 400, Margin = 1 }
            };

            var result = writer.Write(apiAddr);
            var base64str = string.Empty;

            using (var bitmap = new System.Drawing.Bitmap(result.Width, result.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb))
            {
                using (var ms = new System.IO.MemoryStream())
                {
                    var bitmapData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, result.Width, result.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                    try
                    {
                        // we assume that the row stride of the bitmap is aligned to 4 byte multiplied by the width of the image   
                        System.Runtime.InteropServices.Marshal.Copy(result.Pixels, 0, bitmapData.Scan0, result.Pixels.Length);
                    }
                    finally
                    {
                        bitmap.UnlockBits(bitmapData);
                    }

                    // PNG or JPEG or whatever you want
                    bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    base64str = Convert.ToBase64String(ms.ToArray());
                }
            }
            return (base64str);
        }
        public IQueryable<DeliveryTokenVM> GetDeliveryTokenForUser(string userId)
        {
            var courts = repo.All<Court>();
            var users = repo.All<ApplicationUser>()
                            .Include(x => x.LawUnit);
            return repo.All<DeliveryAccount>()
                       .Where(x => x.MobileUserId == userId)
                       .Select(x => new DeliveryTokenVM
                       {
                           Id = x.Id,
                           UserId = x.MobileUserId,
                           CourtId = x.CourtId,
                           CourtName = courts.Where(c => c.Id == x.CourtId).Select(s => s.Label).FirstOrDefault(),
                           FullName = users.Where(c => c.Id == x.MobileUserId).Select(s => s.LawUnit != null ? s.LawUnit.FullName : "").FirstOrDefault() ?? "",
                           UserName = users.Where(c => c.Id == x.MobileUserId).Select(s => s.Email).FirstOrDefault() ?? "",
                           UserNameCreate = users.Where(c => c.Id == x.UserId).Select(s => s.Email).FirstOrDefault() ?? "",
                           UserNameExpired = users.Where(c => c.Id == x.UserExpiredId).Select(s => s.Email).FirstOrDefault() ?? "",
                           DateExpired = x.DateExpired,
                           DateCreate = x.DateWrt, 
                           StateName = x.DateExpired != null ? " Изтрит" : (string.IsNullOrEmpty(x.PinHash) ? (string.IsNullOrEmpty(x.MobileToken)? "Нов" : "В процес на сдвояване"  ): "Сдвоен"),
                           IsNew = x.DateExpired == null && string.IsNullOrEmpty(x.PinHash) && string.IsNullOrEmpty(x.MobileToken)
                       });
        }
        public int TokenForUserNewCount(string userId)
        {
            return repo.AllReadonly<DeliveryAccount>()
                       .Where(x => x.UserId == userId &&
                                   x.DateExpired == null &&
                                   string.IsNullOrEmpty(x.PinHash)&& 
                                   string.IsNullOrEmpty(x.MobileToken)
                                   )
                       .Count();
        }
        public bool SaveExpireInfoPlus(ExpiredInfoVM model)
        {
            var saved = repo.GetById<DeliveryAccount>(model.ReturnUrl);
            if (saved != null)
            {
                saved.DateExpired = DateTime.Now;
                saved.UserExpiredId = userContext.UserId;
                saved.DescriptionExpired = model.DescriptionExpired;
                repo.Update(saved);
                repo.SaveChanges();
                return true;
            }
            else
            {
                return false;
            }

        }
    }
}
