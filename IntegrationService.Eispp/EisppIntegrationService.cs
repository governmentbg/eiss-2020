using System.Diagnostics;
using System.ServiceModel;
using System.ServiceProcess;

namespace IntegrationService.Eispp
{
    public class EisppIntegrationService : ServiceBase
    {
        public ServiceHost serviceHost = null;
        public EisppIntegrationService()
        {
            // Name the Windows Service
            ServiceName = Properties.Settings.Default.ServiceName;
        }

        public static void Main()
        {
            try
            {
                ServiceBase.Run(new EisppIntegrationService());
            }
            catch (System.Exception ex)
            {
                EventLog.WriteEntry("Application", ex.ToString(), EventLogEntryType.Error);
            }
            
        }

        // Start the Windows service.
        protected override void OnStart(string[] args)
        {
            if (serviceHost != null)
            {
                serviceHost.Close();
            }

            // Create a ServiceHost for the CalculatorService type and
            // provide the base address.
            serviceHost = new ServiceHost(typeof(EisppService));

            // Open the ServiceHostBase to create listeners and start
            // listening for messages.
            serviceHost.Open();
            Helper.ErrorHelper.WriteToEventLog = true;
        }

        protected override void OnStop()
        {
            if (serviceHost != null)
            {
                serviceHost.Close();
                serviceHost = null;
            }
        }
    }
}
