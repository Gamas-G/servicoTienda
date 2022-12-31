using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Encriptar
{
    public class Seguridad
    {
        public string Encriptar(string Texto)
        {
            string result = string.Empty;
            byte[] encryted = Encoding.Unicode.GetBytes(Texto);
            result = Convert.ToBase64String(encryted);
            return result;
        }

        public string Desencriptar(string TextoEncriptado)
        {
            string result = string.Empty;
            byte[] decryted = Convert.FromBase64String(TextoEncriptado);
            result = Encoding.Unicode.GetString(decryted);

            return result;

        }
    }
}
