
namespace DMPMSSW
{
    using System;
    using Banco.PD3.Persistence;
    using Banco.PD3.Persistence.Entities;
    using System.Data.SqlClient;
    using System.IO;
    using System.Text;
    using System.Security.Cryptography;
    using System.Configuration;
    using System.Net.Sockets;
    using System.Net;
    using System.ServiceProcess;

    public class Monitoreo
    {
        PD3Connection conn1;
        string Qry = string.Empty;
        string SWResponsable = string.Empty;
        int SWCritico = 0;
        private static byte[] goLlave;
        private static byte[] goVector;
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        string path = ConfigurationManager.AppSettings["path"];
        string certificado = ConfigurationManager.AppSettings["certificado"];
        //string path = @"E:\NET64_BAZ\Certs\";
        //string certificado = "baz-50b191cc-c36a-4e3b-83c7-470728efff5d.pd3"; //Producción
        
        public void CargaConfiguracion()
        {
            try
            {
                log.Info(string.Format("Inicia carga de Configuración"));

                DMPMicroservicio.csMsMetodos dllMs = new DMPMicroservicio.csMsMetodos();

                dllMs.CargaConfiguracion("SW");
                Globales.TipoMtrIni = dllMs.TipoMtrIni;
                Globales.HoraIniMtr = dllMs.HoraIniMtr;
                Globales.IntervaloMtr = dllMs.IntervaloMtr;
                Globales.IntervMinMtr = dllMs.IntervMinMtr;
                Globales.ToleranciaMtr = dllMs.ToleranciaMtr;
                Globales.HorarioApertura = dllMs.HorarioApertura;
                Globales.HorarioCierre = dllMs.HorarioCierre;

                Globales.ipNT = ObtenerIP();

                if (Globales.ipNT == "")
                {
                    log.Info("No se logro obtener la ip con método ObtenerIP(), se intentara con GetLocalIpAddres()");
                    Globales.ipNT = GetLocalIPAddress();
                    log.Info("Ip obtenida por el método GetLocalIPAddress(),  IP: " + Globales.ipNT);
                }

                //Todo 
                Globales.TipoWS = dllMs.TipoEquipo();
                //Globales.TipoWS = "WS_SFIN";

                log.Info(string.Format("Termina carga de Configuración"));
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error en CargaConfiguracion: {0}", ex.Message));
            }
        }


        public static string ObtenerIP()
        {
            try
            {
                string localIP = "";
                using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    socket.Connect("8.8.8.8", 65530);
                    IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                    return localIP = endPoint.Address.ToString();
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error en ObtenerIP: {0}", ex.Message));
                return "";
            }
        }
        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            log.Error("No network adapters with an IPv4 address in the system!");
            return "";
        }


        public void InicioAlterno()
        {
            try
            {
                log.Info(string.Format("Inicia proceso InicioAlterno"));

                string HoraProg = string.Empty;
                DMPMicroservicio.csMsMetodos dllMs = new DMPMicroservicio.csMsMetodos();
                HoraProg = dllMs.InicioAlterno(Globales.HoraIniMtr, Globales.IntervaloMtr, "SW");

                log.Info(string.Format("Se programara tarea de Monitoreo Inicial Alterno"));
                TareasProgramadas TareasP = new TareasProgramadas();
                TareasP.Monitoreo(3, "MtrIniAlterSW", "triggerMtrAlterSW", HoraProg);

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

        public bool MonitorSW()
        {
            string strConfig = string.Empty;
            string ConfigII = string.Empty;
            string Servicio = string.Empty;
            string InicioBD = string.Empty;
            string strTipoInicio = string.Empty;
            string strCuenta = string.Empty;
            string Pass = string.Empty;
            string Acciones = string.Empty;
            string RecupFinal = string.Empty;
            string Err1 = string.Empty;
            string Err2 = string.Empty;
            string Err3 = string.Empty;
            string RecupBD = string.Empty;
            string RespExec = string.Empty;
            string DescErr = string.Empty;
            string MsjCritico = string.Empty;
            int ParamId = 0;

            try
            {
                log.Info(string.Format("Inicia proceso de Monitoreo de Servicios Windows"));
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
                    Qry = "exec spConDMPServWinMtr 1,0,'" + Globales.TipoWS + "'";
                    reader = persistence.ScriptToDataReader(Qry);

                    if (reader.HasRows)
                    {
                        log.Info(string.Format("Existen configuraciones a Monitorear y se iniciara su procesamiento"));

                        while (reader.Read())
                        {
                            Servicio = reader[2].ToString().Trim();
                            InicioBD = reader[3].ToString().Trim();
                            Pass = reader[5].ToString().Trim();
                            RecupBD = reader[4].ToString().Trim();
                            SWResponsable = string.Empty;

                            strConfig = ExecuteCommand("sc qc " + Servicio);

                            
                            int idxstart = strConfig.IndexOf("TIPO_INICIO");
                            idxstart += 25;
                            int idxfin = strConfig.IndexOf("_START");
                            strTipoInicio = strConfig.Substring(idxstart, idxfin - idxstart).ToLower();

                            if (InicioBD != strTipoInicio)
                            {
                                ParamId = ObtenParam(Convert.ToInt32(reader[1].ToString().Trim()), "Tipo de Inicio", reader[3].ToString().Trim());

                                RespExec = ExecuteCommand("sc config " + Servicio + " start= " + InicioBD);

                                idxstart = RespExec.IndexOf("ERROR:");
                                if (idxstart != -1)
                                {
                                    BusqCritico(Convert.ToInt32(reader[0].ToString().Trim()));
                                    MsjCritico = SWCritico == 1 ? "CRITICO|" + reader[0].ToString().Trim() + "|" : string.Empty;
                                    idxfin = RespExec.IndexOf("DESCRIPCI");
                                    DescErr = RespExec.Substring(idxstart, idxfin - idxstart).Trim();
                                    log.Info(string.Format(MsjCritico + "El tipo de Inicio del servicio " + Servicio + " esta mal configurado y no se logro corregir - " + DescErr));
                                    dllMs.GeneraHallazgo(ParamId, MsjCritico + "El tipo de Inicio del servicio " + Servicio + " esta mal configurado y no se logro corregir", "SW", 2);
                                }
                                else
                                {
                                    log.Info(string.Format("El tipo de Inicio del servicio " + Servicio + " no era el correcto y se corrigio"));
                                    dllMs.GeneraHallazgo(ParamId, "El tipo de Inicio del servicio " + Servicio + " no era el correcto y se corrigio", "SW", 1);
                                }
                            }
                            
                            if (Pass != string.Empty)
                            {
                                idxstart = strConfig.IndexOf("NOMBRE_INICIO_SERVICIO");
                                idxstart += 24;
                                strCuenta = strConfig.Substring(idxstart).Trim();

                                if (strCuenta != "LocalSystem")
                                {
                                    ParamId = ObtenParam(Convert.ToInt32(reader[1].ToString().Trim()), "Password", reader[5].ToString().Trim());

                                    Pass = Decode(Pass);

                                    RespExec = ExecuteCommand("sc config " + Servicio + " password= " + Pass);

                                    idxstart = RespExec.IndexOf("ERROR:");
                                    if (idxstart != -1)
                                    {
                                        if (SWResponsable == string.Empty)
                                        {
                                            BusqCritico(Convert.ToInt32(reader[0].ToString().Trim()));                                            
                                        }
                                        MsjCritico = SWCritico == 1 ? "CRITICO|" + reader[0].ToString().Trim() + "|" : string.Empty;
                                        idxfin = RespExec.IndexOf("DESCRIPCI");
                                        DescErr = RespExec.Substring(idxstart, idxfin - idxstart).Trim();
                                        log.Info(string.Format(MsjCritico + "No se logro configurar la contraseña del servicio " + Servicio + " - " + DescErr));
                                        dllMs.GeneraHallazgo(ParamId, MsjCritico + "No se logro configurar la contraseña del servicio " + Servicio, "SW", 2);
                                    }
                                    else
                                    {
                                        log.Info(string.Format("Se configuro la contraseña del servicio " + Servicio + " para la cuenta a utilizar"));
                                        dllMs.GeneraHallazgo(ParamId, "Se configuro la contraseña del servicio " + Servicio + " para la cuenta a utilizar", "SW", 1);
                                    }
                                }
                            }
                            
                            if (RecupBD != string.Empty)
                            {
                                strConfig = ExecuteCommand("sc qfailure " + Servicio);

                                ConfigII = strConfig;
                                idxstart = strConfig.IndexOf("DO_REINICIO");
                                idxstart += 27;
                                idxfin = strConfig.IndexOf("MENSAJE_REINICIO");
                                string PerReiniSeg = strConfig.Substring(idxstart, idxfin - idxstart).Trim();

                                idxstart = strConfig.IndexOf("ACCIONES_ERROR");

                                if (idxstart != -1)
                                {
                                    idxstart += 33;
                                    strConfig = strConfig.Substring(idxstart).Trim();

                                    idxstart = strConfig.IndexOf("ms");
                                    Err1 = strConfig.Substring(0, idxstart + 2);

                                    strConfig = strConfig.Substring(idxstart + 2).Trim();
                                    idxstart = strConfig.IndexOf("ms");
                                    if (idxstart != -1)
                                    {
                                        Err2 = strConfig.Substring(0, idxstart + 2);
                                        strConfig = strConfig.Substring(idxstart + 2).Trim();
                                        idxstart = strConfig.IndexOf("ms");
                                        if (idxstart != -1)
                                        {
                                            Err3 = strConfig.Substring(0, idxstart + 2);
                                        }
                                    }

                                    idxstart = Err1.IndexOf("-");
                                    string E1T = Err1.Substring(0, idxstart - 1).Trim();
                                    string E1R = Err1.Substring(idxstart + 11, (Err1.Length - 3) - (idxstart + 11)).Trim();

                                    RecupFinal = "reset= " + PerReiniSeg;
                                    Acciones = " actions= " + TipoRecup(E1T) + "/" + E1R;

                                    if (Err2 != string.Empty)
                                    {
                                        idxstart = Err2.IndexOf("-");
                                        string E2T = Err2.Substring(0, idxstart - 1).Trim();
                                        string E2R = Err2.Substring(idxstart + 11, (Err2.Length - 3) - (idxstart + 11)).Trim();
                                        Acciones += "/" + TipoRecup(E2T) + "/" + E2R;

                                        if (Err3 != string.Empty)
                                        {
                                            idxstart = Err3.IndexOf("-");
                                            string E3T = Err3.Substring(0, idxstart - 1).Trim();
                                            string E3R = Err3.Substring(idxstart + 11, (Err3.Length - 3) - (idxstart + 11)).Trim();
                                            Acciones += "/" + TipoRecup(E3T) + "/" + E3R;
                                        }
                                    }

                                    RecupFinal += Acciones;
                                }

                                if (RecupBD != RecupFinal)
                                {
                                    ParamId = ObtenParam(Convert.ToInt32(reader[1].ToString().Trim()), "Recuperacion", reader[4].ToString().Trim());

                                    RespExec = ExecuteCommand("sc failure " + Servicio + " " + RecupBD);

                                    idxstart = RespExec.IndexOf("ERROR:");
                                    if (idxstart != -1)
                                    {
                                        if (SWResponsable == string.Empty)
                                        {
                                            BusqCritico(Convert.ToInt32(reader[0].ToString().Trim()));
                                        }
                                        MsjCritico = SWCritico == 1 ? "CRITICO|" + reader[0].ToString().Trim() + "|" : string.Empty;
                                        idxfin = RespExec.IndexOf("DESCRIPCI");
                                        DescErr = RespExec.Substring(idxstart, idxfin - idxstart).Trim();
                                        log.Info(string.Format(MsjCritico + "La configuración de la Recuperación del servicio " + Servicio + " es incorrecta y no se logro corregir - " + DescErr));
                                        dllMs.GeneraHallazgo(ParamId, MsjCritico + "La configuración de la Recuperación del servicio " + Servicio + " es incorrecta y no se logro corregir", "SW", 2);
                                    }
                                    else
                                    {
                                        log.Info(string.Format("La configuración de la Recuperación del servicio " + Servicio + " no era la correcta y se corrigio"));
                                        dllMs.GeneraHallazgo(ParamId, "La configuración de la Recuperación del servicio " + Servicio + " no era la correcta y se corrigio", "SW", 1);
                                    }
                                }
                            }
                            ServiceController sc = new ServiceController(Servicio);

                            if ((sc.Status.Equals(ServiceControllerStatus.Stopped)) || (sc.Status.Equals(ServiceControllerStatus.StopPending)))
                            {
                                log.Info("El servicio "+Servicio+" se encontro detenido... ");
                                sc.Start();
                                log.Info("Se incia correctamente el servicio: " + Servicio);
                                sc.Refresh();
                                log.Info("Status final del servicio: "+ sc.Status.ToString());

                                if((sc.Status.Equals(ServiceControllerStatus.Running) || sc.Status.Equals(ServiceControllerStatus.StartPending)))
                                {
                                    dllMs.GeneraHallazgo(ParamId, "El servicio " + Servicio + "se encontrada detenido  y se corrigio", "SW", 1);
                                }
                                else
                                {
                                    dllMs.GeneraHallazgo(ParamId, "El servicio " + Servicio + " se encontrada detenido y NO se logro corregir", "SW", 2);
                                }
                            }
                           
                        }
                    }
                    reader.Close();

                    dllMs.ControlCheck("SW", Globales.WorkStation, "SW");
                    log.Info(string.Format("Termina correctamente el proceso de Monitoreo de Servicios Windows"));
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
                log.Error(string.Format("Error en MonitorSW: {0}", ex.Message));
                return false;
            }
        }

        static string ExecuteCommand(string _Command)
        {
            string result = string.Empty;

            System.Diagnostics.ProcessStartInfo procStartInfo = new System.Diagnostics.ProcessStartInfo("cmd", "/c " + _Command);

            procStartInfo.RedirectStandardOutput = true;
            procStartInfo.UseShellExecute = false;
            procStartInfo.CreateNoWindow = true;
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo = procStartInfo;
            proc.Start();
            return result = proc.StandardOutput.ReadToEnd();

        }

        private int ObtenParam(int TareaId, string Campo, string Valor)
        {
            int Param = 0;

            try
            {
                SqlPersister persistence = new SqlPersister(conn1);
                SqlDataReader readerp = null;
                Qry = "exec spConDMPServWinMtr 2," + TareaId + ",'" + Campo + "','" + Valor + "'";
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

        private string TipoRecup(string tipo)
        {
            string Recup = string.Empty;

            if (tipo == "REINICIAR")
            {
                Recup = "restart";
            }
            else if (tipo == "EJECUTAR PROCESO")
            {
                Recup = "run";
            }
            else if (tipo == "REINICIO")
            {
                Recup = "reboot";
            }

            return Recup;
        }

        private void BusqCritico(int Grupo)
        {
            try
            {
                SqlPersister persistence = new SqlPersister(conn1);
                SqlDataReader readerp = null;
                Qry = "exec spConDMPMtrAnexo 2," + Grupo;
                readerp = persistence.ScriptToDataReader(Qry);

                if (readerp.HasRows)
                {
                    readerp.Read();
                    SWResponsable = readerp[2].ToString().Trim();
                    SWCritico = Convert.ToInt32(readerp[3].ToString().Trim());
                }
                readerp.Close();
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Error en BusqCritico: {0}", ex.Message));
            }
        }

        public static string Decode(string lcTexto)
        {
            string lcDesencriptado = null;
            if (!string.IsNullOrEmpty(lcTexto))
            {
                MemoryStream loMemStream = null;
                try
                {
                    //Carga de datos de la llave
                    goLlave = new byte[32];
                    for (int x = 0; x < 32; x++)
                        goLlave[x] = Convert.ToByte(Math.Round(Math.Abs(Math.Sin(x + 1)) * Byte.MaxValue, 0));
                    //Carga de datos del vector inicial
                    goVector = new byte[16];
                    for (int x = 0; x < 16; x++)
                        goVector[x] = Convert.ToByte(Math.Round(Math.Abs(Math.Cos(x + 1)) * Byte.MaxValue, 0));

                    byte[] loDataBytes = Convert.FromBase64String(lcTexto);
                    //Se genera un MemoryStream conteniendo el texto a desencriptar
                    loMemStream = new MemoryStream(loDataBytes);
                    //Se crea una instancia del objeto AesManaged
                    using (AesManaged loAesManaged = new AesManaged())
                    {
                        loAesManaged.BlockSize = 128;
                        loAesManaged.KeySize = 256;
                        //Se crea un CryptoStream usando el MemoryStream 
                        //pasándole la llave y vector de inicialización.
                        using (CryptoStream loCryptoStream = new CryptoStream(loMemStream,
                            loAesManaged.CreateDecryptor(goLlave, goVector),
                            CryptoStreamMode.Read))
                        {
                            loMemStream = null;
                            //La longitud del texto desencriptado nunca sera mayor al de la Data
                            byte[] loTextoBytes = new byte[loDataBytes.Length];
                            //Se obtiene el texto desencriptado
                            int lnTamano = loCryptoStream.Read(loTextoBytes, 0, loTextoBytes.Length);
                            lcDesencriptado = Encoding.UTF8.GetString(loTextoBytes, 0, lnTamano);
                        }
                    }
                }
                finally
                {
                    if (loMemStream != null) loMemStream.Close();
                }
            }
            return lcDesencriptado;
        }

    }
}
