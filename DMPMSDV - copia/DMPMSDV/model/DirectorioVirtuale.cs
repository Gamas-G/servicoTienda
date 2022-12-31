using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DMPMSDV.model
{
    class DirectorioVirtuale
    {
        public int DVid { get; set; }
        public WebSite WebSite { get; set; }
        public Directory Directory { get; set; }
        public bool Productivo { get; set; }

        public List<DirectorioVirtuale> catalogo( string catalogo )
        {
            List<DirectorioVirtuale> directorioVirtuale = new List<DirectorioVirtuale>();
            try
            {
                directorioVirtuale = JsonConvert.DeserializeObject<List<DirectorioVirtuale>>(catalogo);
            }
            catch (Exception)
            {

            }
            return directorioVirtuale;
        }
    }

    public class WebSite
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string AppPool { get; set; }
        public string FrameworkVersion { get; set; }
        public string PipeLineMode { get; set; }
        public string Port { get; set; }
    }
    public class Directory
    {
        public string Path { get; set; }
        public bool AccessRead { get; set; }
        public bool AccessExecute { get; set; }
        public bool AccessWrite { get; set; }
        public bool AccessScript { get; set; }
        public bool AuthNTLM { get; set; }
        public bool EnableDefaultDoc { get; set; }
        public bool EnableDirBrowsing { get; set; }
        public bool DirBrowseShowDate { get; set; }
        public bool DirBrowseShowTime { get; set; }
        public bool DirBrowseShowSize { get; set; }
        public bool DirBrowseShowExtension { get; set; }
        public bool DirBrowseShowLongDate { get; set; }
        public string DefaultDoc { get; set; }
        public string AppFriendlyName { get; set; }
        public int AppIsolated { get; set; }
        public string AppRoot { get; set; }
        public bool DontLog { get; set; }
        public string AnonymousUserName { get; set; }
        public int AspSessionTimeout { get; set; }
        public string AppPoolId { get; set; }
        public string NuevoAppPool { get; set; }
        public string FrameworkVersion { get; set; }
    }
}
