using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace App_Mundial_Miles.Helpers
{
    public class CulturaPropia : CultureInfo
    {
        public CulturaPropia()
            : base("es-EC")
        {
            setFormatoNumerico();
            //setFormatoMoneda();
            //setFormatoFecha();
        }

        /// <summary>
        /// Establece formatos numéricos
        /// </summary>
        private void setFormatoNumerico()
        {
            NumberFormatInfo n = this.NumberFormat;
            n.NumberNegativePattern = 0;
            n.NumberGroupSeparator = "";
            n.NumberDecimalSeparator = ".";
        }

        /// <summary>
        /// Establece formatos de Moneda
        /// </summary>
        //private void setFormatoMoneda()
        //{
        //    NumberFormatInfo n = this.NumberFormat;
        //    n.CurrencyPositivePattern = 3;
        //    n.CurrencySymbol = "DOLAR";
        //}

        /// <summary>
        /// Establece formatos de fecha
        /// </summary>
        //private void setFormatoFecha()
        //{
        //    DateTimeFormatInfo d = this.DateTimeFormat;
        //    d.LongDatePattern = "'Montcada i Reixac, 'dd' de 'MMMM' de 'yyyy";
        //    d.ShortDatePattern = "dd.MMM.yyyy";
        //    d.FirstDayOfWeek = DayOfWeek.Monday;
        //    d.TimeSeparator = "/";
        //    d.DateSeparator = ".";
        //}
    }
}