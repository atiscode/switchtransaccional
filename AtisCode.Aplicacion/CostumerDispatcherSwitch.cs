using Newtonsoft.Json;
using NLog;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AtisCode.Aplicacion
{
    public class CostumerDispatcherSwitch
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static string GetNewCostumer(Wrapper wrapper)
        {
            var res = "";

            try
            {
                if (wrapper != null && wrapper.Cabecera != null && wrapper.Cabecera.Cliente != null)
                {
                    var cliente = wrapper.Cabecera.Cliente;
                    var tipoDoc = "";
                    if (cliente.Identificacion.Length == 13)
                        tipoDoc = "RUC";
                    else if (cliente.Identificacion.Length == 10)
                        tipoDoc = "C.C";
                    else
                        tipoDoc = "Pasaporte";

                    var costumer = new Costumer
                    {
                        Clave = cliente.Identificacion,
                        Nombre = cliente.NombreCliente,
                        TipoDocumento = tipoDoc,
                        NumeroDocumento = cliente.Identificacion,
                        FechaCreacion = DateTime.Now.ToShortDateString(),
                        Status = ConfigurationManager.AppSettings["Status"].ToString(),//"Buena",
                        Categoria = ConfigurationManager.AppSettings["CategoriaCliente"].ToString(),//"CLIENTES VIAJES",
                        Contribuyente = ConfigurationManager.AppSettings["Contribuyente"].ToString(),//"P.NATURAL NO CONTABILIDAD",
                        Ciudad = ConfigurationManager.AppSettings["Ciudad"].ToString(),//"QUITO",
                        Direccion = !string.IsNullOrEmpty(cliente.Direccion) ? cliente.Direccion : ConfigurationManager.AppSettings["dirMatriz"].ToString(),//"Guipuzcoa E14-46 y Mallorca Edificio QPH",
                        Telefono = !string.IsNullOrEmpty(cliente.Telefono) ? cliente.Telefono : ConfigurationManager.AppSettings["Telefono"].ToString(),//"3956060",
                        Referencia = "NA",//cliente.NombreCliente,
                        CuentaContable = ConfigurationManager.AppSettings["CuentaContable"].ToString(),//"1.1.02.001.009", 
                        GrupoCredito = Convert.ToInt32(ConfigurationManager.AppSettings["GrupoCredito"]),//1,
                        VendedorSeccion = ConfigurationManager.AppSettings["VendedorSeccion"].ToString(),//                 "00000",
                        TributaImpuesto = ConfigurationManager.AppSettings["TributaImpuesto"].ToString(),//"SI",
                        ListaPrecioContado = ConfigurationManager.AppSettings["ListaPrecioContado"].ToString(),//"LISTA ECUADOR",
                        ListaPrecioCredito = ConfigurationManager.AppSettings["ListaPrecioCredito"].ToString(),//"LISTA ECUADOR",
                        LimiteCredito = Convert.ToInt32(ConfigurationManager.AppSettings["LimiteCredito"]),//10000,
                        Email = cliente.Mail,
                        DocumentoElectronico = ConfigurationManager.AppSettings["DocumentoElectronico"].ToString(),//"NO",
                        Relacionado = ConfigurationManager.AppSettings["Relacionado"].ToString(),// "NO"
                    };
                    res = JsonConvert.SerializeObject(costumer);
                };
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Stopped program because of exception");
            }

            return res;
        }

        public async Task<string>  GetCostumerAsync(string trama)
        {
            return await Task.Run(() =>
            {
                var res = string.Empty;
                try
                {
                    Wrapper wrapper = JsonConvert.DeserializeObject<Wrapper>(trama);
                    if (wrapper != null && wrapper.Cabecera != null && wrapper.Cabecera.Cliente != null)
                    {
                        var cliente = wrapper.Cabecera.Cliente;
                        var tipoDoc = "";
                        if (cliente.Identificacion.Length == 13)
                            tipoDoc = "RUC";
                        else if (cliente.Identificacion.Length == 10)
                            tipoDoc = "C.C";
                        else
                            tipoDoc = "Pasaporte";

                        var costumer = new Costumer
                        {
                            Clave = cliente.Identificacion,
                            Nombre = cliente.NombreCliente,
                            TipoDocumento = tipoDoc,
                            NumeroDocumento = cliente.Identificacion,
                            FechaCreacion = DateTime.Now.ToShortDateString(),
                            Status = ConfigurationManager.AppSettings["Status"].ToString(),//"Buena",
                            Categoria = ConfigurationManager.AppSettings["CategoriaCliente"].ToString(),//"CLIENTES VIAJES",
                            Contribuyente = ConfigurationManager.AppSettings["Contribuyente"].ToString(),//"P.NATURAL NO CONTABILIDAD",
                            Ciudad = ConfigurationManager.AppSettings["Ciudad"].ToString(),//"QUITO",
                            Direccion = !string.IsNullOrEmpty(cliente.Direccion) ? cliente.Direccion : ConfigurationManager.AppSettings["dirMatriz"].ToString(),
                            Telefono = !string.IsNullOrEmpty(cliente.Telefono) ? cliente.Telefono : ConfigurationManager.AppSettings["Telefono"].ToString(),
                            Referencia = cliente.NombreCliente,//"",
                            CuentaContable = ConfigurationManager.AppSettings["CuentaContable"].ToString(),//"1.1.02.001.013",
                            GrupoCredito = Convert.ToInt32(ConfigurationManager.AppSettings["GrupoCredito"]),//1,
                            VendedorSeccion = ConfigurationManager.AppSettings["VendedorSeccion"].ToString(),//                 "00000",
                            TributaImpuesto = ConfigurationManager.AppSettings["TributaImpuesto"].ToString(),//"SI",
                            ListaPrecioContado = ConfigurationManager.AppSettings["ListaPrecioContado"].ToString(),//"LISTA ECUADOR",
                            ListaPrecioCredito = ConfigurationManager.AppSettings["ListaPrecioCredito"].ToString(),//"LISTA ECUADOR",
                            LimiteCredito = Convert.ToInt32(ConfigurationManager.AppSettings["LimiteCredito"]),//10000,
                            Email = cliente.Mail,
                            DocumentoElectronico = ConfigurationManager.AppSettings["DocumentoElectronico"].ToString(),//"NO",
                            Relacionado = ConfigurationManager.AppSettings["Relacionado"].ToString(),// "NO"
                        };
                        res = JsonConvert.SerializeObject(costumer);
                    }

                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Stopped program because of exception");
                }
                return res;
            });
        }

        public static IRestResponse InsertCostumerRequest(string json, string apiBaseUri, string apiCostumerUri)
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
                    request.AddParameter("application/json", json, ParameterType.RequestBody);

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
    }
}
