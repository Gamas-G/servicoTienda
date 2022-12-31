using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace HolaMundo
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            //InitializeComponent();
            serviceProcessInstaller1 = new ServiceProcessInstaller();
            serviceProcessInstaller1.Account = ServiceAccount.LocalSystem;
            serviceInstaller1 = new ServiceInstaller();
            serviceInstaller1.ServiceName = "ServiceHolaMundo";
            serviceInstaller1.DisplayName = "ServiceHolaMundo";
            serviceInstaller1.Description = "Servicio de pruebas";
            serviceInstaller1.StartType = ServiceStartMode.Automatic;
            Installers.Add(serviceProcessInstaller1);
            Installers.Add(serviceInstaller1);
        }
    }
}
