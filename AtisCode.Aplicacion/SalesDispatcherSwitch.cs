using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Configuration;
using AtisCode.Aplicacion.Model.db_Safi;
using AtisCode.Aplicacion.Model.db_Aplicont;
using System.Threading;
using AtisCode.Aplicacion.NotasCredito;
using NLog;

namespace AtisCode.Aplicacion
{
    public class SalesDispatcherSwitch
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        public static string ClaveAccesoDocumento = "";
        public static IRestResponse InsertSalesRequest(string sales, string apiBaseUri, string apiCostumerUri)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    var ruc = ConfigurationManager.AppSettings["ruc"];
                    var client1 = new RestClient(apiBaseUri);
                    var request = new RestRequest(Method.POST);
                    request.AddHeader("postman-token", "9df9f6d8-8101-8431-60c4-92e11b8f8e0a");
                    request.AddHeader("cache-control", "no-cache");
                    request.AddHeader("content-type", "application/x-www-form-urlencoded");
                    request.AddParameter("application/x-www-form-urlencoded", "username=super&password=VFQnJ%2BdodRM%3D&ruc=" + ruc + "&year=2020&grant_type=password", ParameterType.RequestBody);
                    IRestResponse response = client1.Execute(request);
                    var token = Tools.GetJson(response.Content);

                    client1 = new RestClient(apiCostumerUri);
                    request = new RestRequest(Method.POST);
                    var autho = "bearer " + token;
                    request.AddHeader("authorization", autho);
                    request.AddHeader("content-type", "application/json");
                    request.AddParameter("application/json", sales, ParameterType.RequestBody);

                    response = client1.Execute(request);
                    return response;
                }
                catch (Exception ex)
                {
                    logger.Info("Llamada Web Service.");
                    logger.Error(ex, "Stopped program because of exception");
                    //var texto = ex.Message;
                }
                return null;
            }
        }
        public static string InsertNcSalesRequest(string sales, string apiBaseUri, string apiCostumerUri)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    var ruc = ConfigurationManager.AppSettings["ruc"];
                    var client1 = new RestClient(apiBaseUri);
                    var request = new RestRequest(Method.POST);
                    request.AddHeader("postman-token", "9df9f6d8-8101-8431-60c4-92e11b8f8e0a");
                    request.AddHeader("cache-control", "no-cache");
                    request.AddHeader("content-type", "application/x-www-form-urlencoded");
                    request.AddParameter("application/x-www-form-urlencoded", "username=super&password=VFQnJ%2BdodRM%3D&ruc=" + ruc + "&year=2020&grant_type=password", ParameterType.RequestBody);
                    IRestResponse response = client1.Execute(request);
                    var token = Tools.GetJson(response.Content);

                    client1 = new RestClient(apiCostumerUri);
                    request = new RestRequest(Method.POST);
                    var autho = "bearer " + token;
                    request.AddHeader("authorization", autho);
                    request.AddHeader("content-type", "application/json");
                    request.AddParameter("application/json", sales, ParameterType.RequestBody);

                    response = client1.Execute(request);
                    return response.Content;
                }
                catch (Exception ex)
                {
                    logger.Info("Llamada Web Service.");
                    logger.Error(ex, "Stopped program because of exception");
                    //var texto = ex.Message;
                }
                return null;
            }
        }
        public static bool ExisteFacturaAplicont(string factura)
        {
            var res = false;
            try
            {
                AppEntities _ctx = new AppEntities();
                var q = "select * from AtisLogTran where Tipo = 'FACTURA' and TramaRespuesta like '%" + factura + "%'";
                var result = _ctx.Database.SqlQuery<AtisLogTranApp>(q);
                res = result.Any();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Stopped program because of exception");
            }
            return res;
        }
        public static AtisLogTranApp GetFacturaAplicont(string factura)
        {
            var res = new AtisLogTranApp();
            try
            {
                AppEntities _ctx = new AppEntities();

                var q = "select * from AtisLogTran where Tipo = 'FACTURA' and TramaRespuesta like '%" + factura + "%'";
                var result = _ctx.Database.SqlQuery<AtisLogTranApp>(q).ToList();
                if (result.Any())
                    res = result[0];
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Stopped program because of exception");
            }

            return res;
        }

        public static string GetNuevaCotizacion(Wrapper wrapper, string codFactura)
        {
            var res = "";
            try
            {
                var fechaEmision = DateTime.Now;
                var iva = ConfigurationManager.AppSettings["iva"] != null ? decimal.Parse(ConfigurationManager.AppSettings["iva"]) : 0;
                if (wrapper?.Detalle?.Factura?.DetalleFactura != null)
                {
                    // Tipos de documentos de identificacion
                    var identComp = "05";
                    if (wrapper.Cabecera.Cliente.Identificacion.Length == 10)
                        identComp = "05";
                    else if (wrapper.Cabecera.Cliente.Identificacion.Length == 13)
                        identComp = "04";
                    else identComp = "06";

                    if (wrapper.Cabecera.Cliente.Identificacion == "9999999999999")
                        identComp = "07";

                    var infoAd = new Dictionary<string, string> {
                    { "email",wrapper.Cabecera.Cliente.Mail},
                 { "Direccion",!string.IsNullOrEmpty(wrapper.Cabecera.Cliente.Direccion)?wrapper.Cabecera.Cliente.Direccion:ConfigurationManager.AppSettings["dirMatriz"].ToString()},
                  { "Telefono",!string.IsNullOrEmpty(wrapper.Cabecera.Cliente.Telefono)?wrapper.Cabecera.Cliente.Telefono:ConfigurationManager.AppSettings["Telefono"].ToString()},
                  { "COMENTARIO1","Comentario 1"},//Campos específicos para cotización
                    { "Vendedor", ConfigurationManager.AppSettings["VendedorSeccion"].ToString()},
                    { "Clave",wrapper.Cabecera.Cliente.Identificacion},
                    { "UGE",ConfigurationManager.AppSettings["UGE"]},


                    { "Vencimiento",wrapper.Cabecera.CotizacionDetalle.Vencimiento/*"23/05/2019"*/},//Campos específicos para cotización
                    { "Plazo_Dias",wrapper.Cabecera.CotizacionDetalle.PlazoDias/*"0"*/},//Campos específicos para cotización
                    { "NoOp","VENTAS"},//Campos nuevo WS actualizacion
                    { "fhcoment",wrapper.Cabecera.CotizacionDetalle.FhComent/*"fh coment prueba"*/},//Campos específicos para cotización
                    { "fhcoment1",wrapper.Cabecera.CotizacionDetalle.FhComent1/*"fh coment1 prueba"*/},//Campos específicos para cotización
                    { "fhcoment2",wrapper.Cabecera.CotizacionDetalle.FhComent2/*"fh coment2 prueba"*/},//Campos específicos para cotización
                };

                    //Comentario1 = worksheet.GetValue(row, 11).ToString(),
                    //                FhComent = worksheet.GetValue(row, 12).ToString(),
                    //                FhComent1 = worksheet.GetValue(row, 13).ToString(),
                    //                FhComent2 = worksheet.GetValue(row, 14).ToString(),
                    //                Bodega = worksheet.GetValue(row, 15).ToString(),

                    var vIvaFact = ((wrapper.Detalle.Factura.SubTotal - wrapper.Detalle.Factura.Descuento) * iva / 100);
                    var baseImponible = wrapper.Detalle.Factura.SubTotal - wrapper.Detalle.Factura.Descuento;

                    var listTotalImpuesto = new List<totalImpuesto>();
                    var totalImp = new totalImpuesto
                    {
                        codigo = "2",
                        codigoPorcentaje = GetCodigoImpuesto(iva).ToString(),
                        baseImponible = baseImponible.ToString("N2").Replace(".", "").Replace(",", "."),//wrapper.Detalle.Factura.SubTotal.ToString("N2").Replace(".", "").Replace(",", "."),
                        valor = vIvaFact.ToString("N2").Replace(".", "").Replace(",", ".")
                    };
                    listTotalImpuesto.Add(totalImp);

                    var listPagos = new List<pago>();
                    var tpago = new pago
                    {
                        formaPago = "20",//ConfigurationManager.AppSettings["FormaPago"].ToString(), //"19",
                        total = wrapper.Detalle.Factura.Total.ToString("N2").Replace(".", "").Replace(",", "."),
                        plazo = "0", // Parametrizar
                        unidadTiempo = "dias" // Parametrizar
                    };
                    listPagos.Add(tpago);
                    var listDetalles = new List<detalle>();
                    foreach (var ele in wrapper.Detalle.Factura.DetalleFactura)
                    {
                        var detAdd = new List<detAdicional>
                    {
                        new detAdicional{nombre = "Bodega",valor=wrapper.Cabecera.CotizacionDetalle.Bodega/*ConfigurationManager.AppSettings["Bodega"].ToString()*/},
                        new detAdicional{nombre = "COMENTARIO1",valor=wrapper.Cabecera.CotizacionDetalle.Comentario1},




                        new detAdicional{nombre = "COMENTARIO2",valor=ele.CodigoProducto+","+wrapper.Cabecera.Cliente.Segmento}
                    };
                        var codigoImp = GetCodigoImpuesto(iva);
                        var vIva = ((ele.CostoUnitario - ele.Descuento) * iva / 100);
                        var detImp = new List<impuesto> {
                        new impuesto
                        {
                            codigo="2",
                            codigoPorcentaje=GetCodigoImpuesto(iva ).ToString(),
                            tarifa=iva.ToString("N2").Replace(".", "").Replace(",", "."),
                            baseImponible=ele.Total.ToString("N2").Replace(".", "").Replace(",", "."),//ele.SubTotal.ToString("N2").Replace(".", "").Replace(",", "."),
                            valor=(vIva).ToString("N2").Replace(".", "").Replace(",", ".")
                        }
                    };
                        var punit = ele.CostoUnitario;// / (1 + iva / 100);
                        var det = new detalle
                        {
                            codigoPrincipal = ele.CodigoProducto,//ConfigurationManager.AppSettings["codProducto"],
                            codigoAuxiliar = ele.CodigoProducto,//ConfigurationManager.AppSettings["codProducto"],
                            descripcion = ConfigurationManager.AppSettings["nombProducto"],
                            cantidad = ele.Cantidad.ToString(),
                            precioUnitario = punit.ToString("N4").Replace(".", "").Replace(",", "."),
                            descuento = ele.Descuento.ToString("N2").Replace(".", "").Replace(",", "."),//"0.00",
                            precioTotalSinImpuesto = (ele.SubTotal).ToString("N2").Replace(".", "").Replace(",", "."),
                            detallesAdicionales = detAdd,
                            impuestos = detImp
                        };
                        listDetalles.Add(det);
                    }
                    var factura = new factura
                    {
                        id = "comprobante",
                        version = "1.1.0",
                        infoTributaria = new infoTributaria
                        {
                            ambiente = ConfigurationManager.AppSettings["ambiente"],
                            tipoEmision = "1", // Revisar valor parametrizacion
                            razonSocial = ConfigurationManager.AppSettings["razonSocial"],
                            nombreComercial = ConfigurationManager.AppSettings["razonSocial"],
                            ruc = ConfigurationManager.AppSettings["ruc"],
                            claveAcceso = GetClaveAcceso(fechaEmision.Day.ToString().PadLeft(2, '0'), fechaEmision.Month.ToString().PadLeft(2, '0'), fechaEmision.Year.ToString().PadLeft(4, '0'), "01", ConfigurationManager.AppSettings["ruc"], ConfigurationManager.AppSettings["ambiente"], codFactura.Substring(0, 6), codFactura.Substring(6).PadLeft(9, '0'), "12345678", "1"),
                            codDoc = "01",
                            estab = ConfigurationManager.AppSettings["estab"],
                            ptoEmi = "007",//ConfigurationManager.AppSettings["ptoEmi"],
                            secuencial = codFactura.Substring(6).PadLeft(9, '0'),
                            dirMatriz = ConfigurationManager.AppSettings["dirMatriz"],
                        },
                        infoFactura = new infoFactura
                        {
                            fechaEmision = fechaEmision.Day.ToString().PadLeft(2, '0') + "/" + fechaEmision.Month.ToString().PadLeft(2, '0') + "/" + fechaEmision.Year.ToString().PadLeft(4, '0'),
                            obligadoContabilidad = "SI", // Verificar para parametrizacion
                            tipoIdentificacionComprador = identComp,
                            razonSocialComprador = wrapper.Cabecera.Cliente.NombreCliente,
                            identificacionComprador = wrapper.Cabecera.Cliente.Identificacion,
                            direccionComprador = wrapper.Cabecera.Cliente.Direccion,
                            totalSinImpuestos = (wrapper.Detalle.Factura.SubTotal - wrapper.Detalle.Factura.Descuento).ToString("N2").Replace(".", "").Replace(",", "."), // TotalBruto - Descuento // wrapper.Detalle.Factura.SubTotal.ToString("N2").Replace(".", "").Replace(",", "."), // SUBTOTAL
                            totalDescuento = wrapper.Detalle.Factura.Descuento.ToString("N2").Replace(".", "").Replace(",", "."), //"0.00",
                            propina = "0.00",
                            importeTotal = wrapper.Detalle.Factura.Total.ToString("N2").Replace(".", "").Replace(",", "."),
                            moneda = "Dolar",
                            totalConImpuestos = listTotalImpuesto,
                            pagos = listPagos
                        },
                        detalles = listDetalles,
                        infoAdicional = infoAd
                    };
                    res = GetXmlSnatShot(factura);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Stopped program because of exception");
            }
            return res;

        }


        public async Task<string> GetCotizationAsync(Wrapper wrapper, string codFactura)
        {
            return await Task.Run(() =>
            {
                var res = string.Empty;
                try
                {
                    var fechaEmision = DateTime.Now;
                    var iva = ConfigurationManager.AppSettings["iva"] != null ? decimal.Parse(ConfigurationManager.AppSettings["iva"]) : 0;
                    if (wrapper?.Detalle?.Factura?.DetalleFactura != null)
                    {
                        // Tipos de documentos de identificacion
                        var identComp = "05";
                        if (wrapper.Cabecera.Cliente.Identificacion.Length == 10)
                            identComp = "05";
                        else if (wrapper.Cabecera.Cliente.Identificacion.Length == 13)
                            identComp = "04";
                        else identComp = "06";

                        if (wrapper.Cabecera.Cliente.Identificacion == "9999999999999")
                            identComp = "07";

                        var infoAd = new Dictionary<string, string> {
                    { "email",wrapper.Cabecera.Cliente.Mail},
                 { "Direccion",!string.IsNullOrEmpty(wrapper.Cabecera.Cliente.Direccion)?wrapper.Cabecera.Cliente.Direccion:ConfigurationManager.AppSettings["dirMatriz"].ToString()},
                  { "Telefono",!string.IsNullOrEmpty(wrapper.Cabecera.Cliente.Telefono)?wrapper.Cabecera.Cliente.Telefono:ConfigurationManager.AppSettings["Telefono"].ToString()},
                  { "COMENTARIO1","Comentario 1"},//Campos específicos para cotización
                    { "Vendedor", ConfigurationManager.AppSettings["VendedorSeccion"].ToString()},
                    { "Clave",wrapper.Cabecera.Cliente.Identificacion},
                    { "UGE",ConfigurationManager.AppSettings["UGE"]},


                    { "Vencimiento",wrapper.Cabecera.CotizacionDetalle.Vencimiento/*"23/05/2019"*/},//Campos específicos para cotización
                    { "Plazo_Dias",wrapper.Cabecera.CotizacionDetalle.PlazoDias/*"0"*/},//Campos específicos para cotización
                    { "NoOp","VENTAS"},//Campos nuevo WS actualizacion
                    { "fhcoment",wrapper.Cabecera.CotizacionDetalle.FhComent/*"fh coment prueba"*/},//Campos específicos para cotización
                    { "fhcoment1",wrapper.Cabecera.CotizacionDetalle.FhComent1/*"fh coment1 prueba"*/},//Campos específicos para cotización
                    { "fhcoment2",wrapper.Cabecera.CotizacionDetalle.FhComent2/*"fh coment2 prueba"*/},//Campos específicos para cotización
                };

                        //Comentario1 = worksheet.GetValue(row, 11).ToString(),
                        //                FhComent = worksheet.GetValue(row, 12).ToString(),
                        //                FhComent1 = worksheet.GetValue(row, 13).ToString(),
                        //                FhComent2 = worksheet.GetValue(row, 14).ToString(),
                        //                Bodega = worksheet.GetValue(row, 15).ToString(),

                        var vIvaFact = ((wrapper.Detalle.Factura.SubTotal - wrapper.Detalle.Factura.Descuento) * iva / 100);
                        var baseImponible = wrapper.Detalle.Factura.SubTotal - wrapper.Detalle.Factura.Descuento;

                        var listTotalImpuesto = new List<totalImpuesto>();
                        var totalImp = new totalImpuesto
                        {
                            codigo = "2",
                            codigoPorcentaje = GetCodigoImpuesto(iva).ToString(),
                            baseImponible = baseImponible.ToString("N2").Replace(".", "").Replace(",", "."),//wrapper.Detalle.Factura.SubTotal.ToString("N2").Replace(".", "").Replace(",", "."),
                            valor = vIvaFact.ToString("N2").Replace(".", "").Replace(",", ".")
                        };
                        listTotalImpuesto.Add(totalImp);

                        //FORMAS DE PAGO
                        var listPagos = new List<pago>();
                        foreach (var item in wrapper.Detalle.Factura.DetalleFactura)
                        {
                            if (string.IsNullOrEmpty(item.FormaPago))
                            {
                                var tpago = new pago
                                {
                                    formaPago = ConfigurationManager.AppSettings["FormaPago"].ToString(),
                                    total = wrapper.Detalle.Factura.Total.ToString("N2").Replace(".", "").Replace(",", "."),
                                    plazo = "30",
                                    unidadTiempo = "dias"
                                };
                                listPagos.Add(tpago);
                                break;
                            }
                            else
                            {
                                var tpago = new pago
                                {
                                    formaPago = item.FormaPago,
                                    total = item.Valor.ToString("N2").Replace(".", "").Replace(",", "."),
                                    plazo = "30",
                                    unidadTiempo = "dias"
                                };
                                listPagos.Add(tpago);
                            }
                        }

                        var listDetalles = new List<detalle>();
                        foreach (var ele in wrapper.Detalle.Factura.DetalleFactura)
                        {
                            var detAdd = new List<detAdicional>
                    {
                        new detAdicional{nombre = "Bodega",valor=wrapper.Cabecera.CotizacionDetalle.Bodega/*ConfigurationManager.AppSettings["Bodega"].ToString()*/},
                        new detAdicional{nombre = "COMENTARIO1",valor=wrapper.Cabecera.CotizacionDetalle.Comentario1},




                        new detAdicional{nombre = "COMENTARIO2",valor=ele.CodigoProducto+","+wrapper.Cabecera.Cliente.Segmento}
                    };
                            var codigoImp = GetCodigoImpuesto(iva);
                            var vIva = ((ele.CostoUnitario - ele.Descuento) * iva / 100);
                            var detImp = new List<impuesto> {
                        new impuesto
                        {
                            codigo="2",
                            codigoPorcentaje=GetCodigoImpuesto(iva ).ToString(),
                            tarifa=iva.ToString("N2").Replace(".", "").Replace(",", "."),
                            baseImponible=ele.Total.ToString("N2").Replace(".", "").Replace(",", "."),//ele.SubTotal.ToString("N2").Replace(".", "").Replace(",", "."),
                            valor=(vIva).ToString("N2").Replace(".", "").Replace(",", ".")
                        }
                    };
                            var punit = ele.CostoUnitario;// / (1 + iva / 100);
                            var det = new detalle
                            {
                                codigoPrincipal = ele.CodigoProducto,//ConfigurationManager.AppSettings["codProducto"],
                                codigoAuxiliar = ele.CodigoProducto,//ConfigurationManager.AppSettings["codProducto"],
                                descripcion = ConfigurationManager.AppSettings["nombProducto"],
                                cantidad = ele.Cantidad.ToString(),
                                precioUnitario = punit.ToString("N4").Replace(".", "").Replace(",", "."),
                                descuento = ele.Descuento.ToString("N2").Replace(".", "").Replace(",", "."),//"0.00",
                                precioTotalSinImpuesto = (ele.SubTotal).ToString("N2").Replace(".", "").Replace(",", "."),
                                detallesAdicionales = detAdd,
                                impuestos = detImp
                            };
                            listDetalles.Add(det);
                        }
                        var factura = new factura
                        {
                            id = "comprobante",
                            version = "1.1.0",
                            infoTributaria = new infoTributaria
                            {
                                ambiente = ConfigurationManager.AppSettings["ambiente"],
                                tipoEmision = "1", // Revisar valor parametrizacion
                                razonSocial = ConfigurationManager.AppSettings["razonSocial"],
                                nombreComercial = ConfigurationManager.AppSettings["razonSocial"],
                                ruc = ConfigurationManager.AppSettings["ruc"],
                                claveAcceso = GetClaveAcceso(fechaEmision.Day.ToString().PadLeft(2, '0'), fechaEmision.Month.ToString().PadLeft(2, '0'), fechaEmision.Year.ToString().PadLeft(4, '0'), "01", ConfigurationManager.AppSettings["ruc"], ConfigurationManager.AppSettings["ambiente"], codFactura.Substring(0, 6), codFactura.Substring(6).PadLeft(9, '0'), "12345678", "1"),
                                codDoc = "01",
                                estab = ConfigurationManager.AppSettings["estab"],
                                ptoEmi = "007",//ConfigurationManager.AppSettings["ptoEmi"],
                                secuencial = codFactura.Substring(6).PadLeft(9, '0'),
                                dirMatriz = ConfigurationManager.AppSettings["dirMatriz"],
                            },
                            infoFactura = new infoFactura
                            {
                                fechaEmision = fechaEmision.Day.ToString().PadLeft(2, '0') + "/" + fechaEmision.Month.ToString().PadLeft(2, '0') + "/" + fechaEmision.Year.ToString().PadLeft(4, '0'),
                                obligadoContabilidad = "SI", // Verificar para parametrizacion
                                tipoIdentificacionComprador = identComp,
                                razonSocialComprador = wrapper.Cabecera.Cliente.NombreCliente,
                                identificacionComprador = wrapper.Cabecera.Cliente.Identificacion,
                                direccionComprador = wrapper.Cabecera.Cliente.Direccion,
                                totalSinImpuestos = (wrapper.Detalle.Factura.SubTotal - wrapper.Detalle.Factura.Descuento).ToString("N2").Replace(".", "").Replace(",", "."), // TotalBruto - Descuento // wrapper.Detalle.Factura.SubTotal.ToString("N2").Replace(".", "").Replace(",", "."), // SUBTOTAL
                                totalDescuento = wrapper.Detalle.Factura.Descuento.ToString("N2").Replace(".", "").Replace(",", "."), //"0.00",
                                propina = "0.00",
                                importeTotal = wrapper.Detalle.Factura.Total.ToString("N2").Replace(".", "").Replace(",", "."),
                                moneda = "Dolar",
                                totalConImpuestos = listTotalImpuesto,
                                pagos = listPagos
                            },
                            detalles = listDetalles,
                            infoAdicional = infoAd
                        };
                        res = GetXmlSnatShot(factura);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Stopped program because of exception");
                }
                return res;
            });
        }

        #region Para carga factura detalles multiples
        public static string GetNewSalesCargaExcelDetallesMultiples(Wrapper wrapper, string codFactura)
        {
            var res = "";
            try
            {
                var fechaEmision = DateTime.Now;
                var iva = ConfigurationManager.AppSettings["iva"] != null ? decimal.Parse(ConfigurationManager.AppSettings["iva"]) : 0;
                if (wrapper?.Detalle?.Factura?.DetalleFactura != null)
                {
                    // Tipos de documentos de identificacion
                    var identComp = "05";
                    if (wrapper.Cabecera.Cliente.Identificacion.Length == 10)
                        identComp = "05";
                    else if (wrapper.Cabecera.Cliente.Identificacion.Length == 13)
                        identComp = "04";
                    else identComp = "06";

                    if (wrapper.Cabecera.Cliente.Identificacion == "9999999999999")
                        identComp = "07";

                    var infoAd = new Dictionary<string, string> {
                    { "email",wrapper.Cabecera.Cliente.Mail},
                 { "Direccion",!string.IsNullOrEmpty(wrapper.Cabecera.Cliente.Direccion)?wrapper.Cabecera.Cliente.Direccion:ConfigurationManager.AppSettings["dirMatriz"].ToString()},
                  { "Telefono",!string.IsNullOrEmpty(wrapper.Cabecera.Cliente.Telefono)?wrapper.Cabecera.Cliente.Telefono:ConfigurationManager.AppSettings["Telefono"].ToString()},
                    { "Vendedor", ConfigurationManager.AppSettings["VendedorSeccion"].ToString()},
                    { "Clave",wrapper.Cabecera.Cliente.Identificacion},
                    { "UGE",ConfigurationManager.AppSettings["UGE"]},
                    //Nuevo --> <campoAdicional nombre='NoOp'>VENTAS</campoAdicional>
                    { "NoOp","VENTAS"},
                };

                    var vIvaFact = ((wrapper.Detalle.Factura.SubTotal/* - wrapper.Detalle.Factura.Descuento*/) * iva / 100); // El subtotal ya tiene restado el descuento
                    var baseImponible = wrapper.Detalle.Factura.SubTotal;// - wrapper.Detalle.Factura.Descuento;  --> El subtotal ya tiene restado el descuento

                    var listTotalImpuesto = new List<totalImpuesto>();
                    var totalImp = new totalImpuesto
                    {
                        codigo = "2",
                        codigoPorcentaje = GetCodigoImpuesto(iva).ToString(),
                        baseImponible = baseImponible.ToString("N2").Replace(".", "").Replace(",", "."),//wrapper.Detalle.Factura.SubTotal.ToString("N2").Replace(".", "").Replace(",", "."),
                        valor = vIvaFact.ToString("N2").Replace(".", "").Replace(",", ".")
                    };
                    listTotalImpuesto.Add(totalImp);

                    var listPagos = new List<pago>();
                    var tpago = new pago
                    {
                        formaPago = ConfigurationManager.AppSettings["FormaPago"].ToString(), //"19",
                        total = wrapper.Detalle.Factura.Total.ToString("N2").Replace(".", "").Replace(",", "."),
                        plazo = "0", // Parametrizar
                        unidadTiempo = "dias" // Parametrizar
                    };
                    listPagos.Add(tpago);
                    var listDetalles = new List<detalle>();
                    foreach (var ele in wrapper.Detalle.Factura.DetalleFactura)
                    {
                        var detAdd = new List<detAdicional>
                    {
                        new detAdicional{nombre = "Bodega",valor=ConfigurationManager.AppSettings["Bodega"].ToString()},
                        new detAdicional{nombre = "COMENTARIO1",valor=ele.Detalle},




                        new detAdicional{nombre = "COMENTARIO2",valor=ele.CodigoProducto+","+wrapper.Cabecera.Cliente.Segmento}
                    };
                        var codigoImp = GetCodigoImpuesto(iva);
                        var vIva = ((ele.CostoUnitario - ele.Descuento) * iva / 100);
                        var detImp = new List<impuesto> {
                        new impuesto
                        {
                            codigo="2",
                            codigoPorcentaje=GetCodigoImpuesto(iva ).ToString(),
                            tarifa=iva.ToString("N2").Replace(".", "").Replace(",", "."),
                            baseImponible=ele.SubTotal.ToString("N2").Replace(".", "").Replace(",", "."),//ele.SubTotal.ToString("N2").Replace(".", "").Replace(",", "."),//ele.Total.ToString("N2").Replace(".", "").Replace(",", "."),//ele.SubTotal.ToString("N2").Replace(".", "").Replace(",", "."),
                            valor=(vIva).ToString("N2").Replace(".", "").Replace(",", ".")
                        }
                    };
                        var punit = ele.CostoUnitario;// / (1 + iva / 100);
                        var det = new detalle
                        {
                            codigoPrincipal = ConfigurationManager.AppSettings["codProducto"],
                            codigoAuxiliar = ConfigurationManager.AppSettings["codProducto"],
                            descripcion = ConfigurationManager.AppSettings["nombProducto"],
                            cantidad = ele.Cantidad.ToString(),
                            precioUnitario = punit.ToString("N4").Replace(".", "").Replace(",", "."), // Verificar N4
                            descuento = ele.Descuento.ToString("N2").Replace(".", "").Replace(",", "."),//"0.00",
                            precioTotalSinImpuesto = (ele.SubTotal).ToString("N2").Replace(".", "").Replace(",", "."),
                            detallesAdicionales = detAdd,
                            impuestos = detImp
                        };
                        listDetalles.Add(det);
                    }
                    var factura = new factura
                    {
                        id = "comprobante",
                        version = "1.1.0",
                        infoTributaria = new infoTributaria
                        {
                            ambiente = ConfigurationManager.AppSettings["ambiente"],
                            tipoEmision = "1", // Revisar valor parametrizacion
                            razonSocial = ConfigurationManager.AppSettings["razonSocial"],
                            nombreComercial = ConfigurationManager.AppSettings["razonSocial"],
                            ruc = ConfigurationManager.AppSettings["ruc"],
                            claveAcceso = GetClaveAcceso(fechaEmision.Day.ToString().PadLeft(2, '0'), fechaEmision.Month.ToString().PadLeft(2, '0'), fechaEmision.Year.ToString().PadLeft(4, '0'), "01", ConfigurationManager.AppSettings["ruc"], ConfigurationManager.AppSettings["ambiente"], codFactura.Substring(0, 6), codFactura.Substring(6).PadLeft(9, '0'), "12345678", "1"),
                            codDoc = "01",
                            estab = ConfigurationManager.AppSettings["estab"],
                            ptoEmi = ConfigurationManager.AppSettings["ptoEmi"],
                            secuencial = codFactura.Substring(6).PadLeft(9, '0'),
                            dirMatriz = ConfigurationManager.AppSettings["dirMatriz"],
                        },
                        infoFactura = new infoFactura
                        {
                            fechaEmision = fechaEmision.Day.ToString().PadLeft(2, '0') + "/" + fechaEmision.Month.ToString().PadLeft(2, '0') + "/" + fechaEmision.Year.ToString().PadLeft(4, '0'),
                            obligadoContabilidad = "SI", // Verificar para parametrizacion
                            tipoIdentificacionComprador = identComp,
                            razonSocialComprador = wrapper.Cabecera.Cliente.NombreCliente,
                            identificacionComprador = wrapper.Cabecera.Cliente.Identificacion,
                            direccionComprador = wrapper.Cabecera.Cliente.Direccion,
                            totalSinImpuestos = (wrapper.Detalle.Factura.SubTotal/* - wrapper.Detalle.Factura.Descuento*/).ToString("N2").Replace(".", "").Replace(",", "."), // TotalBruto - Descuento // wrapper.Detalle.Factura.SubTotal.ToString("N2").Replace(".", "").Replace(",", "."), // SUBTOTAL
                            totalDescuento = wrapper.Detalle.Factura.Descuento.ToString("N2").Replace(".", "").Replace(",", "."), //"0.00",
                            propina = "0.00",
                            importeTotal = wrapper.Detalle.Factura.Total.ToString("N2").Replace(".", "").Replace(",", "."),
                            moneda = "Dolar",
                            totalConImpuestos = listTotalImpuesto,
                            pagos = listPagos
                        },
                        detalles = listDetalles,
                        infoAdicional = infoAd
                    };
                    ClaveAccesoDocumento = factura.infoTributaria.claveAcceso;
                    res = GetXmlSnatShot(factura);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Stopped program because of exception");
            }
            return res;
        }
        #endregion


        public static string GetNewSales(Wrapper wrapper, string codFactura)
        {
            var res = "";
            try
            {
                var fechaEmision = DateTime.Now;
                var iva = ConfigurationManager.AppSettings["iva"] != null ? decimal.Parse(ConfigurationManager.AppSettings["iva"]) : 0;
                if (wrapper?.Detalle?.Factura?.DetalleFactura != null)
                {
                    // Tipos de documentos de identificacion
                    var identComp = "05";
                    if (wrapper.Cabecera.Cliente.Identificacion.Length == 10)
                        identComp = "05";
                    else if (wrapper.Cabecera.Cliente.Identificacion.Length == 13)
                        identComp = "04";
                    else identComp = "06";

                    if (wrapper.Cabecera.Cliente.Identificacion == "9999999999999")
                        identComp = "07";

                    var infoAd = new Dictionary<string, string> {
                    { "email",wrapper.Cabecera.Cliente.Mail},
                 { "Direccion",!string.IsNullOrEmpty(wrapper.Cabecera.Cliente.Direccion)?wrapper.Cabecera.Cliente.Direccion:ConfigurationManager.AppSettings["dirMatriz"].ToString()},
                  { "Telefono",!string.IsNullOrEmpty(wrapper.Cabecera.Cliente.Telefono)?wrapper.Cabecera.Cliente.Telefono:ConfigurationManager.AppSettings["Telefono"].ToString()},
                    { "Vendedor", ConfigurationManager.AppSettings["VendedorSeccion"].ToString()},
                    { "Clave",wrapper.Cabecera.Cliente.Identificacion},
                    { "UGE",ConfigurationManager.AppSettings["UGE"]},
                    //Nuevo --> <campoAdicional nombre='NoOp'>VENTAS</campoAdicional>
                    { "NoOp","VENTAS"},
                };

                    var vIvaFact = ((wrapper.Detalle.Factura.SubTotal - wrapper.Detalle.Factura.Descuento) * iva / 100);
                    var baseImponible = wrapper.Detalle.Factura.SubTotal - wrapper.Detalle.Factura.Descuento;

                    var listTotalImpuesto = new List<totalImpuesto>();
                    var totalImp = new totalImpuesto
                    {
                        codigo = "2",
                        codigoPorcentaje = GetCodigoImpuesto(iva).ToString(),
                        baseImponible = baseImponible.ToString("N2").Replace(".", "").Replace(",", "."),//wrapper.Detalle.Factura.SubTotal.ToString("N2").Replace(".", "").Replace(",", "."),
                        valor = vIvaFact.ToString("N2").Replace(".", "").Replace(",", ".")
                    };
                    listTotalImpuesto.Add(totalImp);

                    var listPagos = new List<pago>();
                    var tpago = new pago
                    {
                        formaPago = ConfigurationManager.AppSettings["FormaPago"].ToString(), //"19",
                        total = wrapper.Detalle.Factura.Total.ToString("N2").Replace(".", "").Replace(",", "."),
                        plazo = "0", // Parametrizar
                        unidadTiempo = "dias" // Parametrizar
                    };
                    listPagos.Add(tpago);
                    var listDetalles = new List<detalle>();
                    foreach (var ele in wrapper.Detalle.Factura.DetalleFactura)
                    {
                        var detAdd = new List<detAdicional>
                    {
                        new detAdicional{nombre = "Bodega",valor=ConfigurationManager.AppSettings["Bodega"].ToString()},
                        new detAdicional{nombre = "COMENTARIO1",valor=ele.Detalle},




                        new detAdicional{nombre = "COMENTARIO2",valor=ele.CodigoProducto+","+wrapper.Cabecera.Cliente.Segmento}
                    };
                        var codigoImp = GetCodigoImpuesto(iva);
                        var vIva = ((ele.CostoUnitario - ele.Descuento) * iva / 100);
                        var detImp = new List<impuesto> {
                        new impuesto
                        {
                            codigo="2",
                            codigoPorcentaje=GetCodigoImpuesto(iva ).ToString(),
                            tarifa=iva.ToString("N2").Replace(".", "").Replace(",", "."),
                            baseImponible=ele.Total.ToString("N2").Replace(".", "").Replace(",", "."),//ele.SubTotal.ToString("N2").Replace(".", "").Replace(",", "."),
                            valor=(vIva).ToString("N2").Replace(".", "").Replace(",", ".")
                        }
                    };
                        var punit = ele.CostoUnitario;// / (1 + iva / 100);
                        var det = new detalle
                        {
                            codigoPrincipal = ConfigurationManager.AppSettings["codProducto"],
                            codigoAuxiliar = ConfigurationManager.AppSettings["codProducto"],
                            descripcion = ConfigurationManager.AppSettings["nombProducto"],
                            cantidad = ele.Cantidad.ToString(),
                            precioUnitario = punit.ToString("N2").Replace(".", "").Replace(",", "."), // Verificar N4
                            descuento = ele.Descuento.ToString("N2").Replace(".", "").Replace(",", "."),//"0.00",
                            precioTotalSinImpuesto = (ele.SubTotal).ToString("N2").Replace(".", "").Replace(",", "."),
                            detallesAdicionales = detAdd,
                            impuestos = detImp
                        };
                        listDetalles.Add(det);
                    }
                    var factura = new factura
                    {
                        id = "comprobante",
                        version = "1.1.0",
                        infoTributaria = new infoTributaria
                        {
                            ambiente = ConfigurationManager.AppSettings["ambiente"],
                            tipoEmision = "1", // Revisar valor parametrizacion
                            razonSocial = ConfigurationManager.AppSettings["razonSocial"],
                            nombreComercial = ConfigurationManager.AppSettings["razonSocial"],
                            ruc = ConfigurationManager.AppSettings["ruc"],
                            claveAcceso = GetClaveAcceso(fechaEmision.Day.ToString().PadLeft(2, '0'), fechaEmision.Month.ToString().PadLeft(2, '0'), fechaEmision.Year.ToString().PadLeft(4, '0'), "01", ConfigurationManager.AppSettings["ruc"], ConfigurationManager.AppSettings["ambiente"], codFactura.Substring(0, 6), codFactura.Substring(6).PadLeft(9, '0'), "12345678", "1"),
                            codDoc = "01",
                            estab = ConfigurationManager.AppSettings["estab"],
                            ptoEmi = ConfigurationManager.AppSettings["ptoEmi"],
                            secuencial = codFactura.Substring(6).PadLeft(9, '0'),
                            dirMatriz = ConfigurationManager.AppSettings["dirMatriz"],
                        },
                        infoFactura = new infoFactura
                        {
                            fechaEmision = fechaEmision.Day.ToString().PadLeft(2, '0') + "/" + fechaEmision.Month.ToString().PadLeft(2, '0') + "/" + fechaEmision.Year.ToString().PadLeft(4, '0'),
                            obligadoContabilidad = "SI", // Verificar para parametrizacion
                            tipoIdentificacionComprador = identComp,
                            razonSocialComprador = wrapper.Cabecera.Cliente.NombreCliente,
                            identificacionComprador = wrapper.Cabecera.Cliente.Identificacion,
                            direccionComprador = wrapper.Cabecera.Cliente.Direccion,
                            totalSinImpuestos = (wrapper.Detalle.Factura.SubTotal - wrapper.Detalle.Factura.Descuento).ToString("N2").Replace(".", "").Replace(",", "."), // TotalBruto - Descuento // wrapper.Detalle.Factura.SubTotal.ToString("N2").Replace(".", "").Replace(",", "."), // SUBTOTAL
                            totalDescuento = wrapper.Detalle.Factura.Descuento.ToString("N2").Replace(".", "").Replace(",", "."), //"0.00",
                            propina = "0.00",
                            importeTotal = wrapper.Detalle.Factura.Total.ToString("N2").Replace(".", "").Replace(",", "."),
                            moneda = "Dolar",
                            totalConImpuestos = listTotalImpuesto,
                            pagos = listPagos
                        },
                        detalles = listDetalles,
                        infoAdicional = infoAd
                    };
                    ClaveAccesoDocumento = factura.infoTributaria.claveAcceso;
                    res = GetXmlSnatShot(factura);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Stopped program because of exception");
            }
            return res;
        }


        public static string GetNewSales(string trama, string codFactura)
        {
            var res = "";
            try
            {
                var fechaEmision = DateTime.Now;
                var iva = ConfigurationManager.AppSettings["iva"] != null ? decimal.Parse(ConfigurationManager.AppSettings["iva"]) : 0;
                Wrapper wrapper = JsonConvert.DeserializeObject<Wrapper>(trama);
                if (wrapper?.Detalle?.Factura?.DetalleFactura != null)
                {
                    var identComp = "05";
                    if (wrapper.Cabecera.Cliente.Identificacion.Length == 10)
                        identComp = "05";
                    else if (wrapper.Cabecera.Cliente.Identificacion.Length == 13)
                        identComp = "04";
                    else identComp = "06";

                    if (wrapper.Cabecera.Cliente.Identificacion == "9999999999999")
                        identComp = "07";

                    var infoAd = new Dictionary<string, string> {
                    { "email",wrapper.Cabecera.Cliente.Mail},
                    { "Direccion",!string.IsNullOrEmpty(wrapper.Cabecera.Cliente.Direccion)?wrapper.Cabecera.Cliente.Direccion: ConfigurationManager.AppSettings["dirMatriz"].ToString()},
                    { "Telefono",!string.IsNullOrEmpty(wrapper.Cabecera.Cliente.Telefono)?wrapper.Cabecera.Cliente.Telefono: ConfigurationManager.AppSettings["Telefono"].ToString()},
                    { "Vendedor",ConfigurationManager.AppSettings["VendedorSeccion"].ToString()},
                    { "Clave",wrapper.Cabecera.Cliente.Identificacion},
                    { "UGE",ConfigurationManager.AppSettings.Get("UGE")},
                    // Nuevo
                    { "NoOp","VENTAS"},
                };

                    var vIvaFact = (wrapper.Detalle.Factura.SubTotal * iva / 100);
                    var listTotalImpuesto = new List<totalImpuesto>();
                    var totalImp = new totalImpuesto
                    {
                        codigo = "2",
                        codigoPorcentaje = GetCodigoImpuesto(iva).ToString(),
                        baseImponible = wrapper.Detalle.Factura.SubTotal.ToString("N2").Replace(".", "").Replace(",", "."),
                        valor = vIvaFact.ToString("N2").Replace(".", "").Replace(",", ".")
                    };
                    listTotalImpuesto.Add(totalImp);

                    var listPagos = new List<pago>();
                    var tpago = new pago
                    {
                        formaPago = ConfigurationManager.AppSettings["FormaPago"].ToString(),
                        total = wrapper.Detalle.Factura.Total.ToString("N2").Replace(".", "").Replace(",", "."),
                        plazo = "30",
                        unidadTiempo = "dias"
                    };
                    listPagos.Add(tpago);
                    var listDetalles = new List<detalle>();
                    foreach (var ele in wrapper.Detalle.Factura.DetalleFactura)
                    {
                        var detAdd = new List<detAdicional>
                    {
                        new detAdicional{nombre = "Bodega",valor= ConfigurationManager.AppSettings["Bodega"].ToString()},
                        new detAdicional{nombre = "COMENTARIO1",valor=ele.Detalle},
                        new detAdicional{nombre = "COMENTARIO2",valor=ele.CodigoProducto+","+wrapper.Cabecera.Cliente.Segmento}
                    };
                        var codigoImp = GetCodigoImpuesto(iva);
                        var vIva = (ele.SubTotal * iva / 100);
                        var detImp = new List<impuesto> {
                        new impuesto
                        {
                            codigo="2",
                            codigoPorcentaje=GetCodigoImpuesto(iva ).ToString(),
                            tarifa=iva.ToString("N2").Replace(".", "").Replace(",", "."),
                            baseImponible=ele.SubTotal.ToString("N2").Replace(".", "").Replace(",", "."),
                            valor=(vIva).ToString("N2").Replace(".", "").Replace(",", ".")
                        }
                    };
                        var punit = ele.CostoUnitario;
                        var det = new detalle
                        {
                            codigoPrincipal = ConfigurationManager.AppSettings["codProducto"],
                            codigoAuxiliar = ConfigurationManager.AppSettings["codProducto"],
                            descripcion = ConfigurationManager.AppSettings["nombProducto"],
                            cantidad = ele.Cantidad.ToString(),
                            precioUnitario = punit.ToString("N2").Replace(".", "").Replace(",", "."),
                            descuento = "0.00",
                            precioTotalSinImpuesto = ele.SubTotal.ToString("N2").Replace(".", "").Replace(",", "."), // (punit * ele.Cantidad).ToString("N2").Replace(".", "").Replace(",", "."),
                            detallesAdicionales = detAdd,
                            impuestos = detImp
                        };
                        listDetalles.Add(det);
                    }
                    var factura = new factura
                    {
                        id = "comprobante",
                        version = "1.1.0",
                        infoTributaria = new infoTributaria
                        {
                            ambiente = ConfigurationManager.AppSettings["ambiente"],
                            tipoEmision = "1",
                            razonSocial = ConfigurationManager.AppSettings["razonSocial"],
                            nombreComercial = ConfigurationManager.AppSettings["razonSocial"],
                            ruc = ConfigurationManager.AppSettings["ruc"],
                            claveAcceso = GetClaveAcceso(fechaEmision.Day.ToString().PadLeft(2, '0'), fechaEmision.Month.ToString().PadLeft(2, '0'), fechaEmision.Year.ToString().PadLeft(4, '0'), "01", ConfigurationManager.AppSettings["ruc"], ConfigurationManager.AppSettings["ambiente"], codFactura.Substring(0, 6), codFactura.Substring(6).PadLeft(9, '0'), "12345678", "1"),
                            codDoc = "01",
                            estab = ConfigurationManager.AppSettings["estab"],
                            ptoEmi = ConfigurationManager.AppSettings["ptoEmi"],
                            secuencial = codFactura.Substring(6).PadLeft(9, '0'),
                            dirMatriz = ConfigurationManager.AppSettings["dirMatriz"],
                        },
                        infoFactura = new infoFactura
                        {
                            fechaEmision = fechaEmision.Day.ToString().PadLeft(2, '0') + "/" + fechaEmision.Month.ToString().PadLeft(2, '0') + "/" + fechaEmision.Year.ToString().PadLeft(4, '0'),
                            obligadoContabilidad = "SI",
                            tipoIdentificacionComprador = identComp,
                            razonSocialComprador = wrapper.Cabecera.Cliente.NombreCliente,
                            identificacionComprador = wrapper.Cabecera.Cliente.Identificacion,
                            direccionComprador = wrapper.Cabecera.Cliente.Direccion,
                            totalSinImpuestos = wrapper.Detalle.Factura.SubTotal.ToString("N2").Replace(".", "").Replace(",", "."),
                            totalDescuento = "0.00",
                            propina = "0.00",
                            importeTotal = wrapper.Detalle.Factura.Total.ToString("N2").Replace(".", "").Replace(",", "."),
                            moneda = "Dolar",
                            totalConImpuestos = listTotalImpuesto,
                            pagos = listPagos
                        },
                        detalles = listDetalles,
                        infoAdicional = infoAd
                    };
                    res = GetXmlSnatShot(factura);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Stopped program because of exception");
            }

            return res;
        }

        public async Task<string> GetInvoiceXMLAsync(Wrapper wrapper, string codFactura)
        {
            return await Task.Run(() =>
            {
                var res = string.Empty;
                try
                {
                    var fechaEmision = DateTime.Now;
                    var iva = ConfigurationManager.AppSettings["iva"] != null ? decimal.Parse(ConfigurationManager.AppSettings["iva"]) : 0;
                    //Wrapper wrapper = JsonConvert.DeserializeObject<Wrapper>(trama);
                    if (wrapper?.Detalle?.Factura?.DetalleFactura != null)
                    {
                        var identComp = "05";
                        if (wrapper.Cabecera.Cliente.Identificacion.Length == 10)
                            identComp = "05";
                        else if (wrapper.Cabecera.Cliente.Identificacion.Length == 13)
                            identComp = "04";
                        else identComp = "06";

                        if (wrapper.Cabecera.Cliente.Identificacion == "9999999999999")
                            identComp = "07";

                        var infoAd = new Dictionary<string, string> {
                    { "email",wrapper.Cabecera.Cliente.Mail},
                    { "Direccion",!string.IsNullOrEmpty(wrapper.Cabecera.Cliente.Direccion)?wrapper.Cabecera.Cliente.Direccion: ConfigurationManager.AppSettings["dirMatriz"].ToString()},
                    { "Telefono",!string.IsNullOrEmpty(wrapper.Cabecera.Cliente.Telefono)?wrapper.Cabecera.Cliente.Telefono: ConfigurationManager.AppSettings["Telefono"].ToString()},
                    { "Vendedor",ConfigurationManager.AppSettings["VendedorSeccion"].ToString()},
                    { "Clave",wrapper.Cabecera.Cliente.Identificacion},
                    { "UGE",ConfigurationManager.AppSettings.Get("UGE")},

                    // Nuevo
                    { "NoOp","VENTAS"},
                };

                        //Agregar campos de cotizacion en caso de corresponder a una
                        if (wrapper.Cabecera.CotizacionDetalle != null)
                        {
                            infoAd.Add("Vencimiento", wrapper.Cabecera.CotizacionDetalle.Vencimiento);
                            infoAd.Add("Plazo_Dias", wrapper.Cabecera.CotizacionDetalle.PlazoDias);
                            infoAd.Add("fhcoment", wrapper.Cabecera.CotizacionDetalle.FhComent);
                            infoAd.Add("fhcoment1", wrapper.Cabecera.CotizacionDetalle.FhComent1);
                            infoAd.Add("fhcoment2", wrapper.Cabecera.CotizacionDetalle.FhComent2);
                        }

                        var vIvaFact = (wrapper.Detalle.Factura.SubTotal * iva / 100);
                        var listTotalImpuesto = new List<totalImpuesto>();
                        var totalImp = new totalImpuesto
                        {
                            codigo = "2",
                            codigoPorcentaje = GetCodigoImpuesto(iva).ToString(),
                            baseImponible = wrapper.Detalle.Factura.SubTotal.ToString("N2").Replace(".", "").Replace(",", "."),
                            valor = vIvaFact.ToString("N2").Replace(".", "").Replace(",", ".")
                        };
                        listTotalImpuesto.Add(totalImp);

                        //FORMAS DE PAGO
                        var listPagos = new List<pago>();
                        foreach (var item in wrapper.Detalle.Factura.DetalleFactura)
                        {
                            if (string.IsNullOrEmpty(item.FormaPago))
                            {
                                var tpago = new pago
                                {
                                    formaPago = ConfigurationManager.AppSettings["FormaPago"].ToString(),
                                    total = wrapper.Detalle.Factura.Total.ToString("N2").Replace(".", "").Replace(",", "."),
                                    plazo = "30",
                                    unidadTiempo = "dias"
                                };
                                listPagos.Add(tpago);
                                break;
                            }
                            else
                            {
                                var tpago = new pago
                                {
                                    formaPago = item.FormaPago,
                                    total = item.Valor.ToString("N2").Replace(".", "").Replace(",", "."),
                                    plazo = "30",
                                    unidadTiempo = "dias"
                                };
                                listPagos.Add(tpago);
                            }
                        }

                        var listDetalles = new List<detalle>();
                        foreach (var ele in wrapper.Detalle.Factura.DetalleFactura)
                        {
                            var detAdd = new List<detAdicional>
                    {
                        new detAdicional{nombre = "Bodega",valor= ConfigurationManager.AppSettings["Bodega"].ToString()},
                        new detAdicional{nombre = "COMENTARIO1",valor=ele.Detalle},
                        new detAdicional{nombre = "COMENTARIO2",valor=ele.CodigoProducto+","+wrapper.Cabecera.Cliente.Segmento}
                    };
                            var codigoImp = GetCodigoImpuesto(iva);
                            var vIva = (ele.SubTotal * iva / 100);
                            var detImp = new List<impuesto> {
                        new impuesto
                        {
                            codigo="2",
                            codigoPorcentaje=GetCodigoImpuesto(iva ).ToString(),
                            tarifa=iva.ToString("N2").Replace(".", "").Replace(",", "."),
                            baseImponible=ele.SubTotal.ToString("N2").Replace(".", "").Replace(",", "."),
                            valor=(vIva).ToString("N2").Replace(".", "").Replace(",", ".")
                        }
                    };
                            var punit = ele.CostoUnitario;
                            var det = new detalle
                            {
                                codigoPrincipal = ConfigurationManager.AppSettings["codProducto"],
                                codigoAuxiliar = ConfigurationManager.AppSettings["codProducto"],
                                descripcion = ConfigurationManager.AppSettings["nombProducto"],
                                cantidad = ele.Cantidad.ToString(),
                                precioUnitario = punit.ToString("N4").Replace(".", "").Replace(",", "."), //N4
                                descuento = "0.00",
                                precioTotalSinImpuesto = ele.SubTotal.ToString("N2").Replace(".", "").Replace(",", "."), // (punit * ele.Cantidad).ToString("N2").Replace(".", "").Replace(",", "."),
                                detallesAdicionales = detAdd,
                                impuestos = detImp
                            };
                            listDetalles.Add(det);
                        }
                        var factura = new factura
                        {
                            id = "comprobante",
                            version = "1.1.0",
                            infoTributaria = new infoTributaria
                            {
                                ambiente = ConfigurationManager.AppSettings["ambiente"],
                                tipoEmision = "1",
                                razonSocial = ConfigurationManager.AppSettings["razonSocial"],
                                nombreComercial = ConfigurationManager.AppSettings["razonSocial"],
                                ruc = ConfigurationManager.AppSettings["ruc"],
                                claveAcceso = GetClaveAcceso(fechaEmision.Day.ToString().PadLeft(2, '0'), fechaEmision.Month.ToString().PadLeft(2, '0'), fechaEmision.Year.ToString().PadLeft(4, '0'), "01", ConfigurationManager.AppSettings["ruc"], ConfigurationManager.AppSettings["ambiente"], codFactura.Substring(0, 6), codFactura.Substring(6).PadLeft(9, '0'), "12345678", "1"),
                                codDoc = "01",
                                estab = ConfigurationManager.AppSettings["estab"],
                                ptoEmi = ConfigurationManager.AppSettings["ptoEmi"],
                                secuencial = codFactura.Substring(6).PadLeft(9, '0'),
                                dirMatriz = ConfigurationManager.AppSettings["dirMatriz"],
                            },
                            infoFactura = new infoFactura
                            {
                                fechaEmision = fechaEmision.Day.ToString().PadLeft(2, '0') + "/" + fechaEmision.Month.ToString().PadLeft(2, '0') + "/" + fechaEmision.Year.ToString().PadLeft(4, '0'),
                                obligadoContabilidad = "SI",
                                tipoIdentificacionComprador = identComp,
                                razonSocialComprador = wrapper.Cabecera.Cliente.NombreCliente,
                                identificacionComprador = wrapper.Cabecera.Cliente.Identificacion,
                                direccionComprador = wrapper.Cabecera.Cliente.Direccion,
                                totalSinImpuestos = wrapper.Detalle.Factura.SubTotal.ToString("N2").Replace(".", "").Replace(",", "."),
                                totalDescuento = "0.00",
                                propina = "0.00",
                                importeTotal = wrapper.Detalle.Factura.Total.ToString("N2").Replace(".", "").Replace(",", "."),
                                moneda = "Dolar",
                                totalConImpuestos = listTotalImpuesto,
                                pagos = listPagos
                            },
                            detalles = listDetalles,
                            infoAdicional = infoAd
                        };
                        res = GetXmlSnatShot(factura);  //Embed Xml Format Json
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Stopped program because of exception");
                }
                return res;
            });
        }


        public async Task<string> GetCreditNoteXMLAsync(WraperNotaCreditoWs wrapper, string codFactura)
        {
            return await Task.Run(() =>
            {
                var res = string.Empty;
                try
                {
                    SafiEntities _ctx = new SafiEntities();
                    var fechaEmision = DateTime.Now;
                    var iva = ConfigurationManager.AppSettings["iva"] != null ? decimal.Parse(ConfigurationManager.AppSettings["iva"]) : 0;
                    //WraperNotaCreditoWs wrapper = JsonConvert.DeserializeObject<WraperNotaCreditoWs>(trama);
                    if (wrapper?.NotaCredito?.Detalle?.DetalleNotaCredito != null)
                    {
                        var cadenas = wrapper.NotaCredito.Detalle.Idfactura.Split('-');
                        var codFact = cadenas.Count() > 2 ? cadenas[2] : "000000001";
                        var noFact = wrapper.NotaCredito.Detalle.Idfactura.Replace("-", "").Trim();
                        var fact = _ctx.FACHIS.FirstOrDefault(t => t.FHFactura == noFact);
                        if (fact != null)
                        {
                            var cliente = _ctx.CXCDIR.FirstOrDefault(t => t.Ruc == fact.FHRuc);
                            if (cliente != null)
                            {
                                var identComp = "05";
                                if (cliente.Ruc.Length == 10)
                                    identComp = "05";
                                else if (cliente.Ruc.Length == 13)
                                    identComp = "04";
                                else identComp = "06";

                                if (cliente.Ruc == "9999999999999")
                                    identComp = "07";

                                var infoAd = new Dictionary<string, string> {
                        { "email", cliente.Email},
                        { "Direccion", !string.IsNullOrEmpty(cliente.Direc1)?cliente.Direc1: ConfigurationManager.AppSettings["dirMatriz"].ToString()},
                        { "Telefono", !string.IsNullOrEmpty(cliente.Telef1)?cliente.Telef1:ConfigurationManager.AppSettings["Telefono"].ToString()},
                        { "Vendedor", ConfigurationManager.AppSettings["VendedorSeccion"].ToString()},
                        { "Clave", cliente.Ruc},
                        { "UGE",ConfigurationManager.AppSettings["UGE"]},
                        { "Comentario","DESCUENTO"},
                         // Nuevo
                        { "NoOp","VENTAS"},
                        };

                                var vIvaFact = (wrapper.NotaCredito.Detalle.DetalleNotaCredito[0].SubTotal * iva / 100);
                                var listTotalImpuesto = new List<totalImpuesto>();
                                var totalImp = new totalImpuesto
                                {
                                    codigo = "2",
                                    codigoPorcentaje = GetCodigoImpuesto(iva).ToString(),
                                    baseImponible = wrapper.NotaCredito.Detalle.DetalleNotaCredito[0].SubTotal.ToString("N2").Replace(".", "").Replace(",", "."),
                                    valor = vIvaFact.ToString("N2").Replace(".", "").Replace(",", ".")
                                };
                                listTotalImpuesto.Add(totalImp);

                                var listPagos = new List<pago>();
                                var tpago = new pago
                                {
                                    formaPago = ConfigurationManager.AppSettings["FormaPago"].ToString(),//"19",
                                    total = wrapper.NotaCredito.Detalle.Valor.ToString("N2").Replace(".", "").Replace(",", "."),
                                    plazo = "0",
                                    unidadTiempo = "dias"
                                };
                                listPagos.Add(tpago);
                                var listDetalles = new List<detalleRet>();
                                foreach (var ele in wrapper.NotaCredito.Detalle.DetalleNotaCredito)
                                {
                                    var detAdd = new List<detAdicional>
                            {
                                new detAdicional{nombre = "Bodega",valor= ConfigurationManager.AppSettings["Bodega"].ToString()},
                                new detAdicional{nombre = "COMENTARIO1",valor=ele.Detalle},
                                new detAdicional{nombre = "COMENTARIO2",valor=ele.CodigoProducto }// +","+wrapper.Cabecera.Cliente.Segmento}
                            };
                                    var codigoImp = GetCodigoImpuesto(iva);
                                    var vIva = ele.SubTotal * (iva / 100);
                                    var detImp = new List<impuesto> {
                                new impuesto
                                {
                                    codigo = "2",
                                    codigoPorcentaje=GetCodigoImpuesto(iva ).ToString(),
                                    tarifa=iva.ToString().Replace(".", "").Replace(",", "."),
                                    baseImponible=ele.SubTotal.ToString("N2").Replace(".", "").Replace(",", "."),
                                    valor=vIva.ToString("N2").Replace(".", "").Replace(",", ".")
                                }
                            };
                                    var punit = ele.SubTotal;// / (1 + iva / 100);
                                    var det = new detalleRet
                                    {
                                        codigoAdicional = ConfigurationManager.AppSettings["codProducto"],
                                        codigoInterno = ConfigurationManager.AppSettings["codProducto"],
                                        descripcion = ConfigurationManager.AppSettings["nombProducto"],
                                        cantidad = ele.Cantidad.ToString(),
                                        precioUnitario = (punit * ele.Cantidad).ToString("N2").Replace(".", "").Replace(",", "."),
                                        descuento = "0.00",
                                        precioTotalSinImpuesto = ele.SubTotal.ToString("N2").Replace(".", "").Replace(",", "."),
                                        detallesAdicionales = detAdd,
                                        impuestos = detImp
                                    };
                                    listDetalles.Add(det);
                                }
                                decimal totalSinImpuesto = GetTotalSinImpuesto(wrapper.NotaCredito.Detalle.Valor, iva);
                                var feDocSustento = "";
                                var facturaMod = _ctx.FACHIS.FirstOrDefault(t => t.FHFactura == noFact);
                                if (facturaMod?.FHFechaf != null)
                                    feDocSustento = facturaMod.FHFechaf.Value.Day.ToString().PadLeft(2, '0') + "/" + facturaMod.FHFechaf.Value.Month.ToString().PadLeft(2, '0') + "/" + facturaMod.FHFechaf.Value.Year.ToString().PadLeft(4, '0');


                                var factura = new notaCredito
                                {
                                    id = "comprobante",
                                    version = "1.1.0",
                                    infoTributaria = new infoTributaria
                                    {
                                        ambiente = ConfigurationManager.AppSettings["ambiente"],
                                        tipoEmision = "1",
                                        razonSocial = ConfigurationManager.AppSettings["razonSocial"],
                                        nombreComercial = ConfigurationManager.AppSettings["razonSocial"],
                                        ruc = ConfigurationManager.AppSettings["ruc"],
                                        claveAcceso = GetClaveAcceso(fechaEmision.Day.ToString().PadLeft(2, '0'), fechaEmision.Month.ToString().PadLeft(2, '0'), fechaEmision.Year.ToString().PadLeft(4, '0'), "01", ConfigurationManager.AppSettings["ruc"], ConfigurationManager.AppSettings["ambiente"], ConfigurationManager.AppSettings["estab"] + ConfigurationManager.AppSettings["ptoEmi"], codFact, "12345678", "1"),
                                        codDoc = "04", // 04 para nota de crédito
                                        estab = ConfigurationManager.AppSettings["estab"],
                                        ptoEmi = ConfigurationManager.AppSettings["ptoEmi"],
                                        secuencial = codFactura.Substring(6).PadLeft(9, '0'),
                                        dirMatriz = ConfigurationManager.AppSettings["dirMatriz"],
                                    },
                                    infoNotaCredito = new infoNotaCredito
                                    {
                                        fechaEmision = fechaEmision.Day.ToString().PadLeft(2, '0') + "/" + fechaEmision.Month.ToString().PadLeft(2, '0') + "/" + fechaEmision.Year.ToString().PadLeft(4, '0'),
                                        obligadoContabilidad = "SI",
                                        tipoIdentificacionComprador = identComp,
                                        razonSocialComprador = cliente.Nombre,
                                        identificacionComprador = cliente.Ruc,
                                        totalSinImpuestos = totalSinImpuesto.ToString("N2").Replace(".", "").Replace(",", "."),
                                        moneda = "Dolar",
                                        totalConImpuestos = listTotalImpuesto,
                                        dirEstablecimiento = cliente.Direc1,
                                        rise = "NO",
                                        codDocModificado = "01",
                                        numDocModificado = GetNumeroDocumentoModificado(wrapper.NotaCredito.Detalle.Idfactura),//wrapper.NotaCredito.Detalle.Idfactura,

                                        //numDocModificado = "001-009-" + wrapper.NotaCredito.Detalle.Idfactura.Substring(6),

                                        fechaEmisionDocSustento = feDocSustento,
                                        motivo = "DEVOLUCION",
                                        valorModificacion = totalSinImpuesto.ToString("N2").Replace(".", "").Replace(",", "."),

                                    },
                                    detalles = listDetalles,
                                    infoAdicional = infoAd
                                };

                                res = GetXmlSnatShot(factura);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Stopped program because of exception");
                }
                return res;

            });
        }


        public static string GetNewNcSales(string trama, string codFactura)
        {
            var res = "";
            try
            {
                SafiEntities _ctx = new SafiEntities();
                var fechaEmision = DateTime.Now;
                var iva = ConfigurationManager.AppSettings["iva"] != null ? decimal.Parse(ConfigurationManager.AppSettings["iva"]) : 0;
                WrapperNotaCredito wrapper = JsonConvert.DeserializeObject<WrapperNotaCredito>(trama);
                if (wrapper?.NotaCredito?.Detalle?.DetallesNotaCredito != null)
                {
                    var cadenas = wrapper.NotaCredito.Detalle.Idfactura.Split('-');
                    var codFact = cadenas.Count() > 2 ? cadenas[2] : "000000001";
                    var noFact = wrapper.NotaCredito.Detalle.Idfactura.Replace("-", "").Trim();

                    //tomar la factura activa
                    var fact = _ctx.FACHIS.FirstOrDefault(t => t.FHFactura == noFact && t.FHEstado == "ACT");

                    if (fact != null)
                    {
                        var cliente = _ctx.CXCDIR.FirstOrDefault(t => t.Ruc == fact.FHRuc);
                        if (cliente != null)
                        {
                            var identComp = "05";
                            if (cliente.Ruc.Length == 10)
                                identComp = "05";
                            else if (cliente.Ruc.Length == 13)
                                identComp = "04";
                            else identComp = "06";

                            if (cliente.Ruc == "9999999999999")
                                identComp = "07";

                            var infoAd = new Dictionary<string, string> {
                        { "email", cliente.Email},
                        { "Direccion", !string.IsNullOrEmpty(cliente.Direc1)?cliente.Direc1: ConfigurationManager.AppSettings["dirMatriz"].ToString()},
                        { "Telefono", !string.IsNullOrEmpty(cliente.Telef1)?cliente.Telef1: ConfigurationManager.AppSettings["Telefono"].ToString()},
                        { "Vendedor",ConfigurationManager.AppSettings["VendedorSeccion"].ToString()},
                        { "Clave", cliente.Ruc},
                        { "UGE",ConfigurationManager.AppSettings["UGE"]},
                        { "Comentario","DESCUENTO"},
                         // Nuevo
                        { "NoOp","VENTAS"},
                        };

                            var vIvaFact = (wrapper.NotaCredito.Detalle.DetallesNotaCredito[0].SubTotal * iva / 100);
                            var listTotalImpuesto = new List<totalImpuesto>();
                            var totalImp = new totalImpuesto
                            {
                                codigo = "2",
                                codigoPorcentaje = GetCodigoImpuesto(iva).ToString(),
                                baseImponible = wrapper.NotaCredito.Detalle.DetallesNotaCredito[0].SubTotal.ToString("N2").Replace(".", "").Replace(",", "."),
                                valor = vIvaFact.ToString("N2").Replace(".", "").Replace(",", ".")
                            };
                            listTotalImpuesto.Add(totalImp);

                            var listPagos = new List<pago>();
                            var tpago = new pago
                            {
                                formaPago = ConfigurationManager.AppSettings["FormaPago"].ToString(),//"19",
                                total = wrapper.NotaCredito.Detalle.Valor.ToString("N2").Replace(".", "").Replace(",", "."),
                                plazo = "0",
                                unidadTiempo = "dias"
                            };
                            listPagos.Add(tpago);
                            var listDetalles = new List<detalleRet>();
                            foreach (var ele in wrapper.NotaCredito.Detalle.DetallesNotaCredito)
                            {
                                var detAdd = new List<detAdicional>
                            {
                                new detAdicional{nombre = "Bodega",valor=ConfigurationManager.AppSettings["Bodega"].ToString()},
                                new detAdicional{nombre = "COMENTARIO1",valor=ele.Detalle},
                                new detAdicional{nombre = "COMENTARIO2",valor=ele.CodigoProducto }// +","+wrapper.Cabecera.Cliente.Segmento}
                            };
                                var codigoImp = GetCodigoImpuesto(iva);
                                var vIva = ele.SubTotal * (iva / 100);
                                var detImp = new List<impuesto> {
                                new impuesto
                                {
                                    codigo="2",
                                    codigoPorcentaje=GetCodigoImpuesto(iva ).ToString(),
                                    tarifa=iva.ToString().Replace(".", "").Replace(",", "."),
                                    baseImponible=ele.SubTotal.ToString("N2").Replace(".", "").Replace(",", "."),
                                    valor=vIva.ToString("N2").Replace(".", "").Replace(",", ".")
                                }
                            };
                                var punit = ele.SubTotal;// / (1 + iva / 100);
                                var det = new detalleRet
                                {
                                    codigoAdicional = ConfigurationManager.AppSettings["codProducto"],
                                    codigoInterno = ConfigurationManager.AppSettings["codProducto"],
                                    descripcion = ConfigurationManager.AppSettings["nombProducto"],
                                    cantidad = ele.Cantidad.ToString(),
                                    precioUnitario = (punit * ele.Cantidad).ToString("N2").Replace(".", "").Replace(",", "."),
                                    descuento = "0.00",
                                    precioTotalSinImpuesto = ele.SubTotal.ToString("N2").Replace(".", "").Replace(",", "."),
                                    detallesAdicionales = detAdd,
                                    impuestos = detImp
                                };
                                listDetalles.Add(det);
                            }
                            decimal totalSinImpuesto = GetTotalSinImpuesto(wrapper.NotaCredito.Detalle.Valor, iva);
                            var feDocSustento = "";
                            var facturaMod = _ctx.FACHIS.FirstOrDefault(t => t.FHFactura == noFact);
                            if (facturaMod?.FHFechaf != null)
                                feDocSustento = facturaMod.FHFechaf.Value.Day.ToString().PadLeft(2, '0') + "/" + facturaMod.FHFechaf.Value.Month.ToString().PadLeft(2, '0') + "/" + facturaMod.FHFechaf.Value.Year.ToString().PadLeft(4, '0');


                            var factura = new notaCredito
                            {
                                id = "comprobante",
                                version = "1.1.0",
                                infoTributaria = new infoTributaria
                                {
                                    ambiente = ConfigurationManager.AppSettings["ambiente"],
                                    tipoEmision = "1",
                                    razonSocial = ConfigurationManager.AppSettings["razonSocial"],
                                    nombreComercial = ConfigurationManager.AppSettings["razonSocial"],
                                    ruc = ConfigurationManager.AppSettings["ruc"],
                                    claveAcceso = GetClaveAcceso(fechaEmision.Day.ToString().PadLeft(2, '0'), fechaEmision.Month.ToString().PadLeft(2, '0'), fechaEmision.Year.ToString().PadLeft(4, '0'), "01", ConfigurationManager.AppSettings["ruc"], ConfigurationManager.AppSettings["ambiente"], ConfigurationManager.AppSettings["estab"] + ConfigurationManager.AppSettings["ptoEmi"], codFact, "12345678", "1"),
                                    codDoc = "04", // 04 para nota de crédito
                                    estab = ConfigurationManager.AppSettings["estab"],
                                    ptoEmi = ConfigurationManager.AppSettings["ptoEmi"],
                                    secuencial = codFactura.Substring(6).PadLeft(9, '0'),
                                    dirMatriz = ConfigurationManager.AppSettings["dirMatriz"],
                                },
                                infoNotaCredito = new infoNotaCredito
                                {
                                    fechaEmision = fechaEmision.Day.ToString().PadLeft(2, '0') + "/" + fechaEmision.Month.ToString().PadLeft(2, '0') + "/" + fechaEmision.Year.ToString().PadLeft(4, '0'),
                                    obligadoContabilidad = "SI",
                                    tipoIdentificacionComprador = identComp,
                                    razonSocialComprador = cliente.Nombre,
                                    identificacionComprador = cliente.Ruc,
                                    totalSinImpuestos = totalSinImpuesto.ToString("N2").Replace(".", "").Replace(",", "."),
                                    moneda = "Dolar",
                                    totalConImpuestos = listTotalImpuesto,
                                    dirEstablecimiento = cliente.Direc1,
                                    rise = "NO",
                                    codDocModificado = "01",
                                    numDocModificado = GetNumeroDocumentoModificado(wrapper.NotaCredito.Detalle.Idfactura),//wrapper.NotaCredito.Detalle.Idfactura,

                                    //numDocModificado = "001-009-" + wrapper.NotaCredito.Detalle.Idfactura.Substring(6),

                                    fechaEmisionDocSustento = feDocSustento,
                                    motivo = "DEVOLUCION",
                                    valorModificacion = totalSinImpuesto.ToString("N2").Replace(".", "").Replace(",", "."),

                                },
                                detalles = listDetalles,
                                infoAdicional = infoAd
                            };
                            res = GetXmlSnatShot(factura);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Stopped program because of exception");
            }

            return res;
        }
        public static string GetNewNcSalesWs(string trama, string codFactura)
        {
            var res = "";

            try
            {
                SafiEntities _ctx = new SafiEntities();
                var fechaEmision = DateTime.Now;
                var iva = ConfigurationManager.AppSettings["iva"] != null ? decimal.Parse(ConfigurationManager.AppSettings["iva"]) : 0;
                WraperNotaCreditoWs wrapper = JsonConvert.DeserializeObject<WraperNotaCreditoWs>(trama);
                if (wrapper?.NotaCredito?.Detalle?.DetalleNotaCredito != null)
                {
                    var cadenas = wrapper.NotaCredito.Detalle.Idfactura.Split('-');
                    var codFact = cadenas.Count() > 2 ? cadenas[2] : "000000001";
                    var noFact = wrapper.NotaCredito.Detalle.Idfactura.Replace("-", "").Trim();

                    //tomar la factura activa
                    var fact = _ctx.FACHIS.FirstOrDefault(t => t.FHFactura == noFact && t.FHEstado == "ACT"); 

                    if (fact != null)
                    {
                        var cliente = _ctx.CXCDIR.FirstOrDefault(t => t.Ruc == fact.FHRuc);
                        if (cliente != null)
                        {
                            var identComp = "05";
                            if (cliente.Ruc.Length == 10)
                                identComp = "05";
                            else if (cliente.Ruc.Length == 13)
                                identComp = "04";
                            else identComp = "06";

                            if (cliente.Ruc == "9999999999999")
                                identComp = "07";

                            var infoAd = new Dictionary<string, string> {
                        { "email", cliente.Email},
                        { "Direccion", !string.IsNullOrEmpty(cliente.Direc1)?cliente.Direc1: ConfigurationManager.AppSettings["dirMatriz"].ToString()},
                        { "Telefono", !string.IsNullOrEmpty(cliente.Telef1)?cliente.Telef1:ConfigurationManager.AppSettings["Telefono"].ToString()},
                        { "Vendedor", ConfigurationManager.AppSettings["VendedorSeccion"].ToString()},
                        { "Clave", cliente.Ruc},
                        { "UGE",ConfigurationManager.AppSettings["UGE"]}, 
                        { "Comentario","DESCUENTO"},
                         // Nuevo
                        { "NoOp","VENTAS"},
                        };

                            var vIvaFact = (wrapper.NotaCredito.Detalle.DetalleNotaCredito[0].SubTotal * iva / 100);
                            var listTotalImpuesto = new List<totalImpuesto>();
                            var totalImp = new totalImpuesto
                            {
                                codigo = "2",
                                codigoPorcentaje = GetCodigoImpuesto(iva).ToString(),
                                baseImponible = wrapper.NotaCredito.Detalle.DetalleNotaCredito[0].SubTotal.ToString("N2").Replace(".", "").Replace(",", "."),
                                valor = vIvaFact.ToString("N2").Replace(".", "").Replace(",", ".")
                            };
                            listTotalImpuesto.Add(totalImp);

                            var listPagos = new List<pago>();
                            var tpago = new pago
                            {
                                formaPago = ConfigurationManager.AppSettings["FormaPago"].ToString(),//"19",
                                total = wrapper.NotaCredito.Detalle.Valor.ToString("N2").Replace(".", "").Replace(",", "."),
                                plazo = "0",
                                unidadTiempo = "dias"
                            };
                            listPagos.Add(tpago);
                            var listDetalles = new List<detalleRet>();
                            foreach (var ele in wrapper.NotaCredito.Detalle.DetalleNotaCredito)
                            {
                                var detAdd = new List<detAdicional>
                            {
                                new detAdicional{nombre = "Bodega",valor= ConfigurationManager.AppSettings["Bodega"].ToString()},
                                new detAdicional{nombre = "COMENTARIO1",valor=ele.Detalle},
                                new detAdicional{nombre = "COMENTARIO2",valor=ele.CodigoProducto }// +","+wrapper.Cabecera.Cliente.Segmento}
                            };
                                var codigoImp = GetCodigoImpuesto(iva);
                                var vIva = ele.SubTotal * (iva / 100);
                                var detImp = new List<impuesto> {
                                new impuesto
                                {
                                    codigo = "2",
                                    codigoPorcentaje=GetCodigoImpuesto(iva ).ToString(),
                                    tarifa=iva.ToString().Replace(".", "").Replace(",", "."),
                                    baseImponible=ele.SubTotal.ToString("N2").Replace(".", "").Replace(",", "."),
                                    valor=vIva.ToString("N2").Replace(".", "").Replace(",", ".")
                                }
                            };
                                var punit = ele.SubTotal;// / (1 + iva / 100);
                                var det = new detalleRet
                                {
                                    codigoAdicional = ConfigurationManager.AppSettings["codProducto"],
                                    codigoInterno = ConfigurationManager.AppSettings["codProducto"],
                                    descripcion = ConfigurationManager.AppSettings["nombProducto"],
                                    cantidad = ele.Cantidad.ToString(),
                                    precioUnitario = (punit * ele.Cantidad).ToString("N2").Replace(".", "").Replace(",", "."),
                                    descuento = "0.00",
                                    precioTotalSinImpuesto = ele.SubTotal.ToString("N2").Replace(".", "").Replace(",", "."),
                                    detallesAdicionales = detAdd,
                                    impuestos = detImp
                                };
                                listDetalles.Add(det);
                            }
                            decimal totalSinImpuesto = GetTotalSinImpuesto(wrapper.NotaCredito.Detalle.Valor, iva);
                            var feDocSustento = "";
                            var facturaMod = _ctx.FACHIS.FirstOrDefault(t => t.FHFactura == noFact);
                            if (facturaMod?.FHFechaf != null)
                                feDocSustento = facturaMod.FHFechaf.Value.Day.ToString().PadLeft(2, '0') + "/" + facturaMod.FHFechaf.Value.Month.ToString().PadLeft(2, '0') + "/" + facturaMod.FHFechaf.Value.Year.ToString().PadLeft(4, '0');


                            var factura = new notaCredito
                            {
                                id = "comprobante",
                                version = "1.1.0",
                                infoTributaria = new infoTributaria
                                {
                                    ambiente = ConfigurationManager.AppSettings["ambiente"],
                                    tipoEmision = "1",
                                    razonSocial = ConfigurationManager.AppSettings["razonSocial"],
                                    nombreComercial = ConfigurationManager.AppSettings["razonSocial"],
                                    ruc = ConfigurationManager.AppSettings["ruc"],
                                    claveAcceso = GetClaveAcceso(fechaEmision.Day.ToString().PadLeft(2, '0'), fechaEmision.Month.ToString().PadLeft(2, '0'), fechaEmision.Year.ToString().PadLeft(4, '0'), "01", ConfigurationManager.AppSettings["ruc"], ConfigurationManager.AppSettings["ambiente"], ConfigurationManager.AppSettings["estab"] + ConfigurationManager.AppSettings["ptoEmi"], codFact, "12345678", "1"),
                                    codDoc = "04", // 04 para nota de crédito
                                    estab = ConfigurationManager.AppSettings["estab"],
                                    ptoEmi = ConfigurationManager.AppSettings["ptoEmi"],
                                    secuencial = codFactura.Substring(6).PadLeft(9, '0'),
                                    dirMatriz = ConfigurationManager.AppSettings["dirMatriz"],
                                },
                                infoNotaCredito = new infoNotaCredito
                                {
                                    fechaEmision = fechaEmision.Day.ToString().PadLeft(2, '0') + "/" + fechaEmision.Month.ToString().PadLeft(2, '0') + "/" + fechaEmision.Year.ToString().PadLeft(4, '0'),
                                    obligadoContabilidad = "SI",
                                    tipoIdentificacionComprador = identComp,
                                    razonSocialComprador = cliente.Nombre,
                                    identificacionComprador = cliente.Ruc,
                                    totalSinImpuestos = totalSinImpuesto.ToString("N2").Replace(".", "").Replace(",", "."),
                                    moneda = "Dolar",
                                    totalConImpuestos = listTotalImpuesto,
                                    dirEstablecimiento = cliente.Direc1,
                                    rise = "NO",
                                    codDocModificado = "01",
                                    numDocModificado = GetNumeroDocumentoModificado(wrapper.NotaCredito.Detalle.Idfactura),//wrapper.NotaCredito.Detalle.Idfactura,

                                    //numDocModificado = "001-009-" + wrapper.NotaCredito.Detalle.Idfactura.Substring(6),

                                    fechaEmisionDocSustento = feDocSustento,
                                    motivo = "DEVOLUCION",
                                    valorModificacion = totalSinImpuesto.ToString("N2").Replace(".", "").Replace(",", "."),

                                },
                                detalles = listDetalles,
                                infoAdicional = infoAd
                            };

                            res = GetXmlSnatShot(factura);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Stopped program because of exception");
            }
            return res;
        }

        public static string ObtenerClaveAccesoDocumento(string codFact)
        {
            var fechaEmision = DateTime.Now;
            string claveDocumento = GetClaveAcceso(fechaEmision.Day.ToString().PadLeft(2, '0'), fechaEmision.Month.ToString().PadLeft(2, '0'), fechaEmision.Year.ToString().PadLeft(4, '0'), "01", ConfigurationManager.AppSettings["ruc"], ConfigurationManager.AppSettings["ambiente"], ConfigurationManager.AppSettings["estab"] + ConfigurationManager.AppSettings["ptoEmi"], codFact, "12345678", "1");
            return claveDocumento;
        }

        private static string GetNumeroDocumentoModificado(string numeroDocumentoFormateado)
        {
            string resultado = "";

            try
            {
                var contador = 1;
                foreach (var item in numeroDocumentoFormateado)
                {
                    // some logic here to determine if this param should be passed in
                    if (contador % 3 == 0 && contador <= 6)
                        resultado = resultado + item + "-";
                    else
                        resultado = resultado + item;
                    contador++;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Stopped program because of exception");
            }

            return resultado;
        }

        private static decimal GetTotalSinImpuesto(decimal valor, decimal iva)
        {
            decimal res = 0;
            try
            {
                res = valor / ((iva / 100) + 1);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Stopped program because of exception");
            }
            return res;
        }
        private static int GetCodigoImpuesto(decimal imp)
        {
            var res = 0;
            try
            {
                switch (imp)
                {
                    case 0:
                        res = 0;
                        break;
                    case 12:
                        res = 2;
                        break;
                    case 14:
                        res = 3;
                        break;
                    default:
                        res = 0;
                        break;
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Stopped program because of exception");
            }

            return res;
        }
        public static string GetXmlSnatShot(factura fact)
        {
            var result = "";

            try
            {
                var res = new XmlDocument();
                var xmlFact = res.CreateElement("factura");

                #region Atributos

                var h1 = res.CreateAttribute("id");
                h1.Value = fact.id;
                xmlFact.Attributes.Append(h1);

                var h2 = res.CreateAttribute("version");
                h2.Value = fact.version;
                xmlFact.Attributes.Append(h2);

                #endregion

                if (fact.infoTributaria != null)
                {
                    var infoTributaria = res.CreateElement("infoTributaria");

                    var ambiente = res.CreateElement("ambiente");
                    ambiente.InnerText = fact.infoTributaria.ambiente;
                    infoTributaria.AppendChild(ambiente);

                    var tipoEmision = res.CreateElement("tipoEmision");
                    tipoEmision.InnerText = fact.infoTributaria.tipoEmision;
                    infoTributaria.AppendChild(tipoEmision);

                    var razonSocial = res.CreateElement("razonSocial");
                    razonSocial.InnerText = fact.infoTributaria.razonSocial;
                    infoTributaria.AppendChild(razonSocial);

                    var nombreComercial = res.CreateElement("nombreComercial");
                    nombreComercial.InnerText = fact.infoTributaria.nombreComercial;
                    infoTributaria.AppendChild(nombreComercial);

                    var ruc = res.CreateElement("ruc");
                    ruc.InnerText = fact.infoTributaria.ruc;
                    infoTributaria.AppendChild(ruc);

                    var claveAcceso = res.CreateElement("claveAcceso");
                    claveAcceso.InnerText = fact.infoTributaria.claveAcceso;
                    infoTributaria.AppendChild(claveAcceso);

                    var codDoc = res.CreateElement("codDoc");
                    codDoc.InnerText = fact.infoTributaria.codDoc;
                    infoTributaria.AppendChild(codDoc);

                    var estab = res.CreateElement("estab");
                    estab.InnerText = fact.infoTributaria.estab;
                    infoTributaria.AppendChild(estab);

                    var ptoEmi = res.CreateElement("ptoEmi");
                    ptoEmi.InnerText = fact.infoTributaria.ptoEmi;
                    infoTributaria.AppendChild(ptoEmi);

                    var secuencial = res.CreateElement("secuencial");
                    secuencial.InnerText = fact.infoTributaria.secuencial;
                    infoTributaria.AppendChild(secuencial);

                    var dirMatriz = res.CreateElement("dirMatriz");
                    dirMatriz.InnerText = fact.infoTributaria.dirMatriz;
                    infoTributaria.AppendChild(dirMatriz);

                    xmlFact.AppendChild(infoTributaria);
                }

                if (fact.infoFactura != null)
                {
                    var infoFactura = res.CreateElement("infoFactura");

                    var fechaEmision = res.CreateElement("fechaEmision");
                    fechaEmision.InnerText = fact.infoFactura.fechaEmision;
                    infoFactura.AppendChild(fechaEmision);

                    var obligadoContabilidad = res.CreateElement("obligadoContabilidad");
                    obligadoContabilidad.InnerText = fact.infoFactura.obligadoContabilidad;
                    infoFactura.AppendChild(obligadoContabilidad);

                    var tipoIdentificacionComprador = res.CreateElement("tipoIdentificacionComprador");
                    tipoIdentificacionComprador.InnerText = fact.infoFactura.tipoIdentificacionComprador;
                    infoFactura.AppendChild(tipoIdentificacionComprador);

                    var razonSocialComprador = res.CreateElement("razonSocialComprador");
                    razonSocialComprador.InnerText = fact.infoFactura.razonSocialComprador;
                    infoFactura.AppendChild(razonSocialComprador);

                    var identificacionComprador = res.CreateElement("identificacionComprador");
                    identificacionComprador.InnerText = fact.infoFactura.identificacionComprador;
                    infoFactura.AppendChild(identificacionComprador);

                    var direccionComprador = res.CreateElement("direccionComprador");
                    direccionComprador.InnerText = fact.infoFactura.direccionComprador;
                    infoFactura.AppendChild(direccionComprador);

                    var totalSinImpuestos = res.CreateElement("totalSinImpuestos");
                    totalSinImpuestos.InnerText = fact.infoFactura.totalSinImpuestos;
                    infoFactura.AppendChild(totalSinImpuestos);

                    var totalDescuento = res.CreateElement("totalDescuento");
                    totalDescuento.InnerText = fact.infoFactura.totalDescuento;
                    infoFactura.AppendChild(totalDescuento);

                    var totalConImpuestos = res.CreateElement("totalConImpuestos");
                    if (fact.infoFactura.totalConImpuestos.Any())
                    {
                        foreach (var ele in fact.infoFactura.totalConImpuestos)
                        {
                            var totalImpuesto = res.CreateElement("totalImpuesto");

                            var codigo = res.CreateElement("codigo");
                            codigo.InnerText = ele.codigo;
                            totalImpuesto.AppendChild(codigo);

                            var codigoPorcentaje = res.CreateElement("codigoPorcentaje");
                            codigoPorcentaje.InnerText = ele.codigoPorcentaje;
                            totalImpuesto.AppendChild(codigoPorcentaje);

                            var baseImponible = res.CreateElement("baseImponible");
                            baseImponible.InnerText = ele.baseImponible;
                            totalImpuesto.AppendChild(baseImponible);

                            var valor = res.CreateElement("valor");
                            valor.InnerText = ele.valor;
                            totalImpuesto.AppendChild(valor);

                            totalConImpuestos.AppendChild(totalImpuesto);
                        }
                    }
                    infoFactura.AppendChild(totalConImpuestos);

                    var propina = res.CreateElement("propina");
                    propina.InnerText = fact.infoFactura.propina;
                    infoFactura.AppendChild(propina);

                    var importeTotal = res.CreateElement("importeTotal");
                    importeTotal.InnerText = fact.infoFactura.importeTotal;
                    infoFactura.AppendChild(importeTotal);

                    var moneda = res.CreateElement("moneda");
                    moneda.InnerText = fact.infoFactura.moneda;
                    infoFactura.AppendChild(moneda);

                    var pagos = res.CreateElement("pagos");
                    if (fact.infoFactura.pagos.Any())
                    {
                        foreach (var ele in fact.infoFactura.pagos)
                        {
                            var pago = res.CreateElement("pago");

                            var formaPago = res.CreateElement("formaPago");
                            formaPago.InnerText = ele.formaPago;
                            pago.AppendChild(formaPago);

                            var total = res.CreateElement("total");
                            total.InnerText = ele.total;
                            pago.AppendChild(total);

                            var plazo = res.CreateElement("plazo");
                            plazo.InnerText = ele.plazo;
                            pago.AppendChild(plazo);

                            var unidadTiempo = res.CreateElement("unidadTiempo");
                            unidadTiempo.InnerText = ele.unidadTiempo;
                            pago.AppendChild(unidadTiempo);

                            pagos.AppendChild(pago);
                        }
                    }
                    infoFactura.AppendChild(pagos);

                    xmlFact.AppendChild(infoFactura);
                }

                var detalles = res.CreateElement("detalles");
                if (fact.detalles.Any())
                {
                    foreach (var ele in fact.detalles)
                    {
                        var detalle = res.CreateElement("detalle");

                        var codigoPrincipal = res.CreateElement("codigoPrincipal");
                        codigoPrincipal.InnerText = ele.codigoPrincipal;
                        detalle.AppendChild(codigoPrincipal);

                        var codigoAuxiliar = res.CreateElement("codigoAuxiliar");
                        codigoAuxiliar.InnerText = ele.codigoAuxiliar;
                        detalle.AppendChild(codigoAuxiliar);

                        var descripcion = res.CreateElement("descripcion");
                        descripcion.InnerText = ele.descripcion;
                        detalle.AppendChild(descripcion);

                        var cantidad = res.CreateElement("cantidad");
                        cantidad.InnerText = ele.cantidad;
                        detalle.AppendChild(cantidad);

                        var precioUnitario = res.CreateElement("precioUnitario");
                        precioUnitario.InnerText = ele.precioUnitario;
                        detalle.AppendChild(precioUnitario);

                        var descuento = res.CreateElement("descuento");
                        descuento.InnerText = ele.descuento;
                        detalle.AppendChild(descuento);

                        var precioTotalSinImpuesto = res.CreateElement("precioTotalSinImpuesto");
                        precioTotalSinImpuesto.InnerText = ele.precioTotalSinImpuesto;
                        detalle.AppendChild(precioTotalSinImpuesto);

                        var detallesAdicionales = res.CreateElement("detallesAdicionales");
                        if (ele.detallesAdicionales.Any())
                        {
                            foreach (var item in ele.detallesAdicionales)
                            {
                                var detAdicional = res.CreateElement("detAdicional");
                                #region Atributos

                                var nombre = res.CreateAttribute("nombre");
                                nombre.Value = item.nombre;
                                detAdicional.Attributes.Append(nombre);

                                var valor = res.CreateAttribute("valor");
                                valor.Value = item.valor;
                                detAdicional.Attributes.Append(valor);

                                #endregion
                                detallesAdicionales.AppendChild(detAdicional);
                            }
                        }
                        detalle.AppendChild(detallesAdicionales);

                        var impuestos = res.CreateElement("impuestos");
                        if (ele.impuestos.Any())
                        {
                            foreach (var item in ele.impuestos)
                            {
                                decimal totalValorImpuesto = (Convert.ToDecimal(item.valor) * Convert.ToInt32(ele.cantidad)) / 100;

                                var impuesto = res.CreateElement("impuesto");

                                var codigo = res.CreateElement("codigo");
                                codigo.InnerText = item.codigo;
                                impuesto.AppendChild(codigo);

                                var codigoPorcentaje = res.CreateElement("codigoPorcentaje");
                                codigoPorcentaje.InnerText = item.codigoPorcentaje;
                                impuesto.AppendChild(codigoPorcentaje);

                                var tarifa = res.CreateElement("tarifa");
                                tarifa.InnerText = item.tarifa;
                                impuesto.AppendChild(tarifa);

                                var baseImponible = res.CreateElement("baseImponible");
                                baseImponible.InnerText = item.baseImponible;
                                impuesto.AppendChild(baseImponible);

                                var valor = res.CreateElement("valor");
                                valor.InnerText = totalValorImpuesto.ToString().Replace(",", ".");
                                impuesto.AppendChild(valor);

                                impuestos.AppendChild(impuesto);
                            }
                        }
                        detalle.AppendChild(impuestos);

                        detalles.AppendChild(detalle);
                    }
                }
                xmlFact.AppendChild(detalles);

                var infoAdicional = res.CreateElement("infoAdicional");
                if (fact.infoAdicional.Any())
                {
                    foreach (var ele in fact.infoAdicional)
                    {
                        var campoAdicional = res.CreateElement("campoAdicional");
                        #region Atributos

                        var nombre = res.CreateAttribute("nombre");
                        nombre.Value = ele.Key;
                        campoAdicional.Attributes.Append(nombre);

                        campoAdicional.InnerText = ele.Value;
                        #endregion
                        infoAdicional.AppendChild(campoAdicional);
                    }
                }
                xmlFact.AppendChild(infoAdicional);

                res.AppendChild(xmlFact);

                var facXml = new FacXmlString
                {
                    XmlString = res.InnerXml
                };
                result = JsonConvert.SerializeObject(facXml);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Stopped program because of exception");
            }
            return result;
        }
        public static string GetXmlSnatShot(notaCredito nota)
        {
            var result = "";
            try
            {
                var res = new XmlDocument();
                var xmlFact = res.CreateElement("notaCredito");

                #region Atributos

                var h1 = res.CreateAttribute("id");
                h1.Value = nota.id;
                xmlFact.Attributes.Append(h1);

                var h2 = res.CreateAttribute("version");
                h2.Value = nota.version;
                xmlFact.Attributes.Append(h2);

                #endregion

                if (nota.infoTributaria != null)
                {
                    var infoTributaria = res.CreateElement("infoTributaria");

                    var ambiente = res.CreateElement("ambiente");
                    ambiente.InnerText = nota.infoTributaria.ambiente;
                    infoTributaria.AppendChild(ambiente);

                    var tipoEmision = res.CreateElement("tipoEmision");
                    tipoEmision.InnerText = nota.infoTributaria.tipoEmision;
                    infoTributaria.AppendChild(tipoEmision);

                    var razonSocial = res.CreateElement("razonSocial");
                    razonSocial.InnerText = nota.infoTributaria.razonSocial;
                    infoTributaria.AppendChild(razonSocial);

                    var nombreComercial = res.CreateElement("nombreComercial");
                    nombreComercial.InnerText = nota.infoTributaria.nombreComercial;
                    infoTributaria.AppendChild(nombreComercial);

                    var ruc = res.CreateElement("ruc");
                    ruc.InnerText = nota.infoTributaria.ruc;
                    infoTributaria.AppendChild(ruc);

                    var claveAcceso = res.CreateElement("claveAcceso");
                    claveAcceso.InnerText = nota.infoTributaria.claveAcceso;
                    infoTributaria.AppendChild(claveAcceso);

                    var codDoc = res.CreateElement("codDoc");
                    codDoc.InnerText = nota.infoTributaria.codDoc;
                    infoTributaria.AppendChild(codDoc);

                    var estab = res.CreateElement("estab");
                    estab.InnerText = nota.infoTributaria.estab;
                    infoTributaria.AppendChild(estab);

                    var ptoEmi = res.CreateElement("ptoEmi");
                    ptoEmi.InnerText = nota.infoTributaria.ptoEmi;
                    infoTributaria.AppendChild(ptoEmi);

                    var secuencial = res.CreateElement("secuencial");
                    secuencial.InnerText = nota.infoTributaria.secuencial;
                    infoTributaria.AppendChild(secuencial);

                    var dirMatriz = res.CreateElement("dirMatriz");
                    dirMatriz.InnerText = nota.infoTributaria.dirMatriz;
                    infoTributaria.AppendChild(dirMatriz);

                    xmlFact.AppendChild(infoTributaria);
                }
                if (nota.infoNotaCredito != null)
                {
                    var infoNotaCredito = res.CreateElement("infoNotaCredito");

                    var fechaEmision = res.CreateElement("fechaEmision");
                    fechaEmision.InnerText = nota.infoNotaCredito.fechaEmision;
                    infoNotaCredito.AppendChild(fechaEmision);

                    var dirEstablecimiento = res.CreateElement("dirEstablecimiento");
                    dirEstablecimiento.InnerText = nota.infoNotaCredito.dirEstablecimiento;
                    infoNotaCredito.AppendChild(dirEstablecimiento);

                    var tipoIdentificacionComprador = res.CreateElement("tipoIdentificacionComprador");
                    tipoIdentificacionComprador.InnerText = nota.infoNotaCredito.tipoIdentificacionComprador;
                    infoNotaCredito.AppendChild(tipoIdentificacionComprador);

                    var razonSocialComprador = res.CreateElement("razonSocialComprador");
                    razonSocialComprador.InnerText = nota.infoNotaCredito.razonSocialComprador;
                    infoNotaCredito.AppendChild(razonSocialComprador);

                    var identificacionComprador = res.CreateElement("identificacionComprador");
                    identificacionComprador.InnerText = nota.infoNotaCredito.identificacionComprador;
                    infoNotaCredito.AppendChild(identificacionComprador);

                    var obligadoContabilidad = res.CreateElement("obligadoContabilidad");
                    obligadoContabilidad.InnerText = nota.infoNotaCredito.obligadoContabilidad;
                    infoNotaCredito.AppendChild(obligadoContabilidad);

                    var rise = res.CreateElement("rise");
                    rise.InnerText = nota.infoNotaCredito.rise;
                    infoNotaCredito.AppendChild(rise);

                    var codDocModificado = res.CreateElement("codDocModificado");
                    codDocModificado.InnerText = nota.infoNotaCredito.codDocModificado;
                    infoNotaCredito.AppendChild(codDocModificado);

                    var numDocModificado = res.CreateElement("numDocModificado");
                    numDocModificado.InnerText = nota.infoNotaCredito.numDocModificado;
                    infoNotaCredito.AppendChild(numDocModificado);

                    var fechaEmisionDocSustento = res.CreateElement("fechaEmisionDocSustento");
                    fechaEmisionDocSustento.InnerText = nota.infoNotaCredito.fechaEmisionDocSustento;
                    infoNotaCredito.AppendChild(fechaEmisionDocSustento);

                    var totalSinImpuestos = res.CreateElement("totalSinImpuestos");
                    totalSinImpuestos.InnerText = nota.infoNotaCredito.totalSinImpuestos;
                    infoNotaCredito.AppendChild(totalSinImpuestos);

                    var valorModificacion = res.CreateElement("valorModificacion");
                    valorModificacion.InnerText = nota.infoNotaCredito.valorModificacion;
                    infoNotaCredito.AppendChild(valorModificacion);

                    var moneda = res.CreateElement("moneda");
                    moneda.InnerText = nota.infoNotaCredito.moneda;
                    infoNotaCredito.AppendChild(moneda);

                    var totalConImpuestos = res.CreateElement("totalConImpuestos");
                    foreach (var ele in nota.infoNotaCredito.totalConImpuestos)
                    {
                        var totalImpuesto = res.CreateElement("totalImpuesto");

                        var codigo = res.CreateElement("codigo");
                        codigo.InnerText = ele.codigo;
                        totalImpuesto.AppendChild(codigo);

                        var codigoPorcentaje = res.CreateElement("codigoPorcentaje");
                        codigoPorcentaje.InnerText = ele.codigoPorcentaje;
                        totalImpuesto.AppendChild(codigoPorcentaje);

                        var baseImponible = res.CreateElement("baseImponible");
                        baseImponible.InnerText = ele.baseImponible;
                        totalImpuesto.AppendChild(baseImponible);

                        var valor = res.CreateElement("valor");
                        valor.InnerText = ele.valor;
                        totalImpuesto.AppendChild(valor);

                        totalConImpuestos.AppendChild(totalImpuesto);
                    }
                    infoNotaCredito.AppendChild(totalConImpuestos);

                    var motivo = res.CreateElement("motivo");
                    motivo.InnerText = nota.infoNotaCredito.motivo;
                    infoNotaCredito.AppendChild(motivo);

                    xmlFact.AppendChild(infoNotaCredito);
                }

                var detalles = res.CreateElement("detalles");
                if (nota.detalles.Any())
                {
                    foreach (var ele in nota.detalles)
                    {
                        var detalle = res.CreateElement("detalle");

                        var codigoInterno = res.CreateElement("codigoInterno");
                        codigoInterno.InnerText = ele.codigoInterno;
                        detalle.AppendChild(codigoInterno);

                        var codigoAdicional = res.CreateElement("codigoAdicional");
                        codigoAdicional.InnerText = ele.codigoAdicional;
                        detalle.AppendChild(codigoAdicional);

                        var descripcion = res.CreateElement("descripcion");
                        descripcion.InnerText = ele.descripcion;
                        detalle.AppendChild(descripcion);

                        var cantidad = res.CreateElement("cantidad");
                        cantidad.InnerText = ele.cantidad;
                        detalle.AppendChild(cantidad);

                        var precioUnitario = res.CreateElement("precioUnitario");
                        precioUnitario.InnerText = ele.precioUnitario;
                        detalle.AppendChild(precioUnitario);

                        var descuento = res.CreateElement("descuento");
                        descuento.InnerText = ele.descuento;
                        detalle.AppendChild(descuento);

                        var precioTotalSinImpuesto = res.CreateElement("precioTotalSinImpuesto");
                        precioTotalSinImpuesto.InnerText = ele.precioTotalSinImpuesto;
                        detalle.AppendChild(precioTotalSinImpuesto);

                        var detallesAdicionales = res.CreateElement("detallesAdicionales");
                        if (ele.detallesAdicionales.Any())
                        {
                            foreach (var item in ele.detallesAdicionales)
                            {
                                var detAdicional = res.CreateElement("detAdicional");
                                #region Atributos

                                var nombre = res.CreateAttribute("nombre");
                                nombre.Value = item.nombre;
                                detAdicional.Attributes.Append(nombre);

                                var valor = res.CreateAttribute("valor");
                                valor.Value = item.valor;
                                detAdicional.Attributes.Append(valor);

                                #endregion
                                detallesAdicionales.AppendChild(detAdicional);
                            }
                        }
                        detalle.AppendChild(detallesAdicionales);

                        var impuestos = res.CreateElement("impuestos");
                        if (ele.impuestos.Any())
                        {
                            foreach (var item in ele.impuestos)
                            {
                                var impuesto = res.CreateElement("impuesto");

                                var codigo = res.CreateElement("codigo");
                                codigo.InnerText = item.codigo;
                                impuesto.AppendChild(codigo);

                                var codigoPorcentaje = res.CreateElement("codigoPorcentaje");
                                codigoPorcentaje.InnerText = item.codigoPorcentaje;
                                impuesto.AppendChild(codigoPorcentaje);

                                var tarifa = res.CreateElement("tarifa");
                                tarifa.InnerText = item.tarifa;
                                impuesto.AppendChild(tarifa);

                                var baseImponible = res.CreateElement("baseImponible");
                                baseImponible.InnerText = item.baseImponible;
                                impuesto.AppendChild(baseImponible);

                                var valor = res.CreateElement("valor");
                                valor.InnerText = item.valor;
                                impuesto.AppendChild(valor);

                                impuestos.AppendChild(impuesto);
                            }
                        }
                        detalle.AppendChild(impuestos);

                        detalles.AppendChild(detalle);
                    }
                }
                xmlFact.AppendChild(detalles);

                var infoAdicional = res.CreateElement("infoAdicional");
                if (nota.infoAdicional.Any())
                {
                    foreach (var ele in nota.infoAdicional)
                    {
                        var campoAdicional = res.CreateElement("campoAdicional");
                        #region Atributos

                        var nombre = res.CreateAttribute("nombre");
                        nombre.Value = ele.Key;
                        campoAdicional.Attributes.Append(nombre);

                        campoAdicional.InnerText = ele.Value;
                        #endregion
                        infoAdicional.AppendChild(campoAdicional);
                    }
                }
                xmlFact.AppendChild(infoAdicional);

                res.AppendChild(xmlFact);

                var facXml = new FacXmlString
                {
                    XmlString = res.InnerXml
                };
                result = JsonConvert.SerializeObject(facXml);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Stopped program because of exception");
            }

            return result;
        }
        private static string GetClaveAcceso(string dia, string mes, string año, string tcomp, string ruc, string ambiente, string serie, string numcomp, string codnumero, string emision)
        {
            string res = "";
            try
            {
                string campos = dia + mes + año + tcomp + ruc + ambiente + serie + numcomp + codnumero + emision;
                string digitosFijos = "765432765432765432765432765432765432765432765432";
                decimal sumatoria = 0;
                decimal numEntero = 0;
                decimal digitoCalculado = 0;

                for (int i = 0; i < 48; i = i + 1)
                    sumatoria = sumatoria + (Convert.ToDecimal(campos.Substring(47 - i, 1)) * Convert.ToDecimal(digitosFijos.Substring(47 - i, 1)));

                numEntero = (Convert.ToInt32(sumatoria) / Convert.ToInt32(11));
                digitoCalculado = 11 - (sumatoria - (numEntero * 11));

                if (digitoCalculado == 10)
                    digitoCalculado = 1;
                if (digitoCalculado == 11)
                    digitoCalculado = 0;

                res = campos + digitoCalculado.ToString();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Stopped program because of exception");
            }

            return res;

        }
    }
}
