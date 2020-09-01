using System.Globalization;
using System.Threading;
using System.Web;
using System.Web.Mvc;


namespace App_Mundial_Miles
{
    /// <summary>
    /// Atributo que puede ser adicinado a una controlador o a una accion para forzar que el contenido
    /// funcione con la culture que esté ene se momento
    /// </summary>
    public class ChangeLanguageAttribute : ActionFilterAttribute
    {

        /// <summary>
        /// Sobreescribe para cambiar de lenguaje
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (HttpContext.Current.Session["culture"] != null)
            {
                CultureInfo culture =
                    CultureInfo.CreateSpecificCulture(HttpContext.Current.Session["culture"].ToString());
                culture.DateTimeFormat = CultureInfo.GetCultureInfo("es-CL").DateTimeFormat ; // fecha por ahora con este formato
                Thread.CurrentThread.CurrentCulture = culture;
                Thread.CurrentThread.CurrentUICulture = culture;
            }
        }

    }
}