namespace OrquestadorCentralWs
{
    using System;
    using System.ServiceProcess;
    static class Program
    {
        static void Main()
        {

            /*if (Environment.UserInteractive)
            {
                var inst = new OrquestadorC();
                inst.OnDebug();
            }*/


            /* Crear servicio de window */
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new OrquestadorC()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
