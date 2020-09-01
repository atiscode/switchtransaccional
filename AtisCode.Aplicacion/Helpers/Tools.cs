using AtisCode.Aplicacion.Model.db_Safi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.IO;


namespace AtisCode.Aplicacion
{
    public static class Tools
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static string ReadSetting(string key)
        {
            try
            {
                var appSettings = ConfigurationManager.AppSettings;
                string result = appSettings[key] ?? "Not Found";
                return result.ToString();
            }
            catch (ConfigurationErrorsException ex)
            {
                logger.Error(ex, System.Reflection.MethodBase.GetCurrentMethod().Name);
                return string.Empty;
            }
        }

        public static bool VerificarConexionDB()
        {
            using (var db = new SafiEntities())
            {
                DbConnection conn = db.Database.Connection;
                try
                {
                    conn.Open();   // check the database connection
                    return true;
                }
                catch
                {
                    return false;
                }
            }
        }

        public static string GetJson(string texto)
        {
            var res = "";
            try
            {
                var json = JsonConvert.DeserializeObject<dynamic>(texto);
                res = json["access_token"];
                return res;
            }
            catch (Exception ex)
            {
                logger.Error(ex, System.Reflection.MethodBase.GetCurrentMethod().Name);
                return res; 
            }
        }

        public static string GetAccessTokenJson(string texto)
        {
            var res = string.Empty;
            try
            {
                var json = JsonConvert.DeserializeObject<dynamic>(texto);
                res = json["access_token"];
                return res;
            }
            catch (Exception ex)
            {
                logger.Error(ex, System.Reflection.MethodBase.GetCurrentMethod().Name);
                return res;
            }
        }

        public static void RegisterException(Exception ex, string nombreMetodoDAL)
        {
            logger.Error(ex, string.Format("UN ERROR OCURRIÓ EN {0}", nombreMetodoDAL));
        }

        public static void RegisterInfo(string message, Exception ex = null)
        {
            logger.Info(ex, message);
        }
    }
}
