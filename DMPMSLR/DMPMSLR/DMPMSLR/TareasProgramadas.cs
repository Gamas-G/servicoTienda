
namespace DMPMSLR
{
    using System;
    using Quartz;
    using Quartz.Impl;

    public class TareasProgramadas
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void Monitoreo(int TipoMtr, string jobx, string triggerx, string HoraAlter)
        {
            try
            {
                log.Info(string.Format("Se programara tarea, Tipo: " + TipoMtr.ToString() + " Job: " + jobx + " Trigger: " + triggerx + " Horario: " + (HoraAlter == string.Empty ? Globales.HoraIniMtr : HoraAlter)));
                //Instanciar planificador
                ISchedulerFactory planificador = new StdSchedulerFactory();

                //Obtener programador de planificador
                IScheduler plan = planificador.GetScheduler();
                plan.Start();

                //Crear Job
                IJobDetail job = JobBuilder.Create<EjecutarTarea>()
                                 .WithIdentity(jobx, "HeimdalMtr")
                                 .Build();

                string[] tiempo;

                if (HoraAlter == string.Empty)
                {
                    tiempo = Globales.HoraIniMtr.Split(':');
                }
                else
                {
                    tiempo = HoraAlter.Split(':');
                }
                string mensaje = String.Empty;

                int hora;
                int minutos;

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
                }
                else if (TipoMtr == 2)
                {
                    //Se repite continuamente despues de un lapso de tiempo asignado
                    int timeinsec = Globales.IntervaloMtr * 60; //Intervalo en min X 60 seg (de cada minuto para obtener el total de segundos)

                    trigger = TriggerBuilder.Create()
                                       .WithIdentity(triggerx, "HeimdalMtr")
                                       .StartNow()
                                       .WithSimpleSchedule(x => x
                                       .WithIntervalInSeconds(timeinsec)
                                       .RepeatForever())
                                       .Build();
                }
                else
                {
                    //hora especifica dia actual
                    trigger = TriggerBuilder.Create()
                                       .WithIdentity(triggerx, "HeimdalMtr")
                                       .WithCronSchedule(string.Format("0 0/{0} {1} * * ?", minutos, hora))
                                       .Build();
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
    }

    public class EjecutarTarea : IJob
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void Execute(IJobExecutionContext context)
        {
            Monitoreo Mtr = new Monitoreo();
            string jobex = context.JobDetail.Key.Name;

            if (jobex == "MonitoreoLR") //Hora Especifica del día
            {
                Mtr.AgendarTarea(2, "CicloMtrLR", "triggerCMtrLR", string.Empty);
            }
            else if (jobex == "CicloMtrLR") //Se repite cada determinado tiempo mientras este activo el servicio
            {
                Mtr.MonitorLR();
            }
            else if (jobex == "MtrIniAlterLR") //Hora Especifica del día
            {
                Mtr.AgendarTarea(2, "CicloMtrLR", "triggerCMtrLR", string.Empty);
            }
        }
    }
}
