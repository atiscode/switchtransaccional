using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace App_Mundial_Miles.Models
{
    public class RetencionesSri
    {
        public string Beneficiario { get; set; }
        public int CodSustento
        {
            get
            {
                return 0;
            }
        }
        public int ProveedorId
        {
            get
            {
                return 1;
            }
        }
        public string ProveedorRuc { get; set; }
        public int TipoComprobante
        {
            get
            {
                return 1;
            }
        }
        public DateTime? FechaRegistro { get; set; }
        public string Establecimiento { get; set; }
        public string PtoEmision { get; set; }
        public string Secuencial { get; set; }
        public DateTime? FechaEmision
        {
            get
            {
                var _fecha = FechaRegistro;
                return _fecha;
            }
        }
        public string Autorizacion { get; set; }
        public decimal BaseNoGravaIva
        {
            get
            {
                return 0;
            }
        }
        public decimal BaseImponible
        {
            get
            {
                return 0;
            }
        }
        public decimal BaseImpGravada { get; set; }
        public decimal BaseExcenta
        {
            get
            {
                return 0;
            }
        }
        public decimal PorcentRetIva { get; set; }
        public decimal MontoIce
        {
            get
            {
                return 0;
            }
        }
        public Single MontoIva { get; set; }
        public decimal RetBienes10
        {
            get
            {
                return 0;
            }
        }
        public decimal RetServicios20 { get; set; }
        public decimal RetServicios30
        {
            get
            {
                decimal res = 0;
                if (PorcentRetIva == 30)
                    res = (decimal)MontoIva * 30 / 100;
                return res;
            }
        }
        public decimal RetBienes70
        {
            get
            {
                decimal res = 0;
                if (PorcentRetIva == 70)
                    res = (decimal)MontoIva * 70 / 100;
                return res;
            }
        }
        public decimal RetServicios100
        {
            get
            {
                decimal res = 0;
                if (PorcentRetIva == 100)
                    res = (decimal)MontoIva;
                return res;
            }
        }
        public decimal TotalBasesImp
        {
            get
            {
                decimal res = (decimal)BaseImpGravada;
                return res;
            }
        }
        public int CodRetAir { get; set; }
        public decimal BaseImpAir
        {
            get
            {
                decimal res = (decimal)BaseImpGravada;
                return res;
            }
        }
        public decimal PorcentAir { get; set; }
        public decimal ValorRetAir{ get; set; }
        public string Establecimiento1
        {
            get
            {
                return "001";
            }
        }
        public string PtoEmision1
        {
            get
            {
                return "001";
            }
        }
        public int SecRet1 { get; set; }
        public string AutRet1 { get; set; }
        public DateTime? FechaEmisionRet { get; set; }
        public int DocModificado
        {
            get
            {
                return 0;
            }
        }
        public int EstabMod
        {
            get
            {
                return 0;
            }
        }
        public int PtoEmisionMod
        {
            get
            {
                return 0;
            }
        }
        public int SecMod
        {
            get
            {
                return 0;
            }
        }
        public int AutMod
        {
            get
            {
                return 0;
            }
        }
        public int NumPlan { get; set; }
        public string FechaCaduci { get; set; }
        public int NCodRef { get; set; }
        public int Orden { get; set; }
        public string Responsable { get; set; }
    }
}