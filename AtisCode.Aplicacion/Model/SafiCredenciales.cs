using AtisCode.Aplicacion.Model.db_Local;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtisCode.Aplicacion.Model
{
    public class SafiCredenciales : SwitchAtiscodeEntities
    {
        Logger logger = LogManager.GetCurrentClassLogger();

        public AtisSafiCredenciales ConsultarCredenciales(int anio)
        {
            try
            {
                AtisSafiCredenciales credenciales = new AtisSafiCredenciales();
                var consulta = AtisSafiCredenciales.Where(s => s.Anio == anio && s.Estado).FirstOrDefault();

                if (consulta != null)
                    credenciales = consulta;

                return credenciales;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Stopped program because of exception");
                throw;
            }
        }
    }
}
