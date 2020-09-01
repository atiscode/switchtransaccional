using System.Web;
using System.Web.Mvc;


namespace App_Mundial_Miles
{
    /// <summary>
    /// Atributo que puede ser adicinado a una controlador o a una accion para forzar que el contenido
    /// se comprima con Gzip o Deflate si el navegador cliente lo soporta
    /// </summary>
    public class CompressContentAttribute : ActionFilterAttribute
    {

        /// <summary>
        /// Sobreescribe para comprimir el contenido generado por un metodo accion
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            GZipEncodePage();
        }

        /// <summary>
        /// Determina si el Gzip es soportado
        /// </summary>
        /// <returns></returns>
        public static bool IsGZipSupported()
        {
            string AcceptEncoding = HttpContext.Current.Request.Headers["Accept-Encoding"];
            if (!string.IsNullOrEmpty(AcceptEncoding) &&
                (AcceptEncoding.Contains("gzip") || AcceptEncoding.Contains("deflate")))
                return true;
            return false;
        }

        /// <summary>
        /// Sets up the current page or handler to use GZip through a Response.Filter
        /// IMPORTANT:  
        /// You have to call this method before any output is generated!
        /// </summary>
        public static void GZipEncodePage()
        {
            HttpResponse Response = HttpContext.Current.Response;

            if (IsGZipSupported())
            {
                string AcceptEncoding = HttpContext.Current.Request.Headers["Accept-Encoding"];

                if (AcceptEncoding.Contains("gzip"))
                {
                    Response.Filter = new System.IO.Compression.GZipStream(Response.Filter,
                                                                           System.IO.Compression.CompressionMode.
                                                                               Compress);
                   // Response.Headers.Remove("Content-Encoding");
                    Response.AppendHeader("Content-Encoding", "gzip");
                }
                else
                {
                    Response.Filter = new System.IO.Compression.DeflateStream(Response.Filter,
                                                                              System.IO.Compression.CompressionMode.
                                                                                  Compress);
                  //  Response.Headers.Remove("Content-Encoding");
                    Response.AppendHeader("Content-Encoding", "deflate");
                }


            }

            // Allow proxy servers to cache encoded and unencoded versions separately
            Response.AppendHeader("Vary", "Content-Encoding");
        }
    }
}