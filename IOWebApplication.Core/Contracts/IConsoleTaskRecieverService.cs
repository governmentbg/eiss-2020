using IOWebApplication.Core.MessageQueue;

namespace IOWebApplication.Core.Contracts
{
    public interface IConsoleTaskRecieverService
    {
        void RecieveMessage(MQMessageModel resultModel);
    }
}
