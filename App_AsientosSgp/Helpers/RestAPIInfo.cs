using AtisCode.Aplicacion.Model;
using AtisCode.Aplicacion.Model.db_Local;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace App_Mundial_Miles.Helpers
{
    public static class RestAPIInfo
    {
        private static readonly SafiCredenciales Credenciales = new SafiCredenciales();

        public static IRestResponse GetRestResponse(string json, string apiBaseUri, string apiCostumerUri, int? anio)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    AtisSafiCredenciales credenciales = new AtisSafiCredenciales();

                    if (anio.HasValue)
                        credenciales = Credenciales.ConsultarCredenciales(anio.Value);
                    else
                        credenciales = Credenciales.ConsultarCredenciales(0);

                    var client1 = new RestClient(apiBaseUri);
                    var request = new RestRequest(Method.POST);
                    request.AddHeader("postman-token", credenciales.PostmanToken);
                    request.AddHeader("cache-control", credenciales.CacheControl);
                    request.AddHeader("content-type", credenciales.ContentType);


                    request.AddParameter("username", credenciales.Username, ParameterType.GetOrPost);
                    request.AddParameter("password", credenciales.Password, ParameterType.GetOrPost);

                    request.AddParameter("year", credenciales.Anio, ParameterType.GetOrPost);
                    request.AddParameter("grant_type", credenciales.GrantType, ParameterType.GetOrPost);

                    //string encodedBody = string.Format("username={0}&password= {1}&ruc={2}&year={2}&grant_type={2}", "super", "VFQnJ+dodRM=", ruc, "2018", "password");
                    //request.AddParameter("application/x-www-form-urlencoded", encodedBody, ParameterType.RequestBody);

                    IRestResponse response = client1.Execute(request);
                    var token = Tools.GetJson(response.Content);

                    if (!string.IsNullOrEmpty(token))
                    {
                        client1 = new RestClient(apiCostumerUri);
                        request = new RestRequest(Method.POST);
                        var autho = "bearer " + token;
                        request.AddHeader("authorization", autho);
                        request.AddHeader("content-type", "application/json");
                        request.AddParameter("application/json", json, ParameterType.RequestBody);

                        response = client1.Execute(request);
                        return response;
                    }
                    else
                    {
                        response.ErrorMessage = EnumsInfo.MensajeErrorGeneracionTokenCredenciales;
                        return response;
                    }

                }
                catch (Exception ex)
                {
                    throw;
                }
            }
        }
    }
}