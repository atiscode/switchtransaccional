using System.Collections.Generic;

namespace App_Mundial_Miles.Models
{
    public class JsonResultState
    {
        public List<string> Errors { get; set; }
        public string Xhtml { get; set; }
        public string Result { get; set; }
        public IDictionary<string,string> Parametros { get; set; }
        public IDictionary<string, object> JsonData { get; set; }

        public JsonResultState()
        {
            Errors = new List<string>();
            Parametros = new SortedList<string, string>();
            JsonData = new Dictionary<string, object>();
            Result = "OK";
        }
    }
}