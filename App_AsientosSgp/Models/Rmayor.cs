using System;
using System.Collections.Generic;
using System.Linq;

namespace App_Mundial_Miles.Models
{
    public class Rmayor
    {
        public string NumAsiento { get; set; }
        public string NumCuenta { get; set; }
        public string NombreCuenta { get; set; }
        public string NumDiario { get; set; }
        public DateTime Fecha { get; set; }
        public string Detalle { get; set; }
        public decimal Debe { get; set; }
        public decimal Haber { get; set; }

    }

    public class dtoRMayor
    {
        public decimal SaldoAnterior { get; set; }

        public decimal TotalDebe
        {
            get { return Items.Sum(t => t.detcon_debe); }
        }

        public decimal TotalHaber
        {
            get { return Items.Sum(t => t.detcon_haber); }
        }
        public List<RMayorMod> Items { get; set; }
        public dtoRMayor()
        {
             Items = new List<RMayorMod>();
        }
    }
    public class RMayorMod
    {
        public int detcon_ID { get; set; }
        public string detcon_origen { get; set; }
        public string detcon_asiento { get; set; }
        public string detcon_numero { get; set; }
        public string detcon_plan { get; set; }
        public DateTime detcon_Fecha { get; set; }
        public bool detcon_automatica { get; set; }
        public string detcon_centro { get; set; }
        public string detcon_subcentro { get; set; }
        public string detcon_detalle { get; set; }
        public decimal detcon_debe { get; set; }
        public decimal detcon_haber { get; set; }
        public string nom_plan { get; set; }
    }
}