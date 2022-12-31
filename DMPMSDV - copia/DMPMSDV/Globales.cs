
namespace DMPMSDV
{
    using System;

    public static class Globales
    {
        public static string TipoWS = Environment.MachineName.Substring(0, 2).ToUpper();
        public static string WorkStation = Environment.MachineName.ToUpper();
        public static string ipNT;
        public static string TipoMtrIni;
        public static string HoraIniMtr;
        public static int IntervaloMtr;
        public static int IntervMinMtr;
        public static int ToleranciaMtr;
        public static string HorarioApertura;
        public static string HorarioCierre;
        public static string catalogoDVDecrypt;
    }
}
