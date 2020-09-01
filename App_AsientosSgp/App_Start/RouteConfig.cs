using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace App_Mundial_Miles
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            //routes.MapRoute(
            //    name: "CargaFactura",
            //    url: "CargaFactura",
            //    defaults: new { controller = "Factura", action = "Index", id = UrlParameter.Optional }
            //);

            //routes.MapRoute(
            //    name: "CargaNotasCredito",
            //    url: "CargaNotasCredito",
            //    defaults: new { controller = "NotasCredito", action = "Index", id = UrlParameter.Optional }
            //);

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Account", action = "Login", id = UrlParameter.Optional }
            );
        }
    }
}
