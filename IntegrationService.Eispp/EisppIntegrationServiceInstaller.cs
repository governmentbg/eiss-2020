using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace IntegrationService.Eispp
{
    [RunInstaller(true)]
    public class EisppIntegrationServiceInstaller : Installer
    {
        public EisppIntegrationServiceInstaller()
        {
            ServiceProcessInstaller serviceProcessInstaller = new ServiceProcessInstaller();
            ServiceInstaller serviceInstaller = new ServiceInstaller();

            //# Service Account Information
            serviceProcessInstaller.Account = ServiceAccount.LocalSystem;
            serviceProcessInstaller.Username = null;
            serviceProcessInstaller.Password = null;

            //# Service Information
            serviceInstaller.DisplayName = Properties.Settings.Default.ServiceDisplayName;
            serviceInstaller.Description = "This service is sending and receiving messages to / from EISPP.";
            serviceInstaller.StartType = ServiceStartMode.Automatic;

            // This must be identical to the WindowsService.ServiceBase name
            // set in the constructor of WindowsService.cs
            serviceInstaller.ServiceName = Properties.Settings.Default.ServiceName;
            serviceInstaller.AfterInstall += ServiceInstaller_AfterInstall;

            this.Installers.Add(serviceProcessInstaller);
            this.Installers.Add(serviceInstaller);
        }

        private void ServiceInstaller_AfterInstall(object sender, InstallEventArgs e)
        {
            ServiceController sc = new ServiceController("EisppIntegrationService");
            sc.Start();
        }
    }
}