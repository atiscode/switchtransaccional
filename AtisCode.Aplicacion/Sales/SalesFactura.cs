using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtisCode.Aplicacion
{
    public class factura
    {
        public string id { get; set; }
        public string version { get; set; }
        public infoTributaria infoTributaria { get; set; }
        public infoFactura infoFactura { get; set; }
        public List<detalle>  detalles { get; set; }
        public Dictionary<string, string> infoAdicional { get; set; }//campoAdicional
        public factura()
        {
            infoAdicional = new Dictionary<string, string>();
        }
    }
    public class infoTributaria
    {
        public string ambiente { get; set; }
        public string tipoEmision { get; set; }
        public string razonSocial { get; set; }
        public string nombreComercial { get; set; }
        public string ruc { get; set; }
        public string claveAcceso { get; set; }
        public string codDoc { get; set; }
        public string estab { get; set; }
        public string ptoEmi { get; set; }
        public string secuencial { get; set; }
        public string dirMatriz { get; set; }
    }
    public class infoFactura
    {
        public string fechaEmision { get; set; }
        public string obligadoContabilidad { get; set; }
        public string tipoIdentificacionComprador { get; set; }
        public string razonSocialComprador { get; set; }
        public string identificacionComprador { get; set; }
        public string direccionComprador { get; set; }
        public string totalSinImpuestos { get; set; }
        public string totalDescuento { get; set; }
        public string propina { get; set; }
        public string importeTotal { get; set; }
        public string moneda { get; set; }
        public List<totalImpuesto> totalConImpuestos { get; set; }
        public List<pago> pagos { get; set; }
        public infoFactura()
        {
            totalConImpuestos = new List<totalImpuesto>();
            pagos = new List<pago>();
        }
    }
    public class totalImpuesto
    {
        public string codigo { get; set; }
        public string codigoPorcentaje { get; set; }
        public string baseImponible { get; set; }
        public string valor { get; set; }
    }
    public class pago
    {
        public string formaPago { get; set; }
        public string total { get; set; }
        public string plazo { get; set; }
        public string unidadTiempo { get; set; }
    }
    public class detalle
    {
        public string codigoPrincipal { get; set; }
        public string codigoAuxiliar { get; set; }
        public string descripcion { get; set; }
        public string cantidad { get; set; }
        public string precioUnitario { get; set; }
        public string descuento { get; set; }
        public string precioTotalSinImpuesto { get; set; }
        public List<detAdicional> detallesAdicionales { get; set; }//detAdicional
        public List<impuesto> impuestos { get; set; }
        public detalle()
        {
            detallesAdicionales = new List<detAdicional>();
            impuestos = new List<impuesto>();
        }
    }
    public class detAdicional
    {
        public string nombre { get; set; }
        public string valor { get; set; }
    }
    public class impuesto
    {
        public string codigo { get; set; }
        public string codigoPorcentaje { get; set; }
        public string tarifa { get; set; }
        public string baseImponible { get; set; }
        public string valor { get; set; }
    }
}
