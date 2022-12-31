
namespace OrquestadorCentralWs
{
    using System;
    using Quartz;
    using Quartz.Impl;
    using ML;
    using DAL;

    public class TareasProgramadas
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void Monitoreo(int TipoMtr, string jobx, string triggerx, string horario)
        {
            try
            {
                log.Info(string.Format("Se programara tarea, Tipo: " + TipoMtr.ToString() + " Job: " + jobx + " Trigger: " + triggerx + " Horario: " + horario));
                //Instanciar planificador
                ISchedulerFactory planificador = new StdSchedulerFactory();

                //Obtener programador de planificador
                IScheduler plan = planificador.GetScheduler();
                plan.Start();

                //Crear Job
                IJobDetail job = JobBuilder.Create<EjecutarTarea>()
                                 .WithIdentity(jobx, "HeimdalMtr")
                                 .Build();

                string[] tiempo = horario.Split(':');
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
                    log.Info(string.Format("Intervalo: " + Globales.IntervaloMtr.ToString()));
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
                log.Info(string.Format("Tarea programada correctamente"));
            }
            catch (Exception ex)
            {
                log.Info(string.Format("Error en Monitoreo: {0}", ex.Message));
            }
        }
    }

    public class EjecutarTarea : IJob
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void Execute(IJobExecutionContext context)
        {
            string rutaTiendaServidor = string.Empty;
            DBConex dbc = new DAL.DBConex();
            string jobex = context.JobDetail.Key.Name;

            if (jobex == "MantoC") //Hora Especifica todos los días
            {
                dbc.MantoCambios();
            }            
        }
    }
}
