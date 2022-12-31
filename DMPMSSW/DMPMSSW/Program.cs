namespace DMPMSSW
{
    using System.ServiceProcess;
    static class Program
    {
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new ServWin()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
