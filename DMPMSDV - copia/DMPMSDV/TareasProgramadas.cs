
namespace DMPMSDV
{
    using System;
    using Quartz;
    using Quartz.Impl;
    using System.Collections.Generic;

    public class TareasProgramadas
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        //Instanciar planificador
        static ISchedulerFactory planificador = new StdSchedulerFactory();

        //Obtener programador de planificador
        static IScheduler plan = planificador.GetScheduler();

        //List<Jobs> LJ = new List<Jobs>();
        static List<Jobs> lista = new List<Jobs>();

        public void Monitoreo(int TipoMtr, string jobx, string triggerx, string HoraAlter)
        {
            try
            {
                log.Info(string.Format("Se programara tarea, Tipo: " + TipoMtr.ToString() + " Job: " + jobx + " Trigger: " + triggerx + " Horario: " + (HoraAlter == string.Empty ? Globales.HoraIniMtr : HoraAlter)));
                
                plan.Start();
                
                //Construye el Job retornando (gracias al metodo Build) el objeto de tipo IJobDetail
                IJobDetail job = JobBuilder.Create<EjecutarTarea>()
                                 .WithIdentity(jobx, "HeimdalMtr")
                                 .Build();

                lista.Add(new Jobs(jobx, "HeimdalMtr"));

                string[] tiempo;

                if ( HoraAlter.Equals( string.Empty ) )
                {
                    tiempo = Globales.HoraIniMtr.Split(':');//Guardando la hora de apertura tomada de config
                }
                else
                {
                    tiempo = HoraAlter.Split(':');
                }
                string mensaje = String.Empty;

                int hora;//Almacena la hora de apertura
                int minutos;//Almacena los minutos de apertura

                Int32.TryParse(tiempo[0], out hora);
                Int32.TryParse(tiempo[1], out minutos);

                ITrigger trigger;
                if (TipoMtr == 1)
                {
                    //hora especifica todos los dias
                    trigger = TriggerBuilder.Create()
                                       .WithIdentity(triggerx, "HeimdalMtr")
                                       .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(hora, minutos))
                                       .Build();

                    lista.Add(new Jobs(triggerx, "HeimdalMtr"));
                }
                else if (TipoMtr == 2)
                {
                    //Se repite continuamente despues de un lapso de tiempo asignado
                    log.Info(string.Format("Intervalo: " + Globales.IntervaloMtr.ToString()));

                    int timeinsec = Globales.IntervaloMtr * 60; //Intervalo en min X 60 seg (de cada minuto para obtener el total de segundos)

                    trigger = TriggerBuilder.Create()
                                       .WithIdentity(triggerx, "HeimdalMtr")
                                       .StartNow()
                                       .WithSimpleSchedule(x => x
                                       .WithIntervalInSeconds(timeinsec)
                                       .RepeatForever())
                                       .Build();
                    lista.Add(new Jobs(triggerx, "HeimdalMtr"));
                }
                else
                {
                    //hora especifica dia actual
                    trigger = TriggerBuilder.Create()
                                       .WithIdentity(triggerx, "HeimdalMtr")
                                       .WithCronSchedule(string.Format("0 0/{0} {1} * * ?", minutos, hora))
                                       .Build();
                    lista.Add(new Jobs(triggerx, "HeimdalMtr"));
                }

                //usar planificador con el job y trigger
                plan.ScheduleJob(job, trigger);

                log.Info(string.Format("La programación de la tarea termino correctamente"));

            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error en Monitoreo: {0}", ex.Message));
            }
        }

        public static void eliminarJobs()
        {
    
            foreach (var item in lista)
            {
                plan.DeleteJob(new JobKey(item.Nombre, item.Grupo));
                log.Info("Delete");
            }
            log.Info("Termina de eliminar JOBS Dinamicos");
       
        }

    }

    public class EjecutarTarea : IJob
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void Execute(IJobExecutionContext context)
        {
            Monitoreo Mtr = new Monitoreo();
            string jobex = context.JobDetail.Key.Name;

            if (jobex == "Monitoreo") //Hora Especifica del día
            {
                log.Info("Nombre jobex, Monitoreo. Entra a agendar tarea");
                 Mtr.AgendarTarea(2, "CicloMtrDV", "triggerMtrDV", string.Empty);
            }
            else if (jobex == "CicloMtrDV") //Se repite cada determinado tiempo mientras este activo el servicio
            {
                log.Info("Nombre jobex, CicloMtrDV. Entra a monitorDV");
                Mtr.MonitorDV();
            }
            else if (jobex == "MtrIniAlter") //Hora Especifica del día
            {
                log.Info("Nombre jobex, MtrIniAlter. Entra a AgendarTarea");
                Mtr.AgendarTarea(2, "CicloMtrDV", "triggerMtrDV", string.Empty);
            }
        }
    }
}
