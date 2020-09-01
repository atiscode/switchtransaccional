using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace App_Mundial_Miles.Models
{
    public class ConfiguracionGlobal
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    public static class EstadoConfiguraciones {
        public static bool IsLoad  { get; set; }
    }
}