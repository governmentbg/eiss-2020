using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace IOWebApplication.Infrastructure.Contracts
{
    public interface IEMailMessageService
    {
        bool MakeWorkNotificationEMail();
        bool SendMail();
    }
}
