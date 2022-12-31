using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DMPMSDV.model
{
    public class DirectorioIIS
    {
        public string SiteWeb { get; set; }
        public List<DirectorioI> directorio { get; set; }

        public DirectorioIIS()
        {
            directorio = new List<DirectorioI>();
        }
    }

    public class DirectorioI
    {
        public string NombreDV { get; set; }
        public string PathDv { get; set; }
        public string PoolNameDV { get; set; }
    }
}
