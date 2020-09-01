using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using App_Mundial_Miles.Models;

namespace App_Mundial_Miles.ViewModels
{
    public class vmRetencionesSri
    {
        public List<RetencionesSri> Retenciones { get; set; }

        public vmRetencionesSri()
        {
            Retenciones = new List<RetencionesSri>();
        }

        public vmRetencionesSri(List<RetencionesSri> ventas)
        {
            Retenciones = new List<RetencionesSri>();

            var q = new List<RetencionesSri>();
            foreach (var ele in ventas)
            {
                q.Add(new RetencionesSri
                {
                    Beneficiario = ele.Beneficiario,
                    ProveedorRuc = ele.ProveedorRuc,
                    FechaRegistro = ele.FechaRegistro,
                    Establecimiento = ele.Establecimiento,
                    PtoEmision = ele.PtoEmision,
                    Secuencial = ele.Secuencial,
                    Autorizacion = ele.Autorizacion,
                    BaseImpGravada = ele.BaseImpGravada,
                    PorcentRetIva = ele.PorcentRetIva,
                    MontoIva = ele.MontoIva,
                    CodRetAir = ele.CodRetAir,
                    PorcentAir = ele.PorcentAir,
                    SecRet1 = ele.SecRet1,
                    AutRet1 = ele.AutRet1,
                    FechaEmisionRet = ele.FechaEmisionRet,
                    NumPlan = ele.NumPlan,
                    FechaCaduci = ele.FechaCaduci,
                    NCodRef = ele.NCodRef,
                    Orden = ele.Orden,
                    Responsable = ele.Responsable
                });
            }
            Retenciones.AddRange(q);
        }
    }
}