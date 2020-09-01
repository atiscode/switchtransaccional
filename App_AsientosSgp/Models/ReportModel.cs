using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace App_Mundial_Miles.Models
{
    public class ReportModel
    {
        public int Tipo { get; set; }
        public string Anio { get; set; }
        public string Cuenta_Inicial { get; set; }
        public string[] Cuenta_Inicial_Mod { get; set; }
        public string Cuenta_Final { get; set; }
        public string Mes_Inicial { get; set; }
        public string Mes_Final { get; set; }
        
    }
}