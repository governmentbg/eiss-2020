using IO.SignTools.Contracts;
using IOWebApplication.Core.Contracts;
using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Common;
using IOWebApplication.Infrastructure.Extensions;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace IOWebApplication.Core.Services
{
    public class CourtStampCertificateService : BaseService, ICourtStampCertificateService
    {
        private readonly IIOSignToolsService signTools;
        public CourtStampCertificateService(
            IIOSignToolsService _signTools,
            ILogger<CourtStampCertificateService> _logger,
            IUserContext _userContext,
            IRepository _repo)
        {
            signTools = _signTools;
            logger = _logger;
            userContext = _userContext; ;
            repo = _repo;
        }

        public IQueryable<CourtStampCertificateListVM> Select(CourtStampCertificateFilterVM filter)
        {
            Expression<Func<Court, bool>> courtFilter = x => true;
            if (!string.IsNullOrWhiteSpace(filter.CourtName))
            {
                courtFilter = x => EF.Functions.ILike(x.Label, filter.CourtName.ToPaternSearch());
            }
            Expression<Func<Court, object>> orderLinq = x => x.Label;
            switch (filter.OrderMode)
            {
                case "expiring":
                    orderLinq = x => x.StampCertificates.Select(sc => sc.DateTo).Max();
                    break;
            }
            return repo.AllReadonly<Court>()
                        .Where(courtFilter)
                        .OrderBy(orderLinq)
                        .Select(x => new CourtStampCertificateListVM
                        {
                            CourtId = x.Id,
                            CourtName = x.Label,
                            Certificates = x.StampCertificates.Where(c => (c.DateFrom <= filter.ValidTo && c.DateTo >= filter.ValidTo) || filter.ValidTo == null)
                                            .Where(c => c.DateExpired == null || filter.ShowExpired)
                                            .OrderByDescending(c => c.DateTo)
                                            .Select(c => new CourtStampCertificateVM
                                            {
                                                Id = c.Id,
                                                SerialNumber = c.SerialNumber,
                                                Subject = c.Subject,
                                                DateFrom = c.DateFrom,
                                                DateTo = c.DateTo,
                                                IsDeactivated = c.DateExpired != null
                                            })
                        });
        }

        public async Task<SaveResultVM> AddCertificate(int courtId, byte[] certificateContent, string password, bool noCheck)
        {
            try
            {
                var certificate = new X509Certificate2(certificateContent, password);

                var newItem = new CourtStampCertificate()
                {
                    CourtId = courtId,
                    Content = certificateContent,
                    SerialNumber = certificate.SerialNumber,
                    Subject = certificate.Subject,
                    DateFrom = certificate.NotBefore,
                    DateTo = certificate.NotAfter,
                    DateUploaded = DateTime.Now,
                    PassHash = password?.Trim()
                };

                if (!noCheck)
                {
                    var certInfo = await repo.AllReadonly<CourtStampCertificate>()
                                                    .Where(x => x.SerialNumber == certificate.SerialNumber && x.DateExpired == null)
                                                    .Select(x => new
                                                    {
                                                        CourtName = x.Court.Label
                                                    })
                                                    .FirstOrDefaultAsync();

                    if (certInfo != null)
                    {
                        return new SaveResultVM(false, $"Сертификат със номер {certificate.SerialNumber} вече е добавен към съд {certInfo.CourtName}!");
                    }
                }

                repo.Add(newItem);
                await repo.SaveChangesAsync();

                return new SaveResultVM(true);
            }
            catch (CryptographicException aex)
            {
                if (aex.Message.Contains("password", StringComparison.InvariantCultureIgnoreCase))
                {
                    return new SaveResultVM(false, "Грешна парола за сертификат");
                }
                return new SaveResultVM(false, aex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "AddCertificate");
                return new SaveResultVM(false, ex.Message);
            }
        }

        public Task<CourtStampInfoVM> GetCertificate(int courtId = 0)
        {
            if (courtId == 0)
            {
                courtId = userContext.CourtId;
            }

            var dtNow = DateTime.Now;

            return repo.AllReadonly<CourtStampCertificate>()
                                        .Where(x => x.CourtId == courtId)
                                        .Where(x => x.DateFrom < dtNow && x.DateTo >= dtNow)
                                        .Where(x => x.DateExpired == null)
                                        .OrderByDescending(x => x.DateTo)
                                        .Select(x => new CourtStampInfoVM
                                        {
                                            CertificateContent = x.Content,
                                            PassHash = x.PassHash
                                        }).FirstOrDefaultAsync();
        }

        private iText.Kernel.Geom.Rectangle getStampLocation(int sourceType, int blankMode, bool wideStamp)
        {
            switch (sourceType)
            {
                case SourceTypeSelectVM.CaseSessionAct:
                default:
                    if (wideStamp)
                        return new iText.Kernel.Geom.Rectangle(300, 800, 280, 28);
                    else
                        return new iText.Kernel.Geom.Rectangle(400, 800, 180, 28);
            }
        }

        public async Task<CourtStampResponseVM> Stamp(CourtStampRequestVM request)
        {
            int courtId = request.CourtId;
            if (courtId == 0)
            {
                courtId = userContext.CourtId;
            }


            var courtStampt = await GetCertificate(courtId);

            if (courtStampt == null)
            {
                return new CourtStampResponseVM()
                {
                    Result = false,
                    StampError = "Невалиден електронен печат на съд"
                };
            }

            try
            {
                var stampedPdf = signTools.StampIt(request.PdfContent, new IO.SignTools.Models.IOStampOptions()
                {
                    Coordinates = getStampLocation(request.SourceType, request.BlankMode, request.WideStamp),
                    DisplayText = request.StampContent,
                    Password = courtStampt.PassHash,
                    Stamp = courtStampt.CertificateContent,
                    Font = "Fonts/times.ttf",
                    Reason = request.StampReason.EmptyToNull(),
                    Location = request.StampLocation.EmptyToNull()
                });
                if (stampedPdf != null && stampedPdf.Length > request.PdfContent.Length)
                {
                    return new CourtStampResponseVM()
                    {
                        Result = true,
                        StampedPdfContent = stampedPdf
                    };
                }
                return new CourtStampResponseVM()
                {
                    Result = false,
                    StampError = "Неуспешно полагане на електронен печат"
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Stamp error. Съд.{courtId}");
                return new CourtStampResponseVM()
                {
                    Result = false,
                    StampError = "Неочаквана грешка при поставяне на електронен печат"
                };
            }
        }
    }
}
