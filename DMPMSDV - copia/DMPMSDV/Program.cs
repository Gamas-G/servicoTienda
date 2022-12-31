namespace DMPMSDV
{
    using System;
    using System.Configuration.Install;
    using System.Reflection;
    using System.ServiceProcess;
    static class Program
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        static void Main(string[] args)
        {
            //MODO DEBUG
            /*if (Environment.UserInteractive)
            {
                log.Info("Haz entrado");
                //var parameter = string.Concat(args);
                //switch (parameter)
                //{
                var instll = new DMPMSDV();
                instll.OnDebug();
                //case "--install":
                //    ManagedInstallerClass.InstallHelper(new[] { Assembly.GetExecutingAssembly().Location });
                //    break;
                //case "--uninstall":
                //    ManagedInstallerClass.InstallHelper(new[] { "/u", Assembly.GetExecutingAssembly().Location });
                //    break;
                //}
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                    new DMPMSDV()
                };
                ServiceBase.Run(ServicesToRun);
            }*/


            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new DMPMSDV()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
