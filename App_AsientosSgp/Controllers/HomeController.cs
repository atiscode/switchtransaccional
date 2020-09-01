using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using App_Mundial_Miles.Models;
using App_Mundial_Miles;
using App_Mundial_Miles.Bussines;
using App_Mundial_Miles.Helpers;
using AtisCode.Aplicacion.Model.db_Safi;
using AtisCode.Aplicacion.Model.db_Local;
using Newtonsoft.Json;
using System.Data.SqlClient;
using NLog;

namespace App_Mundial_Miles.Controllers
{
    public class HomeController : Controller
    {
        SwitchAtiscodeEntities db = new SwitchAtiscodeEntities();
        private Logger logger = LogManager.GetCurrentClassLogger();

        [OutputCache(Duration = 0, VaryByParam = "none")]
        public ActionResult Index(string path = "")
        {
            try
            {

                ViewBag.Message = "Generar Factura Contable";

                ViewBag.NombreAplicacion = ConfigurationManager.AppSettings["nombreAplicacion"];
                ViewBag.Segmento = ConfigurationManager.AppSettings["segmento"];
                ViewBag.TipoAmbiente = ConfigurationManager.AppSettings["TipoAmbiente"];
                ViewBag.Empresa = ConfigurationManager.AppSettings["Empresa"];

                var conexion = ConfigurationManager.ConnectionStrings["SwitchAtiscodeEntities"].ConnectionString;

                string[] parts = conexion.Split(';');
                string dataSource = "";
                for (int i = 0; i < parts.Length; i++)
                {
                    string part = parts[i].Trim();
                    if (part.Contains("data source="))
                    {
                        dataSource = part.Replace("provider connection string=\"data source=", "");
                        break;
                    }
                }
                bool flag = Tools.VerificarConexionesAmbiente(dataSource);

                if (flag)
                    ViewBag.AmbienteBD = "Base de pruebas - " + dataSource;
                else
                    ViewBag.AmbienteBD = "Base de producción - " + dataSource;

                var registrosCanalaSecuenciales = db.ConsultarSecuencialCanal(ConfigurationManager.AppSettings["segmento"]).FirstOrDefault();
                if (registrosCanalaSecuenciales != null)
                {
                    ViewBag.SecuencialFC = registrosCanalaSecuenciales.SecuencialFacturas;
                    ViewBag.SecuencialNC = registrosCanalaSecuenciales.SecuencialNotaCredito;
                }
                else
                {
                    ViewBag.SecuencialFC = "- Ninguno -";
                    ViewBag.SecuencialNC = "- Ninguno -";
                }


                List<string> configuraciones = ConfigurationManager.AppSettings.AllKeys.Select(key => ConfigurationManager.AppSettings[key]).ToList();

                //var model = new BusinessManager();

                var menu = Session["Menu"] as List<MenuModel> ?? DefineMenu();
                //if (Session["Usuario"] != null)
                //{
                //    var user = (Usuario)Session["Usuario"];
                //    ViewBag.NombreUsuario = user.Nombre;
                //}

                if (Session["lastPath"] != null && path == "")
                    path = Session["lastPath"].ToString();

                // Crea bien la url en cualquier caso y la guarda en una variable global de jquery
                string webapp = this.Request.ApplicationPath;
                string hostapp = this.Request.Url.Scheme + "://" + this.Request.Url.Authority;
                string prefijoUrl = hostapp + webapp;
                if (!prefijoUrl.EndsWith("/")) prefijoUrl = prefijoUrl + "/";
                ViewBag.Host = prefijoUrl;
                Session["prefijoUrl"] = prefijoUrl;

                //if (Session["Aplicacion"] == null || Session["Usuario"] == null)
                //    return RedirectToAction("Exit");
                //var application = Session["Aplicacion"] as Aplicacion;

                var idiomaActual = "es-EC";
                Session["culture"] = idiomaActual;
                ViewBag.Idioma = idiomaActual;

                Thread.CurrentThread.CurrentUICulture = new CulturaPropia();
                Session["culture"] = idiomaActual;


                var id = path.GetIdFromPath();
                ViewBag.EditItemId = id;
                return View("Index", menu);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Stopped program because of exception");
                return View("Index", new List<MenuModel>());
            }
        }

        public static Dictionary<string, TValue> ToDictionary<TValue>(object obj)
        {
            var json = JsonConvert.SerializeObject(obj);
            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, TValue>>(json);
            return dictionary;
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public ActionResult CreaMenu(string dir)
        {
            var menu = Session["Menu"] as List<MenuModel> ?? DefineMenu();
            return PartialView("_menu", menu);
        }
        [OutputCache(Duration = 0, VaryByParam = "none")]
        public RedirectToRouteResult ResponseMenu(string Key)
        {
            var user = Session["Usuario"];
            var tipo = (string)user == "ppm" ? 2 : 1;
            Session["filter_order"] = null;
            switch (Key)
            {
                //Reportes
                case "_R1_M1_":
                    Session["lastPath"] = "CargaFactura/Index";
                    return RedirectToAction("Index", "CargaFactura");
                case "_R1_M2_":
                    Session["lastPath"] = "CargaNotasCredito/Index";
                    return RedirectToAction("Index", "CargaNotasCredito");
                case "_R1_M3_":
                    Session["lastPath"] = "Cotizaciones/Index";
                    return RedirectToAction("Index", "Cotizaciones");
            }
            return null;
        }
        private List<MenuModel> DefineMenu()
        {
            var menu = new List<MenuModel>();
            var user = Session["Usuario"];

            var R1 = new MenuModel(null, "Opciones", "Opciones", "_R1_", MenuModel.TipoElemento.Carpeta);
            var R1_M1 = new MenuModel(R1, "Facturas", "Gestionar Facturas", "_R1_M1_", MenuModel.TipoElemento.Elemento);
            var R1_M2 = new MenuModel(R1, "NotasCredito", "Notas Crédito", "_R1_M2_", MenuModel.TipoElemento.Elemento);
            var R1_M3 = new MenuModel(R1, "Cotizaciones", "Cotizaciones", "_R1_M3_", MenuModel.TipoElemento.Elemento);

            menu.Add(R1);
            menu.Add(R1_M1);
            menu.Add(R1_M2);
            menu.Add(R1_M3);

            Session.Add("Menu", menu);

            // Crea bien la url en cualquier caso y la guarda en una variable global de jquery
            string webapp = this.Request.ApplicationPath;
            string hostapp = this.Request.Url.Scheme + "://" + this.Request.Url.Authority;
            string prefijoUrl = hostapp + webapp;
            if (!prefijoUrl.EndsWith("_")) prefijoUrl = prefijoUrl + "_";
            ViewBag.Host = prefijoUrl;
            Session["prefijoUrl"] = prefijoUrl;

            return menu;
        }
        public ActionResult Exit()
        {
            Session.Clear();
            Session.Abandon();
            FormsAuthentication.SignOut();
            //WebSecurity.Logout();
            return RedirectToAction("Login", "Account");
        }
    }
}