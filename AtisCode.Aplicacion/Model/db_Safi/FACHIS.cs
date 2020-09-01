//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AtisCode.Aplicacion.Model.db_Safi
{
    using System;
    using System.Collections.Generic;
    
    public partial class FACHIS
    {
        public int FHID { get; set; }
        public string FHFactura { get; set; }
        public Nullable<int> FHNum_doc { get; set; }
        public int FHTipo { get; set; }
        public int FHClave { get; set; }
        public string FHRuc { get; set; }
        public Nullable<int> FHClave1 { get; set; }
        public Nullable<int> FHDividendos { get; set; }
        public Nullable<System.DateTime> FHFechaf { get; set; }
        public Nullable<System.DateTime> FHFechav { get; set; }
        public Nullable<int> FHProducto { get; set; }
        public Nullable<decimal> FHDcto { get; set; }
        public Nullable<decimal> FHFlete { get; set; }
        public Nullable<decimal> FHOtros { get; set; }
        public Nullable<decimal> FHGabrado0 { get; set; }
        public Nullable<decimal> FHGrabrado { get; set; }
        public Nullable<decimal> FHImpto { get; set; }
        public Nullable<decimal> FHDctog { get; set; }
        public Nullable<int> FHCodRelacionID { get; set; }
        public string FHEstado { get; set; }
        public Nullable<int> FHCodigo { get; set; }
        public Nullable<int> FHBodega { get; set; }
        public string FHHora { get; set; }
        public Nullable<int> FHTd { get; set; }
        public string FHNombre { get; set; }
        public Nullable<int> FHZona { get; set; }
        public string FHComent { get; set; }
        public string FHComent1 { get; set; }
        public string FHComent2 { get; set; }
        public string FHTelf1 { get; set; }
        public Nullable<decimal> FHVentas { get; set; }
        public Nullable<int> FHNo_precio { get; set; }
        public string FHAnticipo { get; set; }
        public Nullable<int> FHN_dias { get; set; }
        public Nullable<int> FHT_pos { get; set; }
        public Nullable<decimal> FHT_posdev { get; set; }
        public int FHPos { get; set; }
        public Nullable<int> FHCodmon { get; set; }
        public Nullable<decimal> FHMontomon { get; set; }
        public Nullable<decimal> FHT_cambio { get; set; }
        public Nullable<System.DateTime> FHFechaC { get; set; }
        public Nullable<int> FHComp { get; set; }
        public string FHInc_iva { get; set; }
        public Nullable<decimal> FHNo_forma { get; set; }
        public Nullable<int> FHC_costo { get; set; }
        public string FHGuia { get; set; }
        public string FHTransporte { get; set; }
        public string FHCitrans { get; set; }
        public string FHPlacatrans { get; set; }
        public Nullable<System.DateTime> FHFechaenvio { get; set; }
        public Nullable<System.DateTime> FHFechaRecep { get; set; }
        public Nullable<decimal> FHIva_bien { get; set; }
        public Nullable<decimal> FHIva_serv { get; set; }
        public Nullable<decimal> FHTotgeneral { get; set; }
        public Nullable<bool> FHAutorizacion { get; set; }
        public Nullable<int> FHCodUsuAutoriza { get; set; }
        public Nullable<System.DateTime> FHFch_Impresion { get; set; }
        public Nullable<int> FHAsientoCostoVenta { get; set; }
        public Nullable<int> FHFacNv { get; set; }
        public Nullable<int> FHIdIvaPre { get; set; }
        public Nullable<decimal> FHValorIvaPre { get; set; }
        public Nullable<decimal> FHIce { get; set; }
        public Nullable<decimal> FHTotalNeto { get; set; }
        public string FHUsuario { get; set; }
        public Nullable<decimal> FHDescIva { get; set; }
        public Nullable<decimal> FHDesc0 { get; set; }
        public Nullable<int> FHClave2 { get; set; }
        public Nullable<int> FHClave3 { get; set; }
        public Nullable<int> FHClave4 { get; set; }
        public Nullable<int> FHClave5 { get; set; }
        public Nullable<int> FHNoOp { get; set; }
        public Nullable<int> FHClaveCliente2 { get; set; }
        public Nullable<decimal> FHNo_Objeto_IVA { get; set; }
        public bool FHDocumentoElectronico { get; set; }
        public string FHNumeroAutorizacion { get; set; }
        public Nullable<System.DateTime> FHFechaAutorizacion { get; set; }
        public string FHClaveAcceso { get; set; }
        public string FHAsiento { get; set; }
        public string FHNumeroComprobante { get; set; }
        public Nullable<int> FHCompSRIID { get; set; }
        public string FHCompSRITD { get; set; }
        public string FHCompSRIDetalle { get; set; }
        public Nullable<bool> FHExportador { get; set; }
        public string FHComercioExterior { get; set; }
        public string FHIncoTermFacturaCIF { get; set; }
        public string FHLugarIncoTerm { get; set; }
        public Nullable<int> FHPaisOrigen { get; set; }
        public string FHPuertoEmbarque { get; set; }
        public string FHPuertoDestino { get; set; }
        public Nullable<int> FHPaisDestino { get; set; }
        public Nullable<int> FHPaisAdquisicion { get; set; }
        public string FHTotalSinImpuestoFOB { get; set; }
        public Nullable<decimal> FHFleteInternacional { get; set; }
        public Nullable<decimal> FHSeguroInternacional { get; set; }
        public Nullable<decimal> FHGastosAduaneros { get; set; }
        public Nullable<decimal> FHOtrosGastosTransporte { get; set; }
    }
}
