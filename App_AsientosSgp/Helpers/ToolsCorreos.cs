using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Collections;
using System.Xml;
using App_Mundial_Miles.Models;
using System.IO;

namespace App_Mundial_Miles
{
    public static class ToolsCorreos
    {

        public enum TiposCorreos
        {
            Cotizacion= 100,
            Reservacion =200,
            OrdenServicio = 300
        }

        public enum Estado
        {
            Pendiente = 1,
            Procesando = 2,
            Procesado = 3,
            Error = 4
        } 

        public static string GetFilterReport(string hostUrl, NameValueCollection nameValueParams, string recordSelectionQuery="", NameValueCollection nameValueFormulas = null)
        {
            XmlReaderSettings settings = new XmlReaderSettings();

           settings.Schemas.Add("netlaisreports.xsd", hostUrl + "netlaisreports.xsd");
           settings.ValidationType = ValidationType.Schema;

            XmlDocument document = new XmlDocument();
            document.Schemas = settings.Schemas;

            XmlDeclaration declaracion = document.CreateXmlDeclaration("1.0", "utf-8", "no");
            document.AppendChild(declaracion);
            XmlElement main = document.CreateElement("dataReporte");
            XmlAttribute a1 = document.CreateAttribute("xmlns");
            a1.Value = hostUrl + "netlaisreports.xsd";
            main.Attributes.Append(a1);

            if (recordSelectionQuery != "")
            {
                XmlElement sf = document.CreateElement("SelectionFormula");
                sf.InnerText = recordSelectionQuery;
                main.AppendChild(sf); 
            }
          

            if (nameValueFormulas != null)
            {
                XmlElement formulas = document.CreateElement("Formulas");

                foreach (string key in nameValueFormulas.Keys )
                {
                    XmlElement f = document.CreateElement("formula");
                    XmlAttribute name = document.CreateAttribute("name");
                    name.Value = key;
                    XmlAttribute value = document.CreateAttribute("value");
                    value.Value = nameValueParams[key];
                    f.Attributes.Append(name);
                    f.Attributes.Append(value);
                    formulas.AppendChild(f);
                }
                main.AppendChild(formulas);
            }

            if (nameValueParams != null)
            {
                XmlElement parametros = document.CreateElement("Params");

                foreach (string key in nameValueParams.Keys )
                {
                    XmlElement f = document.CreateElement("parametro");
                    XmlAttribute name = document.CreateAttribute("name");
                    name.Value = key ;
                    XmlAttribute value = document.CreateAttribute("value");
                    value.Value = nameValueParams[key];
                    f.Attributes.Append(name);
                    f.Attributes.Append(value);
                    parametros.AppendChild(f);
                }
                main.AppendChild(parametros);
            }

           


            document.AppendChild(main);
            return document.InnerXml ;
        }

    }
}