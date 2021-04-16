using IOWebApplication.Core.Contracts;
using IOWebApplication.Core.MessageQueue;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace IOWebApplication.Core.Services.Tasks
{
    public class ConsoleTaskRecieverService : IConsoleTaskRecieverService
    {
        private readonly ILogger logger;
        private readonly IConsoleTaskExecuteMessageService executeMessageService;


        public ConsoleTaskRecieverService(
            ILogger<ConsoleTaskRecieverService> logger,
            IConsoleTaskExecuteMessageService executeMessageService
            )
        {
            this.logger = logger;
            this.executeMessageService = executeMessageService;
        }

        public void RecieveMessage(MQMessageModel resultModel)
        {
            MethodInfo[] methodInfos = typeof(ConsoleTaskExecuteMessageService)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance);

            foreach (var method in methodInfos)
            {
                if(resultModel.Method == method.Name)
                {
                    method.Invoke(executeMessageService, null);
                }
            }
        }
    }
}
