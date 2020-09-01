using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using App_Mundial_Miles;
using App_Mundial_Miles.Helpers;
using App_Mundial_Miles.Models;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using LinqToExcel;
using Excel = Microsoft.Office.Interop.Excel;
using AtisCode.Aplicacion;
using Newtonsoft.Json;
using AtisCode.Aplicacion.Model.db_Local;
using AtisCode.Aplicacion.Model.db_Safi;
using System.Net.Http;
using NLog;
using OfficeOpenXml;

namespace App_Mundial_Miles.Controllers
{

    public class CargaNotasCreditoController : Controller
    {
        static string apiBaseUri = ConfigurationManager.AppSettings["apiBaseUri"].ToString();//"http://172.16.36.84:8081/safitoken";
        static string apiCostumerUri = ConfigurationManager.AppSettings["apiCostumerUri"].ToString();//"http://172.16.36.84:8082/api/migration";
        static string apiSalesUri = ConfigurationManager.AppSettings["apiSalesUri"].ToString()+"credito";//"http://172.16.36.84:8083/api/sri/credito";

        //const string apiBaseUri = "http://172.16.36.84:8084/safitoken";
        //const string apiCostumerUri = "http://172.16.36.84:8085/api/migration";
        //const string apiSalesUri = "http://172.16.36.84:8086/api/sri/credito";

        BussinesLogTran modelLog = new BussinesLogTran();
        BussinesFactura modelFact = new BussinesFactura();
        BussinesTipoDocumento modelTDoc = new BussinesTipoDocumento();

        SafiEntities _safiCtx = new SafiEntities();
        private Excel.Application ApExcel;
        private List<string> ListError = new List<string>();

        private List<DocumentoGenerado> ListadoDocumentosGenerados = new List<DocumentoGenerado>();

        private Logger logger = LogManager.GetCurrentClassLogger();
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
                        var almacenFisico = Tools.CrearCaminos(basePath, new List<string>() { ConfigurationManager.AppSettings.Get("Empresa").ToString(), ConfigurationManager.AppSettings.Get("segmento").ToString(), Path.GetFileNameWithoutExtension(file.FileName) });
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
                            listError.Add("No se cargaron datos");
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
                Xhtml = this.PartialViewToString("_DocumentosGenerados", documentos);

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

        public dtoMundialMilesRet ToEntidadHojaExcelList(string pathDelFicheroExcel, out List<string> error)
        {
            try
            {
                error = new List<string>();
                var res = new dtoMundialMilesRet();
                BussinesLogTran modelo = new BussinesLogTran();

                var erroresValidacion = VerificarEstructuraExcelNotaCredito(pathDelFicheroExcel);

                if (erroresValidacion.Count > 0)
                {
                    foreach (var item in erroresValidacion)
                    {
                        error.Add(item);
                    }
                    return null;
                }

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
                                res.Cabeceras.Add(new dtoCabeceraRet
                                {
                                    Factura = worksheet.GetValue(row, 1).ToString(),
                                    Motivo = worksheet.GetValue(row, 3).ToString(),
                                    TotalFactura = worksheet.GetValue(row, 7).ToString(),
                                    CantDetalle = "1",
                                    Segmento = ConfigurationManager.AppSettings.Get("segmento"),
                                });

                                res.Detalles.Add(new dtoDetalleRet
                                {
                                    RucProveedor = worksheet.GetValue(row, 2).ToString(),
                                    Detalle = worksheet.GetValue(row, 3).ToString(),
                                    Cantidad = worksheet.GetValue(row, 4).ToString(),
                                    Valor = worksheet.GetValue(row, 5).ToString(),
                                    SubTotal = worksheet.GetValue(row, 6).ToString(),
                                    FechaVenta = worksheet.GetValue(row, 8).ToString(),
                                    CodigoProducto = ConfigurationManager.AppSettings["codProducto"],
                                });
                            }

                            var secuencialCabecera = Convert.ToUInt64(modelo.GetSecuencial("NOTACREDITO", ConfigurationManager.AppSettings.Get("segmento").ToString()));
                            foreach (var item in res.Cabeceras)
                            {
                                item.SecuencialFactura = secuencialCabecera.ToString();
                                secuencialCabecera += 1;
                            }

                            var secuencialDetalle = Convert.ToUInt64(modelo.GetSecuencial("NOTACREDITO", ConfigurationManager.AppSettings.Get("segmento").ToString()));
                            foreach (var item in res.Detalles)
                            {
                                item.SecuencialFactura = secuencialDetalle.ToString();
                                secuencialDetalle += 1;
                            }
                        }
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

        private List<string> VerificarEstructuraExcelNotaCredito(string pathDelFicheroExcel)
        {
            List<string> errores = new List<string>();
            try
            {
                FileInfo existingFile = new FileInfo(pathDelFicheroExcel);

                using (ExcelPackage package = new ExcelPackage(existingFile))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.First();


                    if (worksheet != null)
                    {
                        int colCount = worksheet.Dimension.End.Column;  //get Column Count
                        int rowCount = worksheet.Dimension.End.Row;     //get row count
                        for (int row = 2; row <= rowCount; row++)
                        {
                            for (int col = 1; col <= colCount; col++)
                            {
                                var error = "";
                                string columna = (worksheet.Cells[1, col].Value ?? "").ToString().Trim(); // Nombre de la Columna
                                string valorColumna = (worksheet.Cells[row, col].Value ?? "").ToString().Trim();

                                error = Tools.ValidarCampo(columna, valorColumna, true);

                                if (!string.IsNullOrEmpty(error))
                                {
                                    error = "Fila: " + row + " - " + error;
                                    errores.Add(error);
                                }
                                //Console.WriteLine(" Row:" + row + " column:" + col + " Value:" + worksheet.Cells[row, col].Value.ToString().Trim());
                            }
                        }
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

        private List<WrapperNotaCredito> ProcesaInfoAsientos(dtoMundialMilesRet dto)
        {
            var data = new List<WrapperNotaCredito>();
            try
            {
                if (dto.Cabeceras.Any())
                {
                    foreach (var ele in dto.Cabeceras.Where(t => t.Factura != null))
                    {
                        var w = new WrapperNotaCredito();
                        var cab = new Cabecera();
                        var listDetalle = new List<DetalleNotaCredito>();

                        decimal valor = 0;
                        var detalles = dto.Detalles.Where(t => t.SecuencialFactura == ele.SecuencialFactura);
                        if (detalles.Any())
                        {
                            foreach (var item in detalles)
                            {
                                decimal iva = ConfigurationManager.AppSettings.Get("iva") != null ? decimal.Parse(ConfigurationManager.AppSettings.Get("iva")) : 0;
                                int cantidad = !string.IsNullOrEmpty(item.Cantidad) ? int.Parse(item.Cantidad) : 1;
                                decimal itemValor = !string.IsNullOrEmpty(item.Valor) ? decimal.Parse(item.Valor) : 0;//pUnit

                                decimal subtotal = (itemValor * cantidad);
                                valor = subtotal + (subtotal * (iva / 100));
                                decimal cUnit = itemValor + (itemValor * (iva / 100));


                                var det = new DetalleNotaCredito
                                {
                                    Cantidad = !string.IsNullOrEmpty(item.Cantidad) ? int.Parse(item.Cantidad) : 1,
                                    Detalle = item.Detalle,
                                    Valor = valor,
                                    SubTotal = !string.IsNullOrEmpty(item.Cantidad) ? decimal.Parse(item.SubTotal) : 0,
                                    CostoUnitario = cUnit,
                                    CodigoCategoria = item.CodigoCategoria,
                                    CodigoProducto = item.CodigoProducto,
                                    RUCProveedor = item.RucProveedor,
                                    Proveedor = item.Proveedor,
                                    FechaVenta = !string.IsNullOrEmpty(item.Cantidad) ? DateTime.Parse(item.FechaVenta) : DateTime.Now
                                };
                                listDetalle.Add(det);
                            }
                        }

                        var nc = new NotaCredito
                        {
                            Idfactura = ele.Factura,
                            Motivo = ele.Motivo,
                            Valor = !string.IsNullOrEmpty(ele.TotalFactura) ? decimal.Parse(ele.TotalFactura) : 0,
                            Secuencial = ele.SecuencialFactura,
                            Estado = 0,
                            DetallesNotaCredito = new List<DetalleNotaCredito>()
                        };
                        nc.DetallesNotaCredito.AddRange(listDetalle);

                        cab.Detalle = nc;
                        w.NotaCredito = cab;
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

        public void SaveData(WrapperNotaCredito wrapper)
        {
            try
            {
                var request = "";
                var tRespuesta = new Respuesta();
                var estado = "OK";
                var mensaje = "";

                DateTime dinit = DateTime.Now;
                DateTime dfinal = DateTime.Now;
                var trama = JsonConvert.SerializeObject(wrapper);
                var log = new AtisLogTran { TipoSolicitud = 1, NumeroDocumento = "", TramaEntrada = trama, FechaEntrada = DateTime.Now, Estado = estado, TramaRespuesta = "", FechaSalida = DateTime.Now, Tipo = "NOTACREDITO", Secuencial = wrapper.NotaCredito.Detalle.Secuencial, Canal = ConfigurationManager.AppSettings.Get("segmento") };
                modelLog.Add(log);

                var numDoc = wrapper?.NotaCredito?.Detalle?.Secuencial != null ? modelLog.ExisteSecuencialAtisLogTran(wrapper.NotaCredito.Detalle.Secuencial, "NOTACREDITO", ConfigurationManager.AppSettings.Get("segmento")) : "";
                if (string.IsNullOrEmpty(numDoc))
                {
                    var secuencial = modelTDoc.GetSecuencial("NC", ConfigurationManager.AppSettings["segmento"]);
                    modelTDoc.UpdateSecuencial("NC", ConfigurationManager.AppSettings["segmento"]);
                    var fact = new tfacturas { tipo = "NC", factura = ConfigurationManager.AppSettings["estab"] + ConfigurationManager.AppSettings["ptoEmi"] + secuencial.ToString().PadLeft(9, '0'), idFactura = int.Parse(secuencial.ToString()) };
                    modelFact.Add(fact);

                    var sales = SalesDispatcherSwitch.GetNewNcSales(trama, fact.factura);
                    if (!string.IsNullOrEmpty(sales))
                    {
                        dinit = DateTime.Now;
                        request = SalesDispatcherSwitch.InsertNcSalesRequest(sales, apiBaseUri, apiSalesUri);
                        dfinal = DateTime.Now;

                        var obj = JsonConvert.DeserializeObject<RequestData>(request);
                        if (obj != null)
                        {
                            if (obj.StatusCode != 200)
                            {
                                if (obj.Messages.Any()) {

                                    mensaje = obj.Messages[0].Message + "-" + obj.Messages[0].InfoAdditional;

                                }
                                estado = "ERROR";
                                tRespuesta = new Respuesta { mensaje = mensaje, codigoRetorno = obj.StatusCode.ToString(), estado = "ERROR", numeroDocumento = fact.factura };

                                FillResultadoDocumentos(new tfacturas { factura = "No se generó", ClaveAcceso = "No se generó", CodigoError = obj.StatusCode.ToString() }, mensaje);
                            }
                            else
                            {
                                estado = "OK";
                                tRespuesta = new Respuesta { mensaje = "PROCESO OK", codigoRetorno = obj.StatusCode.ToString(), estado = "OK", numeroDocumento = fact.factura };
                                log = new AtisLogTran { TipoSolicitud = 1, NumeroDocumento = fact.factura, TramaEntrada = sales, FechaEntrada = dinit, Estado = estado, TramaRespuesta = JsonConvert.SerializeObject(tRespuesta), FechaSalida = dfinal, Tipo = "NOTACREDITO", Secuencial = wrapper.NotaCredito.Detalle.Secuencial, Canal = ConfigurationManager.AppSettings["segmento"] };
                                modelLog.Add(log);

                                fact.CodigoError = "200";
                                // Llenar resultado de los documentos emitidos
                                FillResultadoDocumentos(fact, "PROCESO OK");
                            }

                        }
                        else
                        {
                            estado = "ERROR";
                            tRespuesta = new Respuesta { mensaje = "Servicios caídos, consulte con su proveedor.", codigoRetorno = "400", estado = "ERROR", numeroDocumento = "" };

                            log = new AtisLogTran { TipoSolicitud = 1, NumeroDocumento = "", TramaEntrada = sales, FechaEntrada = dinit, Estado = estado, TramaRespuesta = JsonConvert.SerializeObject(tRespuesta), FechaSalida = dfinal, Tipo = "FACTURA", Secuencial = wrapper.NotaCredito.Detalle.Secuencial, Canal = ConfigurationManager.AppSettings["segmento"] };
                            modelLog.Add(log);

                            // Llenar resultado de los documentos emitidos
                            FillResultadoDocumentos(new tfacturas { factura = "No se generó", CodigoError = "400", ClaveAcceso = "No se generó" }, "Servicios caídos, consulte con su proveedor.");
                        }

                    }
                    else
                    {
                        if (wrapper?.NotaCredito?.Detalle?.Idfactura != null)
                        {
                            var existe = SalesDispatcherSwitch.ExisteFacturaAplicont(wrapper.NotaCredito.Detalle.Idfactura);
                            if (existe)
                            {
                                var ele = SalesDispatcherSwitch.GetFacturaAplicont(wrapper.NotaCredito.Detalle.Idfactura);
                                estado = "OK";
                                tRespuesta = new Respuesta { mensaje = "PROCESO OK", codigoRetorno = "200", estado = "OK", numeroDocumento = fact.factura };
                                log = new AtisLogTran { TipoSolicitud = 1, NumeroDocumento = ele.NumeroDocumento, TramaEntrada = ele.TramaEntrada, FechaEntrada = ele.FechaEntrada, Estado = ele.Estado, TramaRespuesta = ele.TramaRespuesta, FechaSalida = ele.FechaSalida, Tipo = "NOTACREDITO", Secuencial = ele.Secuencial, Canal = ele.Canal };
                                modelLog.Add(log);

                                fact.CodigoError = "200";
                                // Llenar resultado de los documentos emitidos
                                FillResultadoDocumentos(fact, "PROCESO OK");
                            }
                            else
                            {
                                estado = "ERROR";
                                tRespuesta = new Respuesta { mensaje = "FACTURA NO ENCONTRADA", codigoRetorno = "400", estado = "ERROR", numeroDocumento = fact.factura };
                                log = new AtisLogTran { TipoSolicitud = 1, NumeroDocumento = fact.factura, TramaEntrada = sales, FechaEntrada = dinit, Estado = estado, TramaRespuesta = JsonConvert.SerializeObject(tRespuesta), FechaSalida = dfinal, Tipo = "NOTACREDITO", Secuencial = wrapper.NotaCredito.Detalle.Secuencial, Canal = ConfigurationManager.AppSettings["segmento"] };
                                modelLog.Add(log);

                                fact.CodigoError = "400";
                                fact.ClaveAcceso = "Ninguna.";
                                // Llenar resultado de los documentos emitidos
                                FillResultadoDocumentos(fact, "FACTURA NO ENCONTRADA");
                            }
                        }
                        else
                        {
                            estado = "ERROR";
                            tRespuesta = new Respuesta { mensaje = "ID FACTURA NULL", codigoRetorno = "400", estado = "ERROR", numeroDocumento = fact.factura };
                            log = new AtisLogTran { TipoSolicitud = 1, NumeroDocumento = fact.factura, TramaEntrada = sales, FechaEntrada = dinit, Estado = estado, TramaRespuesta = JsonConvert.SerializeObject(tRespuesta), FechaSalida = dfinal, Tipo = "NOTACREDITO", Secuencial = wrapper.NotaCredito.Detalle.Secuencial, Canal = ConfigurationManager.AppSettings["segmento"] };
                            modelLog.Add(log);

                            fact.CodigoError = "400";
                            fact.ClaveAcceso = "Ninguna.";
                            // Llenar resultado de los documentos emitidos
                            FillResultadoDocumentos(fact, "ID FACTURA NULO");
                        }
                    }
                }
                else
                {
                    estado = "OK";
                    tRespuesta = new Respuesta { mensaje = "El secuencial ya existe en la Nota Crédito: " + numDoc, codigoRetorno = "200", estado = estado, numeroDocumento = numDoc };

                    log = new AtisLogTran { TipoSolicitud = 1, NumeroDocumento = numDoc, TramaEntrada = trama, FechaEntrada = dinit, Estado = estado, TramaRespuesta = JsonConvert.SerializeObject(tRespuesta), FechaSalida = dfinal, Tipo = "NOTACREDITO", Secuencial = wrapper.NotaCredito.Detalle.Secuencial, Canal = ConfigurationManager.AppSettings["segmento"] };
                    modelLog.Add(log);

                    FillResultadoDocumentos(new tfacturas { factura = "No se generó", CodigoError = "200", ClaveAcceso = "No se generó" }, "El secuencial ya existe en la Nota Crédito: " + numDoc);
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
