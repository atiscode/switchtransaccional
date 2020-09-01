using System;
using System.Collections.Generic;
using System.Data;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using App_Mundial_Miles.Models;


namespace App_Mundial_Miles.Bussines
{
    public static class clsExcel
    {
        static string Separator = ('\t').ToString();
        static string SeparatorLine = ('\n').ToString();
        public static string ConvierteLibroMayorToString(List<dtoRMayor> elementos)
        {
            char comillas = '"';

            StringBuilder ResultBuilder = new StringBuilder();
            ResultBuilder.Length = 0;

            List<string> listParametrosOrdenados = new List<string>();

            ResultBuilder.Append(Separator);
            ResultBuilder.Append(Separator);
            ResultBuilder.Append(Separator);
            ResultBuilder.Append("REPORTE DE LIBRO MAYOR");
            ResultBuilder.Append('\n');
            ResultBuilder.Append('\n');


            foreach (var ele in elementos)
            {
                ResultBuilder.Append(Separator);
                ResultBuilder.Append(Separator);
                ResultBuilder.Append(Separator);
                ResultBuilder.Append("Saldo Anterior:");
                ResultBuilder.Append(Separator);
                ResultBuilder.Append(ele.SaldoAnterior.ToString("N2").Replace(".", "").Replace(",", "."));
                ResultBuilder.Append('\n');
                ResultBuilder.Append('\n');

                ResultBuilder.Append("No.Factura");
                ResultBuilder.Append(Separator);
                listParametrosOrdenados.Add("No.Factura");

                ResultBuilder.Append("No.Cuenta");
                ResultBuilder.Append(Separator);
                listParametrosOrdenados.Add("No.Cuenta");

                ResultBuilder.Append("Nombre Cuenta");
                ResultBuilder.Append(Separator);
                listParametrosOrdenados.Add("Nombre Cuenta");

                ResultBuilder.Append("No.Diario");
                ResultBuilder.Append(Separator);
                listParametrosOrdenados.Add("No.Diario");

                ResultBuilder.Append("Fecha");
                ResultBuilder.Append(Separator);
                listParametrosOrdenados.Add("Fecha");

                ResultBuilder.Append("Detalle");
                ResultBuilder.Append(Separator);
                listParametrosOrdenados.Add("Detalle");

                ResultBuilder.Append("Debe");
                ResultBuilder.Append(Separator);
                listParametrosOrdenados.Add("Debe");

                ResultBuilder.Append("Haber");
                ResultBuilder.Append(Separator);
                listParametrosOrdenados.Add("Haber");

                ResultBuilder.Append("Saldo");
                ResultBuilder.Append(Separator);
                listParametrosOrdenados.Add("Saldo");

                if (ResultBuilder.Length > Separator.Trim().Length)
                    ResultBuilder.Length = ResultBuilder.Length - Separator.Trim().Length;

                ResultBuilder.Append('\n');

                decimal saldo = 0;
                decimal saldoAnt = ele.SaldoAnterior;
                foreach (var items in ele.Items)
                {
                    saldo = saldoAnt + items.detcon_debe - items.detcon_haber;
                    saldoAnt = saldo;

                    AdicionaCelda(ref ResultBuilder, items.detcon_asiento.PadLeft(15, '0'));
                    AdicionaCelda(ref ResultBuilder, items.detcon_plan);
                    AdicionaCelda(ref ResultBuilder, items.nom_plan);
                    AdicionaCelda(ref ResultBuilder, items.detcon_numero.PadLeft(15, '0'));
                    AdicionaCelda(ref ResultBuilder, items.detcon_Fecha.ToShortDateString());
                    AdicionaCelda(ref ResultBuilder, items.detcon_detalle);
                    AdicionaCelda(ref ResultBuilder, items.detcon_debe.ToString("N2").Replace(".", "").Replace(",", "."));
                    AdicionaCelda(ref ResultBuilder, items.detcon_haber.ToString("N2").Replace(".", "").Replace(",", "."));
                    AdicionaCelda(ref ResultBuilder, saldo.ToString("N2").Replace(".", "").Replace(",", "."));
                    ResultBuilder.Append('\n');
                }

                ResultBuilder.Length = ResultBuilder.Length - 1;
                ResultBuilder.Append('\n');

                AdicionaCelda(ref ResultBuilder, "");
                AdicionaCelda(ref ResultBuilder, "");
                AdicionaCelda(ref ResultBuilder, "");
                AdicionaCelda(ref ResultBuilder, "");
                AdicionaCelda(ref ResultBuilder, "");
                AdicionaCelda(ref ResultBuilder, "Total: ");
                AdicionaCelda(ref ResultBuilder, ele.TotalDebe.ToString("N2").Replace(".", "").Replace(",", "."));
                AdicionaCelda(ref ResultBuilder, ele.TotalHaber.ToString("N2").Replace(".", "").Replace(",", "."));
                AdicionaCelda(ref ResultBuilder, "");

                ResultBuilder.Append('\n');
                ResultBuilder.Append('\n');
            }
            return ResultBuilder.ToString();
        }
        //public static bool GuardarExcel(string Contenido, string PathFisico, string NombreFichero)
        //{
        //    var res = true;

        //    var path = "";
        //    // Crea el archivo EXCEL
        //    System.Text.UnicodeEncoding Encoding = new System.Text.UnicodeEncoding();
        //    string fileName = Path.Combine(PathFisico, NombreFichero);
        //    TextWriter tw = new StreamWriter(fileName, false, Encoding);
        //    tw.Write(Contenido);
        //    tw.Flush();
        //    tw.Close();

        //    return res;
        //}
        public static bool GuardarExcelMod(string Contenido, string PathFisico, string NombreFichero)
        {
            var res = true;

            var path = "";
            // Crea el archivo EXCEL
            System.Text.UnicodeEncoding Encoding = new System.Text.UnicodeEncoding();
            string fileName = Path.Combine(PathFisico, NombreFichero);
            TextWriter tw = new StreamWriter(fileName, false, Encoding);
            tw.Write(Contenido);
            tw.Flush();
            tw.Close();

            return res;
        }
        private static void AdicionaCelda(ref StringBuilder salida, string valor)
        {
            string Filtro = " ";
            if (valor != null)
            {
                Filtro = valor.Replace("\r", " ");
                Filtro = Filtro.Replace("\n", " ");
                Filtro = Filtro.Replace(Separator, "");
            }

            salida.Append(Filtro);
            salida.Append(Separator);
        }
    }
}


