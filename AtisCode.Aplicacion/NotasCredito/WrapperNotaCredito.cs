using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtisCode.Aplicacion
{
    public partial class WrapperNotaCredito
    {
        public Cabecera NotaCredito { get; set; }
    }
    public partial class Cabecera
    {
        public NotaCredito Detalle { get; set; }
    }
    public partial class NotaCredito
    {
        public string Idfactura { get; set; }
        public string Motivo { get; set; }
        public decimal Valor { get; set; }
        public string Secuencial { get; set; }        
        public int Estado { get; set; }
        public List<DetalleNotaCredito> DetallesNotaCredito { get; set; }
        public NotaCredito()
        {
            DetallesNotaCredito = new List<DetalleNotaCredito>();
        }
    }
    public partial class DetalleNotaCredito
    {
        public int Cantidad { get; set; }
        public string Detalle { get; set; }
        public decimal Valor { get; set; }
        public decimal SubTotal { get; set; }
        public string CodigoCategoria { get; set; }
        public string CodigoProducto { get; set; }
        public string RUCProveedor { get; set; }
        public string Proveedor { get; set; }
        public decimal CostoUnitario { get; set; }
        public DateTime FechaVenta { get; set; }
    }
    //public partial class DetalleNotaCredito
    //{
    //    public NotaCredito notacredito { get; set; }
    //}
}
