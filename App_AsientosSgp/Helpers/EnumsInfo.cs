using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace App_Mundial_Miles.Helpers
{
    public static class EnumsInfo
    {
        public static string DireccionQPH = "Guipuzcoa E14-46 y Mallorca Edificio QPH";
        public static string TelefonoQPH = "3956060";
        public static string TipoDocumentoRUC = "RUC";
        public static string TipoDocumentoCedulaCiudadania = "C.C";
        public static string TipoDocumentoPasaporte = "Pasaporte";

        public static string MensajeRespuestaEstadoOK = "OK";
        public static string MensajeRespuestaEstadoError = "ERROR";
        public static string MensajeRespuestaEstadoInaccesible = "INACCESIBLE";
        public static string MensajeProcesoOK = "PROCESO OK";

        public static string CodigoRespuestaHTTP200 = "200";
        public static string CodigoRespuestaHTTP400 = "400";
        public static string CodigoRespuestaHTTP403 = "403";
        public static string CodigoRespuestaHTTP404 = "404";

        public static string MensajeCodigo400 = "Servicios caídos, consulte con su proveedor.";
        public static string MensajeCodigo404 = "No se encuentra la API.";

        public static string MensajeSecuencialExistenteFactura = "El secuencial ya existe en la factura:";
        public static string MensajeParametrosConfiguracionNoExisten = "Parámetros de configuración no se encuentran en base de datos, por favor contacte con su administrador";

        public static string MensajeErrorGeneracionTokenCredenciales = "El token no se pudo generar, revise las credenciales";
        public static string MensajeErrorFacturaNoEncontrada = "FACTURA NO ENCONTRADA";
        public static string MensajeErrorFacturaIDNull = "ID FACTURA NULL";
        public static string MensajeSeccuencialExistenteNotaCredito = "El secuencial ya existe en la Nota Crédito: ";
        public static string MensajeRevisarConfiguracionAPIS = "Revise la parametrización de las APIs.";

        public static string CodigoTipoDocumentoFactura = "FC";
        public static string CodigoTipoDocumentoNotaCredito = "NC";
    }
}