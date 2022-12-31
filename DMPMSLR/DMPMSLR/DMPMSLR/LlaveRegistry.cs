
namespace DMPMSLR
{
    using System;
    using System.Linq;
    using Microsoft.Win32;

    public enum RaizRegistry
    {
        HKEY_CLASSES_ROOT = 0,
        HKEY_CURRENT_USER = 1,
        HKEY_LOCAL_MACHINE = 2,
        HKEY_USERS = 3,
        HKEY_CURRENT_CONFIG = 4
    }

    public class LlaveRegistry
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public LlaveRegistry(RaizRegistry raiz)
        {
            Raiz = raiz;
        }

        private RegistryKey raizEnegistry { get; set; }

        public RaizRegistry Raiz
        {
            get
            {
                return Raiz;
            }
            set
            {
                CargarRegistry(value);
            }
        }

        private void CargarRegistry(RaizRegistry raiz)
        {
            switch (raiz)
            {
                case RaizRegistry.HKEY_CLASSES_ROOT:
                    raizEnegistry = Registry.ClassesRoot;
                    break;

                case RaizRegistry.HKEY_CURRENT_CONFIG:
                    raizEnegistry = Registry.CurrentConfig;
                    break;

                case RaizRegistry.HKEY_CURRENT_USER:
                    raizEnegistry = Registry.CurrentUser;
                    break;

                case RaizRegistry.HKEY_LOCAL_MACHINE:
                    raizEnegistry = Registry.LocalMachine;
                    break;

                case RaizRegistry.HKEY_USERS:
                    raizEnegistry = Registry.Users;
                    break;
            }
        }

        public string Leer(string ruta, string nombreLlave)
        {
            ruta = ruta.Replace("\\\\", "\\");

            if (Existe(ruta, nombreLlave))
            {
                return raizEnegistry.OpenSubKey(ruta).GetValue(nombreLlave).ToString();
            }
            else
            {
                return "La llave no existe";
            }
        }

        public bool Existe(string ruta, string nombreLlave)
        {
            ruta = ruta.Replace("\\\\", "\\");

            if (raizEnegistry.OpenSubKey(ruta) != null)
            {
                return raizEnegistry.OpenSubKey(ruta).GetValueNames().Contains(nombreLlave);
            }
            else
            {
                return false;
            }
        }

        public RegistryValueKind ObtenTipo(string ruta, string nombreLlave)
        {
            return raizEnegistry.OpenSubKey(ruta).GetValueKind(nombreLlave);
        }

        public bool Crear(string ruta, string nombreLlave, string valor, RegistryValueKind tipo)
        {
            try
            {
                ruta = ruta.Replace("\\\\", "\\");

                if (!ExisteRuta(ruta))
                {
                    raizEnegistry.CreateSubKey(ruta);
                }

                raizEnegistry.OpenSubKey(ruta, RegistryKeyPermissionCheck.ReadWriteSubTree).SetValue(nombreLlave, valor, tipo);

                return true;
            }
            catch (Exception ex)
            {
                log.Error(string.Format("La llave no pudo ser creada en la ruta = {0} Key = {1} Value = {2} \r\n ", ruta, nombreLlave, valor), ex);
                return false;
            }
        }

        public bool ExisteRuta(string ruta)
        {
            RegistryKey subkeys;
            ruta = ruta.Replace("\\\\", "\\");
            string[] partesRuta = ruta.Split(new[] { "\\" }, StringSplitOptions.None);

            if (partesRuta.Length > 1)
            {
                string rutaDeSubLlave = string.Empty;

                for (int indicePalabra = 0; indicePalabra < partesRuta.Length - 1; indicePalabra++)
                {
                    rutaDeSubLlave += partesRuta[indicePalabra] + "\\";
                }

                rutaDeSubLlave = rutaDeSubLlave.Substring(0, rutaDeSubLlave.Length - 1);
                subkeys = raizEnegistry.OpenSubKey(rutaDeSubLlave);

                if (subkeys != null)
                {
                    return raizEnegistry.OpenSubKey(rutaDeSubLlave).GetSubKeyNames().Contains(partesRuta[partesRuta.Length - 1]);
                }
                else
                {
                    return false;
                }
            }
            else
            {
                subkeys = raizEnegistry.OpenSubKey(partesRuta[0]);
                return subkeys != null ? true : false;
            }
        }

        public bool Actualizar(string ruta, string nombreLlave, string valor, RegistryValueKind tipo)
        {
            try
            {
                ruta = ruta.Replace("\\\\", "\\");

                if (ExisteRuta(ruta))
                {
                    raizEnegistry.OpenSubKey(ruta, RegistryKeyPermissionCheck.ReadWriteSubTree).SetValue(nombreLlave, valor, tipo);
                    return true;
                }
                else
                {
                    log.Error(string.Format("La llave no fue actualizada, debido a que no se encuentra en la ruta especificada.  Key = {0} Value = {1} Path = {2}",nombreLlave, valor, ruta));
                    return false;
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("¡No se pudo actualizar la llave!  Key = {0} Value = {1} Path = {2} \r\n ", nombreLlave, valor, ruta), ex);
                return false;
            }
        }
    }
}
