using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Collections;
using App_Mundial_Miles.Models;
using System.IO;
using Newtonsoft.Json;
using System.Configuration;
using System.Web.Configuration;
using NLog;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System.Threading.Tasks;
using AtisCode.Aplicacion;
using App_Mundial_Miles.Helpers;
using RestSharp;
using AtisCode.Aplicacion.NotasCredito;
using System.Runtime.Remoting.Messaging;
using System.ServiceModel.PeerResolvers;

namespace App_Mundial_Miles
{
    public static class Tools
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        //CONSULTAR TOKEN
        public static async Task<string> GetAccessToken(string urlAPI)
        {
            string token = StoredCache.GetTokenCacheStored(); // Get token from cache

            if (!string.IsNullOrEmpty(token))
                return token;

            #region PARAMETROS
            //BODY
            Dictionary<string, string> bodyParameter = new Dictionary<string, string>(){
                    //SAFI only accept this encode for API Token
                    { "application/x-www-form-urlencoded", "username=super&password=VFQnJ%2BdodRM%3D&ruc=" +  ConfigurationManager.AppSettings["ruc"].ToString() + "&year="+DateTime.Now.Year.ToString()+"&grant_type=password" }
                };
            #endregion

            CallAPIGetType call = new CallAPIGetType();

            //API Token SAFI don't header required
            var response = await call.SetContentRequestAPI(urlAPI, Method.POST, null, bodyParameter);

            token = GetAccessTokenJson(response) ?? string.Empty; //Get token property from json response

            StoredCache.UpdateTokenCacheStored(token); //Store token in cache memory
            return token;
        }

        public static string DeserializeSAFIResponse(string response) {
            try
            {
                var responseContentSAFI = JsonConvert.DeserializeObject<ResponseSAFI>(response);

                var messages = responseContentSAFI.Messages.Select(s => s.Type + " : " + s.Message).ToList();

                var finalResponse = string.Join(" ; ", messages);

                return finalResponse;
            }
            catch (Exception ex)
            {
                logger.Error(ex, string.Format("ERROR WHILE TRY DeserializeSAFIResponse, METHOD: ", System.Reflection.MethodBase.GetCurrentMethod().Name));
                return response + " ** " + ex.Message + " ** ";
            }
        }


        public static string ReadSetting(string key)
        {
            try
            {
                var appSettings = ConfigurationManager.AppSettings;
                string result = appSettings[key] ?? "Not Found";
                return result.ToString();
            }
            catch (ConfigurationErrorsException)
            {
                return string.Empty;
            }
        }

        public static int CastToInt(string value) {
            try
            {
                return Convert.ToInt32(value);
            }
            catch (Exception ex)
            {
                logger.Warn(ex, "Error Cast value method CastToInt");
                return 0;
            }
        }

        public static bool VerificarConexionesAmbiente(string cadena)
        {
            if (cadena == "172.16.36.18")
                return true;
            else
                return false;
        }

        public static bool ActualizarAPPSettings(List<ConfiguracionGlobal> configuraciones)
        {
            try
            {
                foreach (var item in configuraciones)
                {
                    Configuration config = WebConfigurationManager.OpenWebConfiguration("/");
                    var clave = config.AppSettings.Settings[item.Key];
                    if (clave != null)
                    {
                        string oldValue = clave.Value;
                        config.AppSettings.Settings[item.Key].Value = item.Value;
                        //config.Save(ConfigurationSaveMode.Modified);
                        config.Save(ConfigurationSaveMode.Minimal, true);
                        ConfigurationManager.RefreshSection("appSettings");
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error writing app settings. " + ex);
                return false;
            }
        }

        public static bool ActualizarParametroAPPSettings(ConfiguracionGlobal configuracion)
        {
            try
            {
                Configuration config = WebConfigurationManager.OpenWebConfiguration("~");
                //string ip = HttpContext.Current.Request.UserHostAddress;
                //logger.Info("Config names: ---> "+ string.Join(";",config.AppSettings.Settings.AllKeys));
                var clave = config.AppSettings.Settings[configuracion.Key];
                if (clave != null)
                {
                    string oldValue = clave.Value;
                    config.AppSettings.Settings[configuracion.Key].Value = configuracion.Value;
                    //config.Save(ConfigurationSaveMode.Modified);
                    //config.Save(ConfigurationSaveMode.Full, true);
                    config.Save(ConfigurationSaveMode.Minimal, false);
                    ConfigurationManager.RefreshSection("appSettings");
                }

                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Stopped program because of exception");
                logger.Warn("Error al querer modificar archivo de configuración: " + configuracion.Key);
                //Console.WriteLine("Error writing app settings. " + ex);
                return false;
            }
        }
        public static Dictionary<string, TValue> ToDictionary<TValue>(object obj)
        {
            var json = JsonConvert.SerializeObject(obj);
            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, TValue>>(json);
            return dictionary;
        }

        public static IEnumerable<SelectListItem> ConvertToSelectListItem<T>(this IEnumerable<T> elements, string selectedItem = "", string selectedText = null, string value = "Id", string name = "Nombre") where T : class
        {
            var list = new List<SelectListItem>();
            if (elements != null && elements.Any())
            {
                foreach (var item in elements)
                {
                    var type = item.GetType();
                    var valueProp = type.GetProperties().FirstOrDefault(t => t.Name.ToLower().Equals(value.ToLower()));
                    var nameProp = type.GetProperties().FirstOrDefault(t => t.Name.ToLower().Equals(name.ToLower()));
                    if (valueProp != null && nameProp != null)
                    {
                        var valueV = valueProp.GetValue(item, null);
                        var nameV = nameProp.GetValue(item, null);
                        var temp = new SelectListItem()
                        {
                            Value = valueV.ToString().Trim(),
                            Text = nameV.ToString().Trim(),
                            Selected = valueV.ToString().Equals(selectedItem)
                        };
                        list.Add(temp);
                    }
                }
                if (!string.IsNullOrEmpty(selectedText))
                {
                    list.Insert(0, new SelectListItem()
                    {
                        Selected = false,
                        Text = selectedText,
                        Value = ""
                    });
                }
            }
            else
            {
                list.Insert(0, new SelectListItem()
                {
                    Selected = true,
                    Text = "No Existen...",
                    Value = "-1"
                });
            }

            return list;
        }

        public static string GetJson(string texto)
        {
            var res = "";
            var json = JsonConvert.DeserializeObject<dynamic>(texto);
            res = json["access_token"];
            return res;
        }

        public static bool IsInRange(this double number, double edadMaxima, double edadMinima)
        {

            var text = number.ToString(CultureInfo.InvariantCulture);
            var array = text.Split(",.".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            //if (array.Length > 1)
            //    return int.Parse(array[1]);
            //return int.Parse(array[0]);
            return edadMinima <= number && number <= edadMaxima;
        }

        public static IEnumerable<SelectListItem> ConvertToSelectListItem<T>(this IEnumerable<T> elements, IEnumerable<T> validos, string selectedItem = "", string selectedText = null, string value = "Id", string name = "Nombre") where T : class
        {
            var list = new List<SelectListItem>();
            var invalids = elements.Except(validos);
            foreach (var item in validos)
            {
                var type = item.GetType();
                var valueProp = type.GetProperties().FirstOrDefault(t => t.Name.ToLower().Equals(value.ToLower()));
                var nameProp = type.GetProperties().FirstOrDefault(t => t.Name.ToLower().Equals(name.ToLower()));
                if (valueProp != null && nameProp != null)
                {
                    var valueV = valueProp.GetValue(item, null);
                    var nameV = nameProp.GetValue(item, null);
                    var temp = new SelectListItem()
                    {
                        Value = valueV.ToString().Trim(),
                        Text = nameV.ToString().Trim(),
                        Selected = valueV.ToString().Equals(selectedItem)
                    };
                    list.Add(temp);
                }
            }
            foreach (var item in invalids)
            {
                var type = item.GetType();
                var valueProp = type.GetProperties().FirstOrDefault(t => t.Name.ToLower().Equals(value.ToLower()));
                var nameProp = type.GetProperties().FirstOrDefault(t => t.Name.ToLower().Equals(name.ToLower()));
                if (valueProp != null && nameProp != null)
                {
                    var valueV = valueProp.GetValue(item, null);
                    var nameV = nameProp.GetValue(item, null);
                    var temp = new SelectListItem()
                    {
                        Value = valueV.ToString(),
                        Text = "☺" + nameV.ToString(),
                        Selected = valueV.ToString().Equals(selectedItem)
                    };
                    list.Add(temp);
                }
            }
            if (!string.IsNullOrEmpty(selectedText))
            {
                list.Insert(0, new SelectListItem()
                {
                    Selected = false,
                    Text = selectedText,
                    Value = ""
                });
            }
            return list;
        }

        public static IEnumerable<SelectListItem> ConvertToSelectListItem<T>(this IEnumerable<T> elements, IEnumerable<string> selectedItems, string selectedText = null, string value = "Id", string name = "Nombre") where T : class
        {
            var list = new List<SelectListItem>();
            foreach (var item in elements)
            {
                var type = item.GetType();
                var valueProp = type.GetProperties().FirstOrDefault(t => t.Name.ToLower().Equals(value.ToLower()));
                var nameProp = type.GetProperties().FirstOrDefault(t => t.Name.ToLower().Equals(name.ToLower()));
                if (valueProp != null && nameProp != null)
                {
                    var valueV = valueProp.GetValue(item, null);
                    var nameV = nameProp.GetValue(item, null);
                    var temp = new SelectListItem()
                    {
                        Value = valueV.ToString().Trim(),
                        Text = nameV.ToString().Trim(),
                        Selected = selectedItems.Contains(valueV.ToString())
                    };
                    list.Add(temp);
                }
            }
            if (selectedText != null)
            {
                list.Insert(0, new SelectListItem()
                {
                    Selected = false,
                    Text = selectedText,
                    Value = ""
                });
            }
            return list;
        }

        public static long GetIdFromPath(this string path)
        {
            var regex = new Regex("[0-9]+");
            var collection = regex.Matches(path);
            if (collection.Count > 0)
                return long.Parse(collection[collection.Count - 1].Value);
            return 0;
        }

        public static string NormalizeForSearch(this string text)
        {
            var temp =
                text.Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u").Replace(
                    " ", "");
            return temp;
        }

        /// <summary>
        ///  A partir de un ModelState de una action POST, crea una lista resumida de los errores que existen
        /// </summary>
        /// <param name="actual">ModelState actual</param>
        /// <returns></returns>
        public static List<string> ErroresValidacionModelState(ModelStateDictionary actual)
        {
            return (from valor in actual.Values
                    where valor.Errors.Any()
                    from error in valor.Errors
                    select error.ErrorMessage).ToList();
        }

        public static string CrearCaminos(string carpetaInicial, List<string> carpetas)
        {
            string camino = carpetaInicial;

            foreach (string ele in carpetas)
            {
                if (!Directory.Exists(Path.Combine(camino, ele)))
                {
                    Directory.CreateDirectory(Path.Combine(camino, ele));
                }
                camino = Path.Combine(camino, ele);
            }

            return camino;

        }

        public static string GetContentType(string extension)
        {
            var contentType = "";

            switch (extension.ToLower())
            {
                case ".pdf":
                    contentType = "application/pdf";
                    break;
                case ".doc":
                    contentType = "application/doc";
                    break;
                case ".docx":
                    contentType = "application/doc";
                    break;
                case ".txt":
                    contentType = "application/txt";
                    break;
                case ".xls":
                    contentType = "application/xls";
                    //Response.ContentType = "application/ms-excel";
                    break;
                case ".xlsx":
                    contentType = "application/xls";
                    //Response.ContentType = "application/ms-excel";
                    break;
                case ".log":
                    contentType = "application/txt";
                    break;
                case ".rar":
                    contentType = "application/rar";
                    break;
                case ".7z":
                    contentType = "application/7z";
                    break;
                case ".jpg":
                    contentType = "application/jpg";
                    break;
                case ".bmp":
                    contentType = "application/bmp";
                    break;
                case ".png":
                    contentType = "application/png";
                    break;
            }

            return contentType;
        }

        public static string GetNameMonth(int mont)
        {
            var res = "";

            switch (mont)
            {
                case 1:
                    res = "Enero";
                    break;
                case 2:
                    res = "Febrero";
                    break;
                case 3:
                    res = "Marzo";
                    break;
                case 4:
                    res = "Abril";
                    break;
                case 5:
                    res = "Mayo";
                    break;
                case 6:
                    res = "Junio";
                    break;
                case 7:
                    res = "Julio";
                    break;
                case 8:
                    res = "Agosto";
                    break;
                case 9:
                    res = "Septiembre";
                    break;
                case 10:
                    res = "Octubre";
                    break;
                case 11:
                    res = "Noviembre";
                    break;
                case 12:
                    res = "Diciembre";
                    break;
            }
            return res;
        }
        public static string GetNameDay(int day)
        {
            var res = "";

            switch (day)
            {
                case 1:
                    res = "Lunes";
                    break;
                case 2:
                    res = "Martes";
                    break;
                case 3:
                    res = "Miércoles";
                    break;
                case 4:
                    res = "Jueves";
                    break;
                case 5:
                    res = "Viernes";
                    break;
                case 6:
                    res = "Sábado";
                    break;
                case 7:
                    res = "Domingo";
                    break;
            }
            return res;
        }

        public static bool ValidarCampoObligatorio(string campo)
        {
            if (string.IsNullOrEmpty(campo))
                return true;
            else
                return false;
        }
        // Flag = True para nota de credito, False para factura
        public static string ValidarCampo(string columna, string valor, bool flag)
        {
            valor = !string.IsNullOrEmpty(valor) ? valor.Trim() : "";

            //ValidadorCampo validacion = new ValidadorCampo();
            bool esNumero;
            int longitudCaracteres = 0;
            var error = "El campo {0} con el valor {1} ,tiene una extensión de caracteres mayor a la permitida. Solo permite {2} caracteres";

            //if (ValidarCampoObligatorio(valor) && valor != "PROVEEDOR" && valor != "CODIGOCATEGORIA") 
            //    return "El campo " + columna + " es requerido.";
            switch (columna)
            {
                case "FACTURA":
                    longitudCaracteres = 15;
                    if (valor.Length > longitudCaracteres)
                    {
                        error = string.Format(error, columna, valor, longitudCaracteres);
                    }
                    else
                    {
                        error = null;
                    }
                    break;
                case "TOTAL":
                    var separadorIncorrecto = valor.IndexOf(".");

                    esNumero = decimal.TryParse(valor, out decimal total);
                    if (!esNumero)
                        error = "Tipo de dato incorrecto para el campo " + columna + " , con el valor " + valor + " .Solo admite valores numéricos";
                    else
                        error = null;

                    if (separadorIncorrecto != -1)
                        error = "Error en la columna " + columna + "; formato incorrecto. El separador decimal debe ser ',' Revisar la descripción de cada columna del formato.";
                    else
                        error = null;

                    break;
                case "CANTIDAD":
                    esNumero = int.TryParse(valor, out int cantidad);
                    if (!esNumero)
                        error = "Tipo de dato incorrecto para el campo " + columna + " , con el valor " + valor + " .Solo admite valores numéricos";
                    else
                        error = null;
                    break;
                case "DETALLE":
                    longitudCaracteres = 300;
                    if (valor.Length > longitudCaracteres)
                    {
                        error = string.Format(error, columna, valor, longitudCaracteres);
                    }
                    else
                    {
                        error = null;
                    }
                    break;
                case "PRECIOUNITARIO":
                    var separadorIncorrecto1 = valor.IndexOf(".");

                    //if (separadorIncorrecto1 != -1)
                    //    valor.Replace(".", ",");
                    esNumero = decimal.TryParse(valor, out decimal precioUnitario);

                    if (!esNumero)
                        error = "Tipo de dato incorrecto para el campo " + columna + " , con el valor " + valor + " .Solo admite valores numéricos";
                    else
                        error = null;

                    if (separadorIncorrecto1 != -1)
                        error = "Error en la columna " + columna + "; formato incorrecto. El separador decimal debe ser ',' Revisar la descripción de cada columna del formato.";
                    else
                        error = null;

                    break;
                //case "SUBTOTAL":
                case "SUBTOTAL":
                    var separadorIncorrecto2 = valor.IndexOf(".");

                    esNumero = decimal.TryParse(valor, out decimal subtotal);
                    if (!esNumero)
                        error = "Tipo de dato incorrecto para el campo " + columna + " , con el valor " + valor + " .Solo admite valores numéricos";
                    else
                        error = null;

                    if (separadorIncorrecto2 != -1)
                        error = "Error en la columna " + columna + "; formato incorrecto. El separador decimal debe ser ',' Revisar la descripción de cada columna del formato.";
                    else
                        error = null;

                    break;
                // CAMPOS FACTURA
                case "CLIENTE":
                    longitudCaracteres = 200;//60;
                    if (valor.Length > longitudCaracteres)
                    {
                        error = string.Format(error, columna, valor, longitudCaracteres);
                    }
                    else
                    {
                        error = null;
                    }
                    break;
                case "IDENTIFICACION":
                    longitudCaracteres = 13;
                    if (valor.Length > longitudCaracteres)
                    {
                        error = string.Format(error, columna, valor, longitudCaracteres);
                    }
                    else
                    {
                        error = null;
                    }
                    break;
                //Nuevo
                case "DESCUENTO":
                    var separadorIncorrecto3 = valor.IndexOf(".");

                    esNumero = decimal.TryParse(valor, out decimal descuento);

                    if (!esNumero)
                        error = "Tipo de dato incorrecto para el campo " + columna + " , con el valor " + valor + " .Solo admite valores numéricos";
                    else
                        error = null;

                    if (separadorIncorrecto3 != -1)
                        error = "Error en la columna " + columna + "; formato incorrecto. El separador decimal debe ser ',' Revisar la descripción de cada columna del formato.";
                    else
                        error = null;

                    break;
                case "DIRECCION":
                    longitudCaracteres = 150;
                    if (valor.Length > longitudCaracteres)
                    {
                        error = string.Format(error, columna, valor, longitudCaracteres);
                    }
                    else
                    {
                        error = null;
                    }
                    break;
                case "MAILCLIENTE":
                    longitudCaracteres = 250;
                    if (valor.Length > longitudCaracteres)
                    {
                        error = string.Format(error, columna, valor, longitudCaracteres);
                    }
                    else
                    {
                        error = null;
                    }
                    break;
                case "TELEFONO":
                    longitudCaracteres = 10;
                    if (valor.Length > longitudCaracteres)
                    {
                        error = string.Format(error, columna, valor, longitudCaracteres);
                    }
                    else
                    {
                        error = null;
                    }
                    break;
                case "FECHAVENTA":
                    string format = "dd/mm/yyyy";
                    DateTime dateTime;
                    if (!DateTime.TryParseExact(valor, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
                        error = "Tipo de dato incorrecto para el campo " + columna + " , con el valor " + valor + " .Solo admite una fecha correcta con el formato: " + format;
                    else
                        error = null;
                    break;
                default:
                    return "";
            }

            return error;
        }

        //Retorna ERRORES si la estructura de la trama está incorrecta o si ocurre algún error al serializar la estructura
        public static string ValidarSchemaJSON(string nombreArchivoJSONSchemma, Object objeto)
        {
            string result = string.Empty;
            try
            {
                //El nombre del schema a validar no puede ser nulo
                if (string.IsNullOrEmpty(nombreArchivoJSONSchemma))
                    return "Name Schema File can't be null ";

                //La trama no puede ser nula
                if (objeto == null)
                    return "Schema invalid. Value can't be null ";

                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory.ToString(), nombreArchivoJSONSchemma); // Ruta del Schema

                var texto = File.ReadAllText(path); // Lee el archivo como texto plano

                var trama = JsonConvert.SerializeObject(objeto); // Serializa el objeto

                JObject json = JObject.Parse(trama); // Convierte la trama serializada a un objeto JSON

                JSchema schema = JSchema.Parse(texto); // Obtiene el Schema del archivo enviado

                IList<string> messages = new List<string>(); // Catch errores en la trama

                bool isValidJSON = json.IsValid(schema, out messages); // Verifica si la trama cumple con los parámetros del Schema

                result = String.Join(" ; ", messages);

                return result;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error");
                return ex.Message;
            }
        }

        public static string ValidarSchemaJSON2(string nombreArchivoJSONSchemma, object objeto, bool isInvoice = true)
        {
            //Build schema automatic JSON
            //https://www.liquid-technologies.com/online-json-to-schema-converter
            try
            {
                //El nombre del schema a validar no puede ser nulo
                if (string.IsNullOrEmpty(nombreArchivoJSONSchemma))
                    return "Name Schema File can't be null ";

                //La trama no puede ser nula
                if (objeto == null)
                    return "Schema invalid. Value can't be null ";

                var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory.ToString(), nombreArchivoJSONSchemma); // Ruta del Schema

                var texto = File.ReadAllText(path); // Lee el archivo como texto plano

                var trama = JsonConvert.SerializeObject(objeto); // Serializa el objeto

                //Valida de acuerdo a las Anotaciones que tiene la clase sobre cada propiedad
                var schema = isInvoice ? NJsonSchema.JsonSchema.FromType<Wrapper>() : NJsonSchema.JsonSchema.FromType<WraperNotaCreditoWs>();

                var schemaData = schema.ToJson();
                var errors = schema.Validate(trama);

                string result = string.Join(" ; ", errors);

                return result;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error");
                return ex.Message;
            }
        }

        public static string GetAccessTokenJson(string texto)
        {
            try
            {
                var json = JsonConvert.DeserializeObject<dynamic>(texto);
                string res = json["access_token"];
                return res;
            }
            catch (Exception ex)
            {
                logger.Error(ex, System.Reflection.MethodBase.GetCurrentMethod().Name);
                return texto;
            }
        }


    }
}