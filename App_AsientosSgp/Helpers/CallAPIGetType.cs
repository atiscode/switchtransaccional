using System;
using RestSharp;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NLog;
using System.Net;

namespace App_Mundial_Miles.Helpers
{
    public class CallAPIGetType
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public CallAPIGetType()
        {
            TimeOut = Tools.CastToInt(!string.IsNullOrEmpty(Tools.ReadSetting("TimeOutService")) ? Tools.ReadSetting("TimeOutService") : "0");
        }

        public int TimeOut { get; set; }

        public async Task<string> SetContentRequestAPI(string url, Method metodo = Method.GET, Dictionary<string, string> headerParameters = null, Dictionary<string, string> bodyParameters = null)
        {
            try
            {
                var conexion = new RestClient(url) { 
                  Timeout = TimeOut
                };

                var request = new RestRequest(metodo); //  GET by default

                //Add parameters to header: Authorization, content-type, etc.
                if (headerParameters != null)
                {
                    foreach (KeyValuePair<string, string> param in headerParameters)
                    {
                        request.AddHeader(param.Key, param.Value);
                    }
                }

                //Add parameter to body request POST
                if (bodyParameters != null) {
                    foreach (KeyValuePair<string, string> param in bodyParameters)
                    {
                        request.AddParameter(param.Key, param.Value, ParameterType.RequestBody);
                    }
                }

                var response = await conexion.ExecuteTaskAsync(request);

                if (response.StatusCode != HttpStatusCode.OK) {
                    StoredCache.UpdateTokenCacheStored(string.Empty); // Clean tokenSAFI
                    logger.Info(string.Format("Url: {0} ; Code Response: {1} ;  Status Code Response: {2} ; Response Content: {3} ; Error Message: {4}", url, (int)response.StatusCode,  response.StatusCode, response.Content, response.ErrorMessage ?? "There is no message in the reply."));
                    return response.ErrorMessage;
                }


                return  response.Content;
            }
            catch (Exception ex)
            {
                logger.Error(ex, string.Format("Url: {0} ; METHOD: ",url, System.Reflection.MethodBase.GetCurrentMethod().Name));
                return ex.Message;
            }
        }

        public async Task<IRestResponse> SetRequestAPI(string url, Method metodo = Method.GET, Dictionary<string, string> headerParameters = null, Dictionary<string, string> bodyParameters = null)
        {
            try
            {
                var conexion = new RestClient(url)
                {
                    Timeout = TimeOut
                };

                var request = new RestRequest(metodo); //  GET by default

                //Add parameters to header: Authorization, content-type, etc.
                if (headerParameters != null)
                {
                    foreach (KeyValuePair<string, string> param in headerParameters)
                    {
                        request.AddHeader(param.Key, param.Value);
                    }
                }

                //Add parameter to body request POST
                if (bodyParameters != null)
                {
                    foreach (KeyValuePair<string, string> param in bodyParameters)
                    {
                        request.AddParameter(param.Key, param.Value, ParameterType.RequestBody);
                    }
                }

                var response = await conexion.ExecuteTaskAsync(request);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    StoredCache.UpdateTokenCacheStored(string.Empty); // Clean tokenSAFI
                    logger.Info(string.Format("Url: {0} ; Status Code Request: {1} ; Request Content: {2} ; Error Message: {3}", url, response.StatusCode, response.Content, response.ErrorMessage ?? string.Empty));
                }

                return response;
            }
            catch (Exception ex)
            {
                logger.Error(ex, string.Format("Url: {0} ; METHOD: ", url, System.Reflection.MethodBase.GetCurrentMethod().Name));
                return null;
            }
        }

    }
}