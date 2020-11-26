using AtisCode.Aplicacion.Model.db_Local;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtisCode.Aplicacion
{
    public class BussinesLogTran
    {
        SwitchAtiscodeEntities model = new SwitchAtiscodeEntities();
        Logger logger = LogManager.GetCurrentClassLogger();

        public void Add(AtisLogTran log)
        {
            try
            {
                model.AtisLogTran.Add(log);
                model.SaveChanges();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Stopped program because of exception");
                throw;
            }
        }

        public async Task AddAsync(AtisLogTran log)
        {
            try
            {
                model.AtisLogTran.Add(log);
                await model.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Tools.RegisterException(ex, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        public async Task AddQueueAsync(AtisQueueTransactions element)
        {
            try
            {
                model.AtisQueueTransactions.Add(element);
                await model.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Tools.RegisterException(ex, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        public string ExisteSecuencialAtisLogTran(string secuencial, string tipo, string segmento)
        {
            string res = "";
            try
            {
                var ele = model.AtisLogTran.FirstOrDefault(t => t.Secuencial == secuencial && !string.IsNullOrEmpty(t.NumeroDocumento) && t.Estado == "OK" && t.Tipo == tipo && t.Canal == segmento);
                if (ele != null)
                    res = ele.NumeroDocumento;
            }
            catch (Exception ex)
            {
                Tools.RegisterException(ex, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
            return res;
        }

        public async Task<string> ExistSequentialAtisLogTran(string secuencial, string tipo, string segmento)
        {
            string numberDocument = string.Empty;
            try
            {
                var document = await model.AtisLogTran.FirstOrDefaultAsync(t => t.Secuencial == secuencial && !string.IsNullOrEmpty(t.NumeroDocumento) && t.Estado == "OK" && t.Tipo == tipo && t.Canal == segmento);
                if (document != null)
                    numberDocument = document.NumeroDocumento;

                return numberDocument;
            }
            catch (Exception ex)
            {
                Tools.RegisterException(ex, System.Reflection.MethodBase.GetCurrentMethod().Name);
                return numberDocument;
            }
        }

        public bool IsValidDateInRange(string channel)
        {
            try
            {
                var informationChannel = model.ConsultarConfiguracionPrincipal(channel).FirstOrDefault();
                return DateTime.Now.Day >= informationChannel.StartControlDayTransactions.Value.Day && DateTime.Now.Day <= informationChannel.FinishControlDayTransactions.Value.Day;
            }
            catch (Exception ex)
            {
                Tools.RegisterException(ex, System.Reflection.MethodBase.GetCurrentMethod().Name);
                return false;
            }
        }

        public async Task<bool> ExistSequentialAtisLogEnqueue(string secuencial, string tipo, string segmento)
        {
            string numberDocument = string.Empty;
            try
            {
                bool exist = await model.AtisQueueTransactions.AnyAsync(t => t.Sequential == secuencial && t.TypeDocument == tipo && t.Channel == segmento);

                return exist;
            }
            catch (Exception ex)
            {
                Tools.RegisterException(ex, System.Reflection.MethodBase.GetCurrentMethod().Name);
                return false;
            }
        }


        public string ExisteSecuencialCotizacionAtisLogTran(string secuencial, string tipo)
        {
            string res = "";
            try
            {
                var ele = model.AtisLogTran.FirstOrDefault(t => t.Secuencial == secuencial && !string.IsNullOrEmpty(t.NumeroDocumento) && t.Estado == "OK" && t.Canal == "GENERAL COTIZACIONES");
                if (ele != null)
                    res = ele.NumeroDocumento;
            }
            catch (Exception ex)
            {
                Tools.RegisterException(ex, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
            return res;
        }


        public bool ExisteCanal(string canal)
        {
            try
            {
                var total = model.ConsultarSecuencialCanal(canal).FirstOrDefault().SecuencialFacturas; // Por cualquiera de los documentos devuelve 0 si no existe en canal.
                if (total > 0)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                Tools.RegisterException(ex, System.Reflection.MethodBase.GetCurrentMethod().Name);
                throw;
            }
        }

        public bool ValidarSecuencialNumerico(string secuencial)
        {
            try
            {
                var secuencialNumerico = Convert.ToInt64(secuencial);
                return true;
            }
            catch (Exception ex)
            {
                Tools.RegisterException(ex, System.Reflection.MethodBase.GetCurrentMethod().Name);
                return false;
            }
        }


        public bool InvocacionCanalCorrecto(string canalInvocacion, string canalParametrizado)
        {
            try
            {
                if (string.Equals(canalInvocacion, canalParametrizado))
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                Tools.RegisterException(ex, System.Reflection.MethodBase.GetCurrentMethod().Name);
                return false;
            }
        }

        public string GetSecuencial(string tipo, string segmento)
        {
            string res = "1";
            try
            {
                var secuencial = model.ConsultarSecuencialCanal(segmento).FirstOrDefault();

                if (secuencial != null)
                {
                    switch (tipo)
                    {
                        case "FACTURA":
                            res = secuencial.SecuencialFacturas.ToString();
                            break;
                        case "NOTACREDITO":
                            res = secuencial.SecuencialNotaCredito.ToString();
                            break;
                        case "COTIZACIONES":
                            res = secuencial.SecuencialCotizaciones.ToString();
                            break;
                        default:
                            res = "1";
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Stopped program because of exception");
            }
            return res;
        }

        public string ExisteSecuencialAtisLogTran(string secuencial)
        {
            string res = "";

            try
            {
                var ele = model.AtisLogTran.FirstOrDefault(t => t.Secuencial == secuencial && !string.IsNullOrEmpty(t.NumeroDocumento) && t.Estado == "OK");
                if (ele != null)
                    res = ele.NumeroDocumento;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Stopped program because of exception");
            }
            return res;
        }
    }
}
