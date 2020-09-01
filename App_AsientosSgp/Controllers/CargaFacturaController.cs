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
using NLog;
using System.Net;
using OfficeOpenXml;
using MoreLinq;

namespace App_Mundial_Miles.Controllers
{
    public class CargaFacturaController : Controller
    {
        //PRODUCCION
        static string apiBaseUri = ConfigurationManager.AppSettings["apiBaseUri"].ToString();//"http://172.16.36.84:8081/safitoken";
        static string apiCostumerUri = ConfigurationManager.AppSettings["apiCostumerUri"].ToString();//"http://172.16.36.84:8082/api/migration";
        static string apiSalesUri = ConfigurationManager.AppSettings["apiSalesUri"].ToString() + "factura";//"http://172.16.36.84:8083/api/sri/credito";

        //const string apiBaseUri = "http://172.16.36.84:8084/safitoken";
        //const string apiCostumerUri = "http://172.16.36.84:8085/api/migration";
        //const string apiSalesUri = "http://172.16.36.84:8086/api/sri/factura";

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
                        var almacenFisico = Tools.CrearCaminos(basePath, new List<string>() { ConfigurationManager.AppSettings.Get("Empresa").ToString(), ConfigurationManager.AppSettings.Get("segmento").ToString(), Path.GetFileNameWithoutExtension(file.FileName) });//Tools.CrearCaminos(basePath, new List<string>() { "PPM", "MUNDIAL_MILES", Path.GetFileNameWithoutExtension(file.FileName) });
                        path = Path.Combine(almacenFisico, file.FileName);
                        file.SaveAs(path);
                        //var asientos = ToEntidadHojaExcelList(path, out listError); // --> FACTURAS CON UN SOLO DETALLE
                        var asientos = ToEntidadFacturaDetallesMultiplesHojaExcelList(path, out listError); // --> FACTURAS CON MÚLTIPLES DETALLES
                        if (asientos != null)
                        {
                            //var data = ProcesaInfoAsientos(asientos); // FACTURAS CON UN SOLO DETALLE
                            var data = ProcesaFacturaDetallesMultiples(asientos); // FACTURAS CON MÚLTIPLES DETALLES
                            
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
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Stopped program because of exception");
                throw;
            }

            return null;
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

                var erroresValidacion = VerificarEstructuraExcelFactura(pathDelFicheroExcel);

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
                            int colCount = worksheet.Dimension.End.Column;  //get Column Count
                            int rowCount = worksheet.Dimension.End.Row;     //get row count
                            for (int row = 2; row <= rowCount; row++)
                            {
                                res.Cabeceras.Add(new dtoCabecera
                                {
                                    NombreCliente = (worksheet.GetValue(row, 1) ?? "").ToString().Trim(),
                                    RucCliente = (worksheet.GetValue(row, 2) ?? "").ToString().Trim(),
                                    DirCliente = (worksheet.GetValue(row, 3) ?? "").ToString().Trim(),
                                    TelefonoCliente = (worksheet.GetValue(row, 4) ?? "").ToString().Trim(),
                                    MailCliente = (worksheet.GetValue(row, 5) ?? "").ToString().Trim(),
                                    ObservFactura = (worksheet.GetValue(row, 13) ?? "").ToString().Trim(),
                                    SubTotalFactura = (worksheet.GetValue(row, 9) ?? "").ToString().Trim(),
                                    TotalFactura = (worksheet.GetValue(row, 11) ?? "").ToString().Trim(),
                                    Descuento = (worksheet.GetValue(row, 10) ?? "").ToString().Trim(),
                                    Segmento = ConfigurationManager.AppSettings.Get("segmento"),
                                    CantDetalle = "1",
                                });

                                res.Detalles.Add(new dtoDetalle
                                {
                                    Detalle = (worksheet.GetValue(row, 6) ?? "").ToString().Trim(),
                                    Cantidad = (worksheet.GetValue(row, 7) ?? "").ToString().Trim(),
                                    Valor =( worksheet.GetValue(row, 8) ?? "").ToString().Trim(), // PrecioUnitario
                                    SubTotal = (worksheet.GetValue(row, 9) ?? "").ToString().Trim(),
                                    CodigoCategoria = "7",//ver nueva
                                    CodigoProducto = ConfigurationManager.AppSettings["codProducto"],
                                    Descuento = (worksheet.GetValue(row, 10) ?? "0").ToString().Trim(),
                                    FechaVenta = (worksheet.GetValue(row, 12) ?? "").ToString().Trim(),
                                });
                            }

                            var secuencialCabecera = Convert.ToUInt64(modelo.GetSecuencial("FACTURA", ConfigurationManager.AppSettings.Get("segmento").ToString()));
                            foreach (var item in res.Cabeceras)
                            {
                                item.SecuencialFactura = secuencialCabecera.ToString();
                                secuencialCabecera += 1;
                            }

                            var secuencialDetalle = Convert.ToUInt64(modelo.GetSecuencial("FACTURA", ConfigurationManager.AppSettings.Get("segmento").ToString()));
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

        public List<Documento> ToEntidadFacturaDetallesMultiplesHojaExcelList(string pathDelFicheroExcel, out List<string> error)
        {
            try
            {
                error = new List<string>();
                BussinesLogTran modelo = new BussinesLogTran();

                List<Documento> documentos = new List<Documento>();

                var erroresValidacion = VerificarEstructuraExcelFactura(pathDelFicheroExcel);

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
                            int colCount = worksheet.Dimension.End.Column;  //get Column Count
                            int rowCount = worksheet.Dimension.End.Row;     //get row count

                            List<dtoCabecera> cabeceras = new List<dtoCabecera>();
                            List<dtoDetalle> detalles = new List<dtoDetalle>();

                            for (int row = 3; row <= rowCount; row++)
                            {

                                cabeceras.Add(new dtoCabecera
                                {
                                    ID = Int32.TryParse((worksheet.GetValue(row, 1) ?? "").ToString().Trim(), out Int32 valorCabecera) ? Convert.ToInt32((worksheet.GetValue(row, 1) ?? "").ToString().Trim()) : 0,
                                    NombreCliente = (worksheet.GetValue(row, 2) ?? "").ToString().Trim(),
                                    RucCliente = (worksheet.GetValue(row, 3) ?? "").ToString().Trim(),
                                    DirCliente = (worksheet.GetValue(row, 4) ?? "").ToString().Trim(),
                                    TelefonoCliente = (worksheet.GetValue(row, 5) ?? "").ToString().Trim(),
                                    MailCliente = (worksheet.GetValue(row, 6) ?? "").ToString().Trim(),
                                    SubTotalFactura = (worksheet.GetValue(row, 7) ?? "").ToString().Trim(),
                                    Descuento = (worksheet.GetValue(row, 8) ?? "").ToString().Trim(),
                                    TotalFactura = (worksheet.GetValue(row, 9) ?? "").ToString().Trim(),
                                    Segmento = ConfigurationManager.AppSettings.Get("segmento"),
                                    ObservFactura = (worksheet.GetValue(row, 17) ?? "").ToString().Trim(),
                                    FechaVenta = (worksheet.GetValue(row, 18) ?? "").ToString().Trim(),
                                    //CantDetalle = "1",
                                });

                                detalles.Add(new dtoDetalle
                                {
                                    ID = Int32.TryParse((worksheet.GetValue(row, 1) ?? "").ToString().Trim(), out Int32 valorDetalle) ? Convert.ToInt32((worksheet.GetValue(row, 1) ?? "").ToString().Trim()) : 0,
                                    Detalle = (worksheet.GetValue(row, 10) ?? "").ToString().Trim(),
                                    Cantidad = (worksheet.GetValue(row, 11) ?? "").ToString().Trim(),
                                    Valor = (worksheet.GetValue(row, 12) ?? "").ToString().Trim(), // PrecioUnitario
                                    SubTotal = (worksheet.GetValue(row, 13) ?? "").ToString().Trim(),
                                    Descuento = (worksheet.GetValue(row, 14) ?? "0").ToString().Trim(),
                                    Total = (worksheet.GetValue(row, 15) ?? "0").ToString().Trim(),
                                    CodigoCategoria = "7",//ver nueva
                                    CodigoProducto = ConfigurationManager.AppSettings["codProducto"],
                                    FechaVenta = (worksheet.GetValue(row, 18) ?? "").ToString().Trim(),
                                });
                            }

                            var secuencialCabecera = Convert.ToUInt64(modelo.GetSecuencial("FACTURA", ConfigurationManager.AppSettings.Get("segmento").ToString()));
                            foreach (var item in cabeceras)
                            {
                                item.SecuencialFactura = secuencialCabecera.ToString();
                                secuencialCabecera += 1;
                            }

                            var secuencialDetalle = Convert.ToUInt64(modelo.GetSecuencial("FACTURA", ConfigurationManager.AppSettings.Get("segmento").ToString()));
                            foreach (var item in detalles)
                            {
                                item.SecuencialFactura = secuencialDetalle.ToString();
                                secuencialDetalle += 1;
                            }

                            cabeceras = cabeceras.DistinctBy(s => s.ID).ToList();

                            foreach (var item in cabeceras)
                            {
                                item.CantidadDetalle = detalles.Where(s => s.ID == item.ID).Count();

                                documentos.Add(new Documento
                                {
                                    Cabeceras = item,
                                    Detalles = detalles.Where(s => s.ID == item.ID).ToList(),
                                });
                            }

                        }
                    }

                    return documentos;
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
                FileInfo existingFile = new FileInfo(pathDelFicheroExcel);

                using (ExcelPackage package = new ExcelPackage(existingFile))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.First();

                    if (worksheet != null)
                    {
                        int colCount = worksheet.Dimension.End.Column;  //get Column Count
                        int rowCount = worksheet.Dimension.End.Row;      //get row count - Cabecera
                        for (int row = 2; row <= rowCount; row++)
                        {
                            for (int col = 1; col <= colCount; col++)
                            {
                                var error = "";
                                string columna = (worksheet.Cells[1, col].Value ?? "").ToString().Trim()  ; // Nombre de la Columna
                                string valorColumna = (worksheet.Cells[row, col].Value ?? "").ToString().Trim();

                                error = Tools.ValidarCampo(columna, valorColumna, true);

                                if (!string.IsNullOrEmpty(error))
                                {
                                    error = "Fila: " + row + " - " + error;
                                    errores.Add(error);
                                }
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

                        //decimal descuentoFactura = !string.IsNullOrEmpty(ele.Descuento) ? decimal.Parse(ele.Descuento) : 0;
                        //decimal valorBrutoFactura = !string.IsNullOrEmpty(ele.SubTotalFactura) ? decimal.Parse(ele.SubTotalFactura) : 0;
                        //var subtotalFactura = valorBrutoFactura - descuentoFactura;

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

        private List<Wrapper> ProcesaFacturaDetallesMultiples(List<Documento> dto)
        {
            var data = new List<Wrapper>();
            try
            {
                if (dto.Any())
                {
                    foreach (var ele in dto)
                    {
                        var w = new Wrapper();
                        var cab = new Cabecera();
                        var det = new Detalle();
                        var cliente = new Cliente
                        {
                            CodigoCliente = !string.IsNullOrEmpty(ele.Cabeceras.RucCliente) ? ele.Cabeceras.RucCliente : "",
                            NombreCliente = !string.IsNullOrEmpty(ele.Cabeceras.NombreCliente) ? ele.Cabeceras.NombreCliente : "",
                            Identificacion = !string.IsNullOrEmpty(ele.Cabeceras.RucCliente) ? ele.Cabeceras.RucCliente : "",
                            Direccion = !string.IsNullOrEmpty(ele.Cabeceras.DirCliente) ? ele.Cabeceras.DirCliente : "",
                            Mail = !string.IsNullOrEmpty(ele.Cabeceras.MailCliente) ? ele.Cabeceras.MailCliente : "",
                            Telefono = !string.IsNullOrEmpty(ele.Cabeceras.TelefonoCliente) ? ele.Cabeceras.TelefonoCliente : "",
                            Segmento = !string.IsNullOrEmpty(ele.Cabeceras.Segmento) ? ele.Cabeceras.Segmento : ""
                        };
                        cab.Cliente = cliente;
                        w.Cabecera = cab;

                        var listDet = new List<DetalleFactura>();
                        var detalles = ele.Detalles;//dto.Detalles.Where(t => t.SecuencialFactura == ele.SecuencialFactura);
                        if (detalles.Any())
                        {
                            foreach (var item in detalles)
                            {
                                decimal iva = ConfigurationManager.AppSettings.Get("iva") != null ? decimal.Parse(ConfigurationManager.AppSettings.Get("iva")) : 0;

                                int cantidad = !string.IsNullOrEmpty(item.Cantidad) ? int.Parse(item.Cantidad) : 0;
                                decimal itemValor = !string.IsNullOrEmpty(item.Valor) ? decimal.Parse(item.Valor) : 0;
                                decimal descuento = !string.IsNullOrEmpty(item.Descuento) ? decimal.Parse(item.Descuento) : 0;

                                decimal subtotal = !string.IsNullOrEmpty(item.SubTotal) ? decimal.Parse(item.SubTotal) : 0;//(itemValor * cantidad) - descuento;
                                decimal total = !string.IsNullOrEmpty(item.Total) ? decimal.Parse(item.Total) : 0;//(itemValor * cantidad) - descuento;
                                decimal valor = subtotal;//subtotal + (subtotal * (iva / 100));
                                decimal cUnit = itemValor;// + (itemValor * (iva / 100));
                                var detItem = new DetalleFactura
                                {
                                    Cantidad = cantidad,
                                    Detalle = !string.IsNullOrEmpty(item.Detalle) ? item.Detalle : "",

                                    Valor = valor, //subtotal+iva
                                    SubTotal = subtotal,// !string.IsNullOrEmpty(item.SubTotal) ? decimal.Parse(item.SubTotal) : 0,
                                    Descuento = !string.IsNullOrEmpty(item.Descuento) ? decimal.Parse(item.Descuento) : 0,
                                    Total = total,//subtotal,//0,//ver

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

                        //decimal descuentoFactura = !string.IsNullOrEmpty(ele.Descuento) ? decimal.Parse(ele.Descuento) : 0;
                        //decimal valorBrutoFactura = !string.IsNullOrEmpty(ele.SubTotalFactura) ? decimal.Parse(ele.SubTotalFactura) : 0;
                        //var subtotalFactura = valorBrutoFactura - descuentoFactura;

                        var fact = new Factura
                        {
                            Secuencial = !string.IsNullOrEmpty(ele.Cabeceras.SecuencialFactura) ? ele.Cabeceras.SecuencialFactura : "",
                            Observacion = !string.IsNullOrEmpty(ele.Cabeceras.ObservFactura) ? ele.Cabeceras.ObservFactura : "",
                            SubTotal = !string.IsNullOrEmpty(ele.Cabeceras.SubTotalFactura) ? decimal.Parse(ele.Cabeceras.SubTotalFactura) : 0,
                            Total = !string.IsNullOrEmpty(ele.Cabeceras.TotalFactura) ? decimal.Parse(ele.Cabeceras.TotalFactura) : 0,
                            Descuento = !string.IsNullOrEmpty(ele.Cabeceras.Descuento) ? decimal.Parse(ele.Cabeceras.Descuento) : 0,
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
                if (objeto != null) {
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

                //PingHost(apiCostumerUri);

                var requestData = "";
                var tRespuesta = new Respuesta();
                  var estado = "OK";

                DateTime dinit = DateTime.Now;
                DateTime dfinal = DateTime.Now;
                var trama = JsonConvert.SerializeObject(wrapper);
                var log = new AtisLogTran { TipoSolicitud = 1, NumeroDocumento = "", TramaEntrada = trama, FechaEntrada = DateTime.Now, Estado = estado, TramaRespuesta = "", FechaSalida = DateTime.Now, Tipo = "FACTURA", Secuencial = wrapper.Detalle.Factura.Secuencial, Canal = ConfigurationManager.AppSettings.Get("segmento") };
                modelLog.Add(log);

                var numDoc = wrapper?.Detalle?.Factura?.Secuencial != null ? modelLog.ExisteSecuencialAtisLogTran(wrapper.Detalle.Factura.Secuencial, "FACTURA", ConfigurationManager.AppSettings.Get("segmento")) : "";
                if (string.IsNullOrEmpty(numDoc))
                {
                    var costumer = CostumerDispatcherSwitch.GetNewCostumer(wrapper);
                    dinit = DateTime.Now;
                    var requestCostumer = CostumerDispatcherSwitch.InsertCostumerRequest(costumer, apiBaseUri, apiCostumerUri);
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

                            log = new AtisLogTran { TipoSolicitud = 1, NumeroDocumento = "", TramaEntrada = costumer, FechaEntrada = dinit, Estado = estado, TramaRespuesta = JsonConvert.SerializeObject(tRespuesta), FechaSalida = dfinal, Tipo = "FACTURA", Secuencial = wrapper.Detalle.Factura.Secuencial, Canal = ConfigurationManager.AppSettings["segmento"] };
                            modelLog.Add(log);
                            FillResultadoDocumentos(new tfacturas { factura = "No se generó", ClaveAcceso= "No se generó", CodigoError = obj.StatusCode.ToString() }, mensaje);
                        }
                        else
                        {
                            estado = "OK";
                            tRespuesta = new Respuesta { mensaje = "PROCESO OK", codigoRetorno = obj.StatusCode.ToString(), estado = "OK", numeroDocumento = "" };

                            log = new AtisLogTran { TipoSolicitud = 1, NumeroDocumento = "", TramaEntrada = costumer, FechaEntrada = dinit, Estado = estado, TramaRespuesta = JsonConvert.SerializeObject(tRespuesta), FechaSalida = dfinal, Tipo = "FACTURA", Secuencial = wrapper.Detalle.Factura.Secuencial, Canal = ConfigurationManager.AppSettings["segmento"] };
                            modelLog.Add(log);

                            var secuencial = modelTDoc.GetSecuencial("FC", ConfigurationManager.AppSettings.Get("segmento"));
                            modelTDoc.UpdateSecuencial("FC", ConfigurationManager.AppSettings.Get("segmento"));
                            var fact = new tfacturas { tipo = "FC", factura = ConfigurationManager.AppSettings.Get("estab") + ConfigurationManager.AppSettings.Get("ptoEmi") + secuencial.ToString().PadLeft(9, '0'), idFactura = int.Parse(secuencial.ToString()) };
                            modelFact.Add(fact);

                            var sales = SalesDispatcherSwitch.GetNewSalesCargaExcelDetallesMultiples(wrapper, fact.factura);//SalesDispatcherSwitch.GetNewSales(wrapper, fact.factura);////////
                            var claveAccesoDocumento = SalesDispatcherSwitch.ClaveAccesoDocumento; // Para buscar el documento en ComprobantesMasivos
                            
                            dinit = DateTime.Now;
                            var request = SalesDispatcherSwitch.InsertSalesRequest(sales, apiBaseUri, apiSalesUri);
                            requestData = request.Content;
                            dfinal = DateTime.Now;

                            //var datosDocumento = BussinessSafiParametros.GetRespuestaComprobanteMasivo(claveAccesoDocumento);

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

                                log = new AtisLogTran { TipoSolicitud = 1, NumeroDocumento = fact.factura, TramaEntrada = sales, FechaEntrada = dinit, Estado = estado, TramaRespuesta = JsonConvert.SerializeObject(tRespuesta), FechaSalida = dfinal, Tipo = "FACTURA", Secuencial = wrapper.Detalle.Factura.Secuencial, Canal = ConfigurationManager.AppSettings["segmento"] };
                                modelLog.Add(log);
                                FillResultadoDocumentos(fact, mensaje);
                            }
                            else
                            {
                                if (obj.Messages.Any())
                                {
                                    string texto = "";
                                    string clave = "";

                                    if (obj.Messages[0].Message.IndexOf(":") != -1)
                                    {
                                        texto = obj.Messages[0].Message;
                                        clave = obj.Messages[0].Message.Split(':')[1].Substring(0, obj.Messages[0].Message.Split(':')[1].Length - 2);
                                    }
                                    else {
                                        texto = obj.Messages[0].Message;
                                        clave = claveAccesoDocumento;
                                    }

                                    //var comparacionClaveAcceso = claveAccesoDocumento;

                                    fact.ClaveAcceso = clave;
                                    modelFact.Update(fact); // Clave de acceso y factura en entidad fact
                                }
                                estado = "OK";
                                tRespuesta = new Respuesta { mensaje = "PROCESO OK", codigoRetorno = obj.StatusCode.ToString(), estado = "OK", numeroDocumento = fact.factura };
                                log = new AtisLogTran { TipoSolicitud = 1, NumeroDocumento = fact.factura, TramaEntrada = sales, FechaEntrada = dinit, Estado = estado, TramaRespuesta = JsonConvert.SerializeObject(tRespuesta), FechaSalida = dfinal, Tipo = "FACTURA", Secuencial = wrapper.Detalle.Factura.Secuencial, Canal = ConfigurationManager.AppSettings["segmento"] };
                                modelLog.Add(log);

                                fact.CodigoError = "200";
                                // Llenar resultado de los documentos emitidos
                                FillResultadoDocumentos(fact, "PROCESO OK");
                            }
                        }

                    }
                    else
                    {
                        estado = "ERROR";
                        tRespuesta = new Respuesta { mensaje = "Servicios caídos, consulte con su proveedor.", codigoRetorno = "400", estado = "ERROR", numeroDocumento = "" };
                        log = new AtisLogTran { TipoSolicitud = 1, NumeroDocumento = "", TramaEntrada = costumer, FechaEntrada = dinit, Estado = estado, TramaRespuesta = JsonConvert.SerializeObject(tRespuesta), FechaSalida = dfinal, Tipo = "FACTURA", Secuencial = wrapper.Detalle.Factura.Secuencial, Canal = ConfigurationManager.AppSettings["segmento"] };
                        modelLog.Add(log);
                        FillResultadoDocumentos(new tfacturas { factura = "No se generó", CodigoError = "400", ClaveAcceso = "No se generó" }, "Servicios caídos, consulte con su proveedor.");
                    }



                }
                else
                {
                    estado = "OK";
                    tRespuesta = new Respuesta { mensaje = "El secuencial ya existe en la factura: " + numDoc, codigoRetorno = "200", estado = estado, numeroDocumento = numDoc };

                    log = new AtisLogTran { TipoSolicitud = 1, NumeroDocumento = numDoc, TramaEntrada = trama, FechaEntrada = dinit, Estado = estado, TramaRespuesta = JsonConvert.SerializeObject(tRespuesta), FechaSalida = dfinal, Tipo = "FACTURA", Secuencial = wrapper.Detalle.Factura.Secuencial, Canal = ConfigurationManager.AppSettings["segmento"] };
                    modelLog.Add(log);
                    FillResultadoDocumentos(new tfacturas { factura = "No se generó", CodigoError = "200", ClaveAcceso = "No se generó" }, "El secuencial ya existe en la factura: " + numDoc );
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
