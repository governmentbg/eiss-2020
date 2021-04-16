using IOWebApplication.Infrastructure.Contracts;
using IOWebApplication.Infrastructure.Data.Common;
using IOWebApplication.Infrastructure.Data.Models.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IOWebApplication.Infrastructure.Constants;
using IOWebApplication.Infrastructure.Models.ViewModels.Common;
using Microsoft.EntityFrameworkCore;

namespace IOWebApplication.Infrastructure.Services
{
    public class EMailMessageService : IEMailMessageService
    {
        private readonly IRepository repo;
        private readonly IEmailSender emailSender;
        public EMailMessageService(IRepository _repo, IEmailSender _emailSender)
        {
            repo = _repo;
            emailSender = _emailSender;
        }

        public bool SendMail()
        {
            var emailMessages = repo.AllReadonly<EMailMessage>()
                                    .Where(x => x.EMailMessageStateId == NomenclatureConstants.EMailMessageState.ForSend)
                                    .ToList();
            foreach (var emailMessage in emailMessages)
            {
                emailMessage.EMailMessageStateId = NomenclatureConstants.EMailMessageState.Pending;
                repo.Update(emailMessage);
                repo.SaveChanges();
                Task task = emailSender.SendMail(emailMessage.EmailAddress, emailMessage.Title, emailMessage.Body);
                task.Wait();
                emailMessage.EMailMessageStateId = NomenclatureConstants.EMailMessageState.Sended;
                repo.SaveChanges();
            }
            return true;
        }
        public bool MakeWorkNotificationEMail()
        {
            var emailMessages = repo.AllReadonly<EMailMessage>();

            var notifications = repo.AllReadonly<WorkNotification>()
                                    .Include(x => x.User)
                                    .Where(x => x.User.WorkNotificationToMail == true)
                                    .Where(x => !emailMessages.Any(e => e.SourceType == SourceTypeSelectVM.WorkNotification && e.SourceId == x.Id))
                                    .ToList();
            foreach (var notification in notifications)
            {
                var emailMessage = new EMailMessage();
                emailMessage.SourceType = SourceTypeSelectVM.WorkNotification;
                emailMessage.SourceId = notification.Id;
                emailMessage.Body = notification.Description;
                emailMessage.Title = notification.Title;
                emailMessage.EmailAddress = notification.User.Email;
                emailMessage.EMailMessageStateId = NomenclatureConstants.EMailMessageState.ForSend;
                emailMessage.UserId = notification.UserId;
                emailMessage.DateWrt = DateTime.Now;
                repo.Add(emailMessage);
                repo.SaveChanges();
            }
            return true;
        }
    }
}
