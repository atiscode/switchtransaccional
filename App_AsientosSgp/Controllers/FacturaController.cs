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
    public class FacturaController : ApiController
    {
        static string apiBaseUri = ConfigurationManager.AppSettings["apiBaseUri"].ToString();//"http://172.16.36.84:8081/safitoken";
        static string apiCostumerUri = ConfigurationManager.AppSettings["apiCostumerUri"].ToString();//"http://172.16.36.84:8082/api/migration";
        static string apiSalesUri = ConfigurationManager.AppSettings["apiSalesUri"].ToString() + ConfigurationManager.AppSettings["invoiceService"].ToString();//"factura"; // "/facturaMasiva"; //"http://172.16.36.84:8083/api/sri/credito";
        static string codigoNoFacturable = ConfigurationManager.AppSettings["codProductoNF"].ToString();
        static string canal = ConfigurationManager.AppSettings["canal"].ToString();

        private static string TipoDocumento = "FACTURA";

        string[] CodigosNoFacturar = codigoNoFacturable.Split(',');

        BussinesLogTran modelLog = new BussinesLogTran();
        BussinesFactura modelFact = new BussinesFactura();
        BussinesTipoDocumento modelTDoc = new BussinesTipoDocumento();

        private Logger logger = LogManager.GetCurrentClassLogger();

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

                    string validationSchema = Tools.ValidarSchemaJSON2("SchemaInvoice.json", wrapper);
                    string existChannel = modelLog.ExisteCanal(wrapper.Cabecera.Cliente.Segmento) ? string.Empty : string.Format("El canal {0} no existe.", wrapper.Cabecera.Cliente.Segmento);
                    string incorrectChannel = modelLog.InvocacionCanalCorrecto(wrapper.Cabecera.Cliente.Segmento, ConfigurationManager.AppSettings["segmento"].ToString()) ? string.Empty : "Emisión de documentos por el canal incorrecto.";
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

        private async Task<IRestResponse> CreateInvoice(Wrapper trama, string token, string numeroDocumento)
        {
            //Convert to xml format
            var sales = await new SalesDispatcherSwitch().GetInvoiceXMLAsync(trama, numeroDocumento);

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
            var response = await call.SetRequestAPI(apiSalesUri, Method.POST, headerParameter, bodyParameter);

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
                    Secuencial = wrapper?.Detalle?.Factura?.Secuencial ?? string.Empty,
                    Canal = segmento
                });

                #region Validar productos no facturables 
                bool productosGiftCard = wrapper?.Detalle?.Factura?.DetalleFactura.Any(s => CodigosNoFacturar.Contains(s.CodigoProducto)) ?? false;
                if (productosGiftCard)
                {
                    //ERROR PRODUCTO GIFT CARD
                    trackingLog = new ResponseSwitchAPI() { mensaje = "PROCESO OK", codigoRetorno = "200", estado = "OK", numeroDocumento = "000000000000000" };
                    //Save log in AtisLogTran
                    await modelLog.AddAsync(new AtisLogTran
                    {
                        TipoSolicitud = null,
                        NumeroDocumento = string.Empty,
                        TramaEntrada = trama,
                        FechaEntrada = startProcess,
                        Estado = "OK",
                        TramaRespuesta = JsonConvert.SerializeObject(trackingLog),
                        FechaSalida = DateTime.Now,
                        Tipo = TipoDocumento,
                        Secuencial = wrapper?.Detalle?.Factura?.Secuencial ?? "0",
                        Canal = segmento + "- GIFTCARD"
                    });

                    response = Request.CreateResponse(HttpStatusCode.OK, trackingLog);
                    return response;
                }
                #endregion


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

                string numberDocumentFound = await modelLog.ExistSequentialAtisLogTran(wrapper.Detalle.Factura.Secuencial, TipoDocumento, wrapper.Cabecera.Cliente.Segmento);
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

                    bool existSequentialEnqueue = await modelLog.ExistSequentialAtisLogEnqueue(wrapper.Detalle.Factura.Secuencial, TipoDocumento, segmento);

                    //Save in Queue
                    if (!existSequentialEnqueue)
                        await EnqueueTransaction(trackingLog, string.Empty, segmento, wrapper.Detalle.Factura.Secuencial, startProcess, trama);

                    response = Request.CreateResponse(HttpStatusCode.BadRequest, trackingLog);
                    return response;
                }

                startProcess = DateTime.Now;
                string establecimiento = Tools.ReadSetting("estab");
                string puntoEmision = Tools.ReadSetting("ptoEmi");

                long secuencial = await modelTDoc.GetSequential("FC", segmento);
                string numeroDocumento = string.Format("{0}{1}{2}", establecimiento, puntoEmision, secuencial.ToString().PadLeft(9, '0'));

                var responseInvoice = await CreateInvoice(wrapper, token, numeroDocumento);

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

                    //Here build xml once again in facts enqueue
                    //Verify if only build xml once
                    string xmlRequestDocument = await new SalesDispatcherSwitch().GetInvoiceXMLAsync(wrapper, numeroDocumento);

                    bool existSequentialEnqueue = await modelLog.ExistSequentialAtisLogEnqueue(wrapper.Detalle.Factura.Secuencial, TipoDocumento, segmento);

                    //Save in Queue
                    if (!existSequentialEnqueue)
                        await EnqueueTransaction(trackingLog, string.Empty, segmento, wrapper.Detalle.Factura.Secuencial, startProcess, trama);

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

        public async Task<bool> EnqueueTransaction(ResponseSwitchAPI trackingLog, string xmlRequestDocument, string channel, string sequential, DateTime startProcess, string trama)
        {
            try
            {
                //Save in Queue
                await modelLog.AddQueueAsync(new AtisQueueTransactions
                {
                    Channel = channel,
                    DateStart = startProcess,
                    TypeDocument = TipoDocumento,
                    Ready = false,
                    Status = "ERROR",
                    Sequential = sequential,
                    DateEnd = null,
                    ExceptionsService = trackingLog.mensaje,
                    ObjectRequest = trama,
                    RequestXML = xmlRequestDocument
                });

                return true;
            }
            catch (Exception ex)
            {
                AtisCode.Aplicacion.Tools.RegisterInfo("The transaction could not be queued. See logs.", ex);
                AtisCode.Aplicacion.Tools.RegisterException(ex, System.Reflection.MethodBase.GetCurrentMethod().Name);
                return false;
            }
        }

        //public HttpResponseMessage Post(Wrapper wrapper)
        //{
        //    var tRespuesta = new Respuesta();
        //    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, tRespuesta);

        //    try
        //    {
        //        var flagCanalExistente = modelLog.ExisteCanal(wrapper.Cabecera.Cliente.Segmento);
        //        var flagCanalIncorrecto = modelLog.InvocacionCanalCorrecto(wrapper.Cabecera.Cliente.Segmento, ConfigurationManager.AppSettings["segmento"].ToString());

        //        // Validar si el secuencial es numérico
        //        var flagSecuencialNumerico = modelLog.ValidarSecuencialNumerico(wrapper.Detalle.Factura.Secuencial);

        //        var requestData = "";

        //        var estado = "OK";

        //        DateTime dinit = DateTime.Now;
        //        DateTime dfinal = DateTime.Now;

        //        var trama = JsonConvert.SerializeObject(wrapper);

        //        //Verificando si en la trama se envió un canal o segmento
        //        if (string.IsNullOrEmpty(wrapper.Cabecera.Cliente.Segmento))
        //        {
        //            estado = "ERROR";
        //            Respuesta2 tRespuestaF = new Respuesta2 { mensaje = "Canal Vacío, verificar trama.", codigoRetorno = "450", estado = "ERROR", numeroDocumento = "No se generó.", secuencial = wrapper.Detalle.Factura.Secuencial };

        //            modelLog.Add(new AtisLogTran { TipoSolicitud = 2, NumeroDocumento = "", TramaEntrada = trama, FechaEntrada = dinit, Estado = estado, TramaRespuesta = JsonConvert.SerializeObject(tRespuestaF), FechaSalida = dfinal, Tipo = "FACTURA", Secuencial = wrapper.Detalle.Factura.Secuencial, Canal = ConfigurationManager.AppSettings["segmento"] });
        //            response = Request.CreateResponse(HttpStatusCode.Created, tRespuestaF);
        //            return response;
        //        }

        //        //Verificando si el secuencial es numérico
        //        if (!flagSecuencialNumerico)
        //        {
        //            estado = "ERROR";
        //            Respuesta2 tRespuestaF = new Respuesta2 { mensaje = "Error en el secuencial.", codigoRetorno = "451", estado = "ERROR", numeroDocumento = "No se generó.", secuencial = wrapper.Detalle.Factura.Secuencial };

        //            modelLog.Add(new AtisLogTran { TipoSolicitud = 2, NumeroDocumento = "", TramaEntrada = trama, FechaEntrada = dinit, Estado = estado, TramaRespuesta = JsonConvert.SerializeObject(tRespuestaF), FechaSalida = dfinal, Tipo = "FACTURA", Secuencial = wrapper.Detalle.Factura.Secuencial, Canal = ConfigurationManager.AppSettings["segmento"] });
        //            response = Request.CreateResponse(HttpStatusCode.Created, tRespuestaF);
        //            return response;
        //        }

        //        //Verificando si existe el canal
        //        if (!flagCanalExistente)
        //        {
        //            estado = "ERROR";
        //            Respuesta2 tRespuestaF = new Respuesta2 { mensaje = "El canal no existe.", codigoRetorno = "452", estado = "ERROR", numeroDocumento = "No se generó.", secuencial = wrapper.Detalle.Factura.Secuencial };

        //            modelLog.Add(new AtisLogTran { TipoSolicitud = 2, NumeroDocumento = "", TramaEntrada = trama, FechaEntrada = dinit, Estado = estado, TramaRespuesta = JsonConvert.SerializeObject(tRespuestaF), FechaSalida = dfinal, Tipo = "FACTURA", Secuencial = wrapper.Detalle.Factura.Secuencial, Canal = ConfigurationManager.AppSettings["segmento"] });
        //            response = Request.CreateResponse(HttpStatusCode.Created, tRespuestaF);
        //            return response;
        //        }

        //        //Verificando si se emitió por el enlace correcto
        //        if (!flagCanalIncorrecto)
        //        {
        //            estado = "ERROR";
        //            Respuesta2 tRespuestaF = new Respuesta2 { mensaje = "Emisión de documentos por el canal incorrecto.", codigoRetorno = "453", estado = "ERROR", numeroDocumento = "No se generó.", secuencial = wrapper.Detalle.Factura.Secuencial };

        //            modelLog.Add(new AtisLogTran { TipoSolicitud = 2, NumeroDocumento = "", TramaEntrada = trama, FechaEntrada = dinit, Estado = estado, TramaRespuesta = JsonConvert.SerializeObject(tRespuestaF), FechaSalida = dfinal, Tipo = "FACTURA", Secuencial = wrapper.Detalle.Factura.Secuencial, Canal = ConfigurationManager.AppSettings["segmento"] });
        //            response = Request.CreateResponse(HttpStatusCode.Created, tRespuestaF);
        //            return response;
        //        }

        //        var log = new AtisLogTran { TipoSolicitud = 2, NumeroDocumento = "", TramaEntrada = trama, FechaEntrada = DateTime.Now, Estado = estado, TramaRespuesta = "", FechaSalida = DateTime.Now, Tipo = "FACTURA", Secuencial = wrapper.Detalle.Factura.Secuencial, Canal = ConfigurationManager.AppSettings["segmento"] };
        //        modelLog.Add(log);

        //        //Validar que no este en el listado de codigos a no facturar
        //        for (int i = 0; i < CodigosNoFacturar.Length; i++)
        //        {
        //            var datos = wrapper?.Detalle?.Factura?.DetalleFactura.FirstOrDefault().CodigoProducto.ToString();

        //            if (datos.Equals(CodigosNoFacturar[i]))
        //            {
        //                estado = "OK";
        //                tRespuesta = new Respuesta { mensaje = "PROCESO OK", codigoRetorno = "200", estado = "OK", numeroDocumento = "00000000000000" };

        //                //CAMBIAR EL NOMBRE DEL CANAL
        //                canal = "";
        //                canal = ConfigurationManager.AppSettings["canal"].ToString() + "- GIFTCARD";

        //                var log1 = new AtisLogTran { TipoSolicitud = 2, NumeroDocumento = "", TramaEntrada = trama, FechaEntrada = DateTime.Now, Estado = estado, TramaRespuesta = JsonConvert.SerializeObject(tRespuesta), FechaSalida = DateTime.Now, Tipo = "FACTURA", Secuencial = wrapper.Detalle.Factura.Secuencial, Canal = canal };
        //                modelLog.Add(log1);

        //                mandarSafi = false;
        //                break;
        //            }
        //            else
        //            {
        //                mandarSafi = true;
        //            }
        //        }

        //        if (mandarSafi == true)
        //        {

        //            var numDoc = wrapper?.Detalle?.Factura?.Secuencial != null ? modelLog.ExisteSecuencialAtisLogTran(wrapper.Detalle.Factura.Secuencial, "FACTURA", wrapper.Cabecera.Cliente.Segmento) : "";
        //            if (string.IsNullOrEmpty(numDoc))
        //            {
        //                var costumer = CostumerDispatcherSwitch.GetNewCostumer(trama);
        //                dinit = DateTime.Now;
        //                var requestCostumer = CostumerDispatcherSwitch.InsertCostumerRequest(costumer, apiBaseUri, apiCostumerUri);
        //                requestData = requestCostumer != null ? requestCostumer.Content : "";
        //                dfinal = DateTime.Now;

        //                RequestData obj = JsonConvert.DeserializeObject<RequestData>(requestData);
        //                if (obj != null)
        //                {
        //                    if (obj.StatusCode != 200)
        //                    {
        //                        var mensaje = "";
        //                        if (obj.Messages.Any())
        //                            mensaje = obj.Messages[0].Message + "-" + obj.Messages[0].InfoAdditional;
        //                        estado = "ERROR";
        //                        tRespuesta = new Respuesta { mensaje = mensaje, codigoRetorno = obj.StatusCode.ToString(), estado = "ERROR", numeroDocumento = "" };

        //                        log = new AtisLogTran { TipoSolicitud = 2, NumeroDocumento = "", TramaEntrada = costumer, FechaEntrada = dinit, Estado = estado, TramaRespuesta = JsonConvert.SerializeObject(tRespuesta), FechaSalida = dfinal, Tipo = "FACTURA", Secuencial = wrapper.Detalle.Factura.Secuencial, Canal = ConfigurationManager.AppSettings["segmento"] };
        //                        modelLog.Add(log);
        //                    }
        //                    else
        //                    {
        //                        estado = "OK";
        //                        tRespuesta = new Respuesta { mensaje = "PROCESO OK", codigoRetorno = obj.StatusCode.ToString(), estado = "OK", numeroDocumento = "" };

        //                        log = new AtisLogTran { TipoSolicitud = 2, NumeroDocumento = "", TramaEntrada = costumer, FechaEntrada = dinit, Estado = estado, TramaRespuesta = JsonConvert.SerializeObject(tRespuesta), FechaSalida = dfinal, Tipo = "FACTURA", Secuencial = wrapper.Detalle.Factura.Secuencial, Canal = ConfigurationManager.AppSettings["segmento"] };
        //                        modelLog.Add(log);

        //                        var secuencial = modelTDoc.GetSecuencial("FC", ConfigurationManager.AppSettings["segmento"]);

        //                        // Si el secuencial es 0 es porque no se encuentra bien parametrizado en la tabla tdocumentosContador
        //                        if (secuencial == 0)
        //                        {
        //                            estado = "ERROR";
        //                            tRespuesta = new Respuesta { mensaje = "Servicios caídos, consulte con su proveedor.", codigoRetorno = "400", estado = "ERROR", numeroDocumento = "" };

        //                            log = new AtisLogTran { NumeroDocumento = "", TramaEntrada = costumer, FechaEntrada = dinit, Estado = estado, TramaRespuesta = JsonConvert.SerializeObject(tRespuesta), FechaSalida = dfinal, Tipo = "FACTURA", Secuencial = wrapper.Detalle.Factura.Secuencial, Canal = ConfigurationManager.AppSettings["segmento"] };
        //                            modelLog.Add(log);
        //                        }

        //                        modelTDoc.UpdateSecuencial("FC", ConfigurationManager.AppSettings["segmento"]);
        //                        var fact = new tfacturas { tipo = "FC", factura = ConfigurationManager.AppSettings["estab"] + ConfigurationManager.AppSettings["ptoEmi"] + secuencial.ToString().PadLeft(9, '0'), idFactura = int.Parse(secuencial.ToString()) };
        //                        modelFact.Add(fact);

        //                        var sales = SalesDispatcherSwitch.GetNewSales(trama, fact.factura);
        //                        dinit = DateTime.Now;
        //                        var request = SalesDispatcherSwitch.InsertSalesRequest(sales, apiBaseUri, apiSalesUri);
        //                        requestData = request.Content;
        //                        dfinal = DateTime.Now;

        //                        obj = JsonConvert.DeserializeObject<RequestData>(requestData);
        //                        if (obj.StatusCode != 200)
        //                        {
        //                            var mensaje = "";
        //                            if (obj.Messages.Any())
        //                            {
        //                                foreach (var ele in obj.Messages)
        //                                {
        //                                    mensaje += ele.Message + ",";
        //                                }
        //                            }
        //                            estado = "ERROR";
        //                            tRespuesta = new Respuesta { mensaje = mensaje, codigoRetorno = obj.StatusCode.ToString(), estado = "ERROR", numeroDocumento = fact.factura };

        //                            log = new AtisLogTran { TipoSolicitud = 2, NumeroDocumento = fact.factura, TramaEntrada = sales, FechaEntrada = dinit, Estado = estado, TramaRespuesta = JsonConvert.SerializeObject(tRespuesta), FechaSalida = dfinal, Tipo = "FACTURA", Secuencial = wrapper.Detalle.Factura.Secuencial, Canal = ConfigurationManager.AppSettings["segmento"] };
        //                            modelLog.Add(log);
        //                        }
        //                        else
        //                        {
        //                            if (obj.Messages.Any())
        //                            {
        //                                var texto = obj.Messages[0].Message.Split(':');
        //                                var clave = texto[1].Substring(0, texto[1].Length - 2);
        //                                fact.ClaveAcceso = clave;
        //                                modelFact.Update(fact);
        //                            }
        //                            estado = "OK";
        //                            tRespuesta = new Respuesta { mensaje = "PROCESO OK", codigoRetorno = obj.StatusCode.ToString(), estado = "OK", numeroDocumento = fact.factura };
        //                            log = new AtisLogTran { TipoSolicitud = 2, NumeroDocumento = fact.factura, TramaEntrada = sales, FechaEntrada = dinit, Estado = estado, TramaRespuesta = JsonConvert.SerializeObject(tRespuesta), FechaSalida = dfinal, Tipo = "FACTURA", Secuencial = wrapper.Detalle.Factura.Secuencial, Canal = ConfigurationManager.AppSettings["segmento"] };
        //                            modelLog.Add(log);
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    estado = "ERROR";
        //                    tRespuesta = new Respuesta { mensaje = "Servicios caídos, consulte con su proveedor.", codigoRetorno = "400", estado = "ERROR", numeroDocumento = "" };

        //                    log = new AtisLogTran { NumeroDocumento = "", TramaEntrada = costumer, FechaEntrada = dinit, Estado = estado, TramaRespuesta = JsonConvert.SerializeObject(tRespuesta), FechaSalida = dfinal, Tipo = "FACTURA", Secuencial = wrapper.Detalle.Factura.Secuencial, Canal = ConfigurationManager.AppSettings["segmento"] };
        //                    modelLog.Add(log);
        //                }
        //            }
        //            else
        //            {
        //                estado = "OK";
        //                Respuesta2 tRespuestaE = new Respuesta2 { mensaje = "El secuencial ya existe en la factura: " + numDoc, codigoRetorno = "200", estado = estado, numeroDocumento = numDoc, secuencial = wrapper.Detalle.Factura.Secuencial };

        //                log = new AtisLogTran { NumeroDocumento = numDoc, TramaEntrada = trama, FechaEntrada = dinit, Estado = estado, TramaRespuesta = JsonConvert.SerializeObject(tRespuestaE), FechaSalida = dfinal, Tipo = "FACTURA", Secuencial = wrapper.Detalle.Factura.Secuencial, Canal = ConfigurationManager.AppSettings["segmento"] };
        //                modelLog.Add(log);
        //                return Request.CreateResponse(HttpStatusCode.Created, tRespuestaE);
        //            }
        //        }

        //        response = Request.CreateResponse(HttpStatusCode.Created, tRespuesta);
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.Error(ex, "Stopped program because of exception");
        //        tRespuesta = new Respuesta { mensaje = "Un error ocurrió." + ex.Message, codigoRetorno = "454", estado = "ERROR", numeroDocumento = "No se generó." };
        //        response = Request.CreateResponse(HttpStatusCode.Created, tRespuesta);
        //    }

        //    return response;
        //}


    }
}