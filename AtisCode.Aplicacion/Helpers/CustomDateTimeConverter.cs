using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtisCode.Aplicacion.Helpers
{
    public class CustomDateTimeConverter : DateTimeConverterBase//IsoDateTimeConverter
    {

        /// <summary>
        /// DateTime format
        /// </summary>
        private const string Format = "yyyy-MM-dd HH:mm:ss.ffffff"; //For specific Validation
        private const string Format2 = "yyyy-MM-dd HH:mm:ss"; //For specific validation

        /// <summary>
        /// Writes value to JSON
        /// </summary>
        /// <param name="writer">JSON writer</param>
        /// <param name="value">Value to be written</param>
        /// <param name="serializer">JSON serializer</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(((DateTime)value).ToString(Format));
        }

        /// <summary>
        /// Reads value from JSON
        /// </summary>
        /// <param name="reader">JSON reader</param>
        /// <param name="objectType">Target type</param>
        /// <param name="existingValue">Existing value</param>
        /// <param name="serializer">JSON serialized</param>
        /// <returns>Deserialized DateTime</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            try
            {
                DateTime dateFormatter;

                if (reader.Value == null)
                {
                    JObject jObject = JObject.Load(reader);
                    var date = jObject["date"];

                    dateFormatter = DateTime.Parse((string)date);
                    Tools.RegisterInfo("Convertion Datetime format new. See logs.");
                } else
                    dateFormatter = DateTime.Parse(reader.Value.ToString());

                return dateFormatter;
            }
            catch (Exception ex)
            {
                Tools.RegisterInfo("Error in CustomDateTimeConverter , Exception trigger format datetime. See logs.", ex);
                Tools.RegisterException(ex, System.Reflection.MethodBase.GetCurrentMethod().Name);
                return null;
            }

        }
    }
}
