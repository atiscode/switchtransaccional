using App_Mundial_Miles.App_Start;
using App_Mundial_Miles.Models;
using AtisCode.Aplicacion.Model.db_Local;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace App_Mundial_Miles
{
    public class MvcApplication : System.Web.HttpApplication
    {
        SwitchAtiscodeEntities db = new SwitchAtiscodeEntities();

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        //public override void Init()
        //{
        //    base.Init();

        //    if (!EstadoConfiguraciones.IsLoad)
        //    {

        //        var segmento = ConfigurationManager.AppSettings["canal"];
        //        var aplicacion = ConfigurationManager.AppSettings["nombreAplicacion"];

        //        var aplicacionID = db.AtisAplicacion.FirstOrDefault(s => s.Nombre == aplicacion && s.Estado).AtisAplicacionID;
        //        var ambiente = db.AtisAPIConfiguracionEntorno.Where(s => s.AtisAplicacionID == aplicacionID).ToList().First(); // El ambiente que fue seleccionado durante la creación de la aplicacion centralizada

        //        Tools.ActualizarParametroAPPSettings( new ConfiguracionGlobal() { Key = "TipoAmbiente", Value = ambiente.TipoAmbiente }); // Actualizando TipoAmbiente del web config

        //        var enlacesConexionWS = db.ConsultarElancesWebServicesAmbiente(aplicacion, ConfigurationManager.AppSettings["TipoAmbiente"]).ToList(); // Buscando los enlaces API con el nombre de la aplicación y el tipo Ambiente

        //        var configuracion = db.ConsultarConfiguracionPrincipal(segmento).FirstOrDefault();
        //        var diccionario = Tools.ToDictionary<string>(configuracion);

        //        var configuraciones = diccionario.Select(s => new ConfiguracionGlobal() { Key = s.Key, Value = s.Value }).ToList();

        //        foreach (var item in configuraciones)
        //        {
        //            switch (item.Key)
        //            {
        //                case "Iva":
        //                    item.Key = "iva";
        //                    break;
        //                case "Ambiente":
        //                    item.Key = "ambiente";
        //                    break;
        //                case "Estab":
        //                    item.Key = "estab";
        //                    break;
        //                case "PtoEmi":
        //                    item.Key = "ptoEmi";
        //                    break;
        //                case "RazonSocial":
        //                    item.Key = "razonSocial";
        //                    break;
        //                case "Ruc":
        //                    item.Key = "ruc";
        //                    break;
        //                case "DirMatriz":
        //                    item.Key = "dirMatriz";
        //                    break;
        //                case "Segmento":
        //                    item.Key = "segmento";
        //                    break;
        //                case "CodProducto":
        //                    item.Key = "codProducto";
        //                    break;
        //                case "NombreProducto":
        //                    item.Key = "nombProducto";
        //                    break;
        //                case "Uge":
        //                    item.Key = "UGE";
        //                    break;
        //            }
        //        }

        //        foreach (var item in enlacesConexionWS)
        //        {
        //            ConfiguracionGlobal configuracionElance = new ConfiguracionGlobal
        //            {
        //                Key = item.Nombre,
        //                Value = item.UrlAPI
        //            };
        //            configuraciones.Add(configuracionElance);
        //        }
        //        EstadoConfiguraciones.IsLoad = Tools.ActualizarAPPSettings(configuraciones);
        //    }
        //}


    }
}
