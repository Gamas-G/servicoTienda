
namespace OrquestadorCentralWs.DAL
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Configuration;
    using System.Xml;
    using ML;
    using System.IO;
    using System.Net;
    using System.Linq;
    using System.Net.Sockets;
    using System.Diagnostics;

    public class DBConex
    {
        public static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);        

        public bool GuardaMasivaHallaz(string HallazXml, int Suc)
        {
            bool RespGuardar = false;
            try
            {
                Log.Info("DBConex - Inicia GuardaMasivaHallaz");

                if (HallazXml != "Se reportan 0 Hallazgos")
                {
                    HallazXml = HallazXml.Replace("<?xml version=\"1.0\" encoding=\"UTF-8\"?>", " ");

                    string Conexion = ConfigurationManager.ConnectionStrings["ConexString"].ConnectionString;

                    string DesCprit = Seguridad.DesEncriptar(Conexion);

                    Log.Info("LogConexion prueba: " + DesCprit);
                    


                    Globales.ConnString = (DesCprit);

                    using (SqlConnection conexionBD = new SqlConnection(Globales.ConnString))
                    {
                        using (SqlCommand cmd = new SqlCommand())
                        {
                            cmd.Connection = conexionBD;
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.CommandText = "spInsDMPMtrHallazgosC";

                            if (conexionBD.State != ConnectionState.Open)
                                conexionBD.Open();

                            XmlDocument doc = new XmlDocument();
                            doc.LoadXml(HallazXml);

                            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                            {
                                if (node.HasChildNodes)
                                {
                                    cmd.Parameters.Clear();
                                    for (int i = 0; i < node.ChildNodes.Count; i++)
                                    {
                                        if (node.ChildNodes[i].Name == "fiIdParametros")
                                        {
                                            cmd.Parameters.Add("@piIdParam", SqlDbType.Int).Value = Convert.ToInt32(node.ChildNodes[i].InnerText);
                                        }
                                        else if (node.ChildNodes[i].Name == "fcWorkstation")
                                        {
                                            cmd.Parameters.Add("@pcWs", SqlDbType.NVarChar, 15).Value = node.ChildNodes[i].InnerText;
                                        }
                                        else if (node.ChildNodes[i].Name == "fdFechaHora")
                                        {
                                            cmd.Parameters.Add("@pdFechaHora", SqlDbType.DateTime).Value = Convert.ToDateTime(node.ChildNodes[i].InnerText);
                                        }
                                        else if (node.ChildNodes[i].Name == "fcDetalle")
                                        {
                                            cmd.Parameters.Add("@pcDetalle", SqlDbType.NVarChar, 250).Value = node.ChildNodes[i].InnerText;
                                        }
                                        else if (node.ChildNodes[i].Name == "fiIdEstatus")
                                        {
                                            cmd.Parameters.Add("@piEstatus", SqlDbType.Int).Value = Convert.ToInt32(node.ChildNodes[i].InnerText);
                                        }
                                    }
                                    cmd.Parameters.Add("@piIdSuc", SqlDbType.Int).Value = Suc;
                                    cmd.ExecuteNonQuery();
                                }
                            }
                            conexionBD.Close();
                            RespGuardar = true;
                            Log.Info("GuardaMasivaHallaz - Se guardaron correctamente los Hallazgos reportados");
                        }
                    }
                }
                else
                {
                    RespGuardar = true;
                    Log.Info("GuardaMasivaHallaz - Se reportaron 0 Hallazgos de sucursal: " + Suc);
                }

                Log.Info("DBConex - Termina correctamente GuardaMasivaHallaz");
            }
            catch (Exception Ex)
            {
                RespGuardar = false;
                Log.Error("DBConex - Error en GuardaMasivaHallaz: " + Ex.Message);
            }
            return RespGuardar;
        }

        public bool CheckRepHallaz(int Opc, int Suc)
        {
            bool CheckOk = false;
            try
            {
                Log.Info("DBConex - Inicia CheckRepHallaz");
                
                string DesCprit = Seguridad.DesEncriptar(ConfigurationManager.ConnectionStrings["ConexString"].ConnectionString);
                Globales.ConnString = (DesCprit);


                using (SqlConnection conexionBD = new SqlConnection(Globales.ConnString))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = conexionBD;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "spUpdDMPMtrConfigMantoC";

                        if (conexionBD.State != ConnectionState.Open)
                            conexionBD.Open();

                        cmd.Parameters.Clear();
                        cmd.Parameters.Add("@piIdOpc", SqlDbType.Int).Value = Opc;
                        cmd.Parameters.Add("@piSuc", SqlDbType.Int).Value = Suc;

                        cmd.ExecuteNonQuery();
                    }
                    conexionBD.Close();
                }                
                CheckOk = true;
                Log.Info("DBConex - Termina correctamente CheckRepHallaz");
            }
            catch (Exception Ex)
            {
                CheckOk = false;
                Log.Error("DBConex - Error en CheckRepHallaz: " + Ex.Message);
            }

            return CheckOk;
        }

        public string BusqCfgOrqSuc(int Opc, int Suc)
        {
            string Respcfg = string.Empty;
            DataTable dt = new DataTable();

            try
            {
                Log.Info("DBConex - Inicia BusqCfgOrqSuc1");
                
                string DesCprit = Seguridad.DesEncriptar(ConfigurationManager.ConnectionStrings["ConexString"].ConnectionString);
                Globales.ConnString = (DesCprit);

                using (SqlConnection conexionBD = new SqlConnection(Globales.ConnString))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = conexionBD;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "spConDMPMtrConfigMantoC";

                        if (conexionBD.State != ConnectionState.Open)
                            conexionBD.Open();

                        cmd.Parameters.Clear();
                        cmd.Parameters.Add("@piTipoCon", SqlDbType.Int).Value = Opc;
                        cmd.Parameters.Add("@piSuc", SqlDbType.Int).Value = Suc;
                        cmd.Parameters.Add("@pdFecha", SqlDbType.SmallDateTime).Value = DateTime.Now;

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dt);
                        }
                    }

                    if (dt.Rows.Count > 0)
                    {
                        Respcfg = dt.Rows[0].ItemArray[0].ToString() + "|" + dt.Rows[0].ItemArray[1].ToString() + "|" + dt.Rows[0].ItemArray[2].ToString();
                    }
                    conexionBD.Close();
                }

                Log.Info("DBConex - Termina correctamente BusqCfgOrqSuc");
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("Error en BusqCfgOrqSuc: {0}", ex.Message));
                return ex.Message;
            }
            return Respcfg;
        }

        public string ObtCambios(string FechaCambios, int Suc)
        {
            string RespCambios = string.Empty;
            var dataSet = new DataSet();

            try
            {
                Log.Info("DBConex - Inicia ObtCambios");

                string DesCprit = Seguridad.DesEncriptar(ConfigurationManager.ConnectionStrings["ConexString"].ConnectionString);
                Globales.ConnString = (DesCprit);

                using (SqlConnection conexionBD = new SqlConnection(Globales.ConnString))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = conexionBD;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "spConDMPMtrConfigMantoC";

                        if (conexionBD.State != ConnectionState.Open)
                            conexionBD.Open();

                        cmd.Parameters.Clear();
                        cmd.Parameters.Add("@piTipoCon", SqlDbType.Int).Value = 4;
                        cmd.Parameters.Add("@piSuc", SqlDbType.Int).Value = 0;
                        cmd.Parameters.Add("@pdFecha", SqlDbType.SmallDateTime).Value = Convert.ToDateTime(FechaCambios);

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dataSet);
                        }
                    }

                    if (dataSet.Tables[0].Rows.Count > 0)
                    {
                        XmlDocument doc = new XmlDocument();
                        XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
                        XmlElement root = doc.DocumentElement;
                        doc.InsertBefore(xmlDeclaration, root);
                        XmlElement Cambio = doc.CreateElement(string.Empty, "Cambios", string.Empty);
                        doc.AppendChild(Cambio);

                        int reg = 1;
                        foreach (DataRow row in dataSet.Tables[0].Rows)
                        {
                            XmlElement RegCambio = doc.CreateElement(string.Empty, "registro" + reg.ToString(), string.Empty);
                            Cambio.AppendChild(RegCambio);

                            XmlElement Campo2 = doc.CreateElement(string.Empty, "fcTabla", string.Empty);
                            XmlText text2 = doc.CreateTextNode(row.ItemArray[0].ToString().Trim());
                            Campo2.AppendChild(text2);
                            RegCambio.AppendChild(Campo2);

                            XmlElement Campo3 = doc.CreateElement(string.Empty, "fcAccion", string.Empty);
                            XmlText text3 = doc.CreateTextNode(row.ItemArray[1].ToString().Trim());
                            Campo3.AppendChild(text3);
                            RegCambio.AppendChild(Campo3);

                            XmlElement Campo4 = doc.CreateElement(string.Empty, "fcParametros", string.Empty);
                            XmlText text4 = doc.CreateTextNode(row.ItemArray[2].ToString().Trim());
                            Campo4.AppendChild(text4);
                            RegCambio.AppendChild(Campo4);

                            reg += 1;
                        }

                        if(File.Exists(Globales.RutaXml + "CambiosInfoMtr" + Suc.ToString() + ".xml"))
                        {
                            File.Delete(Globales.RutaXml + "CambiosInfoMtr" + Suc.ToString() + ".xml");
                        }

                        doc.Save(Globales.RutaXml + "CambiosInfoMtr" + Suc.ToString() + ".xml");
                        Log.Info(string.Format("Termina creación de XML CambiosInfoMtr" + Suc.ToString() + ""));
                        RespCambios = File.ReadAllText(Globales.RutaXml + "CambiosInfoMtr" + Suc.ToString() + ".xml");
                        File.Delete(Globales.RutaXml + "CambiosInfoMtr" + Suc.ToString() + ".xml");
                    }
                    else
                    {
                        RespCambios = "No existen cambios";
                    }
                    conexionBD.Close();
                }


            }
            catch (Exception ex)
            {
                Log.Error(string.Format("Error en ObtCambios: {0}", ex.Message));
                return ex.Message;
            }
            return RespCambios;
        }

        public bool CargaConfiguracion()
        {
            Globales.IpWs = ObtenerIP(); //Se obtiene IP con nuevo metodo, e independiente del sp

            var dataSet = new DataSet();
            try
            {
                Log.Info("DBConex - Inicia CargaConfiguracion");

                string DesCprit = Seguridad.DesEncriptar(ConfigurationManager.ConnectionStrings["ConexString"].ConnectionString);
                
                Globales.ConnString = (DesCprit);

                using (SqlConnection conexionBD = new SqlConnection(Globales.ConnString))
                {
                    if (conexionBD.State != ConnectionState.Open)
                        conexionBD.Open();

                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = conexionBD;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "spConDMPMtrConfigMantoC";
                        
                        cmd.Parameters.Clear();
                        cmd.Parameters.Add("@piTipoCon", SqlDbType.Int).Value = 3;
                        cmd.Parameters.Add("@piSuc", SqlDbType.Int).Value = 0;
                        cmd.Parameters.Add("@pdFecha", SqlDbType.SmallDateTime).Value = DateTime.Now;

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dataSet);
                        }
                    }

                    if (dataSet.Tables[0].Rows.Count > 0)
                    {
                        foreach (DataRow row in dataSet.Tables[0].Rows)
                        {
                            if(row.ItemArray[1].ToString().Trim() == "Ruta generación XML")
                            {
                                Globales.RutaXml = row.ItemArray[2].ToString().Trim();//Aquí guarda la ruta de los archivos XML
                            }
                            else if (row.ItemArray[1].ToString().Trim() == "Horario Mantenimiento tabla Cambios")
                            {
                                Globales.MantoTablaCmb = row.ItemArray[2].ToString().Trim();
                            }
                        }
                    }
                    conexionBD.Close();
                }
                Log.Info("DBConex - Termina correctamente CargaConfiguracion");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("Error en CargaConfiguracion: {0}", ex.Message));
                return false;
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
                    Log.Info("localIP:" + localIP);
                    
                    return localIP = endPoint.Address.ToString();
                }
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("Error en ObtenerIP: {0}", ex.Message));
                return ex.Message;
            }
        }

        public bool GuardaEstacTrab(string cadena, int suc)
        {
            bool RespGuardar = false;
            string[] Estaciones;
            try
            {
                Log.Info("DBConex - Inicia GuardaEstacTrab");

                var stringConexion = ConfigurationManager.ConnectionStrings["ConexString"].ConnectionString;

                string DesCprit = Seguridad.DesEncriptar(stringConexion);
                Log.Info("String conexion pruebas..." + stringConexion);

                Globales.ConnString = (DesCprit);

                Estaciones = cadena.Split('|');
                
                using (SqlConnection conexionBD = new SqlConnection(Globales.ConnString))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = conexionBD;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "spUpdDMPMtrConfigMantoC";

                        if (conexionBD.State != ConnectionState.Open)
                            conexionBD.Open();

                        foreach (string ws in Estaciones)
                        {
                            cmd.Parameters.Clear();
                            cmd.Parameters.Add("@piIdOpc", SqlDbType.Int).Value = 3;
                            cmd.Parameters.Add("@piSuc", SqlDbType.Int).Value = suc;
                            cmd.Parameters.Add("@piEqui", SqlDbType.Int).Value = Convert.ToInt32(ws.Substring(0,1));
                            cmd.Parameters.Add("@piCant", SqlDbType.Int).Value = Convert.ToInt32(ws.Substring(2));
                            cmd.ExecuteNonQuery();
                        }
                        conexionBD.Close();
                        RespGuardar = true;
                        Log.Info("GuardaEstacTrab - Se actualizaron correctamente las estaciones de trabajo de la sucursal " + suc.ToString());
                    }
                }                
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("Error en GuardaEstacTrab: {0}", ex.Message));
                return RespGuardar;
            }
            return RespGuardar;
        }

        public void MantoCambios()
        {
            var dataSet = new DataSet();

            try
            {
                Log.Info("DBConex - Inicia MantoCambios");                

                string DesCprit = Seguridad.DesEncriptar(ConfigurationManager.ConnectionStrings["ConexString"].ConnectionString);
                Globales.ConnString = (DesCprit);

                using (SqlConnection conexionBD = new SqlConnection(Globales.ConnString))
                {
                    using (SqlCommand cmd = new SqlCommand())
                    {
                        cmd.Connection = conexionBD;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "spConDMPMtrConfigMantoC";

                        if (conexionBD.State != ConnectionState.Open)
                            conexionBD.Open();

                        cmd.Parameters.Clear();
                        cmd.Parameters.Add("@piTipoCon", SqlDbType.Int).Value = 5;
                        cmd.Parameters.Add("@piSuc", SqlDbType.Int).Value = 0;
                        cmd.Parameters.Add("@pdFecha", SqlDbType.SmallDateTime).Value = DateTime.Now;

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            da.Fill(dataSet);
                        }
                    }

                    if (dataSet.Tables[0].Rows.Count > 0)
                    {
                        foreach (DataRow row in dataSet.Tables[0].Rows)
                        {
                            using (SqlCommand cmd = new SqlCommand())
                            {
                                cmd.Connection = conexionBD;
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.CommandText = "spDelDMPMtrConfigMantoC";

                                cmd.Parameters.Clear();
                                cmd.Parameters.Add("@piTipoCon", SqlDbType.Int).Value = 1;
                                cmd.Parameters.Add("@piSuc", SqlDbType.Int).Value = 0;
                                cmd.Parameters.Add("@pdFecha", SqlDbType.SmallDateTime).Value = Convert.ToDateTime(row.ItemArray[0].ToString().Trim());

                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                    conexionBD.Close();
                }

                Log.Info("DBConex - Termina correctamente MantoCambios");
            }
            catch (Exception ex)
            {
                Log.Error(string.Format("Error en MantoCambios: {0}", ex));
            }
        }
    }
}
