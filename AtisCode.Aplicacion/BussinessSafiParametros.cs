using AtisCode.Aplicacion.Model.db_SafiParametros;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AtisCode.Aplicacion
{
    public partial class RespuestaComprobante {
        public bool Estado { get; set; }
        //public ComprobanteMasivoHistorial Documento{ get; set; }
        public string XMLDocumento { get; set; }
        public string NumeroDocumento { get; set;  }
        public string ClaveAcceso { get; set; }
        public string Mensaje { get; set; }
        public int CodigoRespuesta { get; set;  }
    }
    public class BussinessSafiParametros
    {
        private static SafiBDDParametrosEntities model = new SafiBDDParametrosEntities();
       private static  Logger logger = LogManager.GetCurrentClassLogger();

        public static RespuestaComprobante GetRespuestaComprobanteMasivo(string claveAcceso) {
            try
            {
                ComprobanteMasivoHistorial info = new ComprobanteMasivoHistorial();
                info = model.ComprobanteMasivoHistorial.FirstOrDefault(s => s.MensajeProceso.Contains(claveAcceso));

                var DatosDocumento = GetNumeroDocumentoFromXML(info.Xml);

                return new RespuestaComprobante { Estado = true, XMLDocumento = info.Xml , Mensaje = info.MensajeProceso, CodigoRespuesta = 200};
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Stopped program because of exception");
                return new RespuestaComprobante { Estado = false, XMLDocumento = "No se generó ningun documento", Mensaje = ex.Message, CodigoRespuesta = 400 };
            }
        }


        public static List<string> GetNumeroDocumentoFromXML(string myXmlString) {
            try
            {
                List<string> DatosDocumento = new List<string>();

                XmlDocument xml = new XmlDocument();
                xml.LoadXml(myXmlString); // suppose that myXmlString contains "<Names>...</Names>"

                XmlNodeList xnList = xml.SelectNodes("/infoTributaria");
                foreach (XmlNode xn in xnList)
                {
                    string ClaveAcceso = xn["claveAcceso"].InnerText;

                    string CodigoDocumento = xn["codDoc"].InnerText;
                    string Establecimiento = xn["estab"].InnerText;
                    string PuntoEmision = xn["ptoEmi"].InnerText;
                    string Secuencial = xn["secuencial"].InnerText;

                    string NumeroDocumento = CodigoDocumento + Establecimiento + PuntoEmision + Secuencial;

                    DatosDocumento.Add(ClaveAcceso); // 1  --> La clave de acceso
                    DatosDocumento.Add(NumeroDocumento); // 2  --> Número de documentos
                }
                return DatosDocumento;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Stopped program because of exception");
                return new List<string> { ex.Message, ex.Message };
            }

        }

    }
}
