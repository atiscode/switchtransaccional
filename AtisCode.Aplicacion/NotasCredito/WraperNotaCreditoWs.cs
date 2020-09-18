using Newtonsoft.Json;
using NLog.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using AtisCode.Aplicacion.Helpers;
using System.ComponentModel.DataAnnotations;

namespace AtisCode.Aplicacion.NotasCredito
{
    public partial class WraperNotaCreditoWs
    {
        public WraperNotaCreditoWs()
        {
            EsCargaDocumentos = false;
        }
        [Required]
        public CabeceraWs NotaCredito { get; set; }
        public bool EsCargaDocumentos { get; set; }
    }
    public partial class CabeceraWs
    {
        [Required]
        public NotaCreditoWs Detalle { get; set; }
    }
    public partial class DetalleNotaCreditoWs
    {
        public NotaCreditoWs notacredito { get; set; }
    }
    public partial class DetalleNotaCreditoWs

    {
        //// [JsonProperty("CodigoCliente")]
        [Required]
        public int Cantidad { get; set; }

        //// [JsonProperty("NombreCliente")]
        [Required]
        public string Detalle { get; set; }

        //// [JsonProperty("RazonSocial")]
        [Required]
        public decimal Valor { get; set; }

        //Adicionales
        [Required]
        public decimal SubTotal { get; set; }
        [Required]
        public string CodigoCategoria { get; set; }
        [Required]
        public string CodigoProducto { get; set; }
        [Required]
        public string RUCProveedor { get; set; }
        [Required]
        public string Proveedor { get; set; }
        [Required]
        public decimal CostoUnitario { get; set; }

        //[JsonIgnore]
        [Required]
        [JsonConverter(typeof(CustomDateTimeConverter))]
        [DataMember(Name = "FechaVenta")]
        public DateTime? FechaVenta { get; set; }
    }
    public partial class NotaCreditoWs
    {
        [Required]
        public string Idfactura { get; set; }
        [Required]
        public string Motivo { get; set; }
        [Required]
        public decimal Valor { get; set; }
        [Required]
        public string Secuencial { get; set; }
        [Required]
        public List<DetalleNotaCreditoWs> DetalleNotaCredito { get; set; }
        [Required]
        public int Estado { get; set; }
    }
}
