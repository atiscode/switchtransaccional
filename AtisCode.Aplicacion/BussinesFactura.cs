using AtisCode.Aplicacion.Model.db_Safi;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtisCode.Aplicacion
{
    public class BussinesFactura
    {
        SafiEntities model = new SafiEntities();
        Logger logger = LogManager.GetCurrentClassLogger();

        public void Add(tfacturas log)
        {
            try
            {
                model.tfacturas.Add(log);
                model.SaveChanges();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Stopped program because of exception");
                throw;
            }
        }

        public void Update(tfacturas fact)
        {
            try
            {
                var item = model.tfacturas.FirstOrDefault(t => t.id == fact.id);
                if (item != null)
                    item.factura = fact.factura;

                model.SaveChanges();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Stopped program because of exception");
                throw;
            }

        }
    }
}
