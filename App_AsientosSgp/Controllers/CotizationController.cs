using App_Mundial_Miles.Helpers;
using App_Mundial_Miles.Models;
using AtisCode.Aplicacion;
using AtisCode.Aplicacion.Model;
using AtisCode.Aplicacion.Model.db_Local;
using AtisCode.Aplicacion.Model.db_Safi;
using Newtonsoft.Json;
using NLog;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Information;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;
using System.Web.Http;
using System.Web.SessionState;

namespace App_Mundial_Miles.Controllers
{
    public class CotizationController : ApiController
    {
        static string apiBaseUri = ConfigurationManager.AppSettings["apiBaseUri"].ToString();//"http://172.16.36.84:8081/safitoken";
        static string apiCostumerUri = ConfigurationManager.AppSettings["apiCostumerUri"].ToString();//"http://172.16.36.84:8082/api/migration";
        static string apiSalesCotization = ConfigurationManager.AppSettings["apiCotization"].ToString();
        static string canal = ConfigurationManager.AppSettings["canal"].ToString();

        private static string TipoDocumento = "COTIZACIONES";

        BussinesLogTran modelLog = new BussinesLogTran();
        BussinesFactura modelFact = new BussinesFactura();
        BussinesTipoDocumento modelTDoc = new BussinesTipoDocumento();

        private Logger logger = LogManager.GetCurrentClassLogger();
        // GET: api/Cotization
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Cotization/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Cotization
        private async Task<string> Validations(Wrapper wrapper)
        {
            // 1. VALIDAR LA TRAMA
            // 2. SI EL CANAL NO EXISTE O NO SE ENCUENTRA PARAMETRIZADO
            // 3. SI SE ESTÁ EMITIENDO POR EL ENDPOINT INCORRECTO
            // 4. SI EL SECUENCIAL NO ES NUMERICO
            // 5. SECUENCIAL EXISTENTE QUE ESTÁ ASOCIADO A OTRA FACTURA
            return await Task.Run(() =>
            {
                string result = string.Empty;
                try
                {
                    if (wrapper == null)
                        return "Verificar que la trama corresponda al tipo de documento factura.";

                    if (wrapper?.Cabecera == null || wrapper?.Detalle == null)
                        return "Verificar que la trama corresponda al tipo de documento factura.";

                    //Convertir todos los metodos de validacion en asincronos
                    List<string> errors = new List<string>();

                    string validationSchema = string.Empty;// Tools.ValidarSchemaJSON2("SchemaInvoice.json", wrapper);
                    string existChannel = modelLog.ExisteCanal(wrapper.Cabecera.Cliente.Segmento) ? string.Empty : string.Format("El canal {0} no existe.", wrapper.Cabecera.Cliente.Segmento);
                    string incorrectChannel = string.Empty;//modelLog.InvocacionCanalCorrecto(wrapper.Cabecera.Cliente.Segmento, ConfigurationManager.AppSettings["segmento"].ToString()) ? string.Empty : "Emisión de documentos por el canal incorrecto.";
                    string validNumericSequential = modelLog.ValidarSecuencialNumerico(wrapper.Detalle.Factura.Secuencial) ? string.Empty : string.Format("Error en el secuencial {0}. Debe ser un valor numérico.", wrapper.Detalle.Factura.Secuencial);

                    if (!string.IsNullOrEmpty(validationSchema))
                        errors.Add(validationSchema);

                    if (!string.IsNullOrEmpty(existChannel))
                        errors.Add(existChannel);

                    if (!string.IsNullOrEmpty(incorrectChannel))
                        errors.Add(incorrectChannel);

                    if (!string.IsNullOrEmpty(validNumericSequential))
                        errors.Add(validNumericSequential);

                    result = string.Join(" ; ", errors);

                    return result;
                }
                catch (Exception ex)
                {
                    return "Verify Validations method exceptions: " + ex.Message;
                }
            });
        }

        //CREACION O ACTUALIZACIÓN DE CLIENTE
        private async Task<IRestResponse> CreateOrUpdateCustomer(string trama, string token)
        {
            var costumer = await new CostumerDispatcherSwitch().GetCostumerAsync(trama);

            #region PARAMETROS

            token = "bearer " + token;

            //HEADER
            Dictionary<string, string> headerParameter = new Dictionary<string, string>(){
                    { "authorization", token},
                    { "content-type", "application/json"},
                };

            //BODY
            Dictionary<string, string> bodyParameter = new Dictionary<string, string>(){
                    { "application/json", costumer }
                };
            #endregion

            CallAPIGetType call = new CallAPIGetType();
            var response = await call.SetRequestAPI(apiCostumerUri, Method.POST, headerParameter, bodyParameter);

            #region Delay in service
            int timeDelay = Convert.ToInt32(string.IsNullOrEmpty(Tools.ReadSetting("DelayCliente")) ? Tools.ReadSetting("DelayCliente") : "0");

            if (timeDelay > 0)
                await Task.Delay(timeDelay);
            #endregion

            return response;
        }

        private async Task<IRestResponse> CreateCotization(Wrapper trama, string token, string numeroDocumento)
        {
            //Convert to xml format
            var sales = await new SalesDispatcherSwitch().GetCotizationAsync(trama, numeroDocumento);

            #region PARAMETROS

            token = "bearer " + token;

            //HEADER
            Dictionary<string, string> headerParameter = new Dictionary<string, string>(){
                    { "authorization", token},
                    { "content-type", "application/json"},
                };

            //BODY
            Dictionary<string, string> bodyParameter = new Dictionary<string, string>(){
                    { "application/json", sales }
                };
            #endregion

            CallAPIGetType call = new CallAPIGetType();
            var response = await call.SetRequestAPI(apiSalesCotization, Method.POST, headerParameter, bodyParameter);

            #region Delay in service
            int timeDelay = Convert.ToInt32(string.IsNullOrEmpty(Tools.ReadSetting("DelayFactura")) ? Tools.ReadSetting("DelayFactura") : "0");
            if (timeDelay > 0)
                await Task.Delay(timeDelay);
            #endregion

            return response;
        }

        public async Task<HttpResponseMessage> Post(Wrapper wrapper)
        {
            DateTime startProcess = DateTime.Now; // Init process datetime
            string trama = JsonConvert.SerializeObject(wrapper); // Json  
            string segmento = Tools.ReadSetting("segmento");
            ResponseSwitchAPI trackingLog = new ResponseSwitchAPI();

            try
            {
                HttpResponseMessage response = new HttpResponseMessage();
                //Save log in AtisLogTran
                await modelLog.AddAsync(new AtisLogTran
                {
                    TipoSolicitud = null,
                    NumeroDocumento = string.Empty,
                    TramaEntrada = trama,
                    FechaEntrada = startProcess,
                    Estado = "OK",
                    TramaRespuesta = string.Empty,
                    FechaSalida = DateTime.Now,
                    Tipo = TipoDocumento,
                    //Secuencial = wrapper?.Detalle?.Factura?.Secuencial ?? string.Empty,
                    Secuencial = wrapper != null ? (modelLog.ValidarSecuencialNumerico(wrapper?.Detalle?.Factura?.Secuencial) ? wrapper.Detalle.Factura.Secuencial : string.Empty) : string.Empty,
                    Canal = segmento
                });

                //Check validations
                string validationsMessages = await Validations(wrapper);
                bool validationOk = string.IsNullOrEmpty(validationsMessages);

                if (!validationOk)
                {
                    trackingLog = new ResponseSwitchAPI() { mensaje = validationsMessages, codigoRetorno = Convert.ToString((int)SwitchCode.ERROR_VALIDATIONS), estado = "ERROR", numeroDocumento = string.Empty };
                    //Save log in AtisLogTran
                    await modelLog.AddAsync(new AtisLogTran
                    {
                        TipoSolicitud = null,
                        NumeroDocumento = string.Empty,
                        TramaEntrada = trama,
                        FechaEntrada = startProcess,
                        Estado = "ERROR",
                        TramaRespuesta = JsonConvert.SerializeObject(trackingLog),
                        FechaSalida = DateTime.Now,
                        Tipo = TipoDocumento,
                        Secuencial = wrapper?.Detalle?.Factura?.Secuencial ?? "0",
                        Canal = segmento
                    });

                    response = Request.CreateResponse(HttpStatusCode.BadRequest, trackingLog);
                    return response;
                }

                string numberDocumentFound = !wrapper.EsCargaDocumentos ? await modelLog.ExistSequentialAtisLogTran(wrapper.Detalle.Factura.Secuencial, TipoDocumento, wrapper.Cabecera.Cliente.Segmento) : string.Empty;
                if (!string.IsNullOrEmpty(numberDocumentFound))
                {
                    trackingLog = new ResponseSwitchAPI() { mensaje = string.Format("El secuencial ya existe en la factura : {0}", numberDocumentFound), codigoRetorno = "200", estado = HttpStatusCode.OK.ToString(), numeroDocumento = numberDocumentFound };
                    //Save log in AtisLogTran
                    await modelLog.AddAsync(new AtisLogTran
                    {
                        TipoSolicitud = null,
                        NumeroDocumento = numberDocumentFound,
                        TramaEntrada = trama,
                        FechaEntrada = startProcess,
                        Estado = "OK",
                        TramaRespuesta = JsonConvert.SerializeObject(trackingLog),
                        FechaSalida = DateTime.Now,
                        Tipo = TipoDocumento,
                        Secuencial = wrapper.Detalle.Factura.Secuencial,
                        Canal = segmento
                    });
                    response = Request.CreateResponse(HttpStatusCode.OK, trackingLog);
                    return response;
                }

                startProcess = DateTime.Now;
                string token = await Tools.GetAccessToken(apiBaseUri); // Get Token
                var responseCliente = await CreateOrUpdateCustomer(trama, token);

                //Check customer API response errors
                if (responseCliente.StatusCode != HttpStatusCode.OK)
                {
                    trackingLog = new ResponseSwitchAPI() { mensaje = string.IsNullOrEmpty(responseCliente.Content) ? responseCliente.ErrorMessage : responseCliente.Content, codigoRetorno = Convert.ToString((int)SwitchCode.ERROR_CUSTOMER_API), estado = "ERROR", numeroDocumento = string.Empty };
                    //Save log in AtisLogTran
                    await modelLog.AddAsync(new AtisLogTran
                    {
                        TipoSolicitud = null,
                        NumeroDocumento = string.Empty,
                        TramaEntrada = trama,
                        FechaEntrada = startProcess,
                        Estado = "ERROR",
                        TramaRespuesta = JsonConvert.SerializeObject(trackingLog),
                        FechaSalida = DateTime.Now,
                        Tipo = TipoDocumento,
                        Secuencial = wrapper.Detalle.Factura.Secuencial,
                        Canal = segmento
                    });

                    response = Request.CreateResponse(HttpStatusCode.BadRequest, trackingLog);
                    return response;
                }

                startProcess = DateTime.Now;
                string establecimiento = Tools.ReadSetting("estab");
                string puntoEmision = Tools.ReadSetting("ptoEmi");

                long secuencial = await modelTDoc.GetSequential("FC", segmento);
                string numeroDocumento = string.Format("{0}{1}{2}", establecimiento, puntoEmision, secuencial.ToString().PadLeft(9, '0'));

                var responseInvoice = await CreateCotization(wrapper, token, numeroDocumento);

                if (responseInvoice.StatusCode == HttpStatusCode.OK)
                {
                    await modelTDoc.UpdateSequential("FC", segmento); // Update next document number sequential
                    trackingLog = new ResponseSwitchAPI() { mensaje = "PROCESO OK", codigoRetorno = "200", estado = HttpStatusCode.OK.ToString(), numeroDocumento = numeroDocumento };
                    response = Request.CreateResponse(HttpStatusCode.OK, trackingLog);
                }
                else
                {
                    string errorMessage = string.IsNullOrEmpty(responseInvoice.Content) ? responseInvoice.ErrorMessage : Tools.DeserializeSAFIResponse(responseInvoice.Content);

                    trackingLog = new ResponseSwitchAPI() { mensaje = errorMessage, codigoRetorno = Convert.ToString((int)SwitchCode.ERROR_SALES_API), estado = "ERROR", numeroDocumento = string.Empty };

                    //Save log in AtisLogTran
                    await modelLog.AddAsync(new AtisLogTran
                    {
                        TipoSolicitud = null,
                        NumeroDocumento = string.Empty,
                        TramaEntrada = trama,
                        FechaEntrada = startProcess,
                        Estado = "ERROR",
                        TramaRespuesta = JsonConvert.SerializeObject(trackingLog),
                        FechaSalida = DateTime.Now,
                        Tipo = TipoDocumento,
                        Secuencial = wrapper.Detalle.Factura.Secuencial,
                        Canal = segmento,
                    });

                    response = Request.CreateResponse(HttpStatusCode.BadRequest, trackingLog);
                    return response;
                }

                //Save log in AtisLogTran
                await modelLog.AddAsync(new AtisLogTran
                {
                    TipoSolicitud = null,
                    NumeroDocumento = trackingLog.numeroDocumento,
                    TramaEntrada = trama,
                    FechaEntrada = startProcess,
                    Estado = responseInvoice.StatusCode.ToString(),
                    TramaRespuesta = JsonConvert.SerializeObject(trackingLog),
                    FechaSalida = DateTime.Now,
                    Tipo = TipoDocumento,
                    Secuencial = wrapper.Detalle.Factura.Secuencial,
                    Canal = segmento
                });

                return response;
            }
            catch (Exception ex)
            {
                AtisCode.Aplicacion.Tools.RegisterException(ex, System.Reflection.MethodBase.GetCurrentMethod().Name);

                trackingLog = new ResponseSwitchAPI()
                {
                    mensaje = ex.Message,
                    codigoRetorno = Convert.ToString((int)SwitchCode.ERROR_EXCEPTIONS),
                    estado = "ERROR",
                    numeroDocumento = string.Empty
                };

                //Save log in AtisLogTran
                await modelLog.AddAsync(new AtisLogTran
                {
                    TipoSolicitud = null,
                    NumeroDocumento = string.Empty,
                    TramaEntrada = trama,
                    FechaEntrada = startProcess,
                    Estado = "ERROR",
                    TramaRespuesta = JsonConvert.SerializeObject(trackingLog),
                    FechaSalida = DateTime.Now,
                    Tipo = TipoDocumento,
                    Secuencial = wrapper.Detalle.Factura.Secuencial,
                    Canal = segmento
                });

                return Request.CreateResponse(HttpStatusCode.InternalServerError, trackingLog);
            }
        }

        // PUT: api/Cotization/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Cotization/5
        public void Delete(int id)
        {
        }
    }
}
