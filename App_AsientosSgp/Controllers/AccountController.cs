using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using App_Mundial_Miles.Models;
using AtisCode.Aplicacion.Model.db_Local;
using System.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;
using NLog;
using App_Mundial_Miles.Helpers;

namespace App_Mundial_Miles.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        SwitchAtiscodeEntities db = new SwitchAtiscodeEntities();

        private Logger logger = LogManager.GetCurrentClassLogger();

        public AccountController()
        {
            //string url = ConfigurationManager.AppSettings["apiBaseUri"].ToString();

            //Dictionary<string, string> headerParameter = new Dictionary<string, string>(){
            //    { "postman-token", "9df9f6d8-8101-8431-60c4-92e11b8f8e0a"},
            //    { "cache-control", "no-cache"},
            //    { "content-type", "application/x-www-form-urlencoded"}
            //};

            //Dictionary<string, string> bodyParameter = new Dictionary<string, string>(){
            //    { "application/x-www-form-urlencoded", "username=super&password=VFQnJ%2BdodRM%3D&ruc=" +  ConfigurationManager.AppSettings["ruc"].ToString() + "&year="+DateTime.Now.Year.ToString()+"&grant_type=password" }
            //};

            //CallAPIGetType call = new CallAPIGetType();
            //var response = call.SetRequestAPI(url, RestSharp.Method.POST, headerParameter, bodyParameter);

        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            try
            {
                var conexion = ConfigurationManager.ConnectionStrings["SwitchAtiscodeEntities"].ConnectionString;

                //SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["SafiEntities"].ConnectionString);
                //con.Open();
                //con.Close();

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
                { // Pruebas
                    var aplicacionesPruebas = db.AtisAPIConfiguracionEntorno.Where(s => s.TipoAmbiente == "PRUEBAS" && s.Estado).Select(s => s.AtisAplicacionID).Distinct().ToList();
                    ViewBag.listadoPuntos = aplicacionesPruebas != null ? new SelectList(db.ListarAplicaciones().Where(s => aplicacionesPruebas.Contains(s.AtisAplicacionID) && s.Estado).Select(s => new AplicacionesInfo { AtisAplicacionID = s.AtisAplicacionID, RazonSocial = s.RazonSocial + " - " + s.Segmento }), "AtisAplicacionID", "RazonSocial") : new SelectList(new List<SelectListItem> { });
                    ViewBag.informacionAmbiente = "(Se encuentra en el ambiente de pruebas.)";
                    ViewBag.totalListadoPuntos = aplicacionesPruebas != null ? db.SwitchTransacciones.Count() : 0;// 
                }
                else
                { // Produccion
                    var aplicacionesProduccion = db.AtisAPIConfiguracionEntorno.Where(s => s.TipoAmbiente == "PRODUCCION" || s.TipoAmbiente == "PRODUCCIÓN" && s.Estado).Select(s => s.AtisAplicacionID).Distinct().ToList();
                    ViewBag.listadoPuntos = aplicacionesProduccion != null ? new SelectList(db.ListarAplicaciones().Where(s => aplicacionesProduccion.Contains(s.AtisAplicacionID) && s.Estado).Select(s => new AplicacionesInfo { AtisAplicacionID = s.AtisAplicacionID, RazonSocial = s.RazonSocial + " - " + s.Segmento }), "AtisAplicacionID", "RazonSocial") : new SelectList(new List<SelectListItem> { });
                    ViewBag.informacionAmbiente = "(Se encuentra en el ambiente de producción.)";
                    ViewBag.totalListadoPuntos = aplicacionesProduccion != null ? db.SwitchTransacciones.Count() : 0;// 

                }

                ViewBag.ReturnUrl = returnUrl;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Stopped program because of exception");
                throw;
            }

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        //[ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model, string returnUrl)
        {
            try
            {
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
                { // Pruebas
                    var aplicacionesPruebas = db.AtisAPIConfiguracionEntorno.Where(s => s.TipoAmbiente == "PRUEBAS" && s.Estado).Select(s => s.AtisAplicacionID).Distinct().ToList();
                    ViewBag.listadoPuntos = aplicacionesPruebas != null ? new SelectList(db.AtisAplicacion.Where(s => s.Estado && aplicacionesPruebas.Contains(s.AtisAplicacionID)).ToList(), "AtisAplicacionID", "Nombre") : new SelectList(new List<SelectListItem> { });
                    ViewBag.informacionAmbiente = "(Se encuentra en el ambiente de pruebas.)";
                    ViewBag.totalListadoPuntos = aplicacionesPruebas != null ? db.SwitchTransacciones.Count() : 0;// 
                }
                else
                { // Produccion
                    var aplicacionesProduccion = db.AtisAPIConfiguracionEntorno.Where(s => s.TipoAmbiente == "PRODUCCION" && s.Estado).Select(s => s.AtisAplicacionID).Distinct().ToList();
                    ViewBag.listadoPuntos = aplicacionesProduccion != null ? new SelectList(db.AtisAplicacion.Where(s => s.Estado && aplicacionesProduccion.Contains(s.AtisAplicacionID)).ToList(), "AtisAplicacionID", "Nombre") : new SelectList(new List<SelectListItem> { });
                    ViewBag.informacionAmbiente = "(Se encuentra en el ambiente de producción.)";
                    ViewBag.totalListadoPuntos = aplicacionesProduccion != null ? db.SwitchTransacciones.Count() : 0;// 
                }

                if (ModelState.IsValid)
                {

                    // chequear el usuario
                    //var clave = FormsAuthentication.HashPasswordForStoringInConfigFile(model.Password, "MD5");
                    //clave = model.Password; // borrar
                    //var usuarios = Model.Usuario.GetElements(t => t.Login == model.UserName && t.Pwd == clave && t.Estado == 1, "Empresa");
                    if (model.UserName == "admin" && model.Password == "admin")
                    {
                        Session["Usuario"] = "admin";
                        FormsAuthentication.RedirectFromLoginPage(model.UserName, model.RememberMe);


                        #region Cargar Archivo de configuraciones para emisión de canal
                        var aplicacionDetalle = db.ListarAplicaciones().Where(s => s.AtisAplicacionID == Int32.Parse(model.Aplicacion)).FirstOrDefault();

                        //logger.Info("Configuracion Inicial"+ aplicacionDetalle.Nombre + " ;" + aplicacionDetalle.Segmento + " ;" + aplicacionDetalle.RazonSocial);

                        Tools.ActualizarParametroAPPSettings(new ConfiguracionGlobal() { Key = "nombreAplicacion", Value = aplicacionDetalle.Nombre }); // Actualizando TipoAmbiente del web config
                        Tools.ActualizarParametroAPPSettings(new ConfiguracionGlobal() { Key = "canal", Value = aplicacionDetalle.Segmento }); // Actualizando TipoAmbiente del web config
                        Tools.ActualizarParametroAPPSettings(new ConfiguracionGlobal() { Key = "Empresa", Value = aplicacionDetalle.RazonSocial }); // Actualizando TipoAmbiente del web config

                        string IPCliente = Request.UserHostAddress;

                        logger.Info("Se generaron cambios en el archivo de configuración de canal de emisión de documentos!!!");
                        logger.Info("Usuario: " + IPCliente + " - Canal que seleccionó: " + aplicacionDetalle.Segmento);

                        var segmento = ConfigurationManager.AppSettings["canal"];
                        var aplicacion = ConfigurationManager.AppSettings["nombreAplicacion"];

                        var aplicacionID = db.AtisAplicacion.FirstOrDefault(s => s.Nombre == aplicacion && s.Estado).AtisAplicacionID;
                        var ambiente = db.AtisAPIConfiguracionEntorno.Where(s => s.AtisAplicacionID == aplicacionID).ToList().First(); // El ambiente que fue seleccionado durante la creación de la aplicacion centralizada

                        Tools.ActualizarParametroAPPSettings(new ConfiguracionGlobal() { Key = "TipoAmbiente", Value = ambiente.TipoAmbiente }); // Actualizando TipoAmbiente del web config

                        var enlacesConexionWS = db.ConsultarElancesWebServicesAmbiente(aplicacion, ConfigurationManager.AppSettings["TipoAmbiente"]).ToList(); // Buscando los enlaces API con el nombre de la aplicación y el tipo Ambiente

                        var configuracion = db.ConsultarConfiguracionPrincipal(aplicacion).FirstOrDefault();
                        var diccionario = Tools.ToDictionary<string>(configuracion);

                        var configuraciones = diccionario.Select(s => new ConfiguracionGlobal() { Key = s.Key, Value = s.Value }).ToList();

                        foreach (var item in configuraciones)
                        {
                            switch (item.Key)
                            {
                                case "Iva":
                                    item.Key = "iva";
                                    break;
                                case "Ambiente":
                                    item.Key = "ambiente";
                                    break;
                                case "Establecimiento":
                                    item.Key = "estab";
                                    break;
                                case "PtoEmi":
                                    item.Key = "ptoEmi";
                                    break;
                                case "RazonSocial":
                                    item.Key = "razonSocial";
                                    break;
                                case "RUC":
                                    item.Key = "ruc";
                                    break;
                                case "DireccionMatriz":
                                    item.Key = "dirMatriz";
                                    break;
                                case "Segmento":
                                    item.Key = "segmento";
                                    break;
                                case "CodProducto":
                                    item.Key = "codProducto";
                                    break;
                                case "NombProducto":
                                    item.Key = "nombProducto";
                                    break;
                                case "Uge":
                                    item.Key = "UGE";
                                    break;
                                case "Tipo":
                                    item.Key = "FormaPago";
                                    break;
                            }

                            Tools.ActualizarParametroAPPSettings(item);
                        }

                        foreach (var item in enlacesConexionWS)
                        {
                            ConfiguracionGlobal configuracionElance = new ConfiguracionGlobal
                            {
                                Key = item.Nombre,
                                Value = item.UrlAPI
                            };
                            //configuraciones.Add(configuracionElance);
                            Tools.ActualizarParametroAPPSettings(configuracionElance);
                        }

                        //Tools.ActualizarAPPSettings(configuraciones);

                        #endregion


                        //if (!string.IsNullOrEmpty(model.Aplicacion))
                        //{
                        //    var aplicacionDetalle = db.ListarAplicaciones().Where(s => s.AtisAplicacionID == Int32.Parse(model.Aplicacion)).FirstOrDefault();

                        //    if (aplicacionDetalle != null)
                        //    {
                        //        Tools.ActualizarParametroAPPSettings(new ConfiguracionGlobal() { Key = "nombreAplicacion", Value = aplicacionDetalle.Nombre }); // Actualizando TipoAmbiente del web config
                        //        Tools.ActualizarParametroAPPSettings(new ConfiguracionGlobal() { Key = "canal", Value = aplicacionDetalle.Segmento }); // Actualizando TipoAmbiente del web config
                        //        Tools.ActualizarParametroAPPSettings(new ConfiguracionGlobal() { Key = "Empresa", Value = aplicacionDetalle.RazonSocial }); // Actualizando TipoAmbiente del web config
                        //    }

                        //    if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["canal"].ToString()) && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["nombreAplicacion"].ToString()))
                        //    {

                        //        var segmento = ConfigurationManager.AppSettings["canal"];
                        //        var aplicacion = ConfigurationManager.AppSettings["nombreAplicacion"];

                        //        var aplicacionID = db.AtisAplicacion.FirstOrDefault(s => s.Nombre == aplicacion && s.Estado).AtisAplicacionID;
                        //        var ambiente = db.AtisAPIConfiguracionEntorno.Where(s => s.AtisAplicacionID == aplicacionID).ToList().First(); // El ambiente que fue seleccionado durante la creación de la aplicacion centralizada

                        //        Tools.ActualizarParametroAPPSettings(new ConfiguracionGlobal() { Key = "TipoAmbiente", Value = ambiente.TipoAmbiente }); // Actualizando TipoAmbiente del web config

                        //        var enlacesConexionWS = db.ConsultarElancesWebServicesAmbiente(aplicacion, ConfigurationManager.AppSettings["TipoAmbiente"]).ToList(); // Buscando los enlaces API con el nombre de la aplicación y el tipo Ambiente

                        //        var configuracion = db.ConsultarConfiguracionPrincipal(aplicacion).FirstOrDefault();
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
                        //                case "Establecimiento":
                        //                    item.Key = "estab";
                        //                    break;
                        //                case "PtoEmi":
                        //                    item.Key = "ptoEmi";
                        //                    break;
                        //                case "RazonSocial":
                        //                    item.Key = "razonSocial";
                        //                    break;
                        //                case "RUC":
                        //                    item.Key = "ruc";
                        //                    break;
                        //                case "DireccionMatriz":
                        //                    item.Key = "dirMatriz";
                        //                    break;
                        //                case "Segmento":
                        //                    item.Key = "segmento";
                        //                    break;
                        //                case "CodProducto":
                        //                    item.Key = "codProducto";
                        //                    break;
                        //                case "NombProducto":
                        //                    item.Key = "nombProducto";
                        //                    break;
                        //                case "Uge":
                        //                    item.Key = "UGE";
                        //                    break;
                        //                case "Tipo":
                        //                    item.Key = "FormaPago";
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


                        return RedirectToAction("Index", "Home", new { tipo = 1 });
                    }
                    else
                    {
                        ModelState.AddModelError("", "Nombre de usuario o contraseña incorrecta.");
                        return View(model);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Nombre de usuario o contraseña incorrecta.");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Stopped program because of exception");
                throw;
            }
        }

        //
        // GET: /Account/VerifyCode
        [AllowAnonymous]
        public async Task<ActionResult> VerifyCode(string provider, string returnUrl, bool rememberMe)
        {
            // Requerir que el usuario haya iniciado sesión con nombre de usuario y contraseña o inicio de sesión externo
            if (!await SignInManager.HasBeenVerifiedAsync())
            {
                return View("Error");
            }
            return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/VerifyCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyCode(VerifyCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // El código siguiente protege de los ataques por fuerza bruta a los códigos de dos factores. 
            // Si un usuario introduce códigos incorrectos durante un intervalo especificado de tiempo, la cuenta del usuario 
            // se bloqueará durante un período de tiempo especificado. 
            // Puede configurar el bloqueo de la cuenta en IdentityConfig
            var result = await SignInManager.TwoFactorSignInAsync(model.Provider, model.Code, isPersistent: model.RememberMe, rememberBrowser: model.RememberBrowser);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(model.ReturnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Código no válido.");
                    return View(model);
            }
        }

        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                    // Para obtener más información sobre cómo habilitar la confirmación de cuenta y el restablecimiento de contraseña, visite http://go.microsoft.com/fwlink/?LinkID=320771
                    // Enviar correo electrónico con este vínculo
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirmar cuenta", "Para confirmar la cuenta, haga clic <a href=\"" + callbackUrl + "\">aquí</a>");

                    return RedirectToAction("Index", "Home");
                }
                AddErrors(result);
            }

            // Si llegamos a este punto, es que se ha producido un error y volvemos a mostrar el formulario
            return View(model);
        }

        //
        // GET: /Account/ConfirmEmail
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return View("Error");
            }
            var result = await UserManager.ConfirmEmailAsync(userId, code);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // No revelar que el usuario no existe o que no está confirmado
                    return View("ForgotPasswordConfirmation");
                }

                // Para obtener más información sobre cómo habilitar la confirmación de cuenta y el restablecimiento de contraseña, visite http://go.microsoft.com/fwlink/?LinkID=320771
                // Enviar correo electrónico con este vínculo
                // string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                // var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "Restablecer contraseña", "Para restablecer la contraseña, haga clic <a href=\"" + callbackUrl + "\">aquí</a>");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // Si llegamos a este punto, es que se ha producido un error y volvemos a mostrar el formulario
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // No revelar que el usuario no existe
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Solicitar redireccionamiento al proveedor de inicio de sesión externo
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/SendCode
        [AllowAnonymous]
        public async Task<ActionResult> SendCode(string returnUrl, bool rememberMe)
        {
            var userId = await SignInManager.GetVerifiedUserIdAsync();
            if (userId == null)
            {
                return View("Error");
            }
            var userFactors = await UserManager.GetValidTwoFactorProvidersAsync(userId);
            var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
            return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
        }

        //
        // POST: /Account/SendCode
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SendCode(SendCodeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Generar el token y enviarlo
            if (!await SignInManager.SendTwoFactorCodeAsync(model.SelectedProvider))
            {
                return View("Error");
            }
            return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return RedirectToAction("Login");
            }

            // Si el usuario ya tiene un inicio de sesión, iniciar sesión del usuario con este proveedor de inicio de sesión externo
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // Si el usuario no tiene ninguna cuenta, solicitar que cree una
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Email });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Manage");
            }

            if (ModelState.IsValid)
            {
                // Obtener datos del usuario del proveedor de inicio de sesión externo
                var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return View("ExternalLoginFailure");
                }
                var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
                var result = await UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return RedirectToLocal(returnUrl);
                    }
                }
                AddErrors(result);
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Aplicaciones auxiliares
        // Se usa para la protección XSRF al agregar inicios de sesión externos
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}