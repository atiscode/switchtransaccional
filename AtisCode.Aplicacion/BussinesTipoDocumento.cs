using AtisCode.Aplicacion.Model.db_Safi;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtisCode.Aplicacion
{
    public class BussinesTipoDocumento
    {
        SafiEntities model = new SafiEntities();
        Logger logger = LogManager.GetCurrentClassLogger();

        public void UpdateSecuencial(string tipo, string segmento)
        {
            try
            {
                var comp = model.tdocumentoscontador.FirstOrDefault(t => t.tipo == tipo && t.canal == segmento);
                if (comp != null)
                {
                    var res = (long)comp.nnum_doc;
                    comp.nnum_doc = res + 1;
                    model.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Tools.RegisterException(ex, System.Reflection.MethodBase.GetCurrentMethod().Name);
                throw;
            }
        }

        public async Task UpdateSequential(string tipo, string segmento) {
            try
            {
                var documento = await model.tdocumentoscontador.FirstOrDefaultAsync(t => t.tipo == tipo && t.canal == segmento);

                if (documento != null)
                {
                    var res = (long)documento.nnum_doc;
                    documento.nnum_doc = res + 1;
                    await model.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Tools.RegisterException(ex, System.Reflection.MethodBase.GetCurrentMethod().Name);
            }
        }

        public async Task<long> GetSequential(string tipo, string segmento)
        {
            long secuencial = 0;
            try
            {
                var documento = await model.tdocumentoscontador.FirstOrDefaultAsync(t => t.tipo == tipo && t.canal == segmento);

                if (documento != null)
                    secuencial = (long)documento.nnum_doc;

                return secuencial;
            }
            catch (Exception ex)
            {
                Tools.RegisterException(ex, System.Reflection.MethodBase.GetCurrentMethod().Name);
                return secuencial;
            }
        }

        public long GetSecuencial(string tipo, string segmento)
        {
            long res = 0;
            try
            {
                var comp = model.tdocumentoscontador.FirstOrDefault(t => t.tipo == tipo && t.canal == segmento);
                if (comp?.nnum_doc != null)
                    res = (long)comp.nnum_doc;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Stopped program because of exception");
            }
            return res;
        }



    }
}
