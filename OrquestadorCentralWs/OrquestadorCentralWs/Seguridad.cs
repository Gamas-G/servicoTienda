using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OrquestadorCentralWs
{
    static class Seguridad
    {
        ///// Encripta una cadena
        //public static string Encriptar(this string _cadenaAencriptar)
        //{
        //    string result = string.Empty;
        //    byte[] encryted = System.Text.Encoding.Unicode.GetBytes(_cadenaAencriptar);
        //    result = Convert.ToBase64String(encryted);
        //    return result;
        //}

        /// Esta función desencripta la cadena que le envíamos en el parámentro de entrada.
        /// 

        public static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static string DesEncriptar(this string _cadenaAdesencriptar)
        {
            string result = string.Empty;
            byte[] decryted = Convert.FromBase64String(_cadenaAdesencriptar);
            //result = System.Text.Encoding.Unicode.GetString(decryted, 0, decryted.ToArray().Length);
            result = System.Text.Encoding.Unicode.GetString(decryted);
            
            return result;
            
        }
    }
}
