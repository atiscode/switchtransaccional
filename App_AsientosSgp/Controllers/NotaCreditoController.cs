using App_Mundial_Miles.Helpers;
using App_Mundial_Miles.Models;
using AtisCode.Aplicacion;
using AtisCode.Aplicacion.Model;
using AtisCode.Aplicacion.Model.db_Local;
using AtisCode.Aplicacion.Model.db_Safi;
using AtisCode.Aplicacion.NotasCredito;
using Newtonsoft.Json;
using NLog;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace App_Mundial_Miles.Controllers
{
    public class NotaCreditoController : ApiController
    {
        static string apiBaseUri = ConfigurationManager.AppSettings["apiBaseUri"].ToString();//"http://172.16.36.84:8081/safitoken";
        static string apiCostumerUri = ConfigurationManager.AppSettings["apiCostumerUri"].ToString();//"http://172.16.36.84:8082/api/migration";
        static string apiSalesUri = ConfigurationManager.AppSettings["apiSalesUri"].ToString() + "/credito";//"http://172.16.36.84:8083/api/sri/credito";

        private static string TipoDocumento = "NOTACREDITO";

        BussinesLogTran modelLog = new BussinesLogTran();
        BussinesFactura modelFact = new BussinesFactura();
        BussinesTipoDocumento modelTDoc = new BussinesTipoDocumento();

        private Logger logger = LogManager.GetCurrentClassLogger();

        private async Task<string> Validations(WraperNotaCreditoWs wrapper)
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
                        return "Verificar que la trama corresponda al tipo de documento Nota de Credito.";

                    //Convertir todos los metodos de validacion en asincronos
                    List<string> errors = new List<string>();

                    string validationSchema = Tools.ValidarSchemaJSON2("SchemaCreditNote.json", wrapper, false);
                    string validNumericSequential = modelLog.ValidarSecuencialNumerico(wrapper.NotaCredito.Detalle.Secuencial) ? string.Empty : string.Format("Error en el secuencial {0}. Debe ser un valor numérico.", wrapper.NotaCredito.Detalle.Secuencial);

                    if (!string.IsNullOrEmpty(validationSchema))
                        errors.Add(validationSchema);

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

        private async Task<IRestResponse> CreateCreditNote(WraperNotaCreditoWs trama, string token, string numeroDocumento)
        {
            //Convert to xml format
            var sales = await new SalesDispatcherSwitch().GetCreditNoteXMLAsync(trama, numeroDocumento);

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
            int timeDelay = Convert.ToInt32(string.IsNullOrEmpty(Tools.ReadSetting("DelayNotaCredito")) ? Tools.ReadSetting("DelayNotaCredito") : "0");
            if (timeDelay > 0)
                await Task.Delay(timeDelay);
            #endregion

            return response;
        }

        public async Task<HttpResponseMessage> Post(WraperNotaCreditoWs wrapper)
        {
            DateTime startProcess = DateTime.Now; // Init process datetime
            string trama = JsonConvert.SerializeObject(wrapper); // Json  
            string segmento = Tools.ReadSetting("segmento");
            ResponseSwitchAPI trackingLog = new ResponseSwitchAPI();
            try
            {
                HttpResponseMessage response = new HttpResponseMessage();

                #region Valid Range Date Channel Transactions
                bool isRangeValid = modelLog.IsValidDateInRange(segmento);
                if (!isRangeValid && !wrapper.EsCargaDocumentos)
                {
                    //ERROR PRODUCTO GIFT CARD
                    trackingLog = new ResponseSwitchAPI() { mensaje = "Invalid range date transaction. Try again later.", codigoRetorno = "400", estado = "ERROR", numeroDocumento = string.Empty };
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
                        Secuencial = wrapper?.NotaCredito.Detalle?.Secuencial ?? "0",
                        Canal = segmento,
                    });

                    response = Request.CreateResponse(HttpStatusCode.BadRequest, trackingLog);
                    return response;
                }
                #endregion

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
                    //Secuencial = wrapper?.NotaCredito?.Detalle?.Secuencial ?? string.Empty,
                    Secuencial = wrapper != null ? (modelLog.ValidarSecuencialNumerico(wrapper.NotaCredito.Detalle.Secuencial) ? wrapper.NotaCredito.Detalle.Secuencial : string.Empty) : string.Empty,
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
                        Secuencial = wrapper?.NotaCredito.Detalle?.Secuencial ?? "0",
                        Canal = segmento
                    });

                    response = Request.CreateResponse(HttpStatusCode.BadRequest, trackingLog);
                    return response;
                }

                string numberDocumentFound = !wrapper.EsCargaDocumentos ? await modelLog.ExistSequentialAtisLogTran(wrapper.NotaCredito.Detalle.Secuencial, TipoDocumento, segmento) : string.Empty;

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
                        Secuencial = wrapper.NotaCredito.Detalle.Secuencial,
                        Canal = segmento
                    });
                    response = Request.CreateResponse(HttpStatusCode.OK, trackingLog);
                    return response;
                }

                startProcess = DateTime.Now;
                string token = await Tools.GetAccessToken(apiBaseUri); // Get Token

                startProcess = DateTime.Now;
                string establecimiento = Tools.ReadSetting("estab");
                string puntoEmision = Tools.ReadSetting("ptoEmi");

                long secuencial = await modelTDoc.GetSequential("NC", segmento);
                string numeroDocumento = string.Format("{0}{1}{2}", establecimiento, puntoEmision, secuencial.ToString().PadLeft(9, '0'));

                var responseCreditNote = await CreateCreditNote(wrapper, token, numeroDocumento);

                var messages = string.IsNullOrEmpty(responseCreditNote.Content) ? responseCreditNote.ErrorMessage : Tools.DeserializeSAFIResponse(responseCreditNote.Content);

                if (responseCreditNote.StatusCode == HttpStatusCode.OK)
                {
                    await modelTDoc.UpdateSequential("NC", segmento); // Update next document number sequential

                    trackingLog = new ResponseSwitchAPI() { mensaje = "PROCESO OK", codigoRetorno = "200", estado = HttpStatusCode.OK.ToString(), numeroDocumento = numeroDocumento };
                    response = Request.CreateResponse(HttpStatusCode.OK, trackingLog);
                }
                else
                {
                    trackingLog = new ResponseSwitchAPI() { mensaje = messages, codigoRetorno = Convert.ToString((int)SwitchCode.ERROR_SALES_API), estado = "ERROR", numeroDocumento = string.Empty };

                    //Save log in AtisLogTran
                    await modelLog.AddAsync(new AtisLogTran
                    {
                        TipoSolicitud = null,
                        NumeroDocumento = numberDocumentFound,
                        TramaEntrada = trama,
                        FechaEntrada = startProcess,
                        Estado = "ERROR",
                        TramaRespuesta = JsonConvert.SerializeObject(trackingLog),
                        FechaSalida = DateTime.Now,
                        Tipo = TipoDocumento,
                        Secuencial = wrapper.NotaCredito.Detalle.Secuencial,
                        Canal = segmento
                    });

                    //Here build xml once again in facts enqueue
                    //Verify if only build xml once
                    string xmlRequestDocument = await new SalesDispatcherSwitch().GetCreditNoteXMLAsync(wrapper, numeroDocumento);

                    bool existSequentialEnqueue = await modelLog.ExistSequentialAtisLogEnqueue(wrapper.NotaCredito.Detalle.Secuencial, TipoDocumento, segmento);

                    //Save in Queue
                    if (!existSequentialEnqueue)
                        //Save in Queue
                        await EnqueueTransaction(trackingLog, xmlRequestDocument, segmento, wrapper.NotaCredito.Detalle.Secuencial, startProcess, trama);

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
                    Estado = responseCreditNote.StatusCode.ToString(),
                    TramaRespuesta = JsonConvert.SerializeObject(trackingLog),
                    FechaSalida = DateTime.Now,
                    Tipo = TipoDocumento,
                    Secuencial = wrapper.NotaCredito.Detalle.Secuencial,
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
                    Secuencial = wrapper.NotaCredito.Detalle.Secuencial,
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
                    Sequential = sequential,
                    DateEnd = null,
                    Status = "ERROR",
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

        //public HttpResponseMessage Post2(WraperNotaCreditoWs wrapper)
        //{
        //    var tRespuesta = new Respuesta();
        //    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.Created, tRespuesta);

        //    try
        //    {
        //        var request = "";
        //        var estado = "OK";
        //        var mensaje = "";

        //        DateTime dinit = DateTime.Now;
        //        DateTime dfinal = DateTime.Now;
        //        var trama = JsonConvert.SerializeObject(wrapper);
        //        var log = new AtisLogTran { TipoSolicitud = 2, NumeroDocumento = "", TramaEntrada = trama, FechaEntrada = DateTime.Now, Estado = estado, TramaRespuesta = "", FechaSalida = DateTime.Now, Tipo = "NOTACREDITO", Secuencial = wrapper.NotaCredito.Detalle.Secuencial, Canal = ConfigurationManager.AppSettings["segmento"] };
        //        modelLog.Add(log);

        //        var numDoc = wrapper?.NotaCredito?.Detalle?.Secuencial != null ? modelLog.ExisteSecuencialAtisLogTran(wrapper.NotaCredito.Detalle.Secuencial, "NOTACREDITO", ConfigurationManager.AppSettings["segmento"]) : "";

        //        if (string.IsNullOrEmpty(numDoc))
        //        {
        //            var secuencial = modelTDoc.GetSecuencial("NC", ConfigurationManager.AppSettings["segmento"]);
        //            modelTDoc.UpdateSecuencial("NC", ConfigurationManager.AppSettings["segmento"]);
        //            var fact = new tfacturas { tipo = "NC", factura = ConfigurationManager.AppSettings["estab"] + ConfigurationManager.AppSettings["ptoEmi"] + secuencial.ToString().PadLeft(9, '0'), idFactura = int.Parse(secuencial.ToString()) };
        //            modelFact.Add(fact);

        //            var sales = SalesDispatcherSwitch.GetNewNcSalesWs(trama, fact.factura);
        //            if (!string.IsNullOrEmpty(sales))
        //            {
        //                dinit = DateTime.Now;
        //                request = SalesDispatcherSwitch.InsertNcSalesRequest(sales, apiBaseUri, apiSalesUri);
        //                dfinal = DateTime.Now;

        //                var obj = JsonConvert.DeserializeObject<RequestData>(request);
        //                if (obj != null)
        //                {
        //                    if (obj.StatusCode != 200)
        //                    {
        //                        if (obj.Messages.Any())
        //                            mensaje = obj.Messages[0].Message + "-" + obj.Messages[0].InfoAdditional;

        //                        estado = "ERROR";
        //                        tRespuesta = new Respuesta { mensaje = mensaje, codigoRetorno = obj.StatusCode.ToString(), estado = "ERROR", numeroDocumento = fact.factura };
        //                        response = Request.CreateResponse(HttpStatusCode.BadRequest, tRespuesta);
        //                    }
        //                    else
        //                    {
        //                        estado = "OK";
        //                        tRespuesta = new Respuesta { mensaje = "PROCESO OK", codigoRetorno = obj.StatusCode.ToString(), estado = "OK", numeroDocumento = fact.factura };
        //                        log = new AtisLogTran { TipoSolicitud = 2, NumeroDocumento = fact.factura, TramaEntrada = sales, FechaEntrada = dinit, Estado = estado, TramaRespuesta = JsonConvert.SerializeObject(tRespuesta), FechaSalida = dfinal, Tipo = "NOTACREDITO", Secuencial = wrapper.NotaCredito.Detalle.Secuencial, Canal = ConfigurationManager.AppSettings["segmento"] };
        //                        modelLog.Add(log);
        //                        response = Request.CreateResponse(HttpStatusCode.Created, tRespuesta);
        //                    }
        //                }
        //                else
        //                {
        //                    estado = "ERROR";
        //                    tRespuesta = new Respuesta { mensaje = "Servicios caídos, consulte con su proveedor.", codigoRetorno = "400", estado = "ERROR", numeroDocumento = "" };

        //                    log = new AtisLogTran { TipoSolicitud = 2, NumeroDocumento = "", TramaEntrada = sales, FechaEntrada = dinit, Estado = estado, TramaRespuesta = JsonConvert.SerializeObject(tRespuesta), FechaSalida = dfinal, Tipo = "FACTURA", Secuencial = wrapper.NotaCredito.Detalle.Secuencial, Canal = ConfigurationManager.AppSettings["segmento"] };
        //                    modelLog.Add(log);
        //                    response = Request.CreateResponse(HttpStatusCode.InternalServerError, tRespuesta);
        //                }
        //            }
        //            else
        //            {
        //                if (wrapper?.NotaCredito?.Detalle?.Idfactura != null)
        //                {
        //                    var existe = SalesDispatcherSwitch.ExisteFacturaAplicont(wrapper.NotaCredito.Detalle.Idfactura);
        //                    if (existe)
        //                    {
        //                        var ele = SalesDispatcherSwitch.GetFacturaAplicont(wrapper.NotaCredito.Detalle.Idfactura);
        //                        estado = "OK";
        //                        tRespuesta = new Respuesta { mensaje = "PROCESO OK", codigoRetorno = "200", estado = "OK", numeroDocumento = fact.factura };
        //                        log = new AtisLogTran { TipoSolicitud = 2, NumeroDocumento = ele.NumeroDocumento, TramaEntrada = ele.TramaEntrada, FechaEntrada = ele.FechaEntrada, Estado = ele.Estado, TramaRespuesta = ele.TramaRespuesta, FechaSalida = ele.FechaSalida, Tipo = "NOTACREDITO", Secuencial = ele.Secuencial, Canal = ele.Canal };
        //                        modelLog.Add(log);

        //                        response = Request.CreateResponse(HttpStatusCode.Created, tRespuesta);
        //                    }
        //                    else
        //                    {
        //                        estado = "ERROR";
        //                        tRespuesta = new Respuesta { mensaje = "FACTURA NO ENCONTRADA", codigoRetorno = "400", estado = "ERROR", numeroDocumento = fact.factura };
        //                        log = new AtisLogTran { TipoSolicitud = 2, NumeroDocumento = fact.factura, TramaEntrada = sales, FechaEntrada = dinit, Estado = estado, TramaRespuesta = JsonConvert.SerializeObject(tRespuesta), FechaSalida = dfinal, Tipo = "NOTACREDITO", Secuencial = wrapper.NotaCredito.Detalle.Secuencial, Canal = ConfigurationManager.AppSettings["segmento"] };
        //                        modelLog.Add(log);

        //                        response = Request.CreateResponse(HttpStatusCode.BadRequest, tRespuesta);
        //                    }
        //                }
        //                else
        //                {
        //                    estado = "ERROR";
        //                    tRespuesta = new Respuesta { mensaje = "ID FACTURA NULL", codigoRetorno = "400", estado = "ERROR", numeroDocumento = fact.factura };
        //                    log = new AtisLogTran { TipoSolicitud = 2, NumeroDocumento = fact.factura, TramaEntrada = sales, FechaEntrada = dinit, Estado = estado, TramaRespuesta = JsonConvert.SerializeObject(tRespuesta), FechaSalida = dfinal, Tipo = "NOTACREDITO", Secuencial = wrapper.NotaCredito.Detalle.Secuencial, Canal = ConfigurationManager.AppSettings["segmento"] };
        //                    modelLog.Add(log);

        //                    response = Request.CreateResponse(HttpStatusCode.BadRequest, tRespuesta);
        //                }
        //            }
        //        }
        //        else
        //        {
        //            estado = "OK";
        //            tRespuesta = new Respuesta { mensaje = "El secuencial ya existe en la Nota Crédito: " + numDoc, codigoRetorno = "200", estado = estado, numeroDocumento = numDoc };

        //            log = new AtisLogTran { TipoSolicitud = 2, NumeroDocumento = numDoc, TramaEntrada = trama, FechaEntrada = dinit, Estado = estado, TramaRespuesta = JsonConvert.SerializeObject(tRespuesta), FechaSalida = dfinal, Tipo = "NOTACREDITO", Secuencial = wrapper.NotaCredito.Detalle.Secuencial, Canal = ConfigurationManager.AppSettings["segmento"] };
        //            modelLog.Add(log);
        //            response = Request.CreateResponse(HttpStatusCode.Created, tRespuesta);
        //        }
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