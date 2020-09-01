using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtisCode.Aplicacion
{
    public class notaCredito
    {
        public string id { get; set; }
        public string version { get; set; }
        public infoTributaria infoTributaria { get; set; }
        public infoNotaCredito infoNotaCredito { get; set; }
        public List<detalleRet> detalles { get; set; }
        public Dictionary<string, string> infoAdicional { get; set; }//campoAdicional
        public notaCredito()
        {
            detalles=new List<detalleRet>();
            infoAdicional = new Dictionary<string, string>();
        }
    }
    public class infoNotaCredito
    {
        public string fechaEmision { get; set; }
        public string dirEstablecimiento { get; set; }
        public string tipoIdentificacionComprador { get; set; }
        public string razonSocialComprador { get; set; }
        public string identificacionComprador { get; set; }
        public string obligadoContabilidad { get; set; }
        public string rise { get; set; }
        public string codDocModificado { get; set; }
        public string numDocModificado { get; set; }
        public string fechaEmisionDocSustento { get; set; }
        public string totalSinImpuestos { get; set; }
        public string valorModificacion { get; set; }
        public string moneda { get; set; }
        public List<totalImpuesto> totalConImpuestos { get; set; }
        public string motivo { get; set; }

        public infoNotaCredito()
        {
            totalConImpuestos = new List<totalImpuesto>();
        }
    }
    public class detalleRet
    {
        public string codigoInterno { get; set; }
        public string codigoAdicional { get; set; }
        public string descripcion { get; set; }
        public string cantidad { get; set; }
        public string precioUnitario { get; set; }
        public string descuento { get; set; }
        public string precioTotalSinImpuesto { get; set; }
        public List<detAdicional> detallesAdicionales { get; set; }//detAdicional
        public List<impuesto> impuestos { get; set; }
        public detalleRet()
        {
            detallesAdicionales = new List<detAdicional>();
            impuestos = new List<impuesto>();
        }
    }
}
