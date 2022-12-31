
namespace DMPMSLR
{
    using System;
    using System.Linq;
    using Banco.PD3.Persistence;
    using Banco.PD3.Persistence.Entities;
    using System.Data.SqlClient;    
    using Microsoft.Win32;

    public class Monitoreo
    {
        PD3Connection conn1;
        string Qry = string.Empty;
        string LRResponsable = string.Empty;
        int LRCritico = 0;
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        string path = @"E:\ADN\NET64_BAZ\Certs\";
        string certificado = "baz-52bd6396-0f8d-4188-959e-4f1083bfd836.pd3"; //Desarrollo
        //string path = @"K:\NET64_BAZ\Certs\";
        //string certificado = "baz-50b191cc-c36a-4e3b-83c7-470728efff5d.pd3"; //Producción

        public void CargaConfiguracion()
        {
            try
            {
                log.Info(string.Format("Inicia carga de Configuración"));

                DMPMicroservicio.csMsMetodos dllMs = new DMPMicroservicio.csMsMetodos();

                dllMs.CargaConfiguracion("LR");
                Globales.TipoMtrIni = dllMs.TipoMtrIni;
                Globales.HoraIniMtr = dllMs.HoraIniMtr;
                Globales.IntervaloMtr = dllMs.IntervaloMtr;
                Globales.IntervMinMtr = dllMs.IntervMinMtr;
                Globales.ToleranciaMtr = dllMs.ToleranciaMtr;
                Globales.HorarioApertura = dllMs.HorarioApertura;
                Globales.HorarioCierre = dllMs.HorarioCierre;
                Globales.ipNT = dllMs.ObtenerIP();
                //Globales.TipoWS = dllMs.TipoEquipo(); //Producción
                Globales.TipoWS = "WS_SFIN";   //Desarrollo             

                log.Info(string.Format("Termina carga de Configuración"));
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error en CargaConfiguracion: {0}", ex.Message));
            }
        }

        public void InicioAlterno()
        {
            try
            {
                log.Info(string.Format("Inicia proceso InicioAlterno"));

                string HoraProg = string.Empty;
                DMPMicroservicio.csMsMetodos dllMs = new DMPMicroservicio.csMsMetodos();
                HoraProg = dllMs.InicioAlterno(Globales.HoraIniMtr, Globales.IntervaloMtr, "LR");

                log.Info(string.Format("Se programara tarea de Monitoreo Inicial Alterno"));
                TareasProgramadas TareasP = new TareasProgramadas();
                TareasP.Monitoreo(3, "MtrIniAlterLR", "triggerMtrAlterLR", HoraProg);

                log.Info(string.Format("Termina proceso InicioAlterno"));
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error en InicioAlterno: {0}", ex.Message));
            }
        }

        public void AgendarTarea(int Tipo, string Tarea, string Gatillo, string HoraAlter)
        {
            TareasProgramadas TareasP = new TareasProgramadas();
            TareasP.Monitoreo(Tipo, Tarea, Gatillo, HoraAlter);
        }

        public bool MonitorLR()
        {
            string RutaLlave = string.Empty;
            string NombreLlave = string.Empty;
            bool SeActualiza = false;
            bool EsServWin = false;
            int ParamId = 0;
            int ParamIdSw = 0;
            string MsjCritico = string.Empty;

            try
            {
                log.Info(string.Format("Inicia proceso de Monitoreo de Llaves de Registro"));
                DateTime HApert = DateTime.ParseExact(Globales.HorarioApertura, "HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                DateTime HCierre = DateTime.ParseExact(Globales.HorarioCierre, "HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                DateTime HAct = DateTime.ParseExact(DateTime.Now.Hour.ToString("00") + ":" + DateTime.Now.Minute.ToString("00"), "HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                log.Info(string.Format("HApert:" + HApert.ToString() + ", HCierre:" + HCierre.ToString() + ", HAct:" + HAct.ToString()));

                if (HAct >= HApert && HAct <= HCierre) //No se realizara el monitoreo mientras
                {

                    DMPMicroservicio.csMsMetodos dllMs = new DMPMicroservicio.csMsMetodos();
                    if (Environment.MachineName.Substring(0, 2).ToUpper() == "WS")
                    {
                        path = @"K:\NET64_BAZ\Certs\";
                    }
                    conn1 = new PD3Connection();
                    conn1 = Certificater.GetPD3Connection(path, certificado);
                    SqlPersister persistence = new SqlPersister(conn1);
                    SqlDataReader reader = null;
                    Qry = "exec spConDMPLlaveRegMtr 1,0,'" + Globales.TipoWS + "'";
                    reader = persistence.ScriptToDataReader(Qry);

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            RutaLlave = reader[3].ToString().Trim();
                            NombreLlave = reader[2].ToString().Trim();

                            string[] pathKeyRegistry = RutaLlave.Split(new[] { "\\" }, StringSplitOptions.None);

                            string pathWithoutKey = ExtraeKeyDelPath(RutaLlave);
                            LlaveRegistry llave = CreaRoot(pathKeyRegistry[0]);

                            EsServWin = false;
                            if (pathWithoutKey.Contains(@"SYSTEM\CurrentControlSet\Services"))
                            {
                                EsServWin = true;
                            }

                            bool existeLlave = llave.Existe(pathWithoutKey, NombreLlave);
                            string valor = llave.Leer(string.Join("\\", pathKeyRegistry.Skip(1)), NombreLlave);
                            string ValorDeseado = reader[5].ToString().Trim();

                            RegistryValueKind keyType = RegistryValueKind.String;

                            if (!valor.Equals("La llave no existe"))
                            {
                                keyType = llave.ObtenTipo(string.Join("\\", pathKeyRegistry.Skip(1)), NombreLlave);
                            }
                            else
                            {
                                switch (reader[4].ToString().Trim())
                                {
                                    case "REG_SZ":
                                        keyType = RegistryValueKind.String;
                                        break;

                                    case "REG_EXPAND_SZ":
                                        keyType = RegistryValueKind.ExpandString;
                                        break;

                                    case "REG_BINARY":
                                        keyType = RegistryValueKind.Binary;
                                        break;

                                    case "REG_DWORD":
                                        keyType = RegistryValueKind.DWord;
                                        break;

                                    case "REG_MULTI_SZ":
                                        keyType = RegistryValueKind.MultiString;
                                        break;

                                    case "REG_QWORD":
                                        keyType = RegistryValueKind.QWord;
                                        break;
                                }
                            }

                            log.Info(string.Format("Key = {0} Value = {1} Path = {2}", NombreLlave, valor, RutaLlave));

                            if (!existeLlave)
                            {
                                ParamId = ObtenParam(Convert.ToInt32(reader[1].ToString().Trim()), "Ruta", reader[3].ToString().Trim());
                                existeLlave = llave.Crear(pathWithoutKey, NombreLlave, ValorDeseado, keyType);
                                if (existeLlave)
                                {
                                    valor = ValorDeseado;
                                    if (EsServWin)
                                    {
                                        log.Info(string.Format("El parametro " + LrToSw(NombreLlave) + " del servicio " + pathKeyRegistry[4] + " no existia y se genero nuevamente."));
                                        dllMs.GeneraHallazgo(ParamId, "El parametro " + LrToSw(NombreLlave) + " del servicio " + pathKeyRegistry[4] + " no existia y se genero nuevamente.", "LR", 1);
                                    }
                                    else
                                    {
                                        log.Info(string.Format("La Llave " + NombreLlave + " no existia y se genero nuevamente."));
                                        dllMs.GeneraHallazgo(ParamId, "La Llave " + NombreLlave + " no existia y se genero nuevamente", "LR", 1);
                                    }
                                }
                                else
                                {
                                    if (EsServWin)
                                    {
                                        ParamIdSw = BusqSWCritico(Convert.ToInt32(reader[0].ToString().Trim()),"SW");
                                        log.Info(string.Format(ParamIdSw.ToString()));
                                        MsjCritico = LRCritico == 1 ? "CRITICO|" + reader[0].ToString().Trim() + "|" : string.Empty;
                                        log.Info(string.Format(MsjCritico + "El parametro " + LrToSw(NombreLlave) + " del servicio " + pathKeyRegistry[4] + " no existe y no se logro generar."));                                        
                                        dllMs.GeneraHallazgo(ParamIdSw, MsjCritico + "El parametro " + LrToSw(NombreLlave) + " del servicio " + pathKeyRegistry[4] + " no existe y no se logro generar.", "LR", 1);
                                    }
                                    else
                                    {
                                        ParamIdSw = BusqSWCritico(Convert.ToInt32(reader[0].ToString().Trim()), "LR");
                                        MsjCritico = LRCritico == 1 ? "CRITICO|" + reader[0].ToString().Trim() + "|" : string.Empty;
                                        log.Info(string.Format(MsjCritico + "La Llave " + NombreLlave + " no existe y no se logro generarla."));                                        
                                        dllMs.GeneraHallazgo(ParamId, MsjCritico + "La Llave " + NombreLlave + " no existe y no se logro generarla", "LR", 2);
                                    }
                                }
                            }

                            log.Info(string.Format("existeLlave {0}, ValorDeseado {1}", existeLlave, ValorDeseado));
                            if (existeLlave && ValorDeseado != valor)
                            {
                                ParamId = ObtenParam(Convert.ToInt32(reader[1].ToString().Trim()), "ValorLlave", reader[5].ToString().Trim());
                                SeActualiza = llave.Actualizar(pathWithoutKey, NombreLlave, ValorDeseado, keyType);
                                if (SeActualiza)
                                {
                                    if (EsServWin)
                                    {
                                        log.Info(string.Format("El parametro " + LrToSw(NombreLlave) + " del servicio " + pathKeyRegistry[4] + " no tenia el Valor correcto y se actualizo."));
                                        dllMs.GeneraHallazgo(ParamId, "El parametro " + LrToSw(NombreLlave) + " del servicio " + pathKeyRegistry[4] + " no tenia el Valor correcto y se actualizo.", "LR", 1);
                                    }
                                    else
                                    {
                                        log.Info(string.Format("La Llave " + NombreLlave + " no tenia el Valor correcto y se actualizo."));
                                        dllMs.GeneraHallazgo(ParamId, "La Llave " + NombreLlave + " no tenia el Valor correcto y se actualizo", "LR", 1);
                                    }
                                }
                                else
                                {
                                    if (EsServWin)
                                    {
                                        ParamIdSw = BusqSWCritico(Convert.ToInt32(reader[0].ToString().Trim()), "SW");
                                        log.Info(string.Format(ParamIdSw.ToString()));
                                        MsjCritico = LRCritico == 1 ? "CRITICO|" + reader[0].ToString().Trim() + "|" : string.Empty;
                                        log.Info(string.Format(MsjCritico + "El parametro " + LrToSw(NombreLlave) + " del servicio " + pathKeyRegistry[4] + " no tiene el valor correcto y no se logro actualizarlo al deseado."));                                        
                                        dllMs.GeneraHallazgo(ParamIdSw, MsjCritico + "El parametro " + LrToSw(NombreLlave) + " del servicio " + pathKeyRegistry[4] + " no tiene el valor correcto y no se logro actualizarlo al deseado.", "LR", 1);
                                    }
                                    else
                                    {
                                        ParamIdSw = BusqSWCritico(Convert.ToInt32(reader[0].ToString().Trim()), "LR");
                                        MsjCritico = LRCritico == 1 ? "CRITICO|" + reader[0].ToString().Trim() + "|" : string.Empty;
                                        log.Info(string.Format(MsjCritico + "La Llave " + NombreLlave + " no tiene el valor correcto y no se logro actualizarlo al deseado."));                                        
                                        dllMs.GeneraHallazgo(ParamId, MsjCritico + "La Llave " + NombreLlave + " no tiene el valor correcto y no se logro actualizarlo al deseado", "LR", 2);
                                    }
                                }
                            }
                        }
                    }
                    reader.Close();

                    dllMs.ControlCheck("LR", Globales.WorkStation, "LR");
                    log.Info(string.Format("Termina correctamente el proceso de Monitoreo de Llaves de Registro"));
                    return true;
                }
                else
                {
                    log.Info(string.Format("Horario de Tienda Cerrada-Sin Monitoreo"));
                    return true;
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error en MonitorLR: {0}", ex.Message));
                return false;
            }
        }

        private static string ExtraeKeyDelPath(string fullPath)
        {
            string[] pathKeyRegistry = fullPath.Split(new[] { "\\" }, StringSplitOptions.None);
            return string.Join("\\", pathKeyRegistry.Skip(1));
        }

        private static LlaveRegistry CreaRoot(string registry)
        {
            return new LlaveRegistry((RaizRegistry)Enum.Parse(typeof(RaizRegistry), registry));
        }

        private int ObtenParam(int TareaId, string Campo, string Valor)
        {
            int Param = 0;

            try
            {
                SqlPersister persistence = new SqlPersister(conn1);
                SqlDataReader readerp = null;
                Qry = "exec spConDMPLlaveRegMtr 2," + TareaId + ",'" + Campo + "','" + Valor + "'";
                readerp = persistence.ScriptToDataReader(Qry);

                if (readerp.HasRows)
                {
                    readerp.Read();
                    Param = Convert.ToInt32(readerp[0].ToString().Trim());
                }
                readerp.Close();
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error en ObtenParam: {0}", ex.Message));
                return 0;
            }
            return Param;
        }

        private string LrToSw(string Param)
        {
            string SwName = string.Empty;
            try
            {
                switch (Param)
                {
                    case "Description":
                        SwName = "-Descripción- de la sección General";
                        break;
                    case "DisplayName":
                        SwName = "-Nombre para mostrar- de la sección General";
                        break;
                    case "ImagePath":
                        SwName = "-Ruta de acceso al ejecutable- de la sección General";
                        break;
                    case "ObjectName":
                        SwName = "-Esta cuenta- de la sección Iniciar sesión";
                        break;
                    case "FailureCommand":
                        SwName = "-Ejecutar Programa- de la sección Recuperación";
                        break;
                    case "RebootMessage":
                        SwName = "-Mensaje a enviar antes de reiniciar el equipo- de la sección Recuperación";
                        break;
                    default:
                        SwName = Param;
                        break;
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error en LrToSw: {0}", ex.Message));
                return Param;
            }
            return SwName;
        }

        private int BusqSWCritico(int Grupo, string Tipo)
        {
            int Param = 0;

            try
            {
                SqlPersister persistence = new SqlPersister(conn1);
                SqlDataReader readerp = null;
                if (Tipo == "SW")
                {
                    Qry = "exec spConDMPMtrAnexo 1," + Grupo + ",2";
                }
                else
                {
                    Qry = "exec spConDMPMtrAnexo 2," + Grupo;
                }
                readerp = persistence.ScriptToDataReader(Qry);

                if (readerp.HasRows)
                {
                    readerp.Read();
                    if (Tipo == "SW")
                    {
                        Param = Convert.ToInt32(readerp[6].ToString().Trim());                        
                    }
                    LRResponsable = readerp[2].ToString().Trim();
                    LRCritico = Convert.ToInt32(readerp[3].ToString().Trim());
                }
                readerp.Close();
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error en BusqSWCritico: {0}", ex.Message));
            }

            return Param;
        }
    }
}
