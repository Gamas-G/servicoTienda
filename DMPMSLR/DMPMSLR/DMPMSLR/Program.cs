﻿namespace DMPMSLR
{
    using System.ServiceProcess;
    static class Program
    {
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new LlavesReg()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
