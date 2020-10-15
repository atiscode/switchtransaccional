using App_Mundial_Miles.Helpers;
using App_Mundial_Miles.Models;
using AtisCode.Aplicacion;
using AtisCode.Aplicacion.Model.db_Local;
using AtisCode.Aplicacion.Model.db_Safi;
using LinqToExcel;
using Newtonsoft.Json;
using NLog;
using OfficeOpenXml;
using OfficeOpenXml.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace App_Mundial_Miles.Controllers
{
    public class CotizacionesController : Controller
    {
        static string apiBaseUri = ConfigurationManager.AppSettings["apiBaseUri"].ToString();//"http://172.16.36.84:8081/safitoken";
        static string apiCostumerUri = ConfigurationManager.AppSettings["apiCostumerUri"].ToString();//"http://172.16.36.84:8082/api/migration";
        static string apiSalesUri = "http://172.19.3.84:8086/api/cotizacion"; //ConfigurationManager.AppSettings["apiSalesUri"].ToString() + "credito";//"http://172.16.36.84:8083/api/sri/credito";

        //const string apiBaseUri = "http://172.16.36.84:8084/safitoken";
        //const string apiCostumerUri = "http://172.16.36.84:8085/api/migration";
        //const string apiSalesUri = "http://172.16.36.84:8086/api/sri/credito";

        BussinesLogTran modelLog = new BussinesLogTran();
        BussinesFactura modelFact = new BussinesFactura();
        BussinesTipoDocumento modelTDoc = new BussinesTipoDocumento();

        SafiEntities _safiCtx = new SafiEntities();
        private Microsoft.Office.Interop.Excel.Application ApExcel;
        private List<string> ListError = new List<string>();

        private List<DocumentoGenerado> ListadoDocumentosGenerados = new List<DocumentoGenerado>();

        private Logger logger = LogManager.GetCurrentClassLogger();
        // GET: Cotizaciones
        public ActionResult Index()
        {
            return PartialView("_index");
        }
        [HttpPost]
        public JsonResult UploadDocument(HttpPostedFileBase file)
        {
            try
            {
                var path = "";
                var listError = new List<string>();
                if (file != null && file.ContentLength > 0)
                {
                    try
                    {
                        var basePath = ConfigurationManager.AppSettings.Get("RepositorioDocumentos");
                        var almacenFisico = Tools.CrearCaminos(basePath, new List<string>() { ConfigurationManager.AppSettings.Get("Empresa").ToString(), ConfigurationManager.AppSettings.Get("segmento").ToString(), Path.GetFileNameWithoutExtension(file.FileName) });//Tools.CrearCaminos(basePath, new List<string>() { "PPM", "MUNDIAL_MILES", Path.GetFileNameWithoutExtension(file.FileName) });
                        path = Path.Combine(almacenFisico, file.FileName);
                        file.SaveAs(path);
                        var asientos = ToEntidadHojaExcelList(path, out listError);
                        if (asientos != null)
                        {
                            var data = ProcesaInfoAsientos(asientos);

                            foreach (var item in data)
                            {
                                SaveData(item);
                            }

                            Session["documentosGenerados"] = ListadoDocumentosGenerados;
                        }
                        else
                        {
                            listError.Add("No  cargaron los datos");
                        }
                    }
                    catch (Exception ex)
                    {
                        listError.Add(ex.Message);
                    }
                }

                Session["error"] = listError;
                return null;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Stopped program because of exception");
                throw;
            }

        }
        public JsonResult GetErrores()
        {
            try
            {
                JsonResultState state = new JsonResultState();
                var Xhtml = "";

                var errores = (List<string>)Session["error"];
                Xhtml = this.PartialViewToString("_lisError", errores);

                state.Xhtml = Xhtml;
                var res = new JsonResult { Data = state, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                return res;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Stopped program because of exception");
                throw;
            }

        }

        public JsonResult GetDocumentosGenerados()
        {
            try
            {
                JsonResultState state = new JsonResultState();
                var Xhtml = "";

                var documentos = (List<DocumentoGenerado>)Session["documentosGenerados"];

                if (documentos != null)
                    Xhtml = this.PartialViewToString("_DocumentosGenerados", documentos);
                else
                    Xhtml = this.PartialViewToString("_DocumentosGenerados", new List<DocumentoGenerado>());

                state.Xhtml = Xhtml;
                var res = new JsonResult { Data = state, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                return res;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Stopped program because of exception");
                throw;
            }
        }

        public dtoMundialMiles ToEntidadHojaExcelList(string pathDelFicheroExcel, out List<string> error)
        {
            try
            {
                error = new List<string>();
                BussinesLogTran modelo = new BussinesLogTran();

                var res = new dtoMundialMiles();
                try
                {

                    FileInfo existingFile = new FileInfo(pathDelFicheroExcel);
                    using (ExcelPackage package = new ExcelPackage(existingFile))
                    {
                        // get the first worksheet in the workbook
                        ExcelWorksheet worksheet = package.Workbook.Worksheets.First();

                        if (worksheet != null)
                        {
                            List<Cotizacion> listadoCotizaciones = new List<Cotizacion>();

                            int colCount = worksheet.Dimension.End.Column;  //get Column Count
                            int rowCount = worksheet.Dimension.End.Row;     //get row count
                            for (int row = 2; row <= rowCount; row++)
                            {
                                Cotizacion registro = new Cotizacion
                                {
                                    Cliente = (worksheet.GetValue(row, 1) ?? "").ToString().Trim(),
                                    Identificacion = (worksheet.GetValue(row, 2) ?? "").ToString().Trim(),
                                    MailCliente = (worksheet.GetValue(row, 3) ?? "").ToString().Trim(),
                                    Direccion = (worksheet.GetValue(row, 4) ?? "").ToString().Trim(),


                                    Detalle = (worksheet.GetValue(row, 5) ?? "").ToString().Trim(),
                                    Cantidad = (worksheet.GetValue(row, 6) ?? "").ToString().Trim(),
                                    PrecioUnitario = (worksheet.GetValue(row, 7) ?? "").ToString().Trim(),
                                    SubTotal = (worksheet.GetValue(row, 8) ?? "").ToString().Trim(),
                                    Descuento = (worksheet.GetValue(row, 9) ?? "").ToString().Trim(),
                                    Total = (worksheet.GetValue(row, 10) ?? "").ToString().Trim(),
                                    NumeroPedido = (worksheet.GetValue(row, 11) ?? "").ToString().Trim(),
                                    Comentario1 = (worksheet.GetValue(row, 12) ?? "").ToString().Trim(),
                                    Comentario2 = (worksheet.GetValue(row, 13) ?? "").ToString().Trim(),
                                    NumeroOP = (worksheet.GetValue(row, 14) ?? "").ToString().Trim(),
                                    Bodega = (worksheet.GetValue(row, 8) ?? "").ToString().Trim(),
                                    //ProductoServicio = worksheet.GetValue(row, 16).ToString()
                                };
                                listadoCotizaciones.Add(registro);

                                res.Cabeceras.Add(new dtoCabecera
                                {
                                    NombreCliente = (worksheet.GetValue(row, 1) ?? "").ToString().Trim(),
                                    RucCliente = (worksheet.GetValue(row, 2) ?? "").ToString().Trim(),
                                    MailCliente = (worksheet.GetValue(row, 3) ?? "").ToString().Trim(),
                                    DirCliente = (worksheet.GetValue(row, 4) ?? "").ToString().Trim(),
                                    //Comentarios
                                    FhComent = (worksheet.GetValue(row, 5) ?? "").ToString().Trim(),
                                    FhComent1 = (worksheet.GetValue(row, 6) ?? "").ToString().Trim(),
                                    FhComent2 = (worksheet.GetValue(row, 7) ?? "").ToString().Trim(),
                                    Bodega = (worksheet.GetValue(row, 8) ?? "").ToString().Trim(),


                                    TelefonoCliente = "",
                                    ObservFactura = "",
                                    CantDetalle = (worksheet.GetValue(row, 10) ?? "").ToString().Trim(),
                                    SubTotalFactura = (worksheet.GetValue(row, 12) ?? "").ToString().Trim(),
                                    Descuento = (worksheet.GetValue(row, 13) ?? "").ToString().Trim(),
                                    TotalFactura = (worksheet.GetValue(row, 14) ?? "").ToString().Trim(),
                                    Vencimiento = (worksheet.GetValue(row, 15) ?? "").ToString().Trim(),// Fecha de Emison
                                    Segmento = ConfigurationManager.AppSettings.Get("segmento"),
                                    Comentario1 = "-",//worksheet.GetValue(row, 11).ToString(), // COMENTARIO 1
                                    PlazoDias = "0", // Ya no va
                                    
                                });

                                res.Detalles.Add(new dtoDetalle
                                {
                                    Detalle = "",//worksheet.GetValue(row, 5).ToString(),
                                    CodigoProducto = (worksheet.GetValue(row, 9) ?? "").ToString().Trim(),//ConfigurationManager.AppSettings["codProducto"],
                                    Cantidad = (worksheet.GetValue(row, 10) ?? "").ToString().Trim(),
                                    Valor = (worksheet.GetValue(row, 11) ?? "").ToString().Trim(), // PrecioUnitario
                                    SubTotal = (worksheet.GetValue(row, 12) ?? "").ToString().Trim(),
                                    CodigoCategoria = "7",//ver nueva
                                    Descuento = (worksheet.GetValue(row, 13) ?? "").ToString().Trim(),
                                });
                            }

                            // Seteando secuenciales de cotizaciones
                            var secuencialCotizaciones = Convert.ToUInt64(modelo.GetSecuencial("COTIZACIONES", "GENERAL COTIZACIONES"));
                            foreach (var item in listadoCotizaciones)
                            {
                                item.Secuencial = secuencialCotizaciones.ToString();
                                secuencialCotizaciones += 1;
                            }
                        }
                    }

                    
                    var secuencialCabecera = Convert.ToUInt64(modelo.GetSecuencial("COTIZACIONES", ConfigurationManager.AppSettings.Get("segmento").ToString()));
                    foreach (var item in res.Cabeceras)
                    {
                        item.SecuencialFactura = secuencialCabecera.ToString();
                        secuencialCabecera += 1;
                    }


                    var secuencialDetalle = Convert.ToUInt64(modelo.GetSecuencial("COTIZACIONES", ConfigurationManager.AppSettings.Get("segmento").ToString()));
                    foreach (var item in res.Detalles)
                    {
                        item.SecuencialFactura = secuencialDetalle.ToString();
                        secuencialDetalle += 1;
                    }

                    return res;
                }
                catch (Exception ex)
                {
                    error.Add(ex.Message);
                }
                return null;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Stopped program because of exception");
                throw;
            }

        }

        private List<string> VerificarEstructuraExcelFactura(string pathDelFicheroExcel)
        {
            List<string> errores = new List<string>();
            try
            {
                var excel = new ExcelQueryFactory(pathDelFicheroExcel);
                var worksheetNames = excel.GetWorksheetNames().ToArray();

                for (int i = 0; i < worksheetNames.Length; i++)
                {
                    var columnNames = excel.GetColumnNames(worksheetNames[i]); // Nombre de columnas
                    var titleRow = FindTitleRow(excel.WorksheetNoHeader(worksheetNames[i]));
                    int contadorFila = 0;
                    // Recorriendo todas las filas
                    foreach (var fila in excel.WorksheetNoHeader(worksheetNames[i]).Where(x => x[0] != "")) //worksheetNames[i]).Where(x => x[1] != "")
                    {
                        if (contadorFila > 0) // Para no evaluar la cabecera de titulos de las columnas
                        {
                            // Recorriendo todas las columnas
                            foreach (var columna in columnNames)
                            {
                                //var tempCol = "";
                                var indiceColumna = 0;
                                //bool existeSecuencial = false;
                                var error = "";
                                indiceColumna = GetColumnIndex(titleRow, columna);
                                // Siempre que encuentre la columna
                                if (indiceColumna != -1)
                                {
                                    var valor = fila[indiceColumna].Cast<string>();
                                    error = Tools.ValidarCampo(columna, valor, false);

                                    if (!string.IsNullOrEmpty(error))
                                    {
                                        error = "Fila: " + contadorFila + " - " + error;
                                        errores.Add(error);
                                    }
                                }
                            }
                        }
                        contadorFila++;
                    }
                }
                return errores;
            }
            catch (Exception ex)
            {
                errores.Add(ex.Message);
                logger.Info("------- Errores de documento de carga -----------");
                logger.Error(ex, "Stopped program because of exception");
                return errores;
            }
        }

        private RowNoHeader FindTitleRow(IQueryable<RowNoHeader> rows)
        {
            try
            {
                return rows.FirstOrDefault();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Stopped program because of exception");
                throw;
            }
        }

        private int GetColumnIndex(List<LinqToExcel.Cell> titleRow, string title)
        {
            try
            {
                return titleRow.FindIndex(c => c.ToString() == title);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Stopped program because of exception");
                throw;
            }
        }

        private List<Wrapper> ProcesaInfoAsientos(dtoMundialMiles dto)
        {
            var data = new List<Wrapper>();
            try
            {
                if (dto.Cabeceras.Any())
                {
                    foreach (var ele in dto.Cabeceras.Where(t => t.NombreCliente != null))
                    {
                        var w = new Wrapper();
                        var cab = new Cabecera();
                        var det = new Detalle();
                        var cliente = new Cliente
                        {
                            CodigoCliente = !string.IsNullOrEmpty(ele.RucCliente) ? ele.RucCliente : "",
                            NombreCliente = !string.IsNullOrEmpty(ele.NombreCliente) ? ele.NombreCliente : "",
                            Identificacion = !string.IsNullOrEmpty(ele.RucCliente) ? ele.RucCliente : "",
                            Direccion = !string.IsNullOrEmpty(ele.DirCliente) ? ele.DirCliente : "",
                            Mail = !string.IsNullOrEmpty(ele.MailCliente) ? ele.MailCliente : "",
                            Telefono = !string.IsNullOrEmpty(ele.TelefonoCliente) ? ele.TelefonoCliente : "",
                            Segmento = !string.IsNullOrEmpty(ele.Segmento) ? ele.Segmento : ""
                        };
                        cab.Cliente = cliente;
                        cab.CotizacionDetalle = new DetallesCotizacion
                        {
                            PlazoDias = ele.PlazoDias,
                            Vencimiento = ele.Vencimiento,
                            Comentario1 = ele.Comentario1,
                            FhComent = ele.FhComent,
                            FhComent1 = ele.FhComent1,
                            FhComent2 = ele.FhComent2,
                            Bodega = ele.Bodega,
                        };

                        w.Cabecera = cab;
                        var listDet = new List<DetalleFactura>();
                        var detalles = dto.Detalles.Where(t => t.SecuencialFactura == ele.SecuencialFactura);
                        if (detalles.Any())
                        {
                            foreach (var item in detalles)
                            {
                                decimal iva = ConfigurationManager.AppSettings.Get("iva") != null ? decimal.Parse(ConfigurationManager.AppSettings.Get("iva")) : 0;

                                int cantidad = !string.IsNullOrEmpty(item.Cantidad) ? int.Parse(item.Cantidad) : 1;
                                decimal itemValor = !string.IsNullOrEmpty(item.Valor) ? decimal.Parse(item.Valor) : 0;
                                decimal descuento = !string.IsNullOrEmpty(item.Descuento) ? decimal.Parse(item.Descuento) : 0;

                                decimal subtotal = (itemValor * cantidad) - descuento;
                                decimal valor = subtotal;//subtotal + (subtotal * (iva / 100));
                                decimal cUnit = itemValor;// + (itemValor * (iva / 100));
                                var detItem = new DetalleFactura
                                {
                                    Cantidad = cantidad,
                                    Detalle = !string.IsNullOrEmpty(item.Detalle) ? item.Detalle : "",

                                    Valor = valor, //subtotal+iva
                                    SubTotal = subtotal,// !string.IsNullOrEmpty(item.SubTotal) ? decimal.Parse(item.SubTotal) : 0,
                                    Descuento = !string.IsNullOrEmpty(item.Descuento) ? decimal.Parse(item.Descuento) : 0,
                                    Total = subtotal,//0,//ver

                                    CodigoCategoria = !string.IsNullOrEmpty(item.CodigoCategoria) ? item.CodigoCategoria : "",
                                    CodigoProducto = !string.IsNullOrEmpty(item.CodigoProducto) ? item.CodigoProducto : "",
                                    RUCProveedor = !string.IsNullOrEmpty(item.RucProveedor) ? item.RucProveedor : "",
                                    Proveedor = !string.IsNullOrEmpty(item.Proveedor) ? item.Proveedor : "",
                                    CostoUnitario = cUnit, //cunit+iva
                                    FechaVenta = !string.IsNullOrEmpty(item.FechaVenta) ? DateTime.Parse(item.FechaVenta) : DateTime.Now
                                };
                                listDet.Add(detItem);
                            }
                        }

                        var fact = new Factura
                        {
                            Secuencial = !string.IsNullOrEmpty(ele.SecuencialFactura) ? ele.SecuencialFactura : "",
                            Observacion = !string.IsNullOrEmpty(ele.ObservFactura) ? ele.ObservFactura : "",
                            SubTotal = !string.IsNullOrEmpty(ele.SubTotalFactura) ? decimal.Parse(ele.SubTotalFactura) : 0,
                            Total = !string.IsNullOrEmpty(ele.TotalFactura) ? decimal.Parse(ele.TotalFactura) : 0,
                            Descuento = !string.IsNullOrEmpty(ele.Descuento) ? decimal.Parse(ele.Descuento) : 0,
                        };
                        fact.DetalleFactura.AddRange(listDet);

                        det.Factura = fact;
                        w.Detalle = det;
                        data.Add(w);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Stopped program because of exception");
            }

            return data;
        }

        public void FillResultadoDocumentos(tfacturas objeto = null, string mensaje = "")
        {
            try
            {
                if (objeto != null)
                {
                    DocumentoGenerado documento = new DocumentoGenerado
                    {
                        Estado = objeto.CodigoError,
                        Numero = objeto.factura,
                        Descripcion = mensaje,
                        ClaveAcceso = objeto.ClaveAcceso
                    };
                    ListadoDocumentosGenerados.Add(documento);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Stopped program because of exception");
            }
        }

        public void SaveData(Wrapper wrapper)
        {
            try
            {
                var requestData = "";
                var tRespuesta = new Respuesta();
                var estado = "OK";

                DateTime dinit = DateTime.Now;
                DateTime dfinal = DateTime.Now;
                var trama = JsonConvert.SerializeObject(wrapper);
                var log = new AtisLogTran { TipoSolicitud = 1, NumeroDocumento = "", TramaEntrada = trama, FechaEntrada = DateTime.Now, Estado = estado, TramaRespuesta = "", FechaSalida = DateTime.Now, Tipo = "COTIZACIONES", Secuencial = wrapper.Detalle.Factura.Secuencial, Canal = "GENERAL COTIZACIONES" };
                modelLog.Add(log);

                var numDoc = wrapper?.Detalle?.Factura?.Secuencial != null ? modelLog.ExisteSecuencialCotizacionAtisLogTran(wrapper.Detalle.Factura.Secuencial, "COTIZACIONES") : "";
                if (string.IsNullOrEmpty(numDoc))
                {
                    var costumer = CostumerDispatcherSwitch.GetNewCostumer(wrapper);
                    dinit = DateTime.Now;
                    var requestCostumer = CostumerDispatcherSwitch.InsertCostumerRequest(costumer, apiBaseUri, apiCostumerUri); // Cliente
                    requestData = requestCostumer != null ? requestCostumer.Content : "";
                    dfinal = DateTime.Now;

                    RequestData obj = JsonConvert.DeserializeObject<RequestData>(requestData);
                    if (obj != null)
                    {
                        if (obj.StatusCode != 200)
                        {
                            var mensaje = "";
                            if (obj.Messages.Any() && obj.Messages != null)
                                mensaje = obj.Messages[0].Message + "-" + obj.Messages[0].InfoAdditional;
                            estado = "ERROR";
                            tRespuesta = new Respuesta { mensaje = mensaje, codigoRetorno = obj.StatusCode.ToString(), estado = "ERROR", numeroDocumento = "" };

                            log = new AtisLogTran { TipoSolicitud = 1, NumeroDocumento = "", TramaEntrada = costumer, FechaEntrada = dinit, Estado = estado, TramaRespuesta = JsonConvert.SerializeObject(tRespuesta), FechaSalida = dfinal, Tipo = "COTIZACIONES", Secuencial = wrapper.Detalle.Factura.Secuencial, Canal = "GENERAL COTIZACIONES" };
                            modelLog.Add(log);

                            FillResultadoDocumentos(new tfacturas { factura = "No se generó", ClaveAcceso = "No se generó", CodigoError = obj.StatusCode.ToString() }, mensaje);
                        }
                        else
                        {
                            estado = "OK";
                            tRespuesta = new Respuesta { mensaje = "PROCESO OK", codigoRetorno = obj.StatusCode.ToString(), estado = "OK", numeroDocumento = "" };

                            log = new AtisLogTran { TipoSolicitud = 1, NumeroDocumento = "", TramaEntrada = costumer, FechaEntrada = dinit, Estado = estado, TramaRespuesta = JsonConvert.SerializeObject(tRespuesta), FechaSalida = dfinal, Tipo = "COTIZACIONES", Secuencial = wrapper.Detalle.Factura.Secuencial, Canal = "GENERAL COTIZACIONES" };
                            modelLog.Add(log);

                            var secuencial = modelTDoc.GetSecuencial("CT", "COTIZACION"/*ConfigurationManager.AppSettings.Get("segmento")*/);
                            modelTDoc.UpdateSecuencial("CT", "COTIZACION" /*ConfigurationManager.AppSettings.Get("segmento")*/);
                            var fact = new tfacturas { tipo = "CT", factura = ConfigurationManager.AppSettings.Get("estab") + "007" + secuencial.ToString().PadLeft(9, '0'), idFactura = int.Parse(secuencial.ToString()) };
                            modelFact.Add(fact);

                            var sales = SalesDispatcherSwitch.GetNuevaCotizacion(wrapper, fact.factura);
                            dinit = DateTime.Now;
                            var request = SalesDispatcherSwitch.InsertSalesRequest(sales, apiBaseUri, apiSalesUri);
                            requestData = request.Content;
                            dfinal = DateTime.Now;

                            obj = JsonConvert.DeserializeObject<RequestData>(requestData);
                            if (obj.StatusCode != 200)
                            {
                                var mensaje = "";
                                if (obj.Messages.Any())
                                {
                                    foreach (var ele in obj.Messages)
                                    {
                                        mensaje += ele.Message + ",";
                                    }
                                }
                                estado = "ERROR";
                                tRespuesta = new Respuesta { mensaje = mensaje, codigoRetorno = obj.StatusCode.ToString(), estado = "ERROR", numeroDocumento = fact.factura };

                                log = new AtisLogTran { TipoSolicitud = 1, NumeroDocumento = fact.factura, TramaEntrada = sales, FechaEntrada = dinit, Estado = estado, TramaRespuesta = JsonConvert.SerializeObject(tRespuesta), FechaSalida = dfinal, Tipo = "COTIZACIONES", Secuencial = wrapper.Detalle.Factura.Secuencial, Canal = "GENERAL COTIZACIONES" };
                                modelLog.Add(log);

                                FillResultadoDocumentos(fact, mensaje);
                            }
                            else
                            {
                                if (obj.Messages.Any())
                                {
                                    var texto = obj.Messages[0].Message.Split(':');
                                    var clave = texto[1].Substring(0, texto[1].Length - 2);
                                    fact.ClaveAcceso = clave;
                                    modelFact.Update(fact);
                                }
                                estado = "OK";
                                tRespuesta = new Respuesta { mensaje = "PROCESO OK", codigoRetorno = obj.StatusCode.ToString(), estado = "OK", numeroDocumento = fact.factura };
                                log = new AtisLogTran { TipoSolicitud = 1, NumeroDocumento = fact.factura, TramaEntrada = sales, FechaEntrada = dinit, Estado = estado, TramaRespuesta = JsonConvert.SerializeObject(tRespuesta), FechaSalida = dfinal, Tipo = "COTIZACIONES", Secuencial = wrapper.Detalle.Factura.Secuencial, Canal = "GENERAL COTIZACIONES" };
                                modelLog.Add(log);

                                fact.CodigoError = "200";

                                FillResultadoDocumentos(fact, "PROCESO OK");
                            }
                        }
                    }
                    else
                    {
                        estado = "ERROR";
                        tRespuesta = new Respuesta { mensaje = "Servicios caídos, consulte con su proveedor.", codigoRetorno = "400", estado = "ERROR", numeroDocumento = "" };
                        log = new AtisLogTran { TipoSolicitud = 1, NumeroDocumento = "", TramaEntrada = costumer, FechaEntrada = dinit, Estado = estado, TramaRespuesta = JsonConvert.SerializeObject(tRespuesta), FechaSalida = dfinal, Tipo = "COTIZACIONES", Secuencial = wrapper.Detalle.Factura.Secuencial, Canal = "GENERAL COTIZACIONES" };
                        modelLog.Add(log);

                        FillResultadoDocumentos(new tfacturas { factura = "No se generó", CodigoError = "400", ClaveAcceso = "No se generó" }, "Servicios caídos, consulte con su proveedor.");
                    }
                }
                else
                {
                    estado = "OK";
                    tRespuesta = new Respuesta { mensaje = "El secuencial ya existe en la factura: " + numDoc, codigoRetorno = "200", estado = estado, numeroDocumento = numDoc };

                    log = new AtisLogTran { TipoSolicitud = 1, NumeroDocumento = numDoc, TramaEntrada = trama, FechaEntrada = dinit, Estado = estado, TramaRespuesta = JsonConvert.SerializeObject(tRespuesta), FechaSalida = dfinal, Tipo = "COTIZACIONES", Secuencial = wrapper.Detalle.Factura.Secuencial, Canal = "GENERAL COTIZACIONES" };
                    modelLog.Add(log);

                    FillResultadoDocumentos(new tfacturas { factura = "No se generó", CodigoError = "200", ClaveAcceso = "No se generó" }, "El secuencial ya existe en la factura: " + numDoc);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Stopped program because of exception");
                throw;
            }

        }
    }
}